
using Ast2;
using System;
using System.Collections.Generic;

namespace ProjectionalBlazorMonaco
{
    public static class Tutorial
    {
        public static Action<Node, Ast2ProjEditor>[] Builders = new Action<Node, Ast2ProjEditor>[]
        {
            (n, e) => n.AddChild(new ReadOnlyTextNode("1. Read-only nodes:") { Style = Styles.UnderlineStyle })
                .AddChild(ReadOnlyTextNode.NewLine())
                .AddChild(new ReadOnlyTextNode("This is a ReadOnlyTextNode, this text cannot be edited."))
                .AddChild(ReadOnlyTextNode.NewLine()),
            (n, e) => n.AddChild(new ReadOnlyTextNode("2. Editable nodes:") { Style = Styles.UnderlineStyle })
                .AddChild(ReadOnlyTextNode.NewLine())
                .AddChild(new EditableTextNode("This is an EditableTextNode, this text can be edited.")),
            (n, e) =>
            {
                EditableTextNode.TextHolder textHolder = new EditableTextNode.TextHolder() { Text = "Node" };
                n.AddChild(new ReadOnlyTextNode("3. Synchronized editable nodes:") { Style = Styles.UnderlineStyle })
                .AddChild(ReadOnlyTextNode.NewLine())
                .AddChild(new EditableTextNode(textHolder))
                .AddChild(new ReadOnlyTextNode(" is synchronized through a TextHolder with "))
                .AddChild(new EditableTextNode(textHolder))
                .AddChild(new ReadOnlyTextNode(" . Editing either will affect the other."));
            },
            (n, e) => {
                e.FactoryRegistry.Add((typeof(ReadOnlyTextNode), () => new Banana()));
                e.FactoryRegistry.Add((typeof(ReadOnlyTextNode), () => new Strawberry()));
                e.FactoryRegistry.Add((typeof(EditableTextNode), () => new EditableTextNode("Bus")));
                e.FactoryRegistry.Add((typeof(EditableTextNode), () => new EditableTextNode("Busstop")));

                n.AddChild(new ReadOnlyTextNode("4. Hole nodes:") { Style = Styles.UnderlineStyle })
                .AddChild(ReadOnlyTextNode.NewLine())
                .AddChild(new HoleNode<ReadOnlyTextNode>())
                .AddChild(new ReadOnlyTextNode(" is a HoleNode that can be filled with registered ReadOnlyTextNode completion objects using ctrl+space"))
                .AddChild(ReadOnlyTextNode.NewLine())
                .AddChild(new HoleNode<EditableTextNode>())
                .AddChild(new ReadOnlyTextNode(" <- this one can be filled with registered EditableTextNode completion objects using ctrl+space"));
            },

            (n, e) => {
                n.AddChild(new ReadOnlyTextNode("5. Reference nodes:") { Style = Styles.UnderlineStyle })
                .AddChild(ReadOnlyTextNode.NewLine())
                .AddChild(new HoleNode<ReferenceNode<EditableTextNode>>())
                .AddChild(new ReadOnlyTextNode(" is a Hole for ReferenceNode that can refer to "))
                .AddChild(new EditableTextNode("an existing EditableTextNode."));
            },

            (n, e) => {
                n.AddChild(new ReadOnlyTextNode("6. Enum nodes:") { Style = Styles.UnderlineStyle })
                .AddChild(ReadOnlyTextNode.NewLine())
                .AddChild(new EnumNode(new HashSet<string>{ "foo", "bar", "car" }, "foo"))
                .AddChild(new ReadOnlyTextNode(" is an EnumNode that only allows values from a predefined set"));
            },
            (n, e) => {
                n.AddChild(new ReadOnlyTextNode("7. Toggle nodes:") { Style = Styles.UnderlineStyle })
                .AddChild(ReadOnlyTextNode.NewLine())
                .AddChild(new ToggleNode())
                .AddChild(new ReadOnlyTextNode(" is a ToggleNode that can be edited with mouse or spacebar"));
            },
            (n, e) => {
                n.AddChild(new ReadOnlyTextNode("8. List nodes:") { Style = Styles.UnderlineStyle })
                .AddChild(ReadOnlyTextNode.NewLine())
                .AddChild(new ListNode<ReadOnlyTextNode>("[", "]", ", ", new List<ReadOnlyTextNode>{ new ReadOnlyTextNode("cat"), new ReadOnlyTextNode("dog") }))
                .AddChild(new ReadOnlyTextNode(" is a ListNode that can add and remove elements (use Enter, Comma, Backspace)"));
            }
        };
    }

    public class Banana : ReadOnlyTextNode
    {
        public Banana() : base("Banana")
        {
        }
    }

    public class Strawberry : ReadOnlyTextNode
    {
        public Strawberry() : base("Strawberry")
        {
        }
    }
}
