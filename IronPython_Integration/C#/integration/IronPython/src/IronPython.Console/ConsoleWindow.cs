/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Package;

using Microsoft.Samples.VisualStudio.IronPython.Interfaces;
using IronPython.Hosting;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using IServiceProvider = System.IServiceProvider;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Composition.Hosting;
using System.Windows.Forms;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using IronPython.EditorExtensions;

namespace Microsoft.Samples.VisualStudio.IronPython.Console
{
    /// <summary>
    /// This is the class that implements the tool window that will host the console.
    /// The console is implemented as a text view control (provided by the shell) hosted
    /// inside this tool window; the text inside the control is handled by a text buffer
    /// object (also provided by the shell).
    /// </summary>
    [Guid("5f50e2df-8fd8-4a07-ac79-829ee3dc7c7c")]
    internal class ConsoleWindow : ToolWindowPane, IConsoleText
    {
        IWpfTextView _textView;
        ITextBuffer mefTextBuffer;
        IWpfTextViewHost _textViewHost;

        // The Stream object built on top of the text buffer.
        private TextBufferStream textStream;
        // The text view used to visualize the text inside the console
        private IVsTextView textView;
        // The service provider used to access the global services
        private System.IServiceProvider globalProvider;

        // This command service is used to hide the one created in the base class because
        // there is no way to add a parent command target to it, so we will have to create
        // a new one and return it in our version of GetService.
        private OleMenuCommandService commandService;

        // List of lines that the user has typed on the console and not returned yet by
        // the ReadLine method
        private CommandBuffer inputBuffer;
        // Buffer of commands for the history.
        private HistoryBuffer history;

        /// <summary>
        /// Standard constructor for the console window.
        /// This constructor will use as global service provider the one exposed by the package class
        /// and will use it to create and initialize the text buffer.
        /// </summary>
        public ConsoleWindow() :
            this(new ServiceProvider((IOleServiceProvider)PythonConsolePackage.GetGlobalService(typeof(IOleServiceProvider))))
        {}

        /// <summary>
        /// Creates a new ConsoleWindow object.
        /// This constructor uses the service provider passed as an argument to create and initialize
        /// the text buffer.
        /// </summary>
        public ConsoleWindow(IServiceProvider provider) :
            base(null)
        {
            if (null == provider)
                throw new ArgumentNullException("provider");
            globalProvider = provider;
        }

        ICompletionBroker completionBroker;

        protected override void Initialize()
        {
            base.Initialize();

            IComponentModel compMod = GetService(typeof(SComponentModel)) as IComponentModel;
            ITextBufferFactoryService bufferFactory = compMod.GetService<ITextBufferFactoryService>();
            ITextEditorFactoryService editorFactory = compMod.GetService<ITextEditorFactoryService>();
            IVsEditorAdaptersFactoryService adapterFactory = compMod.GetService<IVsEditorAdaptersFactoryService>();
            IContentTypeRegistryService registryService = compMod.GetService<IContentTypeRegistryService>();

            completionBroker = compMod.GetService<ICompletionBroker>();

            textView  = adapterFactory.CreateVsTextViewAdapter(GetService(typeof(IOleServiceProvider)) as IOleServiceProvider);
            IVsTextBuffer textBuffer = adapterFactory.CreateVsTextBufferAdapter(GetService(typeof(IOleServiceProvider)) as IOleServiceProvider);
            uint textViewInitFlags = (uint)TextViewInitFlags.VIF_DEFAULT
                    | (uint)TextViewInitFlags.VIF_HSCROLL
                    | (uint)TextViewInitFlags.VIF_VSCROLL;
            textBuffer.InitializeContent("", 0);
            textView.Initialize(textBuffer as IVsTextLines, IntPtr.Zero, textViewInitFlags, null);

            // Create Dev10 objects
            _textView = adapterFactory.GetWpfTextView(textView);
            mefTextBuffer = adapterFactory.GetDataBuffer(textBuffer);
            
            IVsUserData userData = textView as IVsUserData;
            if (userData != null)
            {
                Guid g = Microsoft.VisualStudio.Editor.DefGuidList.guidIWpfTextViewHost;
                object obj;
                int hr = userData.GetData(ref g, out obj);
                if (hr == VSConstants.S_OK)
                {
                    _textViewHost = obj as IWpfTextViewHost;
                }
            }


            //Initialize the history
            history = new HistoryBuffer();

            // Create the stream on top of the text buffer.
            textStream = new TextBufferStream(mefTextBuffer);

            // Initialize the engine.
            InitializeEngine();

            IContentType ipContentType = registryService.GetContentType(PyContentTypeDefinition.ConsoleContentType);
            mefTextBuffer.ChangeContentType(ipContentType, null);

            adapterFactory.GetWpfTextView(textView).Caret.MoveTo(new SnapshotPoint(mefTextBuffer.CurrentSnapshot, mefTextBuffer.CurrentSnapshot.Length));

            // Set the title of the window.
            this.Caption = Resources.ToolWindowTitle;

            // Set the icon of the toolwindow.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 0;

            return;
        }

        /// <summary>
        /// Performs the clean-up operations for this object.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // Dispose the stream.
                    if (null != textStream)
                    {
                        ((IDisposable)textStream).Dispose();
                        textStream = null;
                    }

                    // Close the text view.
                    if (null != textView)
                    {
                        // Remove the command filter.
                        textView.RemoveCommandFilter((IOleCommandTarget)this);
                        // Release the text view.
                        textView.CloseView();
                        textView = null;
                    }

                    // Dispose the command service.
                    if (null != commandService)
                    {
                        commandService.Dispose();
                        commandService = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Thread function used to run the IronPython engine in a different thread.
        /// This thread will run until the IConsole implementation of this window will return
        /// null from the ReadLine method.
        /// </summary>
        private void InitializeEngine()
        {
            // Get the engine provider service to set this console window as the console
            // object associated with the shared engine.
            IPythonEngineProvider engineProvider = (IPythonEngineProvider)globalProvider.GetService(typeof(IPythonEngineProvider));
            if (null != engineProvider)
            {
                IEngine engine = engineProvider.GetSharedEngine();
                engine.StdErr = textStream;
                engine.StdOut = textStream;
                string version = string.Format(System.Globalization.CultureInfo.CurrentCulture,
                                               Resources.EngineVersionFormat,
                                               engine.Version.Major,
                                               engine.Version.Minor,
                                               engine.Version.Build);

                // Write engine version end copyright on the console.
                using (StreamWriter writer = new StreamWriter(textStream as Stream))
                {
                    writer.WriteLine(version);
                    writer.WriteLine(engine.Copyright);
                }

                // Create the buffer that will handle the commands to the engine.
                inputBuffer = new CommandBuffer(engine);
            }
        }

        /// <summary>
        /// Set the cursor at the end of the current buffer and, if needed, scrolls the text
        /// view so that the cursor is visible.
        /// </summary>
        private void SetCursorAtEndOfBuffer()
        {
            // If the text view is not created, then there is no reason to set the cursor.
            if (null != _textView)
            {
                _textView.Caret.MoveTo(new SnapshotPoint(mefTextBuffer.CurrentSnapshot, mefTextBuffer.CurrentSnapshot.Length));
            }
        }

        /// <summary>
        /// Return the service of the given type.
        /// This override is needed to be able to use a different command service from the one
        /// implemented in the base class.
        /// </summary>
        protected override object GetService(Type serviceType)
        {
            if ((typeof(IOleCommandTarget) == serviceType) || 
                (typeof(System.ComponentModel.Design.IMenuCommandService) == serviceType))
            {
                if (null != commandService)
                {
                    return commandService;
                }
            }
            return base.GetService(serviceType);
        }

        /// <summary>
        /// Function called when the window frame is set on this tool window.
        /// </summary>
        public override void OnToolWindowCreated()
        {
            // Call the base class's implementation.
            base.OnToolWindowCreated();

            // Register this object as command filter for the text view so that it will
            // be possible to intercept some command.
            IOleCommandTarget originalFilter;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                textView.AddCommandFilter((IOleCommandTarget)this, out originalFilter));
            // Create a command service that will use the previous command target
            // as parent target and will route to it the commands that it can not handle.
            if (null == originalFilter)
            {
                commandService = new OleMenuCommandService(this);
            }
            else
            {
                commandService = new OleMenuCommandService(this, originalFilter);
            }

            // Add the command handler for RETURN.
            CommandID id = new CommandID(
                                typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                                (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.RETURN);
            OleMenuCommand cmd = new OleMenuCommand(new EventHandler(OnReturn), id);
            cmd.BeforeQueryStatus += new EventHandler(UnsupportedOnCompletion);
            commandService.AddCommand(cmd);

            // Command handler for UP and DOWN arrows. These commands are needed to implement
            // the history in the console, but at the moment the implementation is empty.
            id = new CommandID(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                               (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.UP);
            cmd = new OleMenuCommand(new EventHandler(OnHistory), id);
            cmd.BeforeQueryStatus += new EventHandler(SupportCommandOnInputPosition);
            commandService.AddCommand(cmd);
            id = new CommandID(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                               (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.DOWN);
            cmd = new OleMenuCommand(new EventHandler(OnHistory), id);
            cmd.BeforeQueryStatus += new EventHandler(SupportCommandOnInputPosition);
            commandService.AddCommand(cmd);

            // Command handler for the LEFT arrow. This command handler is needed in order to
            // avoid that the user uses the left arrow to move to the previous line or over the
            // command prompt.
            id = new CommandID(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                               (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.LEFT);
            cmd = new OleMenuCommand(new EventHandler(OnNoAction), id);
            cmd.BeforeQueryStatus += new EventHandler(OnBeforeMoveLeft);
            commandService.AddCommand(cmd);

            // Handle also the HOME command.
            id = new CommandID(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                               (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL);
            cmd = new OleMenuCommand(new EventHandler(OnHome), id);
            commandService.AddCommand(cmd);

            id = new CommandID(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                               (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.BOL_EXT);
            cmd = new OleMenuCommand(new EventHandler(OnShiftHome), id);
            cmd.BeforeQueryStatus += new EventHandler(SupportCommandOnInputPosition);
            commandService.AddCommand(cmd);

            // Adding support for "Clear Pane" command.
            id = new CommandID(typeof(Microsoft.VisualStudio.VSConstants.VSStd97CmdID).GUID,
                               (int)Microsoft.VisualStudio.VSConstants.VSStd97CmdID.ClearPane);
            cmd = new OleMenuCommand(new EventHandler(OnClearPane), id);
            commandService.AddCommand(cmd);

            // Add a command handler for the context menu.
            id = new CommandID(typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID,
                               (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.SHOWCONTEXTMENU);
            cmd = new OleMenuCommand(new EventHandler(ShowContextMenu), id);
            commandService.AddCommand(cmd);

            // Now we set the key binding for this frame to the same value as the text editor
            // so that there will be the same mapping for the commands.
            Guid commandUiGuid = VSConstants.GUID_TextEditorFactory;
          ((IVsWindowFrame)Frame).SetGuidProperty((int)__VSFPROPID.VSFPROPID_InheritKeyBindings, ref commandUiGuid);
        }

        /// <summary>
        /// Return true if the user is currently on the input line.
        /// Here we assume that the input line is always the last one.
        /// </summary>
        private bool IsCurrentLineInputLine()
        {
            return mefTextBuffer.CurrentSnapshot.GetLineFromLineNumber(mefTextBuffer.CurrentSnapshot.LineCount - 1).ExtentIncludingLineBreak.Contains(_textView.Caret.Position.BufferPosition - 1);
        }

        /// <summary>
        /// Returns true if the current position is inside the writable section of the buffer.
        /// </summary>
        private bool IsCurrentPositionInputPosition()
        {
            return !mefTextBuffer.IsReadOnly(_textView.Caret.Position.BufferPosition.Position);
        }

        public string TextOfLine(int line, int endColumn, bool skipReadOnly)
        {
            var mefLine = mefTextBuffer.CurrentSnapshot.GetLineFromLineNumber(line);
            int start = 0;
            if (mefTextBuffer.IsReadOnly(mefLine.Extent.Span))
            {
                start = GetReadOnlyLength(mefTextBuffer.CurrentSnapshot) - mefLine.Start;
            }
            return mefLine.GetText().Substring(start, endColumn);
        }

        #region Command Handlers
        /// <summary>
        /// Set the Supported property on the sender command to true if and only if the
        /// current position of the cursor is an input position.
        /// </summary>
        private void SupportCommandOnInputPosition(object sender, EventArgs args)
        {
            // Check if the sender is a MenuCommand.
            MenuCommand command = sender as MenuCommand;
            if (null == command)
            {
                // This should never happen, but let's handle it just in case.
                return;
            }
        }
        /// <summary>
        /// Command handler for the history commands.
        /// The standard implementation of a console has a history function implemented when
        /// the user presses the UP or DOWN key.
        /// </summary>
        private void OnHistory(object sender, EventArgs e)
        {
            if (!completionBroker.IsCompletionActive(_textView))
            {
                // Get the command to figure out from the ID if we have to get the previous or the
                // next element in the history.
                OleMenuCommand command = sender as OleMenuCommand;
                if (null == command ||
                    command.CommandID.Guid != typeof(Microsoft.VisualStudio.VSConstants.VSStd2KCmdID).GUID)
                {
                    return;
                }
                string historyEntry = null;
                if (command.CommandID.ID == (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.UP)
                {
                    historyEntry = history.PreviousEntry();
                }
                else if (command.CommandID.ID == (int)Microsoft.VisualStudio.VSConstants.VSStd2KCmdID.DOWN)
                {
                    historyEntry = history.NextEntry();
                }
                if (string.IsNullOrEmpty(historyEntry))
                {
                    return;
                }

                int start = GetReadOnlyLength(mefTextBuffer.CurrentSnapshot);
                if (!mefTextBuffer.EditInProgress)
                {
                    var edit = mefTextBuffer.CreateEdit();
                    edit.Replace(new Span(start, mefTextBuffer.CurrentSnapshot.Length - start), historyEntry);
                    edit.Apply();
                }
            }
        }

        /// <summary>
        /// Set the status of the command to Unsupported when the completion window is visible.
        /// </summary>
        private void UnsupportedOnCompletion(object sender, EventArgs args)
        {
            MenuCommand command = sender as MenuCommand;
            if (null == command)
            {
                return;
            }
        }

        /// <summary>
        /// Handles the HOME command in two different ways if the current line is the input
        /// line or not.
        /// </summary>
        private void OnHome(object sender, EventArgs e)
        {
            if (IsCurrentLineInputLine())
            {
                _textView.Caret.MoveTo(new SnapshotPoint(mefTextBuffer.CurrentSnapshot, GetReadOnlyLength(mefTextBuffer.CurrentSnapshot)));
            }
            else
            {
                _textView.Caret.MoveTo(_textView.Caret.ContainingTextViewLine.Start);
            }
        }

        /// <summary>
        /// Overwrite the default 'Shift' + 'HOME' to limit the selection to the input section
        /// of the buffer.
        /// </summary>
        private void OnShiftHome(object sender, EventArgs args)
        {
            SnapshotPoint start;
            if (IsCurrentLineInputLine())
            {
                start = new SnapshotPoint(mefTextBuffer.CurrentSnapshot, GetReadOnlyLength(mefTextBuffer.CurrentSnapshot));
            }
            else
            {
                start = _textView.Caret.ContainingTextViewLine.Start;
            }
            _textView.Selection.Select(new SnapshotSpan(start, _textView.Caret.Position.BufferPosition), true);
        }

        /// <summary>
        /// Determines whether it is possible to move left on the current line.
        /// It is used to avoid a situation where the user moves over the console's prompt.
        /// </summary>
        private void OnBeforeMoveLeft(object sender, EventArgs e)
        {
            // Verify that the sender is of the expected type.
            OleMenuCommand command = sender as OleMenuCommand;
            if (null == command)
            {
                return;
            }
            // As default we don't want to handle this command because it should be handled
            // by the dafault implementation of the text view.
            command.Supported = false;

            if (IsCurrentLineInputLine())
            {
                if (_textView.Caret.Position.BufferPosition.Position <= GetReadOnlyLength(mefTextBuffer.CurrentSnapshot))
                {
                    command.Supported = true;
                }
            }
        }
        /// <summary>
        /// Empty command handler used to overwrite some standard command with an empty action.
        /// </summary>
        private void OnNoAction(object sender, EventArgs e)
        {
            // Do Nothing.
        }
        /// <summary>
        /// Command handler for the RETURN command.
        /// It is called when the user presses the ENTER key inside the console window and
        /// is used to execute the text as an IronPython expression.
        /// </summary>
        private void OnReturn(object sender, EventArgs e)
        {
            if (!completionBroker.IsCompletionActive(_textView))
            {

                // If the user is not on the input line, then this should be a no-action.
                if (!IsCurrentLineInputLine())
                {
                    return;
                }

                ExecuteUserInput();

                SetCursorAtEndOfBuffer();
            }
        }

        private void ExecuteUserInput()
        {
            int readOnlyLength = GetReadOnlyLength(mefTextBuffer.CurrentSnapshot);
            string text = mefTextBuffer.CurrentSnapshot.GetText(readOnlyLength, mefTextBuffer.CurrentSnapshot.Length - readOnlyLength);
            if (text != string.Empty)
            {
                history.AddEntry(text);
            }

            // Add the text to the buffer. Note that the text is always added to this buffer,
            // but it is added to the history only if it is not empty.
            if (null != inputBuffer)
            {
                inputBuffer.Add(text);
            }
        }

        private int GetReadOnlyLength(ITextSnapshot textSnapshot)
        {
            int max = 0;
            foreach (var region in textSnapshot.TextBuffer.GetReadOnlyExtents(new Span(0, textSnapshot.Length)))
            {
                max = max < region.End ? region.End : max;
            }
            return max;
        }

        /// <summary>
        /// Function called when the user select the "Clear Pane" menu item from the context menu.
        /// This will clear the content of the console window leaving only the console cursor and
        /// resizing the read-only region.
        /// </summary>
        private void OnClearPane(object sender, EventArgs args)
        {
            if (!mefTextBuffer.EditInProgress)
            {
                textStream.ClearReadOnlyRegion();
                SetCursorAtEndOfBuffer();
            }
        }

        private void ShowContextMenu(object sender, EventArgs args)
        {
            // Get a reference to the UIShell.
            IVsUIShell uiShell = globalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (null == uiShell)
            {
                return;
            }

            // Get the position of the cursor.
            System.Drawing.Point pt = System.Windows.Forms.Cursor.Position;
            POINTS[] pnts = new POINTS[1];
            pnts[0].x = (short)pt.X;
            pnts[0].y = (short)pt.Y;

            // Show the menu.
            Guid menuGuid = ConsoleGuidList.guidIronPythonConsoleCmdSet;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                uiShell.ShowContextMenu(0, ref menuGuid, (int)PkgCmdIDList.IPConsoleContextMenu, pnts, textView as IOleCommandTarget));
        }
        #endregion

        System.Windows.Controls.UserControl uc = null;

        public override object Content
        {
            get
            {
                if (uc == null)
                {
                    uc = new System.Windows.Controls.UserControl();
                    uc.Content = _textViewHost.HostControl;
                }

                return uc;
            }
        }
    }
}
