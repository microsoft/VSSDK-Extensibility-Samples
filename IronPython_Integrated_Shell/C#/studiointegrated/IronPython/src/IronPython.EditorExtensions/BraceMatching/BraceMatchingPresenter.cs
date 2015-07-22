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
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

namespace IronPython.EditorExtensions
{
    internal class BraceMatchingPresenter
    {
        private IWpfTextView textView;
        private PyBraceMatchProvider braceMatcher;
        private ITextMarkerProviderFactory textMarkerProviderFactory;

        internal BraceMatchingPresenter(IWpfTextView textView, ITextMarkerProviderFactory textMarkerProviderFactory)
        {
            this.textView = textView;
            this.braceMatcher = new PyBraceMatchProvider();
            this.textView.Caret.PositionChanged += new EventHandler<CaretPositionChangedEventArgs>(Caret_PositionChanged);
            this.textMarkerProviderFactory = textMarkerProviderFactory;
        }

        private void Caret_PositionChanged(object source, CaretPositionChangedEventArgs e)
        {
            // update all adornments when caret position is changed
            RemoveAllAdornments(e.TextView.TextBuffer);
            AddAdornments(e);
        }

        private void AddAdornments(CaretPositionChangedEventArgs e)
        {
            // Use the brace matchers to highlight bounds
            if (e.TextView.TextViewLines != null)
            {
                foreach (var spans in braceMatcher.GetBraceMatchingSpans(e.NewPosition.BufferPosition))
                {
                    HighlightBounds(e.TextView.TextBuffer, spans.Item1);
                    HighlightBounds(e.TextView.TextBuffer, spans.Item2);
                }
            }
        }

        private void RemoveAllAdornments(ITextBuffer buffer)
        {
            textMarkerProviderFactory.GetTextMarkerTagger(buffer).RemoveTagSpans(span => true);
        }

        private void HighlightBounds(ITextBuffer buffer, SnapshotSpan span)
        {
            textMarkerProviderFactory.GetTextMarkerTagger(buffer).CreateTagSpan(span.Snapshot.CreateTrackingSpan(span.Span, SpanTrackingMode.EdgeExclusive), new TextMarkerTag("bracehighlight"));
        }
    }
}