/*
 * @(#)StructureTerm.cs
 */

/*
 * @author Mutsunori Banbara
 *         banbara@pascal.seg.kobe-u.ac.jp
 *         Nara National College of Technology
 * @author Naoyuki Tamura
 *         tamura@kobe-u.ac.jp
 *         Kobe University
 * Modified by Jon Cook.
 */

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace JJC.Psharp.Lang
{
    [Serializable]
    public sealed class StructureTerm : Term
    {
        [DebuggerStepThrough]
        public StructureTerm(SymbolTerm _functor, Term[] _args)
        {
            functor = _functor;
            args = _args;
            arity = functor.arity;

            if ((functor.name == ":-") && (args.Length > 0))
                CopySourceInfoFrom(args[0]);
            else
                CopySourceInfoFrom(functor);

            if ((args.Length > 0) && (args[args.Length - 1].EndPosition > -1))
                EndPosition = args[args.Length - 1].EndPosition;
        }

        public SymbolTerm functor
        {
            get;
            set;
        }

        public Term[] args
        {
            get;
            set;
        }

        public int arity
        {
            get;
            set;
        }

        public bool OperatorSyntax { get; set; }

        public string Name()
        {
            return functor.name;
        }
    }
}