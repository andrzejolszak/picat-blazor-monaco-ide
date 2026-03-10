using SoftCircuits.Parsing.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PicatBlazorMonaco.Ast
{
    public class DeclarationParser
    {
        public class Declaration
        {
            public string Comment { get; set; }
            public int NameOffset { get; set; }
            public string Name { get; set; }
            public List<Argument> Args { get; set; } = new List<Argument>(0);
            public string Operator { get; set; }
            public string Body { get; set; }
            public bool IsFunction => this.Operator == "=";

            public bool IsFact => this.Operator == string.Empty && this.Body == string.Empty;
        }

        public class Argument
        {
            public int Offset { get; set; }

            public string Text { get; set; }

            public bool IsVar => char.IsUpper(this.Text[0]) || this.Text[0] == '_';

            public bool IsAtom => char.IsLower(this.Text[0]) || this.Text[0] == '\'';

            public bool IsAssignment => this.Text.Contains("=");

            public bool IsCall => this.Text[0] != '(' && this.Text.Contains("(");
        }

        public class Reference
        {
            public Declaration FirstMatch { get; set; }
            public int NameOffset { get; set; }
            public List<Argument> Args { get; set; } = new List<Argument>(0);
        }

        public static List<Declaration> ParseDeclarations(string input)
        {
            List<Declaration> res = new List<Declaration>();
            ParsingHelper helper = new ParsingHelper(input);
            Declaration nextDeclaration = new Declaration();
            int prevIndex = -1;
            start:
            helper.SkipWhiteSpace();
            if (helper.EndOfText)
            {
                return res;
            }

            if (helper == prevIndex)
            {
                throw new InvalidOperationException("Progress staled at " + helper.Text.Insert(prevIndex, "^"));
            }

            prevIndex = helper.Index;

            if (helper.Peek() == '%')
            {
                nextDeclaration.Comment += helper.ParseToNextLine().Substring(1).TrimStart();
                goto start;
            }
            else if (helper.Peek() == '/' && helper.Peek(1) == '*')
            {
                nextDeclaration.Comment = helper.ParseTo("*/", true);
                goto start;
            }

            int lastPos = helper.Index;
            Declaration current = nextDeclaration;
            nextDeclaration = new Declaration();
            current.Name = helper.ParseTo(':', '.', '(', '?', '-', '=', ' ', '\r', '\n', '\t').Trim();
            current.NameOffset = lastPos;
            if (current.Name == "")
            {
                helper++;
                goto start;
            }

            if (char.IsWhiteSpace(helper.Peek()) || helper.Peek() == '(')
            {
                if (current.Name == "module"
                || current.Name == "import")
                {
                    helper.SkipToNextLine();
                    goto start;
                }

                if (current.Name == "private"
                    || current.Name == "index"
                    || current.Name == "table")
                {
                    helper.SkipToNextLine();
                    goto start;
                }
            }

            res.Add(current);

            helper.SkipWhiteSpace();

            if (helper.Peek() == '(')
            {
                current.Args = ExtractArguments(helper);
            }

            helper.SkipWhiteSpace();

            current.Operator = helper.Parse(':', '-', '=', '?', '>').Trim();

            current.Body = helper.ParseWhile(x =>
            {
                return !(x == '.' && helper.Peek(-1) != '.' && (char.IsWhiteSpace(helper.Peek(1)) || helper.Remaining == 1));
            }).Trim();

            helper++;

            goto start;
        }

        private static List<Argument> ExtractArguments(ParsingHelper helper)
        {
            int lastPos;
            helper++;
            lastPos = helper.Index;
            int nesting = 1;
            List<Argument> args = new List<Argument>();
            int prevIndex = -1;
            while (helper.Remaining > 0 && nesting > 0)
            {
                if (helper == prevIndex)
                {
                    throw new InvalidOperationException("Progress staled at " + helper.Text.Insert(prevIndex, "^"));
                }

                prevIndex = helper.Index;

                char cc = helper.Get();
                if (cc == '.' && char.IsWhiteSpace(helper.Peek()))
                {
                    helper--;
                    return args;
                }

                if (cc == ')' || cc == ']' || cc == '}')
                {
                    nesting--;
                }
                else if (cc == '(' || cc == '[' || cc == '{')
                {
                    nesting++;
                }

                if (nesting == 0 || (nesting == 1 && cc == ','))
                {
                    string arg = helper.Extract(lastPos, helper.Index - 1).Trim();
                    if (arg != string.Empty)
                    {
                        args.Add(new Argument { Offset = lastPos, Text = arg });
                        lastPos = helper.Index;
                    }
                }
            }

            return args;
        }

        public static List<Reference> ParseReferences(string input, List<Declaration> declarations)
        {
            List<Reference> res = new List<Reference>();
            IEnumerable<IGrouping<string, Declaration>> byName = declarations.GroupBy(x => x.Name);
            HashSet<int> declarationOffsets = declarations.Select(x => x.NameOffset).ToHashSet();

            // TODO: inefficient
            ParsingHelper helper = new ParsingHelper(input);
            foreach (IGrouping<string, Declaration> name in byName)
            {
                helper.Index = 0;
                int prevIndex = -1;
                while (!helper.EndOfText)
                {
                    if (helper == prevIndex)
                    {
                        throw new InvalidOperationException("Progress staled at " + helper.Text.Insert(prevIndex, "^"));
                    }

                    prevIndex = helper.Index;

                    if (helper.SkipTo(name.Key))
                    {
                        int offset = helper.Index;
                        if (char.IsLetterOrDigit(helper.Peek(-1)))
                        {
                            helper.ParseCharacters(name.Key.Length);
                            continue;
                        }

                        helper.ParseCharacters(name.Key.Length);

                        if (char.IsLetterOrDigit(helper.Peek()))
                        {
                            continue;
                        }

                        if (declarationOffsets.Contains(offset))
                        {
                            continue;
                        }

                        helper.SkipWhiteSpace();

                        List<Argument> args = new List<Argument>(0);
                        if (helper.Peek() == '(')
                        {
                            args = ExtractArguments(helper);
                        }

                        Declaration target = name.FirstOrDefault(x => x.Args.Count == args.Count);
                        if (target != null)
                        {
                            res.Add(new Reference() { FirstMatch = target, NameOffset = offset, Args = args });
                        }
                    }
                }
            }

            return res;
        }

        public static Declaration ParseBuiltinDeclaration(string builtin)
        {
            ParsingHelper helper = new ParsingHelper(builtin);

            string name = helper.ParseTo(' ', '(', '=', ':');

            helper.SkipWhiteSpace();

            List<Argument> args = new List<Argument>();
            if (helper.Peek() == '(')
            {
                args = ExtractArguments(helper);
            }

            return new Declaration() { NameOffset = -1, Name = name, Args = args };
        }
    }
}
