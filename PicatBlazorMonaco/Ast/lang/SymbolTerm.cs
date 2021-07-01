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
using System.Diagnostics;

namespace JJC.Psharp.Lang
{
    [Serializable]
    public sealed class SymbolTerm : Term
    {
        internal readonly string nameAndArity;
        private static readonly ConcurrentDictionary<string, SymbolTerm> symbolTable = new ConcurrentDictionary<string, SymbolTerm>();
        // f/n型の全リソースのIndexリスト
        //public Term res = Prolog.Nil;
        // f/n型で第一引数が変数のもののIndexリスト
        //public Term res2 = Prolog.Nil;

        private SymbolTerm(string _name, int _arity, string key)
        {
            name = _name;
            arity = _arity;
            nameAndArity = key;
        }

        public string name
        {
            get;
        }

        public int arity
        {
            get;
        }

        /*public static void ClearRes(){
            SymbolTerm sym = null;
            foreach(var symb in symbolTable.Values ) {
                sym = symb;
                sym.res  = Prolog.Nil;
                sym.res2 = Prolog.Nil;
            }
        }*/

        /// <summary>
        /// OBS! Called by reflection, can't rename.
        /// </summary>
        /// <param name="_name"></param>
        /// <returns></returns>
        public static SymbolTerm MakeSymbol(string _name)
        {
            return MakeSymbol(_name, 0);
        }

        /// <summary>
        /// OBS! Called by reflection, can't rename.
        /// </summary>
        /// <param name="_name"></param>
        /// <param name="_arity"></param>
        /// <returns></returns>'
        [DebuggerStepThrough]
        public static SymbolTerm MakeSymbol(string _name, int _arity)
        {
            var key = _name + "/" + _arity;
            SymbolTerm sym;
            if (!symbolTable.TryGetValue(key, out sym))
            {
                sym = new SymbolTerm(_name, _arity, key);
                symbolTable[key] = sym;
            }

            return sym;
        }
    }
}