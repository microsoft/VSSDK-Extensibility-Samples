/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Samples.VisualStudio.IronPython.Interfaces;
using Microsoft.VisualStudio.IronPythonInference;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.TextManager.Interop;

namespace IronPython.EditorExtensions
{
    /// <summary>
    /// Implementation of <see cref="ICompletionSource"/>. Provides the completion sets for the editor. 
    /// </summary>
    internal class CompletionSource : ICompletionSource
    {
        internal static string CompletionSetName = "IronPython Completion";

        private ITextBuffer textBuffer;
        private IGlyphService glyphService;
        private IServiceProvider serviceProvider;

        internal CompletionSource(ITextBuffer textBuffer, IGlyphService glyphService, IServiceProvider serviceProvider)
        {
            this.textBuffer = textBuffer;
            this.glyphService = glyphService;
            this.serviceProvider = serviceProvider;
        }

        #region ICompletionSource Members

        void ICompletionSource.AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            int position = session.GetTriggerPoint(session.TextView.TextBuffer).GetPosition(textBuffer.CurrentSnapshot);
            int line = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(position);
            int column = position - textBuffer.CurrentSnapshot.GetLineFromPosition(position).Start.Position;

            Microsoft.VisualStudio.IronPythonInference.Modules modules = new Microsoft.VisualStudio.IronPythonInference.Modules();

            IList<Declaration> attributes;
            if (textBuffer.GetReadOnlyExtents(new Span(0, textBuffer.CurrentSnapshot.Length)).Count > 0)
            {
                int start;
                var readWriteText = TextOfLine(textBuffer, line, column, out start, true);
                var module = modules.AnalyzeModule(new QuietCompilerSink(), textBuffer.GetFileName(), readWriteText);

                attributes = module.GetAttributesAt(1, column - 1);

                foreach (var attribute in GetEngineAttributes(readWriteText, column - start - 1))
                {
                    attributes.Add(attribute);
                }
            }
            else
            {
                var module = modules.AnalyzeModule(new QuietCompilerSink(), textBuffer.GetFileName(), textBuffer.CurrentSnapshot.GetText());

                attributes = module.GetAttributesAt(line + 1, column);
            }

            completionSets.Add(GetCompletions((List<Declaration>)attributes, session));
        }

        private IList<Declaration> GetEngineAttributes(string lineText, int column)
        {
            var declarations = new List<Declaration>();

            int start = lineText.Substring(0, column).LastIndexOfAny(Constants.Separators) + 1;

            // Now build the string to pass to the engine.
            string engineCommand = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "dir({0})",
                lineText.Substring(start, column - start));

            try
            {
                var members = Engine.Evaluate(engineCommand) as IEnumerable;
                if (null != members)
                {
                    foreach (string member in members)
                    {
                        declarations.Add(new Declaration(member));
                    }
                }
            }
            catch
            {
                // Do nothing => Return the empty declarations list
            }

            return declarations;
        }

        private IEngine engine;
        private IEngine Engine
        {
            get
            {
                if (null == engine)
                {
                    var provider = (IPythonEngineProvider)serviceProvider.GetService(typeof(IPythonEngineProvider));
                    engine = provider.GetSharedEngine();
                }

                return engine;
            }
        }


        /// <summary>
        /// Get a piece of text of a given line in a text buffer
        /// </summary>
        public string TextOfLine(ITextBuffer textBuffer, int lineNumber, int endColumn, out int start, bool skipReadOnly)
        {
            start = 0;
            var line = textBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber);

            if (textBuffer.IsReadOnly(line.Extent.Span))
            {
                start = GetReadOnlyLength(textBuffer.CurrentSnapshot) - line.Start;
            }

            return line.GetText().Substring(start, endColumn - start);
        }

        private int GetReadOnlyLength(ITextSnapshot textSnapshot)
        {
            return textSnapshot.TextBuffer.GetReadOnlyExtents(new Span(0, textSnapshot.Length)).Max(region => region.End);
        }

        /// <summary>
        /// Gets the declarations and snippet entries for the completion
        /// </summary>
        private CompletionSet GetCompletions(List<Declaration> attributes, ICompletionSession session)
        {
            // Add IPy completion
            var completions = new List<Completion>();
            completions.AddRange(attributes.Select(declaration => new PyCompletion(declaration, glyphService)));

            if (completions.Count > 0)
            {
                // Add Snippets entries
                var expansionManager = (IVsTextManager2)this.serviceProvider.GetService(typeof(SVsTextManager));
                var snippetsEnumerator = new SnippetsEnumerator(expansionManager, Constants.IronPythonLanguageServiceGuid);
                completions.AddRange(snippetsEnumerator.Select(expansion => new PyCompletion(expansion, glyphService)));
            }

            // we want the user to get a sorted list
            completions.Sort();

            return
                new CompletionSet("IPyCompletion",
                    "IronPython Completion",
                    CreateTrackingSpan(session.GetTriggerPoint(session.TextView.TextBuffer).GetPosition(textBuffer.CurrentSnapshot)),
                    completions,
                    null)
            ;
        }

        private ITrackingSpan CreateTrackingSpan(int position)
        {
            char[] separators = new[] { '\n', '\r', '\t', ' ', '.', ':', '(', ')', '[', ']', '{', '}', '?', '/', '+', '-', ';', '=', '*', '!', ',', '<', '>' };

            string text = textBuffer.CurrentSnapshot.GetText();
            int last = text.Substring(position).IndexOfAny(separators);
            int first = text.Substring(0, position).LastIndexOfAny(separators) + 1;

            if (last == -1)
                last = text.Length - position;

            return textBuffer.CurrentSnapshot.CreateTrackingSpan(new Span(first, (last + position) - first), SpanTrackingMode.EdgeInclusive);
        }

        #endregion

        public void Dispose()
        { }
    }
}