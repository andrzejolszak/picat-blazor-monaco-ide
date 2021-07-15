using SoftCircuits.Parsing.Helper;
using System;
using System.Collections.Generic;

namespace PicatBlazorMonaco.Ast
{
    public class DeclarationParser
    {
        public class Declaration
        {
            public string Comment { get; set; }
            public int NameOffset { get; set; }
            public string Name { get; set; }
            public List<string> Args { get; set; } = new List<string>();
            public string Operator { get; set; }
            public string Body { get; set; }
        }

        public static List<Declaration> Parse(string input)
        {
            List<Declaration> res = new List<Declaration>();
            ParsingHelper helper = new ParsingHelper(input);
            Declaration nextDeclaration = new Declaration();
            start:
            helper.SkipWhiteSpace();
            if (helper.EndOfText)
            {
                return res;
            }

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

            if (char.IsWhiteSpace(helper.Peek()) || helper.Peek() == '(')
            {
                if(current.Name == "module"
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
                helper++;
                lastPos = helper.Index;
                int nesting = 1;
                List<string> args = new List<string>();
                while (helper.Remaining > 0 && nesting > 0)
                {
                    char cc = helper.Get();
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
                            args.Add(arg);
                            lastPos = helper.Index;
                        }
                    }
                }

                current.Args = args;
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
    }
}
