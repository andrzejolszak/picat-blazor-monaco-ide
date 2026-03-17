using BlazorMonaco;
using BlazorMonaco.Editor;
using BlazorMonaco.Languages;
using IntervalTree;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using PicatBlazorMonaco.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;

namespace Ast2
{
    public class Ast2Editor
    {
        private readonly StandaloneCodeEditor _monacoEditor;
        private TextModel _model;

        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger _logger;
        private string[] _currentErrorDecors;

        private string[] _currentDeclarationDecors;

        private string[] _currentBuiltinReferenceDecors;

        private IntervalTree<int, DeclarationParser.Reference> _currentReferencesIntervalTree = new IntervalTree<int, DeclarationParser.Reference>();

        private IntervalTree<int, DeclarationParser.Declaration> _currentDeclarationsIntervalTree = new IntervalTree<int, DeclarationParser.Declaration>();

        public List<(string, bool?, int)> TestResults = new List<(string, bool?, int)>();

        public List<DeclarationParser.Declaration> Declarations = new();

        private BlazorMonaco.Languages.CompletionList _completionList = new();
        
        public Ast2Editor(StandaloneCodeEditor monacoEditor, IJSRuntime jsRuntime, ILogger logger)
        {
            this._monacoEditor = monacoEditor;
            this._jsRuntime = jsRuntime;
            this._logger = logger;
        }

