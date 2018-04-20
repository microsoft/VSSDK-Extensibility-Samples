/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using System;

namespace JoinLineCommandImplementation
{
    public static class JoinLine
    {
        public static void JoinSelectedLines(ITextView textView)
        {
            if (textView.Selection.IsEmpty)
            {
                return;
            }

            var selectedSpan = textView.Selection.SelectedSpans[0];
            textView.TextBuffer.Replace(selectedSpan, selectedSpan.GetText().Replace("\r\n", " "));

            ThreadHelper.Generic.BeginInvoke(() =>
            {
                var shellCommandDispatcher = GetShellCommandDispatcher(textView);
                Guid cmdGroup = VSConstants.VSStd2K;
                shellCommandDispatcher.Exec(ref cmdGroup, (uint)VSConstants.VSStd2KCmdID.FORMATDOCUMENT,
                    (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, System.IntPtr.Zero, System.IntPtr.Zero);
            });
        }

        private static IOleCommandTarget GetShellCommandDispatcher(ITextView view)
        {
            return ServiceProvider.GlobalProvider.GetService(typeof(SUIHostCommandDispatcher)) as IOleCommandTarget;
        }
    }
}
