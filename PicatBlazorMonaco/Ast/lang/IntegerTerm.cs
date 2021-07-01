/*
 *  @(#)IntegerTerm.cs
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
using System.Diagnostics;

namespace JJC.Psharp.Lang
{
    [Serializable]
    public sealed class IntegerTerm : NumberTerm
    {
        private readonly int val;

        [DebuggerStepThrough]
        public IntegerTerm(int i)
        {
            val = i;
        }

        public int value()
        {
            return val;
        }

        public override string ToString()
        {
            return val.ToString();
        }

        public override int IntValue()
        {
            return val;
        }

        public override long LongValue()
        {
            return val;
        }

        public override double DoubleValue()
        {
            return val;
        }
    }
}