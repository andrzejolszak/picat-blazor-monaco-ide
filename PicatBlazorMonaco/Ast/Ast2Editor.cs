using BlazorMonaco;
using BrowserInterop;
using BrowserInterop.Extensions;
using IntervalTree;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PicatBlazorMonaco.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ast2
{
    public class Ast2Editor
    {
        private readonly MonacoEditor _monacoEditor;
        private TextModel _model;

        private readonly IJSRuntime _jsRuntime;

        private string[] _currentErrorDecors;

        private string[] _currentDeclarationDecors;

        private string[] _currentBuiltinReferenceDecors;

        private List<DeclarationParser.Declaration> _currentDeclarations = new List<DeclarationParser.Declaration>();

        private IntervalTree<int, DeclarationParser.Reference> _currentReferences = new IntervalTree<int, DeclarationParser.Reference>();

        public List<(string, bool?, int)> TestResults = new List<(string, bool?, int)>();

        public Ast2Editor(MonacoEditor monacoEditor, IJSRuntime jsRuntime)
        {
            this._monacoEditor = monacoEditor;
            this._jsRuntime = jsRuntime;
        }

        public async Task Init()
        {
#if DEBUG
            // Debuggger -> null, NoDebugger -> true, Test -> false
            if ((await _jsRuntime.InvokeAsync<object>(@"getWebDriver")) == null)
            {
                // Delay needed for the debugger to be able to attach...
                await Task.Delay(5000);
                WindowConsole.IsEnabled = true;
            }
#else
            WindowConsole.IsEnabled = false;
#endif

            _model = await _monacoEditor.GetModel();
            await _model.PushEOL(EndOfLineSequence.CRLF);
            await _jsRuntime.InvokeVoidAsync(@"initializeCompletions");
            await RefreshCompletions();
        }

        public static StandaloneEditorConstructionOptions GetEditorOptions()
        {
            return new StandaloneEditorConstructionOptions
            {
                Language = "picat",
                Theme = "picatTheme",
                InsertSpaces = true,
                FormatOnPaste = true,
                FormatOnType = true,
                DetectIndentation = true,
                TabSize = 4,
                GlyphMargin = true
            };
        }

        public async Task ConsoleLog(string msg)
        {
            var window = await _jsRuntime.Window();
            await window.Console.Log(msg);
        }

        public async Task ConsoleError(string msg)
        {
            var window = await _jsRuntime.Window();
            await window.Console.Error(msg);
        }

        private async Task<Selection> GetSelection()
        {
            return await this._monacoEditor.GetSelection();
        }

        public async Task<BlazorMonaco.Range> GetDefinition(Position pos)
        {
            int offset = await this._model.GetOffsetAt(pos);
            DeclarationParser.Reference reff = this._currentReferences.Query(offset).FirstOrDefault();
            if (reff != null)
            {
                Position p = await this._model.GetPositionAt(reff.FirstMatch.NameOffset);
                return new BlazorMonaco.Range(p.LineNumber, p.Column, p.LineNumber, p.Column + reff.FirstMatch.Name.Length);
            }
            else
            {
                return null;
            }
        }

        public async Task<int> GetCurrentEditorControlPositionStart()
        {
            Position p = await this._monacoEditor.GetPosition();
            return await _model.GetOffsetAt(p);
        }

        public async Task Select(int position)
        {
            Position newPos = await this.GetPositionAt(position);
            await SetAndRevealPosition(newPos);
        }

        public async Task SetAndRevealPosition(Position position)
        {
            await _monacoEditor.SetPosition(position);
            await _monacoEditor.RevealPositionInCenter(position);
        }

        private async Task<Position> GetPositionAt(int offset)
        {
            return await _model.GetPositionAt(offset);
        }


        public async Task<int> GetSelectionStart(ElementReference element)
        {
            int pos = await _jsRuntime.InvokeAsync<int>("getSelectedStart", element);
            return pos;
        }

        public async Task MoveToError(string line)
        {
            // *** SYNTAX ERROR *** (222-228) wrong head.
            if (line.StartsWith("*** SYNTAX ERROR *** "))
            {
                int open = line.IndexOf("(");
                int close = line.IndexOf(")");
                if (open > 0 && close > 0 && open + 1 < close)
                {
                    string range = line.Substring(open + 1, close - open - 1);
                    string[] lines = range.Split("-");
                    if (lines.Length == 2 && int.TryParse(lines[0], out int start) && int.TryParse(lines[1], out int end) && start >= 0 && start <= end)
                    {
                        Position pos = new Position { LineNumber = start, Column = 1 };
                        await SetAndRevealPosition(pos);
                    }
                }
            }
        }

        public async Task<int> UpdateErrors(string output)
        {
            List<(int startLine, int endLine, string hoverMessage)> errors = new List<(int startLine, int endLine, string hoverMessage)>();
            foreach(string line in output.Split(new char[] { '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries))
            {
                // *** SYNTAX ERROR *** (222-228) wrong head.
                if (line.StartsWith("*** SYNTAX ERROR *** "))
                {
                    int open = line.IndexOf("(");
                    int close = line.IndexOf(")");
                    if (open > 0 && close > 0 && open + 1 < close)
                    {
                        string range = line.Substring(open + 1, close - open - 1);
                        string[] lines = range.Split("-");
                        if (lines.Length == 2 && int.TryParse(lines[0], out int start) && int.TryParse(lines[1], out int end) && start >= 0 && start <= end)
                        {
                            errors.Add((start, end, line));
                        }
                    }
                }
            }

            List<ModelDeltaDecoration> decors = new List<ModelDeltaDecoration>(1);
            foreach ((int startLine, int endLine, string hoverMessage) error in errors)
            {
                ModelDeltaDecoration d = new ModelDeltaDecoration
                {
                    Range = new BlazorMonaco.Range { StartColumn = 1, StartLineNumber = error.startLine, EndColumn = 1, EndLineNumber = error.endLine },
                    Options = new ModelDecorationOptions
                    {
                        IsWholeLine = true,
                        LinesDecorationsClassName = "decorationGlyphMarginClass",
                        HoverMessage = new[] { new MarkdownString { Value = error.hoverMessage } },
                        Minimap = new ModelDecorationMinimapOptions { Color = "red" },
                        OverviewRuler = new ModelDecorationOverviewRulerOptions { Color = "red" }
                    }
                };

                decors.Add(d);
            }

            this._currentErrorDecors = await _monacoEditor.DeltaDecorations(this._currentErrorDecors, decors.ToArray());

            return errors.Count;
        }

        public async Task UpdateDeclarations(string program)
        {
            if(_model == null)
            {
                return;
            }

            List<DeclarationParser.Reference> references = new List<DeclarationParser.Reference>();
            List<DeclarationParser.Declaration> declarations = new List<DeclarationParser.Declaration>();
            List <ModelDeltaDecoration> decors = new List<ModelDeltaDecoration>(1);
            this.TestResults.Clear();
            try
            {
                declarations = DeclarationParser.ParseDeclarations(program);
                foreach (DeclarationParser.Declaration decl in declarations)
                {
                    Position pos = await _model.GetPositionAt(decl.NameOffset);
                    ModelDeltaDecoration d = new ModelDeltaDecoration
                    {
                        Range = new BlazorMonaco.Range { StartColumn = pos.Column, StartLineNumber = pos.LineNumber, EndColumn = pos.Column + decl.Name.Length, EndLineNumber = pos.LineNumber },
                        Options = new ModelDecorationOptions
                        {
                            InlineClassName = "declarationDecoration",
                            HoverMessage = new[] { new MarkdownString { Value = decl.Name + "/" + decl.Args.Count } },
                            Minimap = new ModelDecorationMinimapOptions { Color = "royalblue" },
                            OverviewRuler = new ModelDecorationOverviewRulerOptions { Color = "royalblue" }
                        }
                    };

                    decors.Add(d);

                    if (decl.Name.StartsWith("test_") && decl.Args.Count == 0)
                    {
                        this.TestResults.Add((decl.Name, null, decl.NameOffset));
                    }
                }

                references = DeclarationParser.ParseReferences(program, declarations);
            }
            finally
            {
                this._currentDeclarationDecors = await _monacoEditor.DeltaDecorations(this._currentDeclarationDecors, decors.ToArray());
            }

            this._currentDeclarations = declarations;
            this._currentReferences.Clear();
            foreach (DeclarationParser.Reference reff in references)
            {
                this._currentReferences.Add(reff.NameOffset, reff.NameOffset + reff.FirstMatch.Name.Length, reff);
            }

            // Built-in refences for hover
            List<ModelDeltaDecoration> builtinRefDecors = new List<ModelDeltaDecoration>(1);
            try
            {
                List<DeclarationParser.Reference> builtinReferences = DeclarationParser.ParseReferences(program, BuiltIns.BuiltinsDeclarations);
                foreach (DeclarationParser.Reference reff in builtinReferences)
                {
                    Position pos = await _model.GetPositionAt(reff.NameOffset);
                    ModelDeltaDecoration d = new ModelDeltaDecoration
                    {
                        Range = new BlazorMonaco.Range { StartColumn = pos.Column, StartLineNumber = pos.LineNumber, EndColumn = pos.Column + reff.FirstMatch.Name.Length, EndLineNumber = pos.LineNumber },
                        Options = new ModelDecorationOptions
                        {
                            HoverMessage = new[] { new MarkdownString { Value = reff.FirstMatch.Comment } },
                        }
                    };

                    builtinRefDecors.Add(d);
                }
            }
            finally
            {
                this._currentBuiltinReferenceDecors = await _monacoEditor.DeltaDecorations(this._currentBuiltinReferenceDecors, builtinRefDecors.ToArray());
            }
        }

        public async Task RefreshCompletions()
        {
            List<object> jsCompletions = new List<object>();
            foreach ((string, string, string) o in BuiltIns.Operators)
            {
                // Schema: https://microsoft.github.io/monaco-editor/api/interfaces/monaco.languages.completionitem.html
                dynamic compl = new System.Dynamic.ExpandoObject();
                compl.label = o.Item1;
                compl.insertText = o.Item1;
                compl.detail = "[" + o.Item2 + "]";
                compl.documentation = o.Item3;
                compl.kind = CompletionItemKind.Operator;

                jsCompletions.Add(compl);
            }

            foreach ((string, string, string) o in BuiltIns.Functions)
            {
                dynamic compl = new System.Dynamic.ExpandoObject();
                compl.label = o.Item1;
                compl.insertText = o.Item1;
                compl.detail = "[" + o.Item2 + "]";
                compl.documentation = o.Item3;
                compl.kind = CompletionItemKind.Function;

                jsCompletions.Add(compl);
            }

            DeclarationParser.Declaration prevDecl = null;
            foreach (DeclarationParser.Declaration d in this._currentDeclarations)
            {
                if (prevDecl != null && prevDecl.Name == d.Name && prevDecl.Args.Count == d.Args.Count && d.Comment == null)
                {
                    continue;
                }

                string target = d.Name;
                if (d.Args.Count > 0)
                {
                    target = $"{d.Name}({string.Join(", ", d.Args)})";
                }

                dynamic compl = new System.Dynamic.ExpandoObject();
                compl.label = target;
                compl.insertText = target;
                compl.detail = "[User]";
                compl.documentation = target + ":\r\n" + d.Comment;
                compl.kind = CompletionItemKind.Function;

                jsCompletions.Add(compl);
            }

            await _jsRuntime.InvokeVoidAsync(@"setCompletionsArray", jsCompletions);
        }
    }
}
