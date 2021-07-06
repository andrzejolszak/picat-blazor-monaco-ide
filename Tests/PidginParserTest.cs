using JJC.Psharp.Lang;
using SoftCircuits.Parsing.Helper;
using Xunit;
using Xunit.Abstractions;

namespace CSPrologTest
{
    public class PidginParserTest
    {
        private readonly ITestOutputHelper _output;

        public PidginParserTest(ITestOutputHelper output)
        {
            VariableTerm.EnableSimplifiedSource();
            this._output = output;
        }

        [InlineData(@"foo ( a, d, d ) => sd.gfg(), X . ")]
        [InlineData(@" foo . ")]
        [InlineData(@"foo () => a . ")]
        [InlineData(@"foo(a)?=>a.")]
        [InlineData(@"foo(X),X>1?=>a.")]
        [InlineData(@"foo=>a.")]
        [InlineData(@"foo(a)-->a.")]
        [InlineData(@"foo(a)?=>a. b-->c.")]
        [InlineData(@"foo=1.")]
        [InlineData(@"fibg(N) = F => true.")]
        [InlineData(@"foo=1.")]
        [InlineData(@"import cp, ds.")]
        [InlineData(@"module foo.")]
        [InlineData(@"private sum_list_aux([],Acc,Sum) => Sum = Acc.")]
        [InlineData(@"index (+, -) (-,+) banan(a, b).")]
        [InlineData(@"foo ((a,b), c).")]
        [InlineData(@"d(U-V,X,D) => true.")]
        [InlineData(@"table(+,+,-,min) foo(x, y,z) => true.")]
        [InlineData(@"d(_,_,D) => D=0.")]
        [Theory]
        public void Simple(string input)
        {
            ParsingHelper helper = new ParsingHelper(input);
            start:
            helper.SkipWhiteSpace();
            if (helper.EndOfText)
            {
                return;
            }

            if (helper.Peek() == '%')
            {
                string comment = helper.ParseToNextLine();
                goto start;
            }
            else if (helper.Peek() == '/' && helper.Peek(1) == '*')
            {
                string comment = helper.ParseTo("*/", true);
                goto start;
            }

            int lastPos = helper.Index;
            string name = helper.ParseTo('.', '(', '?', '-', '=', ' ', '\r', '\n', '\t').Trim();

            if ( char.IsWhiteSpace(helper.Peek())
                &&
                (name == "module"
                || name == "import"
                || name == "private"
                || name == "index"
                || name == "table"))
            {
                goto start;
            }

            helper.SkipWhiteSpace();

            if (helper.Peek() == '(')
            {
                helper++;
                lastPos = helper.Index;
                int nesting = 1;
                while (helper.Remaining > 0 && nesting > 0)
                {
                    char cc = helper.Get();
                    if (cc == ')')
                    {
                        nesting--;
                    }
                    else if (cc == '(')
                    {
                        nesting++;
                    }
                }

                string args = helper.Index - lastPos > 1 ? helper.Extract(lastPos, helper.Index - 1).Trim() : null;
            }

            helper.SkipWhiteSpace();

            string op = helper.Parse('-', '=', '?', '>').Trim();

            string body = helper.ParseWhile(x => !(x == '.' && char.IsWhiteSpace(helper.Peek()))).Trim();

            goto start;
        }
    }
}