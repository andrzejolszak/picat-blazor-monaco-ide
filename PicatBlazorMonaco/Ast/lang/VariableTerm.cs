/*
 * @(#)VariableTerm.cs
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
using System.Threading;

namespace JJC.Psharp.Lang
{
    [Serializable]
    public sealed class VariableTerm : Term
    {
        public readonly long ID;
        internal object locked_object;

        // added to support lock/unlock.
        internal object unlocked_object;

        internal Term val;
        private static long ctr = long.MinValue;
        private static bool _simplifiedSource;
        /* START JJC - Added for fork/wait support */
        /* END JJC */
        private string _sourceName;

        public VariableTerm()
        {
            val = this;
            timeStamp = long.MinValue;
        }

        public VariableTerm(long id)
        {
            val = this;
            timeStamp = long.MinValue;
            ID = id;
        }

        public long timeStamp
        {
            get;
            set;
        }

        public string SourceName
        {
            get
            {
                return this._sourceName;
            }
            internal set
            {
                this._sourceName = value;
                if (this.val is VariableTerm && this.val != this)
                {
                    (this.val as VariableTerm).SourceName = value;
                }
            }
        }

        public bool IsBound => this.val != this;

        public static void EnableSimplifiedSource()
        {
            _simplifiedSource = true;
        }

        public static void DisableSimplifiedSource()
        {
            _simplifiedSource = false;
        }
    }
}