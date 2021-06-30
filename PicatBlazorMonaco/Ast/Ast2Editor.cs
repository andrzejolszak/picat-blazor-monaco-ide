using BlazorMonaco;
using BrowserInterop;
using BrowserInterop.Extensions;
using IntervalTree;
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

        private readonly IJSRuntime _jsRuntime;

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

            TextModel m = await _monacoEditor.GetModel();
            await m.PushEOL(EndOfLineSequence.CRLF);
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

        public async Task<int> GetCurrentEditorControlPositionStart()
        {
            Position p = await this._monacoEditor.GetPosition();
            TextModel m = await this._monacoEditor.GetModel();
            return await m.GetOffsetAt(p);
        }

        private async Task Select(int position)
        {
            Position newPos = await this.GetPositionAt(position);
            await SetAndRevealPosition(newPos);
        }

        private async Task SetAndRevealPosition(Position position)
        {
            await _monacoEditor.SetPosition(position);
            await _monacoEditor.RevealPosition(position);
        }

        private async Task<Position> GetPositionAt(int offset)
        {
            TextModel model = await this._monacoEditor.GetModel();
            return await model.GetPositionAt(offset);
        }

        private async Task<string[]> SetStyleForRange(int min, int max, ModelDecorationOptions selectedParentNodeText, string hoverMessage)
        {
            List<ModelDeltaDecoration> decors = new List<ModelDeltaDecoration>(1);
            Position startPos = await this.GetPositionAt(min);
            Position endPos = await this.GetPositionAt(max);
            ModelDeltaDecoration d = new ModelDeltaDecoration
            {
                Range = new BlazorMonaco.Range { StartColumn = startPos.Column, StartLineNumber = startPos.LineNumber, EndColumn = endPos.Column, EndLineNumber = endPos.LineNumber },
                Options = new ModelDecorationOptions
                {
                    InlineClassName = selectedParentNodeText.InlineClassName,
                    Minimap = new ModelDecorationMinimapOptions { Color = "red" },
                    OverviewRuler = new ModelDecorationOverviewRulerOptions { Color = "blue" }
                }
            };

            if (hoverMessage != null)
            {
                d.Options.HoverMessage = new[] { new MarkdownString { Value = hoverMessage } };
            }

            decors.Add(d);

            return await _monacoEditor.DeltaDecorations(null, decors.ToArray());
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
                compl.detail = o.Item2;
                compl.documentation = o.Item3;
                compl.kind = CompletionItemKind.Operator;

                jsCompletions.Add(compl);
            }

            foreach ((string, string, string) o in BuiltIns.Functions)
            {
                // Schema: https://microsoft.github.io/monaco-editor/api/interfaces/monaco.languages.completionitem.html
                dynamic compl = new System.Dynamic.ExpandoObject();
                compl.label = o.Item1;
                compl.insertText = o.Item1;
                compl.detail = o.Item2;
                compl.documentation = o.Item3;
                compl.kind = CompletionItemKind.Function;

                jsCompletions.Add(compl);
            }

            await _jsRuntime.InvokeVoidAsync(@"setCompletionsArray", jsCompletions);
        }
    }
}
