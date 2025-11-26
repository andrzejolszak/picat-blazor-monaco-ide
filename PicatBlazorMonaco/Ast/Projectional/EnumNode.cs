using System;
using System.Collections.Generic;
using System.Linq;

namespace Ast2
{
    public class EnumNode : Node
    {
        public EnumNode(HashSet<string> items, string selection)
        {
            if (!items.Contains(selection))
            {
                throw new InvalidOperationException("Did not find among items: " + selection);
            }

            this.Items = items;
            this.Selection = selection;
        }

        public HashSet<string> Items { get; }
        
        public string Selection { get; set; }

        public override void CreateView(EditorState editorState)
        {
            this.View = new NodeView(this, this.Selection, Styles.NormalTextBlueBackgroundStyle);
        }

        public override List<AstAutocompleteItem> GetCustomCompletions(EditorState state)
        {
            List<AstAutocompleteItem> res = new List<AstAutocompleteItem>();
            foreach (string item in this.Items.OrderBy(x => x))
            {
                AstAutocompleteItem autocomplete = new AstAutocompleteItem(this.Selection, item, "tooltip", "tooltipetext");
                autocomplete.OnItemSelected += () => this.Selection = item;
                res.Add(autocomplete);
            }

            return res;
        }
    }
}