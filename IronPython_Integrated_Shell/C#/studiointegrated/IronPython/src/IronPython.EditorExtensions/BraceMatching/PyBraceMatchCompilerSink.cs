/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using Microsoft.VisualStudio.Text;

namespace IronPython.EditorExtensions
{
    internal class PyBraceMatchCompilerSink : CompilerSink
    {
        ITextSnapshot snapshot;

        internal PyBraceMatchCompilerSink(ITextSnapshot snapshot)
        {
            this.snapshot = snapshot;
            this.Matches = new List<System.Tuple<SnapshotSpan, SnapshotSpan>>();
        }

        internal List<System.Tuple<SnapshotSpan, SnapshotSpan>> Matches { get; private set; }

        public override void MatchPair(CodeSpan opening, CodeSpan closing, int priority)
        {
            Matches.Add(new System.Tuple<SnapshotSpan, SnapshotSpan>(new SnapshotSpan(snapshot, ConvertCodeSpanToSpan(opening)), new SnapshotSpan(snapshot, ConvertCodeSpanToSpan(closing))));
        }

        /// <summary>
        /// Converts between an IronPython's CodeSpan and the new text editor's Span
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private Span ConvertCodeSpanToSpan(CodeSpan location)
        {
            if (location.StartLine > 0 && location.EndLine > 0)
            {
                var startIndex = snapshot.GetLineFromLineNumber(location.StartLine - 1).Start.Position + location.StartColumn - 1;
                var endIndex = snapshot.GetLineFromLineNumber(location.EndLine - 1).Start.Position + location.EndColumn - 1;

                if (startIndex != -1 && startIndex < endIndex && endIndex < snapshot.GetText().Length)
                {
                    return new Span(startIndex, endIndex - startIndex);
                }
            }

            return new Span();
        }

        // for brace matching we don't need to add errors (IronPython engine requires overriding AddError when extending CompilerSink)
        public override void AddError(string path, string message, string lineText, CodeSpan location, int errorCode, Severity severity)
        {
        }
    }
}