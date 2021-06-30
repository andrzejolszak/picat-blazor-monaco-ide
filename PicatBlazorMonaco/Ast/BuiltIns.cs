using System;
using System.Collections.Generic;
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

        public static List<(string, string, string)> Functions = new List<(string, string, string)>
        {
            ("copy_term(Term1)", "System", "copy_term(Term1) = Term2: This function copies Term1 into Term2. If Term1 is an attributed variable, then Term2 will not contain any of the attributes."),
            ("copy_term_shallow(Term1)", "System", "copy_term_shallow(Term1) = Term2: This function copies the skeleton of Term1 into Term2. If Term1 is a variable or an atomic value, then it returns a complete copy of Term1, the same as copy_term(Term1); if Term1 is a list, then it returns a cons [H|T] where both the car H and the cdr T are free variables; otherwise, it is the same as new_struct(name(Term1),arity(Term1))."),
            ("hash_code(Term)", "System", "hash_code(Term) = Code: This function returns the hash code of Term. If Term is a variable, then the returned hash code is always 0."),
            ("to_codes(Term)", "System", "to_codes(Term) = Codes: This function returns a list of character codes of Term."),
            ("to_fstring(Format,Args…)", "System", "to_fstring(Format,Args…): This function converts the arguments in the Args… parameter into a string, according to the format string Format, and returns the string. The number of arguments in Args… cannot exceed 10."),
            ("to_string(Term)", "System", "to_string(Term) = String: This function returns a string representation of Term."),
            ("var(Term)", "System", "var(Term): This predicate is true if Term is a free variable."),
            ("nonvar(Term)", "System", "nonvar(Term): This predicate is true if Term is not a free variable."),
            ("attr_var(Term)", "Attributed Vars", "attr_var(Term): This predicate is true if Term is an attributed variable."),
            ("nonvar(Term)", "Attributed Vars", "nonvar(Term): This predicate is true if Term is not a free variable."),
            ("nonvar(Term)", "Attributed Vars", "dvar(Term): This predicate is true if Term is an attributed domain variable."),
            ("nonvar(Term)", "Attributed Vars", "bool_dvar(Term): This predicate is true if Term is an attributed domain variable whose lower bound is 0 and whose upper bound is 1."),
            ("nonvar(Term)", "Attributed Vars", "dvar_or_int(Term): This predicate is true if Term is an attributed domain variable or an integer."),
            ("nonvar(Term)", "Attributed Vars", "get_attr(X,Key) = Val: This function returns the Val of the key-value pair Key=V al that is attached to X. It throws an error if X has no attribute named Key."),
            ("nonvar(Term)", "Attributed Vars", "get_attr(X,Key,DefaultVal) = Val: This function returns Val of the key-value pair Key=V al that is attached to X. It returns DefaultV al if X does not have the attribute named Key."),
            ("nonvar(Term)", "Attributed Vars", "put_attr(X,Key,V al): This predicate attaches the key-value pair Key=V al to X, where Key is a non-variable term, and V al is any term."),
            ("nonvar(Term)", "Attributed Vars", "put_attr(X,Key): This predicate call is the same as put_attr(X,Key,not_a_value)."),
        };
    }
}
