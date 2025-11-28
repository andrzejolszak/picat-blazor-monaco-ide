using FluentAssertions;
using PicatBlazorMonaco.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static PicatBlazorMonaco.Ast.DeclarationParser;

namespace Tests
{
    public class DeclarationParserTest
    {
        private readonly ITestOutputHelper _output;

        public DeclarationParserTest(ITestOutputHelper output)
        {
            this._output = output;
        }

        [InlineData("foo ( a, d, d ) => sd.gfg(), X . ")]
        [InlineData(" foo . ")]
        [InlineData("foo () => a . ")]
        [InlineData("foo(a)?=>a.")]
        [InlineData("foo(X),X>1?=>a.")]
        [InlineData("foo=>a.")]
        [InlineData("foo=>not a.")]
        [InlineData("foo=>\\+a.")]
        [InlineData("foo=>\\+ a.")]
        [InlineData("foo=>1 =\\= 2.")]
        [InlineData("foo=>1=\\=2.")]
        [InlineData("foo(a)-->a.")]
        [InlineData("foo=1.")]
        [InlineData("foo(N) = F => true.")]
        [InlineData("import cp, ds.\r\nfoo=1.")]
        [InlineData("module bar.\r\nfoo=1.")]
        [InlineData("private\r\nfoo([],Acc,Sum) => Sum = Acc.")]
        [InlineData("index (+, -) (-,+)\nfoo(a, b).")]
        [InlineData("foo ((a,b), c).")]
        [InlineData("foo(U-V,X,D) => true.")]
        [InlineData("table(+,+,-,min)\r\nfoo(x, y,z) => true.")]
        [InlineData("foo(_,_,D) => D=0.")]
        [InlineData("% comment\r\nfoo=1.")]
        [InlineData("/* comment\r\n*/\r\nfoo=1.")]
        [InlineData(@"% phrase/2
foo(P, L) :-
    Goal =.. [P, L,[]],
    call(Goal).")]
        [Theory]
        public void Simple(string input)
        {
            List<Declaration> res = DeclarationParser.ParseDeclarations(input);
            res.Count.Should().Be(1);
            res.Single().Name.Should().Be("foo");

            List<Reference> references = DeclarationParser.ParseReferences(input, res);
            references.Count.Should().Be(0);
        }

        [InlineData("foo(a)?=>a. bar-->c.")]
        [InlineData("foo(a)?=>a.\r\nbar-->c.")]
        [InlineData("foo(a)?=>a.\nbar-->c.")]
        [InlineData("foo(a)?=>a.\r\n\r\n\r\nbar-->c.")]
        [InlineData("foo(a)?=>a.\n\n\nbar-->c.")]
        [Theory]
        public void SimpleMultiple(string input)
        {
            List<Declaration> res = DeclarationParser.ParseDeclarations(input);
            res.Count.Should().Be(2);
            res.Single(x => x.Name == "foo");
            res.Single(x => x.Name == "bar");
        }

        [Fact]
        public void Args()
        {
            List<Declaration> res = DeclarationParser.ParseDeclarations("foo(a)?=>a. bar-->c. yar. far()=>true. dar(a, b). tar({n,m,y}, 2, 3).");
            res.Count(x => x.Args.Count == 0).Should().Be(3);
            res.Count(x => x.Args.Count == 1).Should().Be(1);
            res.Count(x => x.Args.Count == 2).Should().Be(1);
            res.Count(x => x.Args.Count == 3).Should().Be(1);
        }

        [Fact]
        public void LargeExample()
        {
            List<Declaration> res = DeclarationParser.ParseDeclarations(Examples.SeveralExamples);
            res.Count.Should().Be(150);

            List<Reference> references = DeclarationParser.ParseReferences(Examples.SeveralExamples, res);
            references.Count.Should().Be(120);
        }

        [InlineData("...")]
        [InlineData(". .. .")]
        [InlineData("abc")]
        [InlineData("aa => c")]
        [InlineData("aa.b")]
        [InlineData("aa. b")]
        [InlineData("aa.=>(")]
        [InlineData("aa(")]
        [InlineData("aa)")]
        [InlineData("abc())")]
        [InlineData("abc(()")]
        [InlineData("--")]
        [InlineData("abc(a,")]
        [InlineData("abc(a,)")]
        [InlineData("abc(a,b) =")]
        [InlineData("abc(a,n) => foo")]
        [InlineData("abc(a,n) => foo(")]
        [InlineData("abc(a,n) => foo(.")]
        [InlineData("abc(a,n) => foo(1, 2")]
        [InlineData("abc(,n) => foo(,f")]

        [Theory]
        public void SyntaxErrorHandling(string input)
        {
            string program = $@"% phrase/2
foo(P, L) :-
    call(Goal).
{input}
bar(a, X) => true.
zar ?=> foo(a, 1).
{input}";
            List<Declaration> res = DeclarationParser.ParseDeclarations(program);
            res.Single(x => x.Name == "foo");
            res.Single(x => x.Name == "zar");

            List<Reference> references = DeclarationParser.ParseReferences(program, res);
            references.Count.Should().Be(1);
        }
    }
}