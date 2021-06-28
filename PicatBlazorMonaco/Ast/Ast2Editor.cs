using BlazorMonaco;
using BrowserInterop;
using BrowserInterop.Extensions;
using IntervalTree;
using Microsoft.JSInterop;
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

        private long _debugId = 0;

        /// <summary>
        /// Only nodes that have a View are added here, i.e. children-based nodes are not, but you can retrieve them
        /// through the parent relations.
        /// </summary>
        private IntervalTree<int, (int, Node)> _visibleNodesIntervalTree = new IntervalTree<int, (int, Node)>();

        private bool _refreshing;

        public Node Root { get; private set; }

        public List<(Type, Func<Node>)> FactoryRegistry { get; } = new List<(Type, Func<Node>)>();

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
            await Styles.CreateCssStyles(this._jsRuntime);
            await this.AddCommands();
        }

        public void SetRoot(Node root)
        {
            this.Root = root;
            this.CurrentNode = root;
            this.CurrentPosition = new Position();
            this.CurrentOffset = 0;
            this.CurrentSelectionStart = 0;
            this.CurrentSelectionEnd = 0;
        }

        public static StandaloneEditorConstructionOptions GetEditorOptions()
        {
            return new StandaloneEditorConstructionOptions
            {
                Language = "projectional",
                InsertSpaces = true,
                FormatOnPaste = true,
                FormatOnType = true,
                DetectIndentation = false,
                TabSize = 4,
                GlyphMargin = true,
                Minimap = new EditorMinimapOptions { Enabled = false },
                RenderWhitespace = "all",
                // CursorStyle = "underline",
                Value = "<initial state>",
                WordBasedSuggestions = false
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

        private async Task AddCommands()
        {
            await _monacoEditor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.Home, async (editor, keyCode) =>
            {
                await SetAndRevealPosition(new Position { LineNumber = 0, Column = 0 });
            });

            await _monacoEditor.AddCommand((int)KeyCode.Home, async (editor, keyCode) =>
            {
                await SetAndRevealPosition(new Position { LineNumber = this.CurrentPosition.LineNumber, Column = 0 });
            });

            await _monacoEditor.AddCommand((int)KeyCode.End, async (editor, keyCode) =>
            {
                await SetAndRevealPosition(new Position { LineNumber = this.CurrentPosition.LineNumber, Column = 1000000 });
            });

            await _monacoEditor.AddAction("suggest", "", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.Space }, "!suggestWidgetVisible", null, null, 0, async (editor, keyCode) =>
            {
                await this.RefreshCompletions();

                // Show completion programmatic
                using JsonDocument doc2 = JsonDocument.Parse(@"{}");
                JsonElement root2 = doc2.RootElement;
                await _monacoEditor.Trigger("api", "editor.action.triggerSuggest", root2);
                // await _monacoEditor.Trigger("api", "selectNextSuggestion", root2);
            });

            await _monacoEditor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_X, async (editor, keyCode) =>
            {
                Console.WriteLine("CMD:" + keyCode);
            });

            await _monacoEditor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_Y, async (editor, keyCode) =>
            {
                Console.WriteLine("CMD:" + keyCode);
            });


            await _monacoEditor.AddCommand((int)KeyMode.Shift | (int)KeyCode.LeftArrow, async (editor, keyCode) =>
            {
                // registerEditorCommand https://github.com/microsoft/vscode/blob/94c9ea46838a9a619aeafb7e8afd1170c967bb55/src/vs/editor/browser/controller/coreCommands.ts
                using JsonDocument doc2 = JsonDocument.Parse(@"{}");
                JsonElement root2 = doc2.RootElement;
                await _monacoEditor.Trigger("api", "cursorLeftSelect", root2);
            });

            await _monacoEditor.AddCommand((int)KeyMode.Shift | (int)KeyCode.RightArrow, async (editor, keyCode) =>
            {
                using JsonDocument doc2 = JsonDocument.Parse(@"{}");
                JsonElement root2 = doc2.RootElement;
                await _monacoEditor.Trigger("api", "cursorRightSelect", root2);
            });

            await _monacoEditor.AddCommand((int)KeyMode.Shift | (int)KeyCode.UpArrow, async (editor, keyCode) =>
            {
                using JsonDocument doc2 = JsonDocument.Parse(@"{}");
                JsonElement root2 = doc2.RootElement;
                await _monacoEditor.Trigger("api", "cursorUpSelect", root2);
            });

            await _monacoEditor.AddCommand((int)KeyMode.Shift | (int)KeyCode.DownArrow, async (editor, keyCode) =>
            {
                using JsonDocument doc2 = JsonDocument.Parse(@"{}");
                JsonElement root2 = doc2.RootElement;
                await _monacoEditor.Trigger("api", "cursorDownSelect", root2);
            });

            await _monacoEditor.AddAction("RightArrow", "", new[] { (int)KeyCode.RightArrow }, null, null, null, 0, async (editor, keyCode) =>
            {
                using JsonDocument doc2 = JsonDocument.Parse(@"{}");
                JsonElement root2 = doc2.RootElement;
                await _monacoEditor.Trigger("api", "cursorRight", root2);
            });

            await _monacoEditor.AddAction("LeftArrow", "", new[] { (int)KeyCode.LeftArrow }, null, null, null, 0, async (editor, keyCode) =>
            {
                using JsonDocument doc2 = JsonDocument.Parse(@"{}");
                JsonElement root2 = doc2.RootElement;
                await _monacoEditor.Trigger("api", "cursorLeft", root2);
            });

            await _monacoEditor.AddAction("UpArrow", "", new[] { (int)KeyCode.UpArrow }, "!suggestWidgetVisible", null, null, 0, async (editor, keyCode) =>
            {
                using JsonDocument doc2 = JsonDocument.Parse(@"{}");
                JsonElement root2 = doc2.RootElement;
                await _monacoEditor.Trigger("api", "cursorUp", root2);
            });

            await _monacoEditor.AddAction("DownArrow", "", new[] { (int)KeyCode.DownArrow }, "!suggestWidgetVisible", null, null, 0, async (editor, keyCode) =>
            {
                using JsonDocument doc2 = JsonDocument.Parse(@"{}");
                JsonElement root2 = doc2.RootElement;
                await _monacoEditor.Trigger("api", "cursorDown", root2);
            });

            await _monacoEditor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_Z, async (editor, keyCode) =>
            {
                Console.WriteLine("CMD:" + keyCode);
                Position p = await _monacoEditor.GetPosition();
                TextModel m = await _monacoEditor.GetModel();
                await m.ApplyEdits(new List<IdentifiedSingleEditOperation> {
                new IdentifiedSingleEditOperation() {
                    Range = new BlazorMonaco.Range(p.LineNumber, p.Column, p.LineNumber, p.Column),
                    // null for delete
                    Text = "aaa"
                } });

                await SetAndRevealPosition(await m.ModifyPosition(p, 3));

                // TYPE programmatic
                using JsonDocument doc = JsonDocument.Parse(@"{""text"": ""test""}");
                JsonElement root = doc.RootElement;
                await _monacoEditor.Trigger("keyboard", "type", root);
            });

            /*
            import * as monaco from 'monaco-editor/esm/vs/editor/edcore.main';
            import { SimpleEditorModelResolverService } from 'monaco-editor/esm/vs/editor/standalone/browser/simpleServices';
            //*
            //* Monkeypatch to make 'Find All References' work across multiple files
            //* https://github.com/Microsoft/monaco-editor/issues/779#issuecomment-374258435
            //*
            SimpleEditorModelResolverService.prototype.findModel = function(editor, resource) {
                return monaco.editor.getModels().find(model => model.uri.toString() === resource.toString());
            };
            */

            /*
            await m.ApplyEdits(new List<IdentifiedSingleEditOperation> {
                new IdentifiedSingleEditOperation() {
                    Range = range,
                    Text = "aaaaaaa"
                } });
            */
        }

        public async Task OnMouseDown(EditorMouseEvent e)
        {
            TextModel m = await this._monacoEditor.GetModel();
            int pos = await m.GetOffsetAt(e.Target.Position);
            if (pos < 0)
            {
                return;
            }

            await ConsoleLog("OnMouseDown");

            (int, Node) node = this.AtPosition(pos);
            if (node.Item2 != null)
            {
                UserInputResult res = node.Item2.OnMouseClickBubble(this.GetEditorState(), e, node.Item2);
                await this.HandleUserInputResult(res);
            }
        }

        public async Task OnTextTyped(ModelContentChangedEvent eventArg)
        {
            if (this._refreshing)
            {
                return;
            }

            await ConsoleLog("OnTextTyped: " + eventArg.Changes[0]?.Text);

            var window = await _jsRuntime.Window();
            await using (await window.Console.Time("OnTextTyped" + _debugId++))
            {
                if (this._refreshing || eventArg == null)
                {
                    return;
                }

                ModelContentChange change = eventArg.Changes[0];

                if (this.LastCompletionsById.ContainsKey(change.Text))
                {
                    return;
                }

                bool isDel = change.RangeLength != 0;
                string text = (isDel ? new string('\b', change.RangeLength) : string.Empty) + change.Text;

                UserInputResult res = this.CurrentNode.OnTextChangingBubble(this.GetEditorState(), text, this.CurrentNode);
                res.NeedsGlobalEditorRefresh = true;
                await HandleUserInputResult(res);
            }
        }

        private async Task RefreshCompletions()
        {
            List<AstAutocompleteItem> completions = await this.GetCompletions();
            List<object> jsCompletions = new List<object>(completions.Count);
            foreach (AstAutocompleteItem c in completions)
            {

                // Schema: https://microsoft.github.io/monaco-editor/api/interfaces/monaco.languages.completionitem.html
                dynamic compl = new System.Dynamic.ExpandoObject();
                compl.insertText = c.Id;
                compl.filterText = c.SourceToMatch;
                compl.label = c.MenuText;
                compl.documentation = c.DocTitle + " " + c.DocText;
                compl.kind = 1;
                compl.detail = c.Id;

                jsCompletions.Add(compl);
            }

            await _jsRuntime.InvokeVoidAsync(@"setCompletionsArray", jsCompletions);
        }

        public async Task HandleUserInputResult(UserInputResult res)
        {
            if (res.NeedsGlobalEditorRefresh)
            {
                if (this._refreshing)
                {
                    return;
                }

                this.Root.CreateView(this.GetEditorState());
                await this.RefreshWholeEditor(res);
            }
        }

        public async Task RefreshWholeEditor(UserInputResult res)
        {
            if (this._refreshing)
            {
                return;
            }

            var window = await _jsRuntime.Window();
            await using (await window.Console.Time("RefreshWholeEditor" + _debugId++))
            {
                this._refreshing = true;
                this._visibleNodesIntervalTree.Clear();
                this.VisibleNodesList.Clear();

                List<(string text, PositionInfo info, ModelDecorationOptions style, ModelDecorationOptions backgroundStyle, ModelDecorationOptions overlayStyle)> renderInfo = new List<(string text, PositionInfo info, ModelDecorationOptions style, ModelDecorationOptions backgroundStyle, ModelDecorationOptions overlayStyle)>(1024);
                int addedLength = 0;


                await using (await window.Console.Time("RenderViewsRecusive" + _debugId++))
                {
                    RenderViewsRecusive(this.Root, renderInfo, ref addedLength);
                }

                StringBuilder sb = new StringBuilder();
                List<ModelDeltaDecoration> decors = new List<ModelDeltaDecoration>(renderInfo.Count);
                int line = 1;
                int colInLine = 1;
                foreach (var r in renderInfo)
                {
                    sb.Append(r.text);
                    int newLines = r.text.Count(x => x == '\n');
                    int startLine = line;
                    int startCol = colInLine;

                    // OBS: cannot use TextModel.GetPositionAt here, because we still have the previous text
                    if (newLines > 0)
                    {
                        line += newLines;
                        colInLine = 1;
                    }

                    if (r.text != "\r\n")
                    {
                        colInLine += r.text.Length;
                        ModelDeltaDecoration d = new ModelDeltaDecoration
                        {
                            Range = new BlazorMonaco.Range { StartColumn = startCol, StartLineNumber = startLine, EndColumn = colInLine, EndLineNumber = line },
                            Options = new ModelDecorationOptions
                            {
                                InlineClassName = r.style?.InlineClassName ?? "decorationPointer",
                                HoverMessage = new[] { new MarkdownString { Value = r.info.StartOffset + "-" + r.info.EndOffset } },
                                Minimap = new ModelDecorationMinimapOptions { Color = "red" },
                                OverviewRuler = new ModelDecorationOverviewRulerOptions { Color = "blue" }
                            }
                        };
                        decors.Add(d);
                    }
                }

                Position oldPos = this.CurrentPosition;
                Node oldNode = this.CurrentNode;

                await using (await window.Console.Time("Editor.SetValue" + _debugId++))
                {
                    await this._monacoEditor.SetValue(sb.ToString());
                }

                await using (await window.Console.Time("Editor.DeltaDecorations" + _debugId++))
                {
                    _ = await _monacoEditor.DeltaDecorations(null, decors.ToArray());
                }

                this._refreshing = false;

                await using (await window.Console.Time("SetPosition" + _debugId++))
                {
                    if (res.NewLocalPosition.HasValue)
                    {
                        Position newPos = await this.GetPositionAt(res.NewLocalPositionContext.PositionInfo.StartOffset + res.NewLocalPosition.Value);
                        if (newPos != null)
                        {
                            await SetAndRevealPosition(newPos);
                        }
                    }
                    else if (res.ChangeFocusToNode != null)
                    {
                        Position newPos = await this.GetPositionAt(res.ChangeFocusToNode.PositionInfo.StartOffset);
                        if (newPos != null)
                        {
                            await SetAndRevealPosition(newPos);
                        }
                    }
                    else
                    {
                        await SetAndRevealPosition(oldPos);
                    }
                }
            }
        }

        private async Task<Selection> GetSelection()
        {
            return await this._monacoEditor.GetSelection();
        }

        private void RenderViewsRecusive(Node node, List<(string text, PositionInfo info, ModelDecorationOptions style, ModelDecorationOptions backgroundStyle, ModelDecorationOptions overlayStyle)> res, ref int addedLength)
        {
            if (node.View != null && (node.VisualChildren?.Count ?? 0) == 0)
            {
                node.PositionInfo = new PositionInfo { StartOffset = addedLength, EndOffset = addedLength + node.View.Text.Length};
                res.Add((node.View.Text, node.PositionInfo, node.View.Style, node.View.BackgroundStyle, node.View.OverlayStyle));
                addedLength += node.View.Text.Length;
                this._visibleNodesIntervalTree.Add(node.PositionInfo.StartOffset, node.PositionInfo.EndOffset, (this.VisibleNodesList.Count, node));
                this.VisibleNodesList.Add(node);
            }
            else if (node.View == null && node.VisualChildren?.Count > 0)
            {
                int childrenStart = addedLength;
                foreach (Node n in node.VisualChildren)
                {
                    RenderViewsRecusive(n, res, ref addedLength);
                }

                node.PositionInfo = new PositionInfo { StartOffset = childrenStart, EndOffset = addedLength};
            }
            else
            {
                throw new InvalidOperationException($"Either View or Children have to be set on {node.GetType().Name}. viewIsNull={node.View == null}");
            }
        }

        public async Task<int> GetCurrentEditorControlPositionStart()
        {
            Position p = await this._monacoEditor.GetPosition();
            TextModel m = await this._monacoEditor.GetModel();
            return await m.GetOffsetAt(p);
        }

        public int ListIndex() => AtPosition(this.CurrentOffset).Item1;

        public List<Node> VisibleNodesList { get; } = new List<Node>(100);

        public Node CurrentNode { get; private set; }

        public Dictionary<string, AstAutocompleteItem> LastCompletionsById { get; private set; } = new Dictionary<string, AstAutocompleteItem>();

        public Position CurrentPosition { get; private set; } = new Position();

        public int CurrentOffset { get; private set; }
        // TODO: this breaks down after text editign
        public List<string> SelectionStyleIds { get; private set; } = new List<string>(2);
        public int CurrentSelectionStart { get; private set; }
        public int CurrentSelectionEnd { get; private set; }

        public (int VisibleNodesListIndex, Node) AtPosition(int position)
        {
            (int, Node)[] res = this._visibleNodesIntervalTree.Query(position + 1).ToArray();

            if (res.Length == 0)
            {
                return (0, null);
            }

            if (res.Length == 1)
            {
                return res[0];
            }

            if (res.Length > 2)
            {
                throw new InvalidOperationException("res.Length>2 = " + res.Length);
            }

            return res[0].Item1 < res[1].Item1 ? res[0] : res[1];
        }

        private async Task PopupMenu_ClosedWithouSelection(object sender, EventArgs e)
        {
            await this.HandleUserInputResult(UserInputResult.HandledNeedsGlobalRefresh());
        }


        public async Task OnAstCompletionSelected(string completionId)
        {
            await ConsoleLog("OnAstCompletionSelected" + completionId);
            AstAutocompleteItem ai = this.LastCompletionsById[completionId];
            ai.TriggerItemSelected();
            await this.HandleUserInputResult(UserInputResult.HandledNeedsGlobalRefresh());
        }

        public async Task OnKeyUp(KeyboardEvent e)
        {
            await ConsoleLog("OnKeyUp");
            Node currNode = this.CurrentNode;
            UserInputResult res = currNode.OnKeyUpBubble(this.GetEditorState(), e, currNode);
            await this .HandleUserInputResult(res);
        }

        public async Task OnKeyDown(KeyboardEvent e)
        {
            if (this._refreshing)
            {
                return;
            }

            // TODO: convert to actions
            await ConsoleLog("OnKeyDown");
            if (e.CtrlKey && e.KeyCode == KeyCode.RightArrow)
            {
                int listIndex = this.ListIndex();
                if (listIndex < this.VisibleNodesList.Count - 1)
                {
                    int newPosition = this.VisibleNodesList[listIndex + 1].PositionInfo.StartOffset;
                    await this.Select(newPosition);
                }                
            }
            else if (e.CtrlKey && e.KeyCode == KeyCode.LeftArrow)
            {
                int listIndex = this.ListIndex();
                if (listIndex > 0)
                {
                    int newPosition = this.VisibleNodesList[listIndex - 1].PositionInfo.EndOffset;
                    await this.Select(newPosition);
                }
            }
            else if (e.CtrlKey && e.KeyCode == KeyCode.UpArrow)
            {
                /*INode parent = this.SelectedNode.Parent;
                int newPosition = this.NodeManager.GetFirsPositionOfNode(parent);
                this.Select(newPosition);
                e.Handled = true;*/
            }
            else if (e.KeyCode == KeyCode.F5)
            {
                await this.HandleUserInputResult(UserInputResult.HandledNeedsGlobalRefresh());
            }
            else
            {
                Node currNode = this.CurrentNode;
                UserInputResult res = currNode.OnKeyDownBubble(this.GetEditorState(), e, currNode);
                await this.HandleUserInputResult(res);
            }
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

        public Node GetParent(Node node, bool jumpHole)
        {
            Node parent = node?.Parent;
            if (parent == null)
            {
                return null;
            }

            if (jumpHole && parent.GetType().IsGenericType && parent.GetType().GetGenericTypeDefinition() == typeof(HoleNode<>))
            {
                parent = parent.Parent;
            }

            return parent;
        }

        public async Task OnPositionChanged(CursorPositionChangedEvent e)
        {
            if (this._refreshing || e.Source == "deleteLeft")
            {
                return;
            }

            if (e.Source != "api" && e.Source != "mouse")
            {
                await this.SetAndRevealPosition(this.CurrentPosition);
                return;
            }

            // TODO: consider skipping if position unchanged

            var window = await _jsRuntime.Window();
            await using (await window.Console.Time("OnPositionChanged" + _debugId++))
            {
                // await using (await window.Console.Time("GetOffsets"))
                {
                    this.CurrentPosition = e.Position;
                    TextModel m = await this._monacoEditor.GetModel();
                    this.CurrentOffset = await m.GetOffsetAt(e.Position);
                    Selection s = await this._monacoEditor.GetSelection();
                    this.CurrentSelectionStart = await m.GetOffsetAt(new Position() { Column = s.StartColumn, LineNumber = s.StartLineNumber });
                    this.CurrentSelectionEnd = await m.GetOffsetAt(new Position() { Column = s.EndColumn, LineNumber = s.EndLineNumber });
                }

                Node current = this.AtPosition(this.CurrentOffset).Item2;
                await ConsoleLog("CO" + this.CurrentOffset);
                if (current?.PositionInfo == null)
                {
                    return;
                }

                if (current.Unselectable)
                {
                    // TODO: next node?
                }

                if (this.SelectionStyleIds.Count > 0)
                {
                    await this._monacoEditor.DeltaDecorations(SelectionStyleIds.ToArray(), new ModelDeltaDecoration[0]);
                    this.SelectionStyleIds.Clear();
                }

                if (this.CurrentNode != current)
                {
                    EditorState es = this.GetEditorState();
                    UserInputResult? res1 = this.CurrentNode?.OnNodeIsSelectedBubble(es, false, this.CurrentNode);
                    UserInputResult res2 = current.OnNodeIsSelectedBubble(es, true, current);

                    if (res1.HasValue)
                    {
                        await this.HandleUserInputResult(res1.Value);
                    }

                    await this.HandleUserInputResult(res2);
                }

                this.CurrentNode = current;

                Node parent = GetParent(this.CurrentNode, true);
                if (parent != null && parent != this.Root)
                {
                    int min = this.CurrentNode.PositionInfo.StartOffset;
                    int max = this.CurrentNode.PositionInfo.EndOffset;
                    int listIndex = this.ListIndex();

                    for (int i = listIndex - 1; i >= 0; i--)
                    {
                        Node n = this.VisibleNodesList[i];
                        if (GetParent(n, true) != parent)
                        {
                            break;
                        }

                        min = n.PositionInfo.StartOffset;
                    }

                    for (int i = listIndex + 1; i < this.VisibleNodesList.Count; i++)
                    {
                        Node n = this.VisibleNodesList[i];
                        if (GetParent(n, true) != parent)
                        {
                            break;
                        }

                        max = n.PositionInfo.EndOffset;
                    }

                    this.SelectionStyleIds.AddRange(await this.SetStyleForRange(min, max, Styles.SelectedParentNodeText, null));
                }

                this.SelectionStyleIds.AddRange(await this.SetStyleForRange(this.CurrentNode.PositionInfo.StartOffset, this.CurrentNode.PositionInfo.EndOffset, Styles.SelectedNodeText, null));
            }
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

        public EditorState GetEditorState()
        {
            return new EditorState(this, this.CurrentOffset, this.CurrentSelectionStart, this.CurrentSelectionEnd);
        }

        public async Task<List<AstAutocompleteItem>> GetCompletions()
        {
            List<AstAutocompleteItem> completions = this.CurrentNode.GetCustomCompletions(this.GetEditorState());
            this.LastCompletionsById = completions.ToDictionary(x => x.Id);
            return completions;
        }
    }
}
