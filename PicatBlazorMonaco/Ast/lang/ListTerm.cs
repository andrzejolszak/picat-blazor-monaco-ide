/*
 *  @(#)ListTerm.cs
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

namespace JJC.Psharp.Lang
{
    [Serializable]
    public sealed class ListTerm : Term
    {
        public ListTerm(Term _car, Term _cdr)
        {
            car = _car;
            cdr = _cdr;

            CopySourceInfoFrom(car);
            EndPosition = cdr.EndPosition;
        }

        public Term car
        {
            get;
            set;
        }

        public Term cdr
        {
            get;
            set;
        }
    }
}