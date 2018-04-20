/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace LegacyCommandHandler
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("text")]
    [Name(nameof(CommandFilterHookup))]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class CommandFilterHookup : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService = null;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);

            textView.Properties.GetOrCreateSingletonProperty(typeof(CommandFilter),
                () => new CommandFilter(textViewAdapter, textView));
        }
    }
}
