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

namespace JJC.Psharp.Lang {

	[Serializable]
	public abstract class NumberTerm : Term {
	    public abstract int IntValue();
	    public abstract long LongValue();
	    public abstract double DoubleValue();
	}

}