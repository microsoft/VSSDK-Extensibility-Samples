/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    /// <summary>
    /// This class keeps track of the position, size and dockable state of the
    /// window it is associated with. By registering an instance of this class
    /// with a window frame (this can be a tool window or a document window)
    /// Visual Studio will call back the IVsWindowFrameNotify3 methods when
    /// changes occur.
    /// </summary>
    public sealed class WindowStatus : IVsWindowFrameNotify3
    {
        // Private fields to keep track of the last known state
        private int x = 0;
        private int y = 0;
        private int width = 0;
        private int height = 0;
        private bool dockable = false;
        // Output window service
        IVsOutputWindowPane outputPane = null;
        // IVsWindowFrame associated with this status monitor
        IVsWindowFrame frame;

        #region Public properties
        /// <summary>
        /// Return the current horizontal position of the window
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public int X
        {
            get { return x; }
        }
        /// <summary>
        /// Return the current vertical position of the window
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public int Y
        {
            get { return y; }
        }
        /// <summary>
        /// Return the current width of the window
        /// </summary>
        public int Width
        {
            get { return width; }
        }
        /// <summary>
        /// Return the current height of the window
        /// </summary>
        public int Height
        {
            get { return height; }
        }
        /// <summary>
        /// Is the window dockable
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public bool IsDockable
        {
            get { return dockable; }
        }

        /// <summary>
        /// Event that gets fired when the position or the docking state of the window changes
        /// </summary>
        public event EventHandler<EventArgs> StatusChange;

        #endregion

        /// <summary>
        /// WindowStatus Constructor.
        /// </summary>
        /// <param name="outputWindowPane">Events will be reported in the output pane if this interface is provided.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702")]
        public WindowStatus(IVsOutputWindowPane outputWindowPane, IVsWindowFrame frame)
        {
            outputPane = outputWindowPane;
            this.frame = frame;

            if (frame != null)
            {
                VSSETFRAMEPOS[] pos = new VSSETFRAMEPOS[1];
                int x;
                int y;
                int width;
                int height;
                Guid unused;
                frame.GetFramePos(pos, out unused, out x, out y, out width, out height);
                dockable = (pos[0] & VSSETFRAMEPOS.SFP_fFloat) != VSSETFRAMEPOS.SFP_fFloat;
            }
        }

        #region IVsWindowFrameNotify3 Members
        /// <summary>
        /// This is called when the window is being closed
        /// </summary>
        /// <param name="pgrfSaveOptions">Should the document be saved and should the user be prompted.</param>
        /// <returns>HRESULT</returns>
        public int OnClose(ref uint pgrfSaveOptions)
        {
            if (outputPane != null)
                return outputPane.OutputString("  IVsWindowFrameNotify3.OnClose()\n");
            else
                return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        /// <summary>
        /// This is called when a window "dock state" changes. This could be the
        /// result of dragging the window to result in the dock state changing
        /// or this could be as a result of changing the dock style (tabbed, mdi,
        /// dockable, floating,...).
        /// This will likely also result in a different position/size
        /// </summary>
        /// <param name="fDockable">Is the window dockable with an other window</param>
        /// <param name="x">New horizontal position</param>
        /// <param name="y">New vertical position</param>
        /// <param name="w">New width</param>
        /// <param name="h">New Height</param>
        /// <returns>HRESULT</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "x")]
        public int OnDockableChange(int fDockable, int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            width = w;
            height = h;
            dockable = (fDockable != 0);

            GenerateStatusChangeEvent(this, new EventArgs());

            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        /// <summary>
        /// This is called when the window is moved
        /// </summary>
        /// <param name="x">New horizontal position</param>
        /// <param name="y">New vertical position</param>
        /// <param name="w">New width</param>
        /// <param name="h">New Height</param>
        /// <returns>HRESULT</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "y"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "x")]
        public int OnMove(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            width = w;
            height = h;

            GenerateStatusChangeEvent(this, new EventArgs());

            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        /// <summary>
        /// This is called when the window is shown or hidden
        /// </summary>
        /// <param name="fShow">State of the window</param>
        /// <returns>HRESULT</returns>
        public int OnShow(int fShow)
        {
            __FRAMESHOW state = (__FRAMESHOW)fShow;
            if (outputPane != null)
                return outputPane.OutputString(string.Format(CultureInfo.CurrentCulture, "  IVsWindowFrameNotify3.OnShow({0})\n", state.ToString()));
            else
                return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        /// <summary>
        /// This is called when the window is resized
        /// </summary>
        /// <param name="x">New horizontal position</param>
        /// <param name="y">New vertical position</param>
        /// <param name="w">New width</param>
        /// <param name="h">New Height</param>
        /// <returns>HRESULT</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "x"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "y")]
        public int OnSize(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            width = w;
            height = h;

            GenerateStatusChangeEvent(this, new EventArgs());

            return Microsoft.VisualStudio.VSConstants.S_OK;
        }
        

        #endregion

        /// <summary>
        /// Generate the event if someone is listening to it
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="arguments">Event arguments</param>
        private void GenerateStatusChangeEvent(object sender, EventArgs arguments)
        {
            if (StatusChange != null)
                StatusChange.Invoke(this, new EventArgs());
        }
    }
}
