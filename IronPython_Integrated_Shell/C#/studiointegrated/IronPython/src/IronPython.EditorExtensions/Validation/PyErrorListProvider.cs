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
using IronPython.Compiler;
using IronPython.Runtime;
using IronPython.Hosting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.IronPythonInference;

namespace IronPython.EditorExtensions
{
    /// <summary>
    /// Provides errors that should be added to the error list
    /// </summary>
    internal class PyErrorListProvider
    {
        /// <summary>
        /// Gets the errors of the text buffer
        /// </summary>
        /// <param name="textBuffer"></param>
        /// <returns></returns>
        internal IList<ValidationError> GetErrors(ITextBuffer textBuffer)
        {
            var sink = new PyErrorListCompilerSink(textBuffer);
            var modules = new Microsoft.VisualStudio.IronPythonInference.Modules();
            modules.AnalyzeModule(sink, textBuffer.GetFileName(), textBuffer.CurrentSnapshot.GetText());

            return sink.Errors.ToList();
        }
    }
}
