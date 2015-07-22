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
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace IronPython.EditorExtensions
{
    [Export(typeof(ICompletionSourceProvider))]
	[ContentType(PyContentTypeDefinition.ContentType)]
    [ContentType(PyContentTypeDefinition.ConsoleContentType)]
	[Name("Py Completion Source Provider")]
	[Order(Before = "default")]
	internal class CompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        private IGlyphService GlyphService { get; set; }

        [Import(typeof(SVsServiceProvider))]
        private System.IServiceProvider ServiceProvider { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            // Create the completion source
            return new CompletionSource(textBuffer, GlyphService, ServiceProvider);
        }
    }
}