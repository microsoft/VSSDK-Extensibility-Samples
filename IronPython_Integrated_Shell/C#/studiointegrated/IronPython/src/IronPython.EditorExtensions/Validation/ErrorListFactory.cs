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
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Shell;

namespace IronPython.EditorExtensions
{
    /// <summary>
    /// Adds the error list support when the view is created
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
	[ContentType(PyContentTypeDefinition.ContentType)]
	[TextViewRole(PredefinedTextViewRoles.Document)]
    class ErrorListPresenterFactory : IWpfTextViewCreationListener
    {
        [Import]
        private IErrorProviderFactory SquiggleProviderFactory { get; set; }

        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider ServiceProvider { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            // Add the error list support to the just created view
            textView.Properties.GetOrCreateSingletonProperty<ErrorListPresenter>(() =>
                new ErrorListPresenter(textView, SquiggleProviderFactory, ServiceProvider)
            );
        }
    }
}