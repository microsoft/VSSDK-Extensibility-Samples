/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.PlatformUI;

using IServiceProvider = System.IServiceProvider;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.SccProvider
{
    /// <summary>
    /// Summary description for SccProviderToolWindow.
    /// </summary>
    [Guid("B0BAC05D-bbbb-42f2-8085-723ca3712763")]
    public class SccProviderToolWindow : ToolWindowPane
    {
        private SccProviderToolWindowControl control;

        public SccProviderToolWindow() :base(null)
        {
            // set the window title
            this.Caption = Resources.ResourceManager.GetString("ToolWindowCaption");

            // set the CommandID for the window ToolBar
            this.ToolBar = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.imnuToolWindowToolbarMenu);

            // set the icon for the frame
            this.BitmapResourceID = CommandId.ibmpToolWindowsImages;  // bitmap strip resource ID
            this.BitmapIndex = CommandId.iconSccProviderToolWindow;   // index in the bitmap strip

            control = new SccProviderToolWindowControl();
            // Initialize the toolwindow colors to respect the current theme
            SetDefaultColors();

            // Sign up to theme changes to keep the colors up to date
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }

        void SetDefaultColors()
        {
            Color defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            Color defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);

            UpdateWindowColors(defaultBackground, defaultForeground);
        }

        void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            SetDefaultColors();
        }

        override public IWin32Window Window
        {
            get
            {
                return (IWin32Window)control;
            }
        }

        /// <include file='doc\WindowPane.uex' path='docs/doc[@for="WindowPane.Dispose1"]' />
        /// <devdoc>
        ///     Called when this tool window pane is being disposed.
        /// </devdoc>
        override protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from theme changed events
                VSColorTheme.ThemeChanged -= VSColorTheme_ThemeChanged;

                if (control != null)
                {
                    try
                    {
                        if (control is IDisposable)
                            control.Dispose();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.Fail(String.Format("Failed to dispose {0} controls.\n{1}", this.GetType().FullName, e.Message));
                    }
                    control = null;
                } 
                
                IVsWindowFrame windowFrame = (IVsWindowFrame)this.Frame;
                if (windowFrame != null)
                {
                    // Note: don't check for the return code here.
                    windowFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_SaveIfDirty);
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// This function is only used to "do something noticeable" when the toolbar button is clicked.
        /// It is called from the package.
        /// A typical tool window may not need this function.
        /// 
        /// The current behavior change the background color of the control and swaps with the text color
        /// </summary>
        public void ToolWindowToolbarCommand()
        {
            Color defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            Color defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);

            if (this.control.BackColor == defaultBackground)
            {
                // Swap the colors
                UpdateWindowColors(defaultForeground, defaultBackground);
            }
            else
            {
                // Put back the default colors
                UpdateWindowColors(defaultBackground, defaultForeground);
            }
        }

        void UpdateWindowColors(Color clrBackground, Color clrForeground)
        {
            // Update the window background
            this.control.BackColor = clrBackground;
            this.control.ForeColor = clrForeground;

            // Also update the label
            foreach (Control child in this.control.Controls)
            {
                child.BackColor = this.control.BackColor;
                child.ForeColor = this.control.ForeColor;
            }
        }
    }
}
