using BlazorMonaco;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ast2
{
    public class HoleNode<T> : Node
        where T: Node
    {
        public HoleNode(string emptyText = "◊", Func<Node, bool> acceptPredicate = null, T target = default)
        {
            this.EmptyText = emptyText;
            this.AcceptPredicateOrNull = acceptPredicate;
            this.VisualChildren = new List<Node>(1);
            
            if (target != default)
            {
                this.SetChild(target);
            }
        }

        public event Action<HoleNode<T>> ValueChanged = x => { };

        public string EmptyText { get; }

        public string ScratchText { get; private set; } = string.Empty;

        public Func<Node, bool> AcceptPredicateOrNull { get; }

        public override void CreateView(EditorState editorState)
        {
            if (this.VisualChildren.Count == 0)
            {
                if (string.IsNullOrEmpty(this.ScratchText))
                {
                    this.View = new NodeView(this, this.EmptyText, Styles.NormalTextRedBackgroundStyle);
                }
                else
                {
                    this.View = new NodeView(this, this.ScratchText + "◦", Styles.NormalTextRedBackgroundStyle);
                }

                return;
            }

            this.VisualChildren.Single().CreateView(editorState);
        }

        public void SetChild(T target)
        {
            if ((!this.AcceptPredicateOrNull?.Invoke(target)) ?? false)
            {
                throw new InvalidOperationException("tried to set an unsupported value");
            }

            Node childOrNull = this.VisualChildren.FirstOrDefault();
            if (childOrNull != null)
            {
                this.VisualChildren.Clear();
                childOrNull.Parent = null;
                childOrNull.View = null;
            }

            if (target != default)
            {
                this.AddChild(target);
                this.View = null;
                this.ScratchText = string.Empty;
            }

            if (childOrNull != target)
            {
                this.ValueChanged(this);
            }
        }

        public T GetChildOrDefault()
        {
            if (this.VisualChildren.Count == 0)
            {
                return default;
            }

            return this.VisualChildren.Single() as T;
        }

        public override List<AstAutocompleteItem> GetCustomCompletions(EditorState state)
        {
            List<AstAutocompleteItem> res = new List<AstAutocompleteItem>();
            string text = this.View?.Text ?? this.VisualChildren.FirstOrDefault()?.View.Text;
            foreach ((Type Type, Func<Node> Factory) item in state.FactoryRegistry)
            {
                if (typeof(T).IsAssignableFrom(item.Type))
                {
                    AstAutocompleteItem autocomplete = new AstAutocompleteItem(string.IsNullOrEmpty(this.ScratchText) ? text : item.Type.Name, item.Type.Name, "tooltip", "tooltipetext");
                    autocomplete.OnItemSelected += () => this.SetChild(item.Factory.Invoke() as T);
                    res.Add(autocomplete);
                }
            }

            // Handle reference completions here as a special-case
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(ReferenceNode<>))
            {
                Type refType = typeof(T).GetGenericArguments()[0];
                foreach (Node node in state.VisibleNodesList)
                {
                    if (refType.IsAssignableFrom(node.GetType()))
                    {
                        AstAutocompleteItem autocomplete = new AstAutocompleteItem(node.View.Text, "ref -> " + node.View.Text, "Reference", "tooltipetext");

                        // TODO: Activator is slow - cache factories on per constructedRefType basis
                        var type = typeof(ReferenceNode<>);
                        var constructedRefType = type.MakeGenericType(refType);
                        T newRef = (T)Activator.CreateInstance(constructedRefType, node);

                        autocomplete.OnItemSelected += () => this.SetChild(newRef);
                        res.Add(autocomplete);
                    }
                }
            }

            return res;
        }

        protected override UserInputResult OnTextChanging(EditorState state, string insertingText, Node target)
        {
            if (this.GetChildOrDefault() != null)
            {
                if (insertingText == "\b")
                {
                    this.SetChild(null);
                    return UserInputResult.HandledNeedsGlobalRefresh(changeFocusToNode: this);
                }

                return base.OnTextChanging(state, insertingText, target);
            }

            if (insertingText.Contains("\n") || insertingText.Contains(" "))
            {
                return UserInputResult.Empty;
            }

            // Scratch edit
            (UserInputResult res, string newText) = EditableTextNode.HandleTextInsertion(state, insertingText, this, this.ScratchText);
            this.ScratchText = newText;
            return res;
        }

        protected override UserInputResult OnNodeIsSelectedChanged(EditorState state, bool hasFocus, Node target)
        {
            if (!hasFocus && !string.IsNullOrEmpty(this.ScratchText))
            {
                this.ScratchText = string.Empty;
                return UserInputResult.HandledNeedsGlobalRefresh();
            }

            return base.OnNodeIsSelectedChanged(state, hasFocus, target);
        }

        protected override UserInputResult OnKeyDown(EditorState state, KeyboardEvent keys, Node target)
        {
            if (keys.KeyCode == KeyCode.Escape && !string.IsNullOrEmpty(this.ScratchText))
            {
                this.ScratchText = string.Empty;
                return UserInputResult.HandledNeedsGlobalRefresh(this);
            }

            return base.OnKeyDown(state, keys, target);
        }
    }
}
