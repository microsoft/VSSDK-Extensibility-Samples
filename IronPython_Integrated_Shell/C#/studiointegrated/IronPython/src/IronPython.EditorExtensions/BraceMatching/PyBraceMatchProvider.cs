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
using Microsoft.VisualStudio.Text;
using IronPython.Hosting;
using Microsoft.VisualStudio.IronPythonInference;

namespace IronPython.EditorExtensions
{
    /// <summary>
    /// Provides the brace macthing spans.
    /// </summary>
    internal class PyBraceMatchProvider
    {
        /// <summary>
        /// Returns the list of brace matching spans. Each tuple defines from/to the brace matching should be applied.
        /// It's possible to highlight more than one caracter.
        /// </summary>
        /// <param name="caretLocation"></param>
        /// <returns></returns>
        internal IList<Tuple<SnapshotSpan, SnapshotSpan>> GetBraceMatchingSpans(SnapshotPoint caretLocation)
        {
            var snapshot = caretLocation.Snapshot;

            PyBraceMatchCompilerSink sink = new PyBraceMatchCompilerSink(snapshot);
            Microsoft.VisualStudio.IronPythonInference.Modules modules = new Microsoft.VisualStudio.IronPythonInference.Modules();
            modules.AnalyzeModule(sink, snapshot.TextBuffer.GetFileName(), snapshot.GetText());

            //adjust off-by-one due to conversion from old text buffer system to new system, filtering to only adjust current location
            var newMatches = sink.Matches.Select(m => new Tuple<SnapshotSpan, SnapshotSpan>(new SnapshotSpan(m.Item2.Snapshot, m.Item1.Start.Position + 1, m.Item1.Length), new SnapshotSpan(m.Item2.Snapshot, m.Item2.Start.Position + 1, m.Item2.Length)))
                .Where(t => t.Item1.Contains(caretLocation) || t.Item2.Contains(caretLocation));

            return newMatches.ToList();
        }
    }
}