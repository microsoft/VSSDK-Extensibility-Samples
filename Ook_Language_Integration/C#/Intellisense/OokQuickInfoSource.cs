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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace OokLanguage
{
    /// <summary>
    /// Factory for quick info sources
    /// </summary>
    [Export(typeof(IQuickInfoSourceProvider))]
    [ContentType("ook!")]
    [Name("ookQuickInfo")]
    class OokQuickInfoSourceProvider : IQuickInfoSourceProvider
    {

        [Import]
        IBufferTagAggregatorFactoryService aggService = null;

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new OokQuickInfoSource(textBuffer, aggService.CreateTagAggregator<OokTokenTag>(textBuffer));
        }
    }

    /// <summary>
    /// Provides QuickInfo information to be displayed in a text buffer
    /// </summary>
    class OokQuickInfoSource : IQuickInfoSource
    {
        private ITagAggregator<OokTokenTag> _aggregator;
        private ITextBuffer _buffer;
        private bool _disposed = false;


        public OokQuickInfoSource(ITextBuffer buffer, ITagAggregator<OokTokenTag> aggregator)
        {
            _aggregator = aggregator;
            _buffer = buffer;
        }

        /// <summary>
        /// Determine which pieces of Quickinfo content should be displayed
        /// </summary>
        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;

            if (_disposed)
                throw new ObjectDisposedException("TestQuickInfoSource");

            var triggerPoint = (SnapshotPoint) session.GetTriggerPoint(_buffer.CurrentSnapshot);

            if (triggerPoint == null)
                return;

            foreach (IMappingTagSpan<OokTokenTag> curTag in _aggregator.GetTags(new SnapshotSpan(triggerPoint, triggerPoint)))
            {
                if (curTag.Tag.type == OokTokenTypes.OokExclamation)
                {
                    var tagSpan = curTag.Span.GetSpans(_buffer).First();
                    applicableToSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(tagSpan, SpanTrackingMode.EdgeExclusive);
                    quickInfoContent.Add("Exclaimed Ook!");
                }
                else if (curTag.Tag.type == OokTokenTypes.OokQuestion)
                {
                    var tagSpan = curTag.Span.GetSpans(_buffer).First();
                    applicableToSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(tagSpan, SpanTrackingMode.EdgeExclusive);
                    quickInfoContent.Add("Question Ook?");
                }
                else if (curTag.Tag.type == OokTokenTypes.OokPeriod)
                {
                    var tagSpan = curTag.Span.GetSpans(_buffer).First();
                    applicableToSpan = _buffer.CurrentSnapshot.CreateTrackingSpan(tagSpan, SpanTrackingMode.EdgeExclusive);
                    quickInfoContent.Add("Regular Ook.");
                }
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}

