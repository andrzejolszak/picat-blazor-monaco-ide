using BlazorMonaco;
using BlazorMonaco.Editor;
using System;
using System.Collections.Generic;

namespace Ast2
{
    public class ToggleNode : Node
    {
        public ToggleNode()
        {
            this.Style = Styles.NormalTextStyle;
            this.VisualChildren = new List<Node>();
        }

        public bool IsChecked { get; set; }

        public event Action<ToggleNode> ValueChanged = x => { };

        public ModelDecorationOptions Style { get; set; }

        public override void CreateView(EditorState editorState)
        {
            this.VisualChildren.Clear();
            this.VisualChildren.Add(new ReadOnlyTextNode("[") { Parent = this });
            this.VisualChildren.Add(new ReadOnlyTextNode(this.IsChecked ? "*" : " ") { Parent = this });
            this.VisualChildren.Add(new ReadOnlyTextNode("]") { Parent = this });

            base.CreateView(editorState);
        }

        protected override UserInputResult OnMouseClick(EditorState state, EditorMouseEvent button, Node target)
        {
            this.IsChecked = !this.IsChecked;
            this.ValueChanged(this);
            return UserInputResult.HandledNeedsGlobalRefresh(this.VisualChildren[1]);
        }

        protected override UserInputResult OnTextChanging(EditorState state, string insertingText, Node target)
        {
            if (insertingText == " ")
            {
                this.IsChecked = !this.IsChecked;
                this.ValueChanged(this);
                return UserInputResult.HandledNeedsGlobalRefresh(this.VisualChildren[1]);
            }

            return base.OnTextChanging(state, insertingText, target);
        }
    }
}
