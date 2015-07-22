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
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using System.Timers;
using System.Diagnostics;

namespace IronPython.EditorExtensions
{
    /// <summary>
    /// Shows errors in the error list
    /// </summary>
    class ErrorListPresenter
    {
        private IWpfTextView textView;
        private PyErrorListProvider errorListProvider;
        private SimpleTagger<ErrorTag> squiggleTagger;
        Microsoft.VisualStudio.Shell.ErrorListProvider errorList;

        private List<TrackingTagSpan<IErrorTag>> previousSquiggles;
        private List<ErrorTask> previousErrors;

        public ErrorListPresenter(IWpfTextView textView, IErrorProviderFactory squiggleProviderFactory, IServiceProvider serviceProvider)
        {
            this.textView = textView;
            this.textView.TextBuffer.Changed += OnTextBufferChanged;
            this.textView.Closed += new EventHandler(OnTextViewClosed);

            this.errorListProvider = new PyErrorListProvider();
            this.squiggleTagger = squiggleProviderFactory.GetErrorTagger(textView.TextBuffer);

            errorList = new Microsoft.VisualStudio.Shell.ErrorListProvider(serviceProvider);

            previousErrors = new List<ErrorTask>();
            previousSquiggles = new List<TrackingTagSpan<IErrorTag>>();

            CreateErrors();

        }

        void OnTextViewClosed(object sender, EventArgs e)
        {
            // when a text view is closed we want to remove the corresponding errors from the error list
            ClearErrors();
        }

        void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // keep the list of errors updated every time the buffer changes
            CreateErrors();
        }

        private void ClearErrors()
        {
            previousSquiggles.ForEach(tag => squiggleTagger.RemoveTagSpans(t => tag.Span == t.Span));
            previousSquiggles.Clear();
            previousErrors.ForEach(task => errorList.Tasks.Remove(task));
            previousErrors.Clear();
        }

        private void CreateErrors()
        {
            var errors = errorListProvider.GetErrors(textView.TextBuffer);

            // Check if we should update the error list based on the error count to avoid refreshing the list without changes
            if (errors.Count != this.previousErrors.Count)
            {
                // remove any previously created errors to get a clean start
                ClearErrors();

                foreach (ValidationError error in errors)
                {
                    // creates the instance that will be added to the Error List
                    ErrorTask task = new ErrorTask();
                    task.Category = TaskCategory.All;
                    task.Priority = TaskPriority.Normal;
                    task.Document = textView.TextBuffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument)).FilePath;
                    task.ErrorCategory = TranslateErrorCategory(error.Severity);
                    task.Text = error.Description;
                    task.Line = textView.TextSnapshot.GetLineNumberFromPosition(error.Span.Start);
                    task.Column = error.Span.Start - textView.TextSnapshot.GetLineFromLineNumber(task.Line).Start;
                    task.Navigate += OnTaskNavigate;
                    errorList.Tasks.Add(task);
                    previousErrors.Add(task);

                    ITrackingSpan span = textView.TextSnapshot.CreateTrackingSpan(error.Span, SpanTrackingMode.EdgeNegative);
                    squiggleTagger.CreateTagSpan(span, new ErrorTag("syntax error", error.Description));
                    previousSquiggles.Add(new TrackingTagSpan<IErrorTag>(span, new ErrorTag("syntax error", error.Description)));
                }
            }
        }

        /// <summary>
        /// Called when the user double-clicks on an entry in the Error List
        /// </summary>
        private void OnTaskNavigate(object source, EventArgs e)
        {
            ErrorTask task = source as ErrorTask;
            if (task != null)
            {
                // move the caret to position of the error
                textView.Caret.MoveTo(new SnapshotPoint(textView.TextSnapshot, textView.TextSnapshot.GetLineFromLineNumber(task.Line).Start + task.Column));
                // set focus to make sure the error is visible to the user
                textView.VisualElement.Focus();
            }
        }

        private TaskErrorCategory TranslateErrorCategory(ValidationErrorSeverity validationErrorSeverity)
        {
            switch (validationErrorSeverity)
            {
                case ValidationErrorSeverity.Error:
                    return TaskErrorCategory.Error;
                case ValidationErrorSeverity.Message:
                    return TaskErrorCategory.Message;
                case ValidationErrorSeverity.Warning:
                    return TaskErrorCategory.Warning;
            }

            return TaskErrorCategory.Error;
        }
    }
}