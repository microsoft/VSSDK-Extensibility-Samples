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

using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace IntraTextAdornmentSample
{
    /// <summary>
    /// Helper class for producing intra-text adornments from data tags.
    /// </summary>
    /// <remarks>
    /// For cases where intra-text adornments do not correspond exactly to tags,
    /// use the <see cref="IntraTextAdornmentTagger"/> base class.
    /// </remarks>
    internal abstract class IntraTextAdornmentTagTransformer<TDataTag, TAdornment>
        : IntraTextAdornmentTagger<TDataTag, TAdornment>, IDisposable
        where TDataTag : ITag
        where TAdornment : UIElement
    {
        protected readonly ITagAggregator<TDataTag> dataTagger;
        protected readonly PositionAffinity? adornmentAffinity;

        /// <param name="adornmentAffinity">Determines whether adornments based on data tags with zero-length spans
        /// will stick with preceding or succeeding text characters.</param>
        protected IntraTextAdornmentTagTransformer(IWpfTextView view, ITagAggregator<TDataTag> dataTagger, PositionAffinity adornmentAffinity = PositionAffinity.Successor)
            : base(view)
        {
            this.adornmentAffinity = adornmentAffinity;
            this.dataTagger = dataTagger;

            this.dataTagger.TagsChanged += HandleDataTagsChanged;
        }

        protected override IEnumerable<Tuple<SnapshotSpan, PositionAffinity?, TDataTag>> GetAdornmentData(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;

            ITextSnapshot snapshot = spans[0].Snapshot;

            foreach (IMappingTagSpan<TDataTag> dataTagSpan in dataTagger.GetTags(spans))
            {
                NormalizedSnapshotSpanCollection dataTagSpans = dataTagSpan.Span.GetSpans(snapshot);

                // Ignore data tags that are split by projection.
                // This is theoretically possible but unlikely in current scenarios.
                if (dataTagSpans.Count != 1)
                    continue;

                SnapshotSpan span = dataTagSpans[0];

                PositionAffinity? affinity = span.Length > 0 ? null : adornmentAffinity;

                yield return Tuple.Create(span, affinity, dataTagSpan.Tag);
            }
        }

        private void HandleDataTagsChanged(object sender, TagsChangedEventArgs args)
        {
            var changedSpans = args.Span.GetSpans(view.TextBuffer.CurrentSnapshot);
            InvalidateSpans(changedSpans);
        }

        public virtual void Dispose()
        {
            dataTagger.Dispose();
        }
    }
}
