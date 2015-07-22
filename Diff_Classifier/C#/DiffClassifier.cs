//***************************************************************************
//
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//***************************************************************************

namespace DiffClassifier
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Classification;

    public class DiffClassifier : IClassifier
    {
        IClassificationTypeRegistryService _classificationTypeRegistry;

        internal DiffClassifier(IClassificationTypeRegistryService registry)
        {
            this._classificationTypeRegistry = registry;
        }

        #region Public Events
        #pragma warning disable 67
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
        #pragma warning restore 67
        #endregion // Public Events

        #region Public Methods
        /// <summary>
        /// Classify the given spans, which, for diff files, classifies
        /// a line at a time.
        /// </summary>
        /// <param name="span">The span of interest in this projection buffer.</param>
        /// <returns>The list of <see cref="ClassificationSpan"/> as contributed by the source buffers.</returns>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            ITextSnapshot snapshot = span.Snapshot;

            List<ClassificationSpan> spans = new List<ClassificationSpan>();

            if(snapshot.Length == 0)
                return spans;

            int startno = span.Start.GetContainingLine().LineNumber;
            int endno = (span.End - 1).GetContainingLine().LineNumber;

            for (int i = startno; i <= endno; i++)
            {
                ITextSnapshotLine line = snapshot.GetLineFromLineNumber(i);

                IClassificationType type = null;
                string text = line.Snapshot.GetText(
                        new SnapshotSpan(line.Start, Math.Min(4, line.Length))); // We only need the first 4 

                if (text.StartsWith("!", StringComparison.Ordinal))
                    type = _classificationTypeRegistry.GetClassificationType("diff.changed");
                else if (text.StartsWith("---", StringComparison.Ordinal))
                    type = _classificationTypeRegistry.GetClassificationType("diff.header");
                else if (text.StartsWith("-", StringComparison.Ordinal))
                    type = _classificationTypeRegistry.GetClassificationType("diff.removed");
                else if (text.StartsWith("<", StringComparison.Ordinal))
                    type = _classificationTypeRegistry.GetClassificationType("diff.removed");
                else if (text.StartsWith("@@", StringComparison.Ordinal))
                    type = _classificationTypeRegistry.GetClassificationType("diff.patchline");
                else if (text.StartsWith("+++", StringComparison.Ordinal))
                    type = _classificationTypeRegistry.GetClassificationType("diff.header");
                else if (text.StartsWith("+", StringComparison.Ordinal))
                    type = _classificationTypeRegistry.GetClassificationType("diff.added");
                else if (text.StartsWith(">", StringComparison.Ordinal))
                    type = _classificationTypeRegistry.GetClassificationType("diff.added");

                else if (text.StartsWith("***", StringComparison.Ordinal))
                {
                    if (i < 2)
                        type = _classificationTypeRegistry.GetClassificationType("diff.header");
                    else
                        type = _classificationTypeRegistry.GetClassificationType("diff.infoline");
                }
                else if (text.Length > 0 && !char.IsWhiteSpace(text[0]))
                    type = _classificationTypeRegistry.GetClassificationType("diff.infoline");

                if (type != null)
                    spans.Add(new ClassificationSpan(line.Extent, type));
            }

            return spans;
        }
        
        #endregion // Public Methods
    }
}
