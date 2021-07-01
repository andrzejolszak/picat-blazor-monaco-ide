/*
 * @(#)SymbolTerm.cs
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
using System.Collections.Concurrent;

namespace JJC.Psharp.Lang
{
    [Serializable]
    public sealed class TriviaTerm : Term
    {
        private static readonly ConcurrentDictionary<string, SymbolTerm> symbolTable = new ConcurrentDictionary<string, SymbolTerm>();

        public TriviaTerm(string text)
        {
            this.text = text;
        }

        public string text
        {
            get;
            set;
        }
    }
}