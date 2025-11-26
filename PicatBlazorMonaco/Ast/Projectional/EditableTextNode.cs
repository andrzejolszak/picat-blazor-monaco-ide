using BlazorMonaco;
using BlazorMonaco.Editor;
using System;
using System.Linq;

namespace Ast2
{
    public class EditableTextNode : Node
    {
        private TextHolder _textHolder;

        public EditableTextNode(string text)
        {
            this._textHolder = new TextHolder();
            this.Text = text;
            this.Style = Styles.NormalTextGrayBackgroundStyle;
        }

        public EditableTextNode(TextHolder sharedTextHolder)
        {
            this._textHolder = sharedTextHolder;
            this.Style = Styles.NormalTextGrayBackgroundStyle;
        }

        public string Text
        {
            get
            {
                return this._textHolder.Text;
            } 
            private set
            {
                this._textHolder.Text = value;
            }
        }

        public ModelDecorationOptions Style { get; set; }

        public string EmptyText { get; set; } = "◦";

        public override void CreateView(EditorState editorState)
        {
            if (string.IsNullOrEmpty(this.Text))
            {
                this.View = new NodeView(this, this.EmptyText, Styles.Scratch);
            }
            else
            {
                this.View = new NodeView(this, this.Text + this.EmptyText, this.Style);
            }
        }

        protected override UserInputResult OnTextChanging(EditorState state, string insertingText, Node target)
        {
            (UserInputResult res, string newText) = HandleTextInsertion(state, insertingText, target, this.Text);
            this.Text = newText;
            return res;
        }

        public static (UserInputResult, string) HandleTextInsertion(EditorState state, string insertingText, Node node, string text)
        {
            int internalOffset = Math.Max(state.SelectionStart, state.SelectionEnd) - node.PositionInfo.StartOffset;
            int backspaceLength = insertingText.Count(x => x == '\b');
            if (insertingText[0] == '\b')
            {
                if (string.IsNullOrEmpty(text))
                {
                    return (UserInputResult.Empty, text);
                }

                int target = internalOffset - backspaceLength;
                int len = backspaceLength;
                if (text[target] == '\n')
                {
                    target--;
                    len++;
                }
                text = text.Remove(target, len);
                insertingText = insertingText.Replace("\b", "");
                if (insertingText.Length == 0)
                {
                    return (UserInputResult.HandledNeedsGlobalRefresh().WithNewLocalPosition(node, Math.Max(state.SelectionStart, state.SelectionEnd) - len - node.PositionInfo.StartOffset), text);
                }
            }

            if (insertingText.Length > 0)
            {
                text = text.Insert(internalOffset - backspaceLength, insertingText);
                return (UserInputResult.HandledNeedsGlobalRefresh().WithNewLocalPosition(node, Math.Min(state.SelectionStart, state.SelectionEnd) + insertingText.Length - node.PositionInfo.StartOffset), text);
            }
            else
            {
                throw new InvalidOperationException(insertingText);
            }
        }

        public class TextHolder
        {
            public string Text { get; set; }
        }
    }
}
