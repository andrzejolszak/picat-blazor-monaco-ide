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
using System.Diagnostics;

namespace JJC.Psharp.Lang
{
    [Serializable]
    public abstract class SourceInfo
    {
        public virtual string DefinitionFile { get; set; }
        public virtual int StartPosition { get; set; } = -1;
        public virtual int EndPosition { get; set; } = -1;
        public int StartCol { get; set; } = -1;
        public int EndCol { get; set; } = -1;
        public virtual int LineNo { get; set; } = -1;
        public virtual string TestGroup { get; set; }
        public virtual bool IsPredefined { get; set; }
        public virtual string CommentHeader { get; set; }
        public virtual string CommentBody { get; set; }

        [DebuggerStepThrough]
        public void CopySourceInfoFrom(SourceInfo other)
        {
            DefinitionFile = other.DefinitionFile;
            StartPosition = other.StartPosition;
            EndPosition = other.EndPosition;
            LineNo = other.LineNo;
            TestGroup = other.TestGroup;
            IsPredefined = other.IsPredefined;
            CommentHeader = other.CommentHeader;
            CommentBody = other.CommentBody;
        }

        public string GetSourceInfoSetters()
        {
            return "";
            /*return $@"{{
            DefinitionFile = ""{this.DefinitionFile}"",
            StartPosition = {this.StartPosition},
            EndPosition = {this.EndPosition},
            LineNo = {this.LineNo},
            TestGroup = ""{this.TestGroup}"",
            IsPredefined = {this.IsPredefined},
            CommentHeader = ""{this.CommentHeader}"",
            CommentBody = ""{this.CommentBody}""}}";*/
        }
    }
}