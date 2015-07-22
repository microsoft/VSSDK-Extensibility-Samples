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
using Microsoft.VisualStudio.Editor;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;

namespace IronPython.EditorExtensions
{
    /// <summary>
    /// Initializes the completion controller when a text view is created
    /// </summary>
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(PyContentTypeDefinition.ContentType)]
    [ContentType(PyContentTypeDefinition.ConsoleContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class CompletionCommandFilterProvider : IVsTextViewCreationListener
    {
        [Import]
        IVsEditorAdaptersFactoryService EditorAdaptersFactory { get; set; }

        void IVsTextViewCreationListener.VsTextViewCreated(Microsoft.VisualStudio.TextManager.Interop.IVsTextView textViewAdapter)
        {
            // Get wpf text view of the adapter 
            var view = this.EditorAdaptersFactory.GetWpfTextView(textViewAdapter);

            // Initialize the completion controller
            var completionController = view.Properties.GetProperty<CompletionController>(typeof(CompletionController));
            if (completionController != null)
                completionController.Initialize(textViewAdapter);
        }
    }
}
