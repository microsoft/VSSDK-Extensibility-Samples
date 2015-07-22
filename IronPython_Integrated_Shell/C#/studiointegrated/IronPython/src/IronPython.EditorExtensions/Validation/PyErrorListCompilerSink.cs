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

namespace IronPython.EditorExtensions
{
    public class PyErrorListCompilerSink : CompilerSink
    {
        private ITextBuffer textBuffer;
        public List<ValidationError> Errors { get; private set; }

        public PyErrorListCompilerSink(ITextBuffer textBuffer)
        {
            this.textBuffer = textBuffer;
            this.Errors = new List<ValidationError>();
        }

        public override void AddError(string path, string message, string lineText, CodeSpan location, int errorCode, Severity severity)
        {
            // keep the error list under 100 items which is a reasonable number and avoids spending too much time on error processing
            if (Errors.Count < 100)
            {
                int startIndex, endIndex;
                if (location.StartLine > 0 && location.EndLine > 0)
                {
                    // get the error bounds to create a span pointing to the error text so it can be navigated to later on by the Error List
                    startIndex = textBuffer.CurrentSnapshot.GetLineFromLineNumber(location.StartLine - 1).Start.Position + location.StartColumn;
                    endIndex = textBuffer.CurrentSnapshot.GetLineFromLineNumber(location.EndLine - 1).Start.Position + location.EndColumn;

                    if (startIndex < endIndex && endIndex < textBuffer.CurrentSnapshot.GetText().Length)
                    {
                        // add the error with all its necessary information
                        Errors.Add(new ValidationError(new Span(startIndex, endIndex - startIndex), message, GetSeverity(severity), ValidationErrorType.Syntactic));

                        if (Errors.Count == 100)
                        {
                            // add a friendly error telling the user the maximum number of errors has been reached
                            Errors.Add(new ValidationError(new Span(startIndex, endIndex - startIndex), "The maximum number of errors or warnings has been reached."));
                        }
                    }
                }
            }
        }

        private ValidationErrorSeverity GetSeverity(Severity severity)
        {
            switch (severity)
            {
                case Severity.Error:
                    return ValidationErrorSeverity.Error;
                case Severity.Warning:
                    return ValidationErrorSeverity.Warning;
                default:
                    return ValidationErrorSeverity.Message;
            }
        }
    }
}