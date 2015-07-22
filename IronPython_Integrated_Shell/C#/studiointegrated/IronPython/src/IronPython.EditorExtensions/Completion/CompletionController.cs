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
using System.Windows.Input;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Runtime.InteropServices;
using IronPython.Compiler;
using IronPython.Runtime;
using Microsoft.VisualStudio.IronPythonInference;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;

namespace IronPython.EditorExtensions
{
    /// <summary>
    /// Triggers the intellisense completion for the iron python editor.
    /// </summary>
    internal class CompletionController : IIntellisenseController, IOleCommandTarget, IVsExpansionClient
    {
        private ITextView textView;
        private IVsTextView vsTextView;
        private IList<ITextBuffer> subjectBuffers;
        private ICompletionBroker completionBrokerMap;
        private ICompletionSession activeSession;
        private IOleCommandTarget nextCommandTarget;
        private IVsExpansionSession expansionSession;
        private System.IServiceProvider serviceProvider;

        /// <summary>
        /// Attaches events for invoking Statement completion 
        /// </summary>
        /// <param name="subjectBuffers"></param>
        /// <param name="textView"></param>
        /// <param name="completionBrokerMap"></param>
        internal CompletionController(IList<ITextBuffer> subjectBuffers, ITextView textView, ICompletionBroker completionBrokerMap, System.IServiceProvider serviceProvider)
        {
            this.subjectBuffers = subjectBuffers;
            this.textView = textView;
            this.completionBrokerMap = completionBrokerMap;
            this.serviceProvider = serviceProvider;
        }

        #region IIntellisenseController Members

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        { }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        { }

        public void Detach(ITextView textView)
        { }

        #endregion

        private void ShowCompletion()
        {
            // If there is no active session
            if (activeSession == null || activeSession.IsDismissed)
            {
                // Check if we are in multi line selection mode (alt + mouse)
                if (this.textView.Selection.Mode != TextSelectionMode.Box)
                {
                    SnapshotPoint? caretPoint;

                    // Determine the position of the caret
                    if (textView.TextViewModel.DataBuffer is IProjectionBuffer)
                    {
                        caretPoint = textView.Caret.Position.Point.GetPoint
                                    (textView.TextViewModel.DataBuffer,
                                     PositionAffinity.Predecessor);
                    }
                    else
                    {
                        caretPoint = textView.Caret.Position.Point.GetPoint
                                                        (textBuffer => (subjectBuffers.Contains(textBuffer)),
                                                         PositionAffinity.Predecessor);
                    }

                    if (caretPoint.HasValue)
                    {
                        // Trigger the completion session
                        activeSession = completionBrokerMap.TriggerCompletion(textView);

                        // Attach to the active session events
                        if (activeSession != null)
                        {
                            activeSession.Dismissed += new System.EventHandler(OnActiveSessionDismissed);
                            activeSession.Committed += new System.EventHandler(OnActiveSessionCommited);
                        }
                    }
                }
            }
        }

        private bool IsCommitKey(char key)
        {
            return key == '.' || key == '(';
        }

        private bool IsTriggerKey(char key)
        {
            // Check if the key should trigger the completion session
            char previousChar = ' ';
            if (this.textView.Caret.Position.BufferPosition.Position > 0)
            {
                previousChar = this.textView.TextSnapshot.GetText(this.textView.Caret.Position.BufferPosition.Position - 1, 1)[0];

            }

            if (IsWhiteSpaceOrSymbol(previousChar) && (char.IsLetter(key) || key == '_' || key == '.'))
                return true;
            else if (char.IsLetterOrDigit(previousChar) && key == '.')
                return true;
            else
                return false;
        }

        private static bool IsWhiteSpaceOrSymbol(char text)
        {
            return char.IsWhiteSpace(text) || Constants.SeparatorsPlusDot.Contains(text);
        }

        private void OnActiveSessionDismissed(object sender, System.EventArgs e)
        {
            activeSession = null;
        }

        private void OnActiveSessionCommited(object sender, System.EventArgs e)
        {
            var selectedCompletion = this.activeSession.SelectedCompletionSet.SelectionStatus.Completion as PyCompletion;

            // Check if the selected completion is a code snippet
            if (selectedCompletion != null && selectedCompletion.VsExpansion.HasValue)
            {
                InsertCodeExpansion(selectedCompletion.VsExpansion.Value);
            }

            activeSession = null;
        }

        private void InsertCodeExpansion(VsExpansion expansion)
        {
            int startLine, startColumn, endLine, endColumn;
            if (activeSession != null)
            {
                // if there is an active completion session we need to use the trigger point of that session
                int position = activeSession.GetTriggerPoint(activeSession.TextView.TextBuffer).GetPosition(textView.TextBuffer.CurrentSnapshot);
                startLine = textView.TextBuffer.CurrentSnapshot.GetLineNumberFromPosition(position);
                startColumn = position - textView.TextBuffer.CurrentSnapshot.GetLineFromPosition(position).Start.Position;

                this.vsTextView.GetCaretPos(out endLine, out endColumn);
            }
            else
            {
                // there is no active completion session so we would use the caret position of the view instead
                this.vsTextView.GetCaretPos(out startLine, out startColumn);
                endColumn = startColumn;
                endLine = startLine;
            }

            InsertCodeExpansion(expansion, startLine, startColumn, endLine, endColumn);
        }

        private void InsertCodeExpansion(VsExpansion expansion, int startLine, int startColumn, int endLine, int endColumn)
        {
            // Insert the selected code snippet and start an expansion session
            IVsTextLines buffer;
            vsTextView.GetBuffer(out buffer);

            // Get the IVsExpansion from the current IVsTextLines
            IVsExpansion vsExpansion = (IVsExpansion)buffer;

            // Call the actual method that performs the snippet insertion
            vsExpansion.InsertNamedExpansion(
                expansion.title,
                expansion.path,
                new TextSpan { iStartIndex = startColumn, iEndIndex = endColumn, iEndLine = endLine, iStartLine = startLine },
                null,
                Constants.IronPythonLanguageServiceGuid,
                0,
                out expansionSession);
        }

        /// <summary>
        /// Initializes the completion controller
        /// </summary>
        /// <param name="commandView"></param>
        internal void Initialize(IVsTextView commandView)
        {
            if (this.vsTextView == null && commandView != null)
            {
                this.vsTextView = commandView;
                this.vsTextView.AddCommandFilter(this, out nextCommandTarget);
            }
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            // Handle VS commands to support code snippets

            if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                if (nCmdID == (uint)VSConstants.VSStd2KCmdID.INSERTSNIPPET || nCmdID == (uint)VSConstants.VSStd2KCmdID.SURROUNDWITH)
                {
                    IVsTextManager2 textManager = (IVsTextManager2)this.serviceProvider.GetService(typeof(SVsTextManager));
                    IVsExpansionManager expansionManager;
                    if (VSConstants.S_OK == textManager.GetExpansionManager(out expansionManager))
                    {
                        expansionManager.InvokeInsertionUI(
                            vsTextView,
                            this,
                            Constants.IronPythonLanguageServiceGuid,
                            null,
                            0,
                            1,
                            null,
                            0,
                            1,
                            "Insert Snippet",
                            string.Empty);
                    }

                    return VSConstants.S_OK;
                }

                if (this.expansionSession != null)
                {
                    // Handle VS Expansion (Code Snippets) keys
                    if ((nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB))
                    {
                        if (expansionSession.GoToNextExpansionField(0) == VSConstants.S_OK)
                            return VSConstants.S_OK;
                    }
                    else if ((nCmdID == (uint)VSConstants.VSStd2KCmdID.BACKTAB))
                    {
                        if (expansionSession.GoToPreviousExpansionField() == VSConstants.S_OK)
                            return VSConstants.S_OK;
                    }
                    else if ((nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN || nCmdID == (uint)VSConstants.VSStd2KCmdID.CANCEL))
                    {
                        if (expansionSession.EndCurrentExpansion(0) == VSConstants.S_OK)
                        {
                            expansionSession = null;

                            return VSConstants.S_OK;
                        }
                    }
                }

                // Handle Edit.ListMembers or Edit.CompleteWord commands
                if ((nCmdID == (uint)VSConstants.VSStd2KCmdID.SHOWMEMBERLIST || nCmdID == (uint)VSConstants.VSStd2KCmdID.COMPLETEWORD))
                {
                    if (activeSession != null)
                    {
                        activeSession.Dismiss();
                    }

                    ShowCompletion();

                    return VSConstants.S_OK;
                }

                // Handle Enter/Tab commit keys
                if (activeSession != null && (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN || nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB))
                {
                    if (activeSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                        activeSession.Commit();
                    else
                        activeSession.Dismiss();

                    return VSConstants.S_OK;
                }

                // Handle Code Snippets after pressing the Tab key without completion
                if (activeSession == null && (nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB))
                {
                    using (var systemState = new SystemState())
                    {
                        // Get the current line text until the cursor
                        var line = this.textView.GetTextViewLineContainingBufferPosition(this.textView.Caret.Position.BufferPosition);
                        var text = this.textView.TextSnapshot.GetText(line.Start.Position, this.textView.Caret.Position.BufferPosition - line.Start.Position);

                        // Create a tokenizer for the text
                        var tokenizer = new Tokenizer(text.ToCharArray(), true, systemState, new CompilerContext(string.Empty, new QuietCompilerSink()));

                        // Get the last token in the text
                        Token currentToken, lastToken = null;
                        while ((currentToken = tokenizer.Next()).Kind != TokenKind.NewLine)
                        {
                            lastToken = currentToken;
                        }

                        if (lastToken != null && lastToken.Kind != TokenKind.Constant)
                        {
                            var expansionManager = (IVsTextManager2)this.serviceProvider.GetService(typeof(SVsTextManager));
                            var snippetsEnumerator = new SnippetsEnumerator(expansionManager, Constants.IronPythonLanguageServiceGuid);

                            // Search a snippet that matched the token text
                            var expansion = snippetsEnumerator.FirstOrDefault(e => e.title == lastToken.Value.ToString());

                            if (expansion.title != null)
                            {
                                // Set the location where the snippet will be inserted
                                int startLine, startColumn, endLine, endColumn;

                                this.vsTextView.GetCaretPos(out startLine, out endColumn);
                                startColumn = endColumn - expansion.title.Length;
                                endLine = startLine;

                                // Insert the snippet
                                InsertCodeExpansion(expansion, startLine, startColumn, endLine, endColumn);

                                return VSConstants.S_OK;
                            }
                        }
                    }
                }

                // Hanlde other keys
                if ((nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR))
                {
                    char typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);

                    if (activeSession == null)
                    {
                        // Handle trigger keys
                        // Check if the typed char is a trigger
                        if (IsTriggerKey(typedChar))
                        {
                            var result = this.nextCommandTarget.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                            ShowCompletion();

                            return result;
                        }
                    }
                    else
                    {
                        // Handle commit keys
                        // Check if the typed char is a commit key
                        if (IsCommitKey(typedChar))
                        {
                            if (activeSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                                activeSession.Commit();
                            else
                                activeSession.Dismiss();

                            var result = this.nextCommandTarget.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                            // Check we should trigger completion after comitting the previous session (for example, after typing dot '.')
                            if (IsTriggerKey(typedChar))
                                ShowCompletion();

                            return result;
                        }
                    }
                }
            }

            // we haven't handled this command so pass it onto the next target
            return this.nextCommandTarget.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K && cCmds > 0)
            {
                // completion commands should be available
                if (((uint)VSConstants.VSStd2KCmdID.SHOWMEMBERLIST == (uint)prgCmds[0].cmdID || (uint)VSConstants.VSStd2KCmdID.COMPLETEWORD == (uint)prgCmds[0].cmdID))
                {
                    prgCmds[0].cmdf = (int)OleConstants.MSOCMDF_ENABLED | (int)OleConstants.MSOCMDF_SUPPORTED;
                    return VSConstants.S_OK;
                }

                // snippet commands should be available
                if ((uint)prgCmds[0].cmdID == (uint)VSConstants.VSStd2KCmdID.INSERTSNIPPET || (uint)prgCmds[0].cmdID == (uint)VSConstants.VSStd2KCmdID.SURROUNDWITH)
                {
                    prgCmds[0].cmdf = (int)OleConstants.MSOCMDF_ENABLED | (int)OleConstants.MSOCMDF_SUPPORTED;
                    return VSConstants.S_OK;
                }
            }

            // we haven't handled this query status so pass it onto the next target
            return this.nextCommandTarget.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        #region IVsExpansionClient Members

        public int EndExpansion()
        {
            return VSConstants.S_OK;
        }

        public int FormatSpan(IVsTextLines pBuffer, TextSpan[] ts)
        {
            return VSConstants.S_OK;
        }

        public int GetExpansionFunction(MSXML.IXMLDOMNode xmlFunctionNode, string bstrFieldName, out IVsExpansionFunction pFunc)
        {
            pFunc = null;

            return VSConstants.S_OK;
        }

        public int IsValidKind(IVsTextLines pBuffer, TextSpan[] ts, string bstrKind, out int pfIsValidKind)
        {
            pfIsValidKind = 1;

            return VSConstants.S_OK;
        }

        public int IsValidType(IVsTextLines pBuffer, TextSpan[] ts, string[] rgTypes, int iCountTypes, out int pfIsValidType)
        {
            pfIsValidType = 1;

            return VSConstants.S_OK;
        }

        public int OnAfterInsertion(IVsExpansionSession pSession)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeInsertion(IVsExpansionSession pSession)
        {
            return VSConstants.S_OK;
        }

        /// <summary>
        /// this is fired when one of our IronPython snippets has been selected by the user
        /// </summary>
        /// <param name="pszTitle"></param>
        /// <param name="pszPath"></param>
        /// <returns></returns>
        public int OnItemChosen(string pszTitle, string pszPath)
        {
            InsertCodeExpansion(new VsExpansion { path = pszPath, title = pszTitle });

            return VSConstants.S_OK;
        }

        public int PositionCaretForEditing(IVsTextLines pBuffer, TextSpan[] ts)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}