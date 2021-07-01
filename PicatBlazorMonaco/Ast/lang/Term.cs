/*
 * @(#)Term.cs
 */

/*
 * @author Mutsunori Banbara
 *         banbara@pascal.seg.kobe-u.ac.jp
 *         Nara National College of Technology
 * @author Naoyuki Tamura
 *         tamura@kobe-u.ac.jp
 *         Kobe University
 * Modified by Jon Cook.
 *
 */

using System;

namespace JJC.Psharp.Lang
{
    [Serializable]
    public abstract class Term : SourceInfo
    {

        public bool IsVariable()
        {
            return this is VariableTerm;
        }

        public bool IsSymbol()
        {
            return this is SymbolTerm;
        }

        public bool IsNumber()
        {
            return this is NumberTerm;
        }

        public bool IsInteger()
        {
            return this is IntegerTerm;
        }

        public bool IsDouble()
        {
            return this is DoubleTerm;
        }

        public bool IsList()
        {
            return this is ListTerm;
        }

        public bool IsStructure()
        {
            return this is StructureTerm;
        }
    }
}