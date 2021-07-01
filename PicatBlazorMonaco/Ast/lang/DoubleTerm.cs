/*
 *  @(#)DoubleTerm.cs
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
    public sealed class DoubleTerm : NumberTerm
    {
        private readonly double val;

        public DoubleTerm(double i)
        {
            val = i;
        }

        public double value()
        {
            return val;
        }

        public override string ToString()
        {
            return val.ToString();
        }

        public override int IntValue()
        {
            return (int)Math.Floor(val);
        }

        public override long LongValue()
        {
            return (long)val;
        }

        public override double DoubleValue()
        {
            return val;
        }
    }
}