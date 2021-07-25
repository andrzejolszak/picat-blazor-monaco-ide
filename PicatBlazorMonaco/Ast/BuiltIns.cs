using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PicatBlazorMonaco.Ast
{
    public class BuiltIns
    {
        public static List<(string, string, string)> Operators = new List<(string, string, string)>
        {
            ("**", "Arithmetic", "Power"),
            ("+", "Arithmetic", "Addition"),
            ("-", "Arithmetic", "Subtraction"),
            ("~", "Arithmetic", "Bitwise completion"),
            ("*", "Arithmetic", "Multiplication"),
            ("/", "Arithmetic", "Division"),
            ("//", "Arithmetic", "Integer division, truncated"),
            ("/>", "Arithmetic", "Integer division (ceiling(X / Y ))"),
            ("/<", "Arithmetic", "Integer division (floor(X / Y )"),
            ("div", "Arithmetic", "Integer division, floored"),
            ("mod", "Arithmetic", "Modulo, same as X - floor(X div Y ) * Y"),
            ("rem", "Arithmetic", "Remainder (X - (X // Y ) * Y )"),
            (">>", "Arithmetic", "Right shift"),
            ("<<", "Arithmetic", "Left shift"),
            ("/'", "Arithmetic", "Bitwise AND"),
            ("ˆ", "Arithmetic", "Bitwise XOR"),
            ("'/", "Arithmetic", "Bitwise OR"),
            ("is", "Arithmetic", "Numerical assignment"),
            ("='=", "Arithmetic", "Numerical inequality"),
            ("=:=", "Arithmetic", "Numerical equality"),
            (":=", "Basic", "Assignment"),
            ("==", "Basic", "Term equality"),
            ("!==", "Basic", "Term inequality"),
            ("=", "Basic", "Unification"),
            ("!=", "Basic", "Cannot unify test"),
            ("=>", "Basic", "Non-backtrackable rule Head,Cond => Body"),
            ("?=>", "Basic", "Backtrackable rule Head,Cond ?=> Body"),
            (",", "Basic", "P, Q: This goal is a conjunction of goal P and goal Q. It is resolved by first resolving P, and then resolving Q. The goal is true if both P and Q are true. Note that the order is important: (P ,Q) is in general not the same as (Q,P )."),
            ("&&", "Basic", "P && Q: This is the same as (P,Q)."),
            (";", "Basic", "P; Q: This goal is a disjunction of goal P and goal Q. It is resolved by first resolving P . If P is true, then the disjunction is true. If P is false, then Q is resolved. The disjunction is true if Q is true. The disjunction is false if both P and Q are false. Note that a disjunction can succeed more than once. Note also that the order is important: (P ; Q) is generally not the same as (Q; P )."),
            ("||", "Basic", "P || Q: This is the same as (P; Q)."),
            ("not", "Basic", "not P: This goal is the negation of P . It is false if P is true, and true if P is false. Note a negation goal can never succeed more than once. Also note that no variables can get instantiated, no matter whether the goal is true or false."),
            ("'+", "Basic", "' + P: This is the same as not P ."),
            ("once", "Basic", "once P: This goal is the same as P , but can never succeed more than once."),
            ("repeat", "Basic", @"repeat: This predicate is defined as follows:
    repeat ?=> true.  
    repeat => repeat."),
            ("if-then", "Basic", "if-then: An if-then statement"),
            ("table", "Basic", "Table mode declaration in the form (M1,M2,…,Mn), where each Mi is one of the following: a plus-sign (+) indicates input, a minus-sign (-) indicates output, max indicates that the corresponding variable should be maximized, and min indicates that the corresponding variable should be minimized. The last mode Mn can be nt, which indicates that the argument is not tabled. Two types of data can be passed to a tabled predicate as an nt argument: (1) global data that are the same to all the calls of the predicate, and (2) data that are functionally dependent on the input arguments. Input arguments are assumed to be ground. Output arguments, including min and max arguments, are assumed to be variables. An argument with the mode min or max is called an objective argument. Only one argument can be an objective to be optimized. As an objective argument can be a compound value, this limit is not essential, and users can still specify multiple objective variables to be optimized. When a table mode declaration is provided, Picat tables only one optimal answer for the same input arguments."),
            ("index", "Basic", "A predicate definition that consists of Horn clauses can be preceded by an index declaration in the form index (M11 ,…,M1n) … (Mm1,…,Mmn)"),
            ("module", "Basic", "In Picat, source files must have the extension name .pi. A module is a source file that begins with a module name declaration in the form: module Name. where Name must be the same as the main file name. A file that does not begin with a module declaration is assumed to belong to the default global module. The following names are reserved for system modules and should not be used to name user modules: basic, bp, cp, glb, io, math, mip, nn, ordset, os, planner, sat, smt, sys, and util."),
            ("import", "Basic", "Import module"),
            ("<from> .. <step> .. <to>", "Arithmetic", "A range (list) of numbers with a step"),
            ("<from> .. <to>", "Arithmetic", "A range (list) of numbers with a step 1"),
        };

        public static List<(string, string, string)> Functions = new List<(string, string, string)>(100);

        public static List<DeclarationParser.Declaration> BuiltinsDeclarations = new List<DeclarationParser.Declaration>(100);

        public static void InitializeFunctions(string data)
        {
            Functions.Clear();
            string[] lines = data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string module = null;
            string entry = string.Empty;
            foreach (string line in lines)
            {
                if (module != null && entry != string.Empty && line[0] != ' ')
                {
                    string name = entry;
                    int closeBracketIndex = entry.IndexOf(')');
                    int colonIndex = entry.IndexOf(':');
                    if (closeBracketIndex > 0)
                    {
                        name = name.Substring(0, closeBracketIndex + 1);
                    }
                    else if (colonIndex > 0)
                    {
                        name = name.Substring(0, colonIndex);
                    }

                    if (colonIndex > 0)
                    {
                        entry = entry.Insert(colonIndex + 2, "\r\n");
                    }

                    Functions.Add((name, module, entry));
                    entry = string.Empty;
                }

                if (line == "-----")
                {
                    module = null;
                }
                else if (module == null)
                {
                    module = line.Trim();
                }
                else
                {
                    if (line[0] == ' ')
                    {
                        entry += "\r\n";
                    }

                    entry += line;
                }
            }

            BuiltinsDeclarations.Clear();
            foreach ((string, string, string) o in BuiltIns.Functions)
            {
                try
                {
                    string decl = o.Item1.Trim();
                    DeclarationParser.Declaration declaration = DeclarationParser.ParseBuiltinDeclaration(decl);
                    declaration.Comment = "[" + o.Item2 + "] " + o.Item3;
                    BuiltinsDeclarations.Add(declaration);
                }
                catch
                {
                    // NOP
                }
            }
        }
    }
}
