using BlazorMonaco;
using BlazorMonaco.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ast2
{
    public struct UserInputResult
    {
        public static readonly Cursor DefaultCursor = Cursors.IBeam;

        public static readonly UserInputResult Empty = new UserInputResult();

        public static readonly UserInputResult Handled = new UserInputResult() { EventHandled = true };

        public static UserInputResult HandledNeedsGlobalRefresh(Node changeFocusToNode = null, Cursor cursor = null) => new UserInputResult() { EventHandled = true, NeedsGlobalEditorRefresh = true, ChangeFocusToNode = changeFocusToNode, MouseCursor = cursor };

        public bool EventHandled { get; set; }
        
        public bool NeedsGlobalEditorRefresh { get; set; }

        public Node ChangeFocusToNode { get; set; }

        public int? NewLocalPosition { get; private set; }

        public Node? NewLocalPositionContext { get; private set; }

        public Cursor MouseCursor { get; set; }

        public UserInputResult WithNewLocalPosition(Node context, int newPos)
        {
            this.NewLocalPosition = newPos;
            this.NewLocalPositionContext = context;
            return this;
        }
    }

    public class Node
    {
        public event Action ViewChangedEvent;

        public Node Parent { get; set; }

        /// <summary>
        /// This is populated if the current Node's View is null, i.e. the node delegates it's view to the Children.
        /// </summary>
        public List<Node>? VisualChildren { get; protected set; }

        /// <summary>
        /// Every added node will have a PositionInfo - even if it only has VisualChildren and no View.
        /// </summary>
        public PositionInfo PositionInfo { get; set; }

        private NodeView? _view;

        /// <summary>
        /// This will only be set if the node had no VisualChildren.
        /// </summary>
        public NodeView? View
        {
            get
            {
                return this._view;
            }

            set
            {
                if (this._view != value)
                {
                    this._view = value;
                    this.ViewChangedEvent?.Invoke();
                }
            } 
        }

        public bool Unselectable { get; set; }

        public List<Node> GetAllAncestors()
        {
            List<Node> res = new List<Node>();
            Node currParent = this.Parent;
            while (currParent != null)
            {
                res.Add(currParent);
                currParent = currParent.Parent;
            }

            return res;
        }

        public bool HasAncestor(Node parent)
        {
            Node currParent = this.Parent;
            while (currParent != null)
            {
                if (currParent == parent)
                {
                    return true;
                }

                currParent = currParent.Parent;
            }

            return false;
        }

        // TODO: (maybe) this and Children will be removed from here when we get proper holes, lists, etc.
        public Node AddChild(Node node)
        {
            if (this.VisualChildren == null)
            {
                this.VisualChildren = new List<Node>();
            }

            this.VisualChildren.Add(node);
            node.Parent = this;
            return this;
        }

        public virtual void CreateView(EditorState editorState)
        {
            foreach (Node child in this.VisualChildren)
            {
                child.CreateView(editorState);
            }
        }

        public UserInputResult OnTextChangingBubble(EditorState state, string insertingText, Node target)
        {
            UserInputResult thisResult = this.OnTextChanging(state, insertingText, target);
            if (thisResult.EventHandled)
            {
                return thisResult;
            }

            if (this.Parent == null)
            {
                return UserInputResult.Handled;
            }

            return this.Parent.OnTextChangingBubble(state, insertingText, target);
        }

        protected virtual UserInputResult OnTextChanging(EditorState state, string insertingText, Node target)
        {
            return UserInputResult.Empty;
        }

        public virtual List<AstAutocompleteItem> GetCustomCompletions(EditorState state)
        {
            if (this.Parent != null && this.Parent.GetType().IsGenericType && this.Parent.GetType().GetGenericTypeDefinition() == typeof(HoleNode<>))
            {
                return this.Parent.GetCustomCompletions(state);
            }

            return new List<AstAutocompleteItem>(0);
        }

        public UserInputResult OnNodeIsSelectedBubble(EditorState state, bool hasFocus, Node target)
        {
            UserInputResult thisResult = this.OnNodeIsSelectedChanged(state, hasFocus, target);
            if (thisResult.EventHandled)
            {
                return thisResult;
            }

            if (this.Parent == null)
            {
                return UserInputResult.Handled;
            }

            return this.Parent.OnNodeIsSelectedBubble(state, hasFocus, target);
        }

        protected virtual UserInputResult OnNodeIsSelectedChanged(EditorState state, bool hasFocus, Node target)
        {
            return UserInputResult.Empty;
        }

        public UserInputResult OnKeyDownBubble(EditorState state, KeyboardEvent keys, Node target)
        {
            UserInputResult thisResult = this.OnKeyDown(state, keys, target);
            if (thisResult.EventHandled)
            {
                return thisResult;
            }

            if (this.Parent == null)
            {
                return UserInputResult.Handled;
            }

            return this.Parent.OnKeyDownBubble(state, keys, target);
        }

        protected virtual UserInputResult OnKeyDown(EditorState state, KeyboardEvent keys, Node target)
        {
            return UserInputResult.Empty;
        }

        public UserInputResult OnKeyUpBubble(EditorState state, KeyboardEvent keys, Node target)
        {
            UserInputResult thisResult = this.OnKeyUp(state, keys, target);
            if (thisResult.EventHandled)
            {
                return thisResult;
            }

            if (this.Parent == null)
            {
                return UserInputResult.Handled;
            }

            return this.Parent.OnKeyUpBubble(state, keys, target);
        }

        protected virtual UserInputResult OnKeyUp(EditorState state, KeyboardEvent keys, Node target)
        {
            return UserInputResult.Empty;
        }

        public UserInputResult OnMouseClickBubble(EditorState state, EditorMouseEvent button, Node target)
        {
            UserInputResult thisResult = this.OnMouseClick(state, button, target);
            if (thisResult.EventHandled)
            {
                return thisResult;
            }

            if (this.Parent == null)
            {
                return UserInputResult.Handled;
            }

            return this.Parent.OnMouseClickBubble(state, button, target);
        }

        protected virtual UserInputResult OnMouseClick(EditorState state, EditorMouseEvent button, Node target)
        {
            return UserInputResult.Empty;
        }
    }

    public class AstAutocompleteItem
    {
        public AstAutocompleteItem(string sourceToMatch, string menuText, string docTitle, string docText, string kind = "1")
        {
            this.Id = Guid.NewGuid().ToString();
            this.SourceToMatch = sourceToMatch;
            this.MenuText = menuText;
            this.DocTitle = docTitle;
            this.DocText = docText;
            this.Kind = kind;
        }

        public string Id { get; }
        public string SourceToMatch { get; }
        public string MenuText { get; }
        public string DocTitle { get; }
        public string DocText { get; }
        public string Kind { get; }

        public event Action OnItemSelected = () => { };

        public void TriggerItemSelected() => this.OnItemSelected();
    }
}
