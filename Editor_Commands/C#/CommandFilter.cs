using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using System;
using OLEConstants = Microsoft.VisualStudio.OLE.Interop.Constants;

namespace EditorCommands
{
    internal sealed class CommandFilter : IOleCommandTarget
    {
        private readonly IWpfTextView textView;
        private readonly IClassifier classifier;
        private readonly SVsServiceProvider globalServiceProvider;
        private IEditorOperations editorOperations;

        public CommandFilter(IWpfTextView textView, IClassifierAggregatorService aggregatorFactory,
            SVsServiceProvider globalServiceProvider, IEditorOperationsFactoryService editorOperationsFactory)
        {
            this.textView = textView;
            classifier = aggregatorFactory.GetClassifier(textView.TextBuffer);
            this.globalServiceProvider = globalServiceProvider;
            editorOperations = editorOperationsFactory.GetEditorOperations(textView);
        }

        public IOleCommandTarget Next { get; internal set; }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            // Command handling
            if (pguidCmdGroup == Constants.EditorCommandsGuid)
            {
                // Dispatch to the correct command handler
                switch (nCmdID)
                {
                    case Constants.ToggleCommentCmdId:
                        return ToggleComment.HandleCommand(textView, classifier, GetShellCommandDispatcher(), editorOperations);
                    case Constants.FormatCodeCmdId:
                        return FormatCode.HandleCommand(textView, GetShellCommandDispatcher());
                    case Constants.DuplicateSelectionCmdId:
                        return DuplicateSelection.HandleCommand(textView, classifier, GetShellCommandDispatcher(), editorOperations);
                    case Constants.DuplicateSelectionReverseCmdId:
                        return DuplicateSelection.HandleCommand(textView, classifier, GetShellCommandDispatcher(), editorOperations, true);
                }
            }

            // No commands called. Pass to next command handler.
            if (Next != null)
            {
                return Next.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }
            return (int)OLEConstants.OLECMDERR_E_UNKNOWNGROUP;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            // Command handling registration
            if (pguidCmdGroup == Constants.EditorCommandsGuid && cCmds == 1)
            {
                switch (prgCmds[0].cmdID)
                {
                    case Constants.ToggleCommentCmdId:
                    case Constants.FormatCodeCmdId:
                    case Constants.DuplicateSelectionCmdId:
                    case Constants.DuplicateSelectionReverseCmdId:
                        prgCmds[0].cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
                        return VSConstants.S_OK;
                }
            }

            if (Next != null)
            {
                return Next.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }
            return (int)OLEConstants.OLECMDERR_E_UNKNOWNGROUP;
        }

        /// <summary>
        /// Get the SUIHostCommandDispatcher from the global service provider.
        /// </summary>
        private IOleCommandTarget GetShellCommandDispatcher()
        {
            return globalServiceProvider.GetService(typeof(SUIHostCommandDispatcher)) as IOleCommandTarget;
        }
    }
}
