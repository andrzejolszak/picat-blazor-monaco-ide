namespace Pidgin.Examples.Expression
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using JJC.Psharp.Lang;
    using Pidgin.Expression;
    using static Pidgin.Parser;

    /// <summary>
    /// Defines the <see cref="PidginPicatParser" />
    /// </summary>
    public static class PidginPicatParser
    {
        /// <summary>
        /// Defines the Println
        /// </summary>
        public static ThreadLocal<Action<string>> Println = new ThreadLocal<Action<string>> { Value = x => Console.WriteLine(x) };

        /// <summary>
        /// Defines the CommaSymbol
        /// </summary>
        public static readonly SymbolTerm CommaSymbol = SymbolTerm.MakeSymbol(",", 2);

        public static readonly SymbolTerm SemicolonSymbol = SymbolTerm.MakeSymbol(";", 2);

        /// <summary>
        /// Defines the ArrowSymbol
        /// </summary>
        public static readonly SymbolTerm ArrowSymbol = SymbolTerm.MakeSymbol(":-", 2);

        /// <summary>
        /// Defines the TrueSymbol
        /// </summary>
        public static readonly SymbolTerm TrueSymbol = SymbolTerm.MakeSymbol("true");

        public static readonly SymbolTerm CutSymbol = SymbolTerm.MakeSymbol("!");

        public static readonly SymbolTerm EmptyListSymbol = SymbolTerm.MakeSymbol("[]", 0);

        /// <summary>
        /// Defines the SingleQuote
        /// </summary>
        private static readonly Parser<char, char> SingleQuote = Try(Char('\''));

        private static readonly Parser<char, char> DoubleQuote = Try(Char('\"'));

        /// <summary>
        /// Defines the Comma
        /// </summary>
        private static readonly Parser<char, char> Comma = Try(Char(','));

        /// <summary>
        /// Defines the Dot
        /// </summary>
        private static readonly Parser<char, char> Dot = Try(Char('.'));

        /// <summary>
        /// Defines the Atom
        /// </summary>
        private static readonly Parser<char, string> Atom
             = Parser<char>.Token(c => char.IsLower(c) || char.IsSymbol(c) || c == '-' || c == ':' || c == '=' || c == '*' || c == '\\')
                 .Then(Parser.AnyCharExcept(' ', '\r', '\n', '\t', '.', ',', ';', '\'', '(', ')', '[', ']', '\"', '/', '\\').ManyString(), (x, y) => x + y)
                 .Assert(x => x.Length > 0 && (x.All(IsCharBased) || !x.Any(IsCharBased)))
                 .Labelled(nameof(Atom));

        private static readonly Parser<char, Term> StringTerm =
            TryTok(
                nameof(StringTerm),
                OneOf
                (
                    Try(String("\"\"").Select(x => "")),
                    Try(AnyCharExcept('\"').AtLeastOnceString().Between(DoubleQuote, DoubleQuote))
                )
                .Select<Term>(x =>
                {
                    if (x == "")
                    {
                        return EmptyListSymbol;
                    }

                    Term reduced = x
                        .Select(y => (Term)new IntegerTerm(y))
                        .Append((Term)EmptyListSymbol)
                        .Reverse()
                        .Aggregate((last, nextLast) => (Term)new ListTerm(nextLast, last));

                    return reduced;
                })
            );

        private static readonly Parser<char, double> DoubleNum =
            DecimalNum.Before(Dot).Then(Digit.AtLeastOnceString(), (x, y) => double.Parse(x + "." + y, System.Globalization.NumberStyles.Any)).Between(SkipWhitespaces);

        /// <summary>
        /// Defines the Symbol
        /// </summary>
        private static readonly Parser<char, Term> Symbol
            = TryTok(
                nameof(Symbol),
                OneOf(
                    Char('!').Select<Term>(x => CutSymbol),
                    Lowercase
                        .Then(Try(LetterOrDigit).Or(Char('_')).ManyString(), (x, y) => x + y)
                        //.Then(Char('/').Then(DecimalNum).Assert(x => x >= 0).Optional(), (x, arity) => (x, arity))
                        .Select<Term>(x => SymbolTerm.MakeSymbol(x, 0)),
                    Parser.AnyCharExcept('\'').ManyString().Between(SingleQuote, SingleQuote).Select<Term>(x => SymbolTerm.MakeSymbol(x))
                )
            .Labelled(nameof(Symbol)));

        /// <summary>
        /// The BinaryOp
        /// </summary>
        /// <param name="op">The op<see cref="string"/></param>
        /// <returns>The <see cref="Parser{char, Func{Term, Term, Term}}"/></returns>
        private static Parser<char, Func<Term, Term, Term>> BinaryOp(string op)
        {
            bool isCharBased = IsCharBased(op[op.Length - 1]);
            return Binary(
                TryTok("BinOp_" + op, 
                    String(op).Then(Lookahead(Parser<char>.Any), (x, nextChar) => (x, nextChar))
                    .Assert(x => char.IsWhiteSpace(x.nextChar) || IsCharBased(x.nextChar) != isCharBased).Between(SkipWhitespaces)
                    .ThenReturn(SymbolTerm.MakeSymbol(op, 2))
                )
            );

        }
        private static bool IsCharBased(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '!' || c == '[' || c == ']' || c == '(' || c == ')';

        /// <summary>
        /// The UnaryOp
        /// </summary>
        /// <param name="op">The op<see cref="string"/></param>
        /// <returns>The <see cref="Parser{char, Func{Term, Term}}"/></returns>
        private static Parser<char, Func<Term, Term>> UnaryOp(string op)
        {
            return Unary(
                TryTok("UnOp_" + op, String(op)
                .Then(Lookahead(Parser<char>.Any), (x, nextChar) => (x, nextChar))
                .Assert(x => x.nextChar != '(').Between(SkipWhitespaces)
                .ThenReturn(SymbolTerm.MakeSymbol(op, 1))));
        }

        /// <summary>
        /// Defines the CustomBinOp
        /// </summary>
        private static readonly Parser<char, Func<Term, Term, Term>> CustomBinOp
            = Binary(TryTok(nameof(CustomBinOp), Atom.Where(x => x != ":-" && x != "|").Select(x => SymbolTerm.MakeSymbol(string.Join(string.Empty, x), 2))));

        /// <summary>
        /// Defines the Variable
        /// </summary>
        private static readonly Parser<char, Term> Variable
            = TryTok(
                nameof(Variable),
                Try(Uppercase).Or(Char('_'))
                .Then(Try(LetterOrDigit).Or(Char('_')).ManyString(), (x, y) => x + y)
                .Select<Term>(x => GetVariable(x))
                .Labelled(nameof(Variable)));

        /// <summary>
        /// Defines the PrologTermParser
        /// </summary>
        private static readonly Parser<char, Term> PrologTermParser =
            TryTok(
                nameof(PrologTermParser),
            OneOf(
                TryTokRec(nameof(ListStruct), () => ListStruct),
                TryTokRec(nameof(CurlyStruct), () => CurlyStruct),
                TryTokRec(nameof(StructureCall), () => StructureCall),
                TryTokRec(nameof(StructureOpCall), () => StructureOpCall),
                StringTerm,
                Symbol,
                Variable,
                TryTok(nameof(DoubleNum), DoubleNum.Select<Term>(x => new DoubleTerm(x))),
                TryTok(nameof(DecimalNum), DecimalNum.Select<Term>(x => new IntegerTerm(x)))
            ));

        /// <summary>
        /// Defines the LineComment
        /// </summary>
        private static Parser<char, Term> LineComment = TryTok(
                nameof(LineComment),
                 Char('%')
                .Then(Parser<char>.Any.Until(Try(Parser<char>.End).Or(Try(EndOfLine).IgnoreResult())))
                .Select(x => string.Join(string.Empty, x))
                .Labelled("line comment")
                .Select<Term>(x => new TriviaTerm(x)));

        /// <summary>
        /// Defines the BlockComment
        /// </summary>
        private static Parser<char, Term> BlockComment = TryTok(
                nameof(BlockComment),
                String("/*")
                .Then(Parser<char>.Any.Until(String("*/")))
                .Select(x => string.Join(string.Empty, x))
                .Labelled(nameof(BlockComment))
                .Select<Term>(x => new TriviaTerm(x)));

        private static ThreadLocal<Dictionary<string, VariableTerm>> variableScope = new ThreadLocal<Dictionary<string, VariableTerm>>() { Value = new Dictionary<string, VariableTerm>() };

        private static VariableTerm GetVariable(string name)
        {
            lock (variableScope)
            {
                if (!variableScope.Value.TryGetValue(name, out VariableTerm variable))
                {
                    variable = new VariableTerm() { SourceName = name };
                    variableScope.Value[name] = variable;
                }

                return variable;
            }
        }

        /// <summary>
        /// Defines the PrologTopLevelParser
        /// </summary>
        private static readonly Parser<char, Term> PrologTopLevelParser =
                TryTok(
                    nameof(PrologTopLevelParser),
                    OneOf(
                        BlockComment,
                        LineComment,
                        OneOf
                        (
                            TryTokRec(nameof(PrologOpTermParser), () => PrologOpTermParser),
                            TryTokRec(nameof(PrologTermParser), () => PrologTermParser)
                        )
                        .Before(Char('.').Before(
                            OneOf(
                                Whitespace,
                                Parser<char>.End.Select(x => ' ')
                            )
                        ).Between(SkipWhitespaces))
                     .Select<Term>(x =>
                     {
                         variableScope.Value.Clear();
                         return x is StructureTerm str && str.functor.name == ":-" ? x : new StructureTerm(ArrowSymbol, new Term[] { x, TrueSymbol });
                     })
                ).Labelled(nameof(PrologTopLevelParser)));

        /// <summary>
        /// Defines the PrologOpTermParser
        /// </summary>
        private static readonly Parser<char, Term> PrologOpTermParser =
            TryTok(
                "OpTerm",
            (
                ExpressionParser.Build<char, Term>(
                expr => (
                    OneOf(
                        TryTokRec(nameof(PrologOpTermParser), () => Parenthesised(PrologOpTermParser)),
                        OptionalParens(PrologTermParser)
                    ),
                    GetOperatorTable(false)
                )
                ).Labelled(nameof(PrologOpTermParser))
            )
        );

        /// <summary>
        /// Defines the PrologOpParamsTermParser
        /// </summary>
        private static readonly Parser<char, Term> PrologOpParamsTermParser =
            TryTok("OpParamsTerm",
            (
                ExpressionParser.Build<char, Term>(
                expr => (
                    OneOf(
                        TryTokRec(nameof(PrologOpTermParser), () => Parenthesised(PrologOpTermParser)),
                        OptionalParens(PrologTermParser)
                    ),
                    GetOperatorTable(true)
                )
                ).Labelled(nameof(PrologOpParamsTermParser))
            )
        );

        /// <summary>
        /// The GetOperatorTable
        /// </summary>
        /// <param name="isParams">The isParams<see cref="bool"/></param>
        /// <returns>The <see cref="OperatorTableRow{char, Term}[]"/></returns>
        private static OperatorTableRow<char, Term>[] GetOperatorTable(bool isParams)
        {
            List<(int, OperatorTableRow<char, Term>)> ops = new List<(int, OperatorTableRow<char, Term>)>()
             {
                        // :- op(900, xfx, [=>]).
                        // yfx -> L
                        // xfx -> N
                        // xfy -> R
                        // http://www.swi-prolog.org/pldoc/man?section=operators

                        // xfy 200:
                        (200, Operator.InfixR(BinaryOp("^"))), 

                        // yfx 400:
                        (400,
                        Operator.InfixL(BinaryOp("*"))
                        .And(Operator.InfixL(BinaryOp("/")))
                        .And(Operator.InfixL(BinaryOp("//")))
                        .And(Operator.InfixL(BinaryOp("div")))
                        .And(Operator.InfixL(BinaryOp("rdiv")))
                        .And(Operator.InfixL(BinaryOp("<<")))
                        .And(Operator.InfixL(BinaryOp(">>")))
                        .And(Operator.InfixL(BinaryOp("mod")))
                        .And(Operator.InfixL(BinaryOp("rem")))),
                        
                        // yfx 500:
                        (500,
                        Operator.InfixL(BinaryOp("+"))
                        .And(Operator.InfixL(BinaryOp("-")))
                        .And(Operator.InfixL(BinaryOp("/\\")))
                        .And(Operator.InfixL(BinaryOp("\\/")))
                        .And(Operator.InfixL(BinaryOp("xor")))),

                        // xfy 600:
                        (600, Operator.InfixR(BinaryOp(":"))), 

                        // xfx 700:
                        (700,
                        Operator.InfixN(BinaryOp("="))
                        .And(Operator.InfixN(BinaryOp("<")))
                        .And(Operator.InfixN(BinaryOp(">")))
                        .And(Operator.InfixN(BinaryOp("=<")))
                        .And(Operator.InfixN(BinaryOp(">=")))
                        .And(Operator.InfixN(BinaryOp("\\=")))
                        .And(Operator.InfixN(BinaryOp("\\==")))
                        .And(Operator.InfixN(BinaryOp("==")))
                        .And(Operator.InfixN(BinaryOp("=:=")))
                        .And(Operator.InfixN(BinaryOp("=\\=")))
                        .And(Operator.InfixN(BinaryOp("=..")))
                        .And(Operator.InfixN(BinaryOp("=@=")))
                        .And(Operator.InfixN(BinaryOp("as")))
                        .And(Operator.InfixN(BinaryOp("is")))),

                        // fy 900:
                        (900,
                        Operator.Prefix(UnaryOp("\\+"))),

                        (999,
                        Operator.InfixN(CustomBinOp)),
            };

            if (!isParams)
            {
                List<(int, OperatorTableRow<char, Term>)> ops2 = new List<(int, OperatorTableRow<char, Term>)>()
                {

                        // xfx 990:
                        (990,
                        Operator.InfixN(BinaryOp(":="))),

                        // xfy 1000:
                        (1000,
                        Operator.InfixR(BinaryOp(","))), 
                        
                        // xfy 1050:
                        (1000,
                        Operator.InfixR(BinaryOp("->"))
                        .And(Operator.InfixR(BinaryOp("*->")))), 

                        // xfy 1100:
                        (1100,
                        Operator.InfixR(BinaryOp(";"))), 

                        // xfx 1200:
                        (1200,
                        Operator.InfixN(BinaryOp("-->"))
                        .And(Operator.InfixN(BinaryOp(":-"))))
                };

                ops.AddRange(ops2);
            }

            return ops.OrderBy(x => x.Item1).Select(x => x.Item2).ToArray();
        }

        /// <summary>
        /// Defines the StructureCallTry
        /// </summary>
        private static readonly Parser<char, Term> StructureCall
            = TryTok(
                nameof(StructureCall),
                Try(Symbol).Or(Atom.Select<Term>(x => SymbolTerm.MakeSymbol(x)))
                .Then(Parenthesised(
                        OneOf(
                            TryTokRec(nameof(PrologOpParamsTermParser), () => PrologOpParamsTermParser),
                            TryTokRec(nameof(PrologTermParser), () => PrologTermParser)
                        ).Separated(Comma)), (x, y) => (x, y))
                    .Between(SkipWhitespaces)
                .Select<Term>(x =>
                {
                    SymbolTerm s = x.x as SymbolTerm;
                    Term[] args = x.y.ToArray();
                    s = SymbolTerm.MakeSymbol(s.name, args.Length);
                    return s.arity == 0 ? (Term)s : new StructureTerm(s, args);
                })
                .Labelled(nameof(StructureCall)));

        /// <summary>
        /// Defines the StructureCallTry
        /// </summary>
        private static readonly Parser<char, Term> StructureOpCall
            = TryTok(
                nameof(StructureOpCall),
                Try(Symbol.Before(Whitespace)).Or(AnyCharExcept(' ', ',', ';').AtLeastOnceString().Select<Term>(x => SymbolTerm.MakeSymbol(x)).Before(Whitespace)).Before(SkipWhitespaces)
                .Then(Parenthesised(
                        OneOf(
                            TryTokRec(nameof(PrologOpTermParser), () => PrologOpTermParser),
                            TryTokRec(nameof(PrologTermParser), () => PrologTermParser)
                        ).AtLeastOnce()), (x, y) => (x, y))
                    .Between(SkipWhitespaces)
                .Select<Term>(x =>
                {
                    SymbolTerm s = x.x as SymbolTerm;
                    Term[] args = x.y.ToArray();
                    s = SymbolTerm.MakeSymbol(s.name, args.Length);
                    return s.arity == 0 ? (Term)s : new StructureTerm(s, args);
                })
                .Labelled(nameof(StructureOpCall)));

        private static readonly Parser<char, Term> ListElem =
            OneOf(
                TryTokRec(nameof(PrologOpParamsTermParser), () => PrologOpParamsTermParser),
                TryTokRec(nameof(PrologTermParser), () => PrologTermParser),
                TryTok(nameof(Atom), Atom.Select<Term>(x => SymbolTerm.MakeSymbol(x)))
            );

        /// <summary>
        /// Defines the List
        /// </summary>
        private static readonly Parser<char, Term> ListStruct
            = TryTok(
                nameof(ListStruct),
                Try(Char('[').Between(SkipWhitespaces).Then(Char(']')).Between(SkipWhitespaces).Select<IEnumerable<Term>>(x => null))
                .Or(Char('[').Then(
                    ListElem
                    .Separated(Comma))
                    .Then(Char('|').Then(ListElem).Optional(), (x, y) => x.Append(y.HasValue ? y.Value : EmptyListSymbol))
                    .Before(Char(']'))
                    .Between(SkipWhitespaces)
                ).Select<Term>(x =>
                {
                    // TODO: gen flatten/replace
                    if (x == null)
                    {
                        return EmptyListSymbol;
                    }

                    Term reduced = x
                        .Reverse()
                        .Aggregate((last, nextLast) => (Term)new ListTerm(nextLast, last));
                    return reduced;
                })
                .Labelled(nameof(ListStruct)));

        /// <summary>
        /// Defines the CurlyStruct
        /// </summary>
        private static readonly Parser<char, Term> CurlyStruct
            = TryTok(
                nameof(CurlyStruct),
                Try(Char('{').Between(SkipWhitespaces).Then(Char('}')).Between(SkipWhitespaces).Select<Term>(x => SymbolTerm.MakeSymbol("{}", 0)))
                .Or(Char('{').Then(
                    OneOf(
                        TryTokRec(nameof(PrologOpTermParser), () => PrologOpTermParser),
                        TryTokRec(nameof(PrologTermParser), () => PrologTermParser),
                        TryTok(nameof(Atom), Atom.Select<Term>(x => SymbolTerm.MakeSymbol(x)))
                    )
                    .Before(Char('}'))
                    .Between(SkipWhitespaces)
                ).Select<Term>(x => new StructureTerm(SymbolTerm.MakeSymbol("{}", 1), new[] { x })))
                .Labelled(nameof(CurlyStruct)));

        /// <summary>
        /// Defines the loggingActive
        /// </summary>
        public static ThreadLocal<bool> loggingActive = new ThreadLocal<bool>() { Value = false };

        /// <summary>
        /// The ParseOrNull
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">The input<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        public static T ParseOrNull<T>(string input) where T : Term
        {
            return ParseManyOrNull(input, packIntoLists: false)?.Cast<T>().SingleOrDefault();
        }

        /// <summary>
        /// The ParseManyOrNull
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">The input<see cref="string"/></param>
        /// <returns>The <see cref="IEnumerable{T}"/></returns>
        public static IEnumerable<Term> ParseManyOrNull(string input, bool discardTrivia = true, bool requireWholeStreamConsumed = true, bool packIntoLists = false)
        {
            try
            {
                List<Term> res = PrologTopLevelParser.Many()
                    .Before(requireWholeStreamConsumed ? Parser<char>.End : Parser<char>.Return(Unit.Value))
                    .ParseOrThrow(input, calculatePos: null)
                    .Where(x => !discardTrivia ? true : !(x is TriviaTerm))
                    .ToList();

                if (!packIntoLists)
                {
                    return res;
                }

                Dictionary<string, List<Term>> grouped = res
                    .Where(x => x is StructureTerm)
                    .Cast<StructureTerm>()
                    .Where(x => x.functor.name == ":-")
                    .GroupBy(x => (x.args[0] is SymbolTerm sym) ? sym.nameAndArity : (x.args[0] as StructureTerm).functor.nameAndArity)
                    .ToDictionary(x => x.Key, y => y.Cast<Term>().ToList());

                List<Term> packed = new List<Term>();
                foreach(IEnumerable<Term> group in grouped.Values)
                {
                    // TODO: gen flatten/replace
                    Term list = group.Prepend(EmptyListSymbol).Aggregate((last, nextLast) => (Term)new ListTerm(nextLast, last));
                    packed.Add(list);
                }

                return packed;
            }
            catch (Exception ex)
            {
                Println.Value(ex.Message);
                return null;
            }
        }

        public static Dictionary<string, Term> CreatePidginUserTable(List<Term> pidgin)
        {
            Dictionary<string, Term> res = new Dictionary<string, Term>();
            Term currPreds = SymbolTerm.MakeSymbol("[]");

            foreach (Term t in pidgin)
            {
                ListTerm lt = t as ListTerm;
                StructureTerm arrow = lt.car as StructureTerm;
                SymbolTerm arrowHead = (arrow.args[0] as StructureTerm)?.functor ?? arrow.args[0] as SymbolTerm;
                res.Add($"STR_/(VAR__{arrowHead.name},VAR__{arrowHead.arity})", t);

                currPreds = new ListTerm(
                    new StructureTerm(SymbolTerm.MakeSymbol(":-", 2), new Term[]
                    {
                        new StructureTerm(SymbolTerm.MakeSymbol("$current_predicates", 2), new Term[]{ SymbolTerm.MakeSymbol(arrowHead.name, arrowHead.arity), new IntegerTerm(arrowHead.arity) }),
                        SymbolTerm.MakeSymbol("true")
                    }), currPreds);
            }

            res.Add($"STR_/(VAR__$current_predicated,VAR__2)", currPreds);

            return res;
        }

        /// <summary>
        /// The ParseOrNull
        /// </summary>
        /// <param name="input">The input<see cref="string"/></param>
        /// <returns>The <see cref="Term"/></returns>
        public static Term ParseOrNull(string input)
        {
            return ParseOrNull<Term>(input);
        }

        /// <summary>
        /// The Parenthesised
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser">The parser<see cref="Parser{char, T}"/></param>
        /// <returns>The <see cref="Parser{char, T}"/></returns>
        private static Parser<char, T> Parenthesised<T>(Parser<char, T> parser) =>
            parser.Between(Char('('), Char(')'));

        /// <summary>
        /// The OptionalParens
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser">The parser<see cref="Parser{char, T}"/></param>
        /// <returns>The <see cref="Parser{char, T}"/></returns>
        private static Parser<char, T> OptionalParens<T>(Parser<char, T> parser) =>
            Try(Parenthesised(parser)).Or(parser);

        /// <summary>
        /// The Binary
        /// </summary>
        /// <param name="op">The op<see cref="Parser{char, SymbolTerm}"/></param>
        /// <returns>The <see cref="Parser{char, Func{Term, Term, Term}}"/></returns>
        private static Parser<char, Func<Term, Term, Term>> Binary(Parser<char, SymbolTerm> op)
            => op.Select<Func<Term, Term, Term>>(opp => (l, r) => new StructureTerm(opp, new[] { l, r }));

        /// <summary>
        /// The Unary
        /// </summary>
        /// <param name="op">The op<see cref="Parser{char, SymbolTerm}"/></param>
        /// <returns>The <see cref="Parser{char, Func{Term, Term}}"/></returns>
        private static Parser<char, Func<Term, Term>> Unary(Parser<char, SymbolTerm> op)
            => op.Select<Func<Term, Term>>(opp => o => new StructureTerm(opp, new[] { o }));

        /// <summary>
        /// Defines the _parseStack
        /// </summary>
        private static ThreadLocal<Stack<string>> _parseStack = new ThreadLocal<Stack<string>>() { Value = new Stack<string>() };

        /// <summary>
        /// Defines the lastPos
        /// </summary>
        private static ThreadLocal<SourcePos> lastPos = new ThreadLocal<SourcePos>();

        /// <summary>
        /// The TryTok
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nameof">The nameof<see cref="string"/></param>
        /// <param name="token">The token<see cref="Parser{char, T}"/></param>
        /// <returns>The <see cref="Parser{char, T}"/></returns>
        public static Parser<char, T> TryTok<T>(string nameof, Parser<char, T> token) =>
            Try(Tok(nameof, token));

        /// <summary>
        /// The TryTokRec
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nameof">The nameof<see cref="string"/></param>
        /// <param name="token">The token<see cref="Func{Parser{char, T}}"/></param>
        /// <returns>The <see cref="Parser{char, T}"/></returns>
        public static Parser<char, T> TryTokRec<T>(string nameof, Func<Parser<char, T>> token) =>
            Try(Tok(nameof, Rec(token)));

        /// <summary>
        /// The Tok
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="token">The token<see cref="Parser{char, T}"/></param>
        /// <returns>The <see cref="Parser{char, T}"/></returns>
        public static Parser<char, T> Tok<T>(string name, Parser<char, T> token) =>
            Parser<char>.CurrentPos
                .Then(Lookahead(AnyCharExcept().ManyString()), (x, y) => (x, y)).Select(x =>
                {
                    if (loggingActive.Value)
                    {
                        if (!lastPos.IsValueCreated || !lastPos.Value.Equals(x.x))
                        {
                            lastPos.Value = x.x;
                            Println.Value($"\r\n\"{x.y.Replace("\r\n", " \\r\\n ").Substring(0, Math.Min(x.y.Length, 10))}...\"  @({lastPos.Value.Line},{lastPos.Value.Col}):");
                            if (_parseStack.Value.Count > 0)
                            {
                                Println.Value("    @ TRY " + string.Join(" --> TRY ", _parseStack.Value.Reverse().ToList()) + ":");
                            }
                        }

                        if (name != nameof(Symbol)
                            && name != nameof(Variable)
                            && !name.StartsWith("BinOp_")
                            && !name.StartsWith("UnOp_")
                            && !name.StartsWith("Directive")
                            && !name.Contains("Comment"))
                        {
                            Println.Value(new String(' ', (_parseStack.Value.Count + 1) * 8) + $"TRY {name}");
                        }

                        _parseStack.Value.Push(name);
                    }

                    return x.x;
                })
                .Then(
                    token.RecoverWith(x =>
                    {
                        if (loggingActive.Value)
                        {
                            _parseStack.Value.Pop();
                            Println.Value(new String(' ', (_parseStack.Value.Count + 1) * 8) + $"NOT {name}");
                        }

                        return Parser<char>.Fail<T>();
                    }),
                    (startPos, parsed) => (startPos, parsed)
                )
                .Then(Parser<char>.CurrentPos, (x, endPosition) => (x.startPos, x.parsed, endPosition))
                .Select(x =>
                {
                    if (loggingActive.Value)
                    {
                        _parseStack.Value.Pop();
                        Println.Value(new String(' ', (_parseStack.Value.Count + 1) * 8) + $"OK {name} => " + x.parsed.GetType().Name + " => \"" + x.parsed.ToString() + "\"");
                    }

                    if (x.parsed is SourceInfo parsedSourceInfo)
                    {
                        parsedSourceInfo.LineNo = x.startPos.Line;
                        parsedSourceInfo.StartCol = x.startPos.Col;
                        parsedSourceInfo.EndCol = x.endPosition.Col;
                    }
                    return x.parsed;
                })
            .Between(SkipWhitespaces);
    }
}
