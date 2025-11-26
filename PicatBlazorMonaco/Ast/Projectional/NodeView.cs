using BlazorMonaco;
using BlazorMonaco.Editor;

namespace Ast2
{
    public class NodeView
    {
        public NodeView(Node node, string text, ModelDecorationOptions style, ModelDecorationOptions backgroundStyle = null, ModelDecorationOptions overlayStyle = null)
        {
            this.Node = node;
            this.Text = text;
            this.Style = style;
            this.BackgroundStyle = backgroundStyle;
            this.OverlayStyle = overlayStyle;
        }

        public string Text { get; }

        public ModelDecorationOptions Style { get; }

        public ModelDecorationOptions BackgroundStyle { get; }

        public ModelDecorationOptions OverlayStyle { get; }
        
        public Node Node { get; }
    }
}