        public async Task Init()
        {
#if DEBUG
            // Debuggger -> null, NoDebugger -> true, Test -> false
            if ((await _jsRuntime.InvokeAsync<object>(@"getWebDriver")) == null)
            {
                // Delay needed for the debugger to be able to attach...
                await Task.Delay(5000);
            }
#endif

            _model = await _monacoEditor.GetModel();
            await _model.PushEOL(EndOfLineSequence.CRLF);
            await _jsRuntime.InvokeVoidAsync(@"initializePicat");

            await BlazorMonaco.Languages.Global.RegisterCompletionItemProvider(_jsRuntime, "picat", async (modelUri, position, context) => _completionList);
            await RefreshCompletions();

            var markers = new List<MarkerData>
            {
                new() {
                    CodeAsObject = new MarkerCode
                    {
                        TargetUri = "https://www.google.com",
                        Value = "my-value"
                    },
                    Message = "Marker example",
                    Severity = MarkerSeverity.Hint,
                    StartLineNumber = 4,
                    StartColumn = 3,
                    EndLineNumber = 4,
                    EndColumn = 7
                }
            };

            await BlazorMonaco.Editor.Global.SetModelMarkers(_jsRuntime, _model, "default", markers);

            await BlazorMonaco.Languages.Global.RegisterCodeActionProvider(_jsRuntime, "picat", async (modelUri, range, context) =>
            {
                var codeActionList = new CodeActionList();
                if (context.Markers.Count == 0)
                    return codeActionList;

                codeActionList.Actions =
                [
                    new CodeAction
                    {
                        Title = "Fix test",
                        Kind = "quickfix",
                        Diagnostics = markers,
                        Edit = new WorkspaceEdit
                        {
                            Edits =
                            [
                                new WorkspaceTextEdit
                                {
                                    ResourceUri = modelUri,
                                    TextEdit = new TextEditWithOptions
                                    {
                                        Range = range,
                                        Text = "THIS"
                                    }
                                }
                            ]
                        },
                        IsPreferred = true
                    }
                ];
                return codeActionList;
            });

            this._logger?.LogInformation("Ast2Editor initialized!");
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

        public void ConsoleLog(string msg)
        {
            this._logger?.LogInformation(msg);
        }

        public void ConsoleError(string msg)
        {
            this._logger?.LogError(msg);
        }

        public async Task<BlazorMonaco.Range> GetDefinition(Position pos)
        {
            int offset = await this._model.GetOffsetAt(pos);
            DeclarationParser.Reference reff = this._currentReferencesIntervalTree.Query(offset).FirstOrDefault();
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

        public async Task<List<BlazorMonaco.Range>> GetReferences(Position pos)
        {
            List<BlazorMonaco.Range> res = new List<BlazorMonaco.Range>();

            int offset = await this._model.GetOffsetAt(pos);

            string name = null;
            int paramCount = 0;

            DeclarationParser.Reference reff = this._currentReferencesIntervalTree.Query(offset).FirstOrDefault();
            name = reff?.FirstMatch?.Name;
            paramCount = reff?.FirstMatch?.Args.Count ?? 0;

            if (name is null)
            {
                DeclarationParser.Declaration decl = this._currentDeclarationsIntervalTree.Query(offset).FirstOrDefault();
                name = decl?.Name;
                paramCount = decl?.Args.Count ?? 0;
            }

            if (name is not null)
            {
                foreach (DeclarationParser.Reference r in this._currentReferencesIntervalTree.Values)
                {
                    if (r.FirstMatch?.Name == name && r.FirstMatch?.Args.Count == paramCount)
                    {
                        Position p = await this._model.GetPositionAt(r.NameOffset);
                        res.Add(new BlazorMonaco.Range(p.LineNumber, p.Column, p.LineNumber, p.Column + r.FirstMatch.Name.Length));
                    }
                }
            }

            return res;
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
            await _monacoEditor.SetPosition(position, "src_src");
            await _monacoEditor.RevealPositionInCenter(position);
        }

        private async Task<Position> GetPositionAt(int offset)
        {
            return await _model.GetPositionAt(offset);
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
            List <ModelDeltaDecoration> decors = new List<ModelDeltaDecoration>(1);
            this.TestResults.Clear();
            try
            {
                this._currentDeclarationsIntervalTree.Clear();
                this.Declarations = DeclarationParser.ParseDeclarations(program);
                foreach (DeclarationParser.Declaration decl in this.Declarations)
                {
                    Position pos = await _model.GetPositionAt(decl.NameOffset);
                    ModelDeltaDecoration d = new ModelDeltaDecoration
                    {
                        Range = new BlazorMonaco.Range { StartColumn = pos.Column, StartLineNumber = pos.LineNumber, EndColumn = pos.Column + decl.Name.Length, EndLineNumber = pos.LineNumber },
                        Options = new ModelDecorationOptions
                        {
                            InlineClassName = "declarationDecoration,whitespaceRedSquare",
                            HoverMessage = new[] { new MarkdownString { Value = decl.Name + "/" + decl.Args.Count } },
                            Minimap = new ModelDecorationMinimapOptions { Color = "royalblue" },
                            OverviewRuler = new ModelDecorationOverviewRulerOptions { Color = "royalblue" },
                            Before = new InjectedTextOptions { Content = "[Test] " }
                        }
                    };

                    decors.Add(d);

                    if (decl.Name.StartsWith("test_") && decl.Args.Count == 0)
                    {
                        this.TestResults.Add((decl.Name, null, decl.NameOffset));
                    }

                    this._currentDeclarationsIntervalTree.Add(decl.NameOffset, decl.NameOffset + decl.Name.Length, decl);
                }

                references = DeclarationParser.ParseReferences(program, this.Declarations);
            }
            finally
            {
                this._currentDeclarationDecors = await _monacoEditor.DeltaDecorations(this._currentDeclarationDecors, decors.ToArray());
            }

            this._currentReferencesIntervalTree.Clear();
            foreach (DeclarationParser.Reference reff in references)
            {
                this._currentReferencesIntervalTree.Add(reff.NameOffset, reff.NameOffset + reff.FirstMatch.Name.Length, reff);
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
            var completionList = new BlazorMonaco.Languages.CompletionList
            {
                Suggestions = new List<BlazorMonaco.Languages.CompletionItem>()
            };

            foreach ((string, string, string) o in BuiltIns.Operators)
            {
                BlazorMonaco.Languages.CompletionItem i = new BlazorMonaco.Languages.CompletionItem
                {
                    LabelAsString = o.Item1,
                    InsertText = o.Item1,
                    Detail = "[" + o.Item2 + "]",
                    DocumentationAsString = o.Item3,
                    Kind = BlazorMonaco.Languages.CompletionItemKind.Operator,
                };

                /*
                    RangeAsObject = new BlazorMonaco.Range
                    {
                        StartLineNumber = 4,
                        StartColumn = 3,
                        EndLineNumber = 4,
                        EndColumn = 7
                    }
                 */

                completionList.Suggestions.Add(i);
            }

            foreach ((string, string, string) o in BuiltIns.Functions)
            {
                CompletionItem i = new CompletionItem
                {
                    LabelAsString = o.Item1,
                    InsertText = o.Item1,
                    Detail = "[" + o.Item2 + "]",
                    DocumentationAsString = o.Item3,
                    Kind = CompletionItemKind.Function,
                };

                completionList.Suggestions.Add(i);
            }

            DeclarationParser.Declaration prevDecl = null;
            foreach (DeclarationParser.Declaration d in this.Declarations)
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

                CompletionItem i = new CompletionItem
                {
                    LabelAsString = target,
                    InsertText = target,
                    Detail = "[User]",
                    DocumentationAsString = target + "\r\n\r\n" + d.Comment,
                    Kind = CompletionItemKind.Value,
                };

                completionList.Suggestions.Add(i);
            }

            this._completionList = completionList;
        }

        public async Task<List<(BlazorMonaco.Range Range, int LensType, string Text)>> GetLenses()
        {
            List<(BlazorMonaco.Range Range, int LensType, string Text)> lenses = new();

            foreach (DeclarationParser.Declaration d in this.Declarations)
            {
                Position pos = await _model.GetPositionAt(d.NameOffset);
                BlazorMonaco.Range range = new BlazorMonaco.Range { StartColumn = pos.Column, StartLineNumber = pos.LineNumber, EndColumn = pos.Column + d.Name.Length, EndLineNumber = pos.LineNumber };

                lenses.Add((range, 676767, "Lens example: " + d.Name + "/" + d.Args.Count));
            }

            return lenses;
        }

        public async Task InvokeLensAction(int lensType, int startLine, int startColumn)
        {
            Console.WriteLine($"Lens action: {lensType} at {startLine}, {startColumn}");
        }

        public async Task<PicatBlazorMonaco.Pages.Index.JsSignatureHelpResponse> GetSingatureHelp(Position pos)
        {
            return new PicatBlazorMonaco.Pages.Index.JsSignatureHelpResponse
            {
                activeParameter = 1,
                activeSignature = 0,
                signatures = new PicatBlazorMonaco.Pages.Index.JsSignatureHelpResponse.JsSignatureHelpResponseSignature[]
                {
                    new PicatBlazorMonaco.Pages.Index.JsSignatureHelpResponse.JsSignatureHelpResponseSignature
                    {
                        activeParameter = 1,
                        documentation = "sig doc",
                        label = "sig label",
                        parameters = new PicatBlazorMonaco.Pages.Index.JsSignatureHelpResponse.JsSignatureHelpResponseParam[]
                        {
                            new PicatBlazorMonaco.Pages.Index.JsSignatureHelpResponse.JsSignatureHelpResponseParam
                            {
                                documentation = "param doc 1",
                                label = "parm label 1"
                            },
                            new PicatBlazorMonaco.Pages.Index.JsSignatureHelpResponse.JsSignatureHelpResponseParam
                            {
                                documentation = "param doc 2",
                                label = "parm label 2"
                            }
                        }
                    }
                }
            };
        }
    }
}
