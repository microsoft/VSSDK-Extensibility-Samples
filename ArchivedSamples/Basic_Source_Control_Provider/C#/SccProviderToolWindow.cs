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

//using IServiceProvider = System.IServiceProvider;
//using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.BasicSccProvider
{
    /// <summary>
    /// Summary description for SccProviderToolWindow.
    /// </summary>
    [Guid("ADC98052-bbbb-42f2-8085-723ca3712763")]
    public class SccProviderToolWindow : ToolWindowPane
    {
        private SccProviderToolWindowControl control;

        public SccProviderToolWindow() :base(null)
        {
            // set the window title
            Caption = Resources.ResourceManager.GetString("ToolWindowCaption");

            // set the CommandID for the window ToolBar
            ToolBar = new CommandID(GuidList.guidSccProviderCmdSet, CommandId.imnuToolWindowToolbarMenu);

            // set the icon for the frame
            BitmapResourceID = CommandId.ibmpToolWindowsImages;  // bitmap strip resource ID
            BitmapIndex = CommandId.iconSccProviderToolWindow;   // index in the bitmap strip

            control = new SccProviderToolWindowControl();

            // Initialize the toolwindow colors to respect the current theme
            SetDefaultColors();

            // Sign up to theme changes to keep the colors up to date
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;
        }
        
        /// <summary>
        /// Ensure the color scheme of the toolwindow matches the current Visual Studio theme
        /// </summary>
        void SetDefaultColors()
        {
            Color defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            Color defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
         
            UpdateWindowColors(defaultBackground, defaultForeground);
        }

        /// <summary>
        /// Called whenever the user changes the Visual Studio color scheme. 
        /// Calls SetDefaultColors()
        /// </summary>
        void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            SetDefaultColors();
        }

        /// <summary>
        /// Getter that returns the control for the Scc Provider Toolwindow
        /// </summary>
        override public IWin32Window Window
        {
            get
            {
                return control;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the ToolWindow and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
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
                        System.Diagnostics.Debug.Fail(string.Format("Failed to dispose {0} controls.\n{1}", GetType().FullName, e.Message));
                    }
                    control = null;
                } 
                
                IVsWindowFrame windowFrame = (IVsWindowFrame)Frame;
                if (windowFrame != null)
                {
                    // Note: don't check for the return code here.
                    windowFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_SaveIfDirty);
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Called whenever the toolbar button of the toolwindow is clicked.
        /// It is called from the package. A typical tool window may not need this function.
        /// Current behavior is to swap the text and background colors
        /// </summary>
        public void ToolWindowToolbarCommand()
        {
            Color defaultBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            Color defaultForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);

            if (control.BackColor == defaultBackground)
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

        /// <summary>
        /// Adjust the toolwindow color values. Adjusts the text color and text 
        /// area background color
        /// </summary>
        /// <param name="clrBackground">The desired color for the background of the text area</param>
        /// <param name="clrForeground">The desired text color</param>
        void UpdateWindowColors (Color clrBackground, Color clrForeground)
        {
            // Update the window background
            control.BackColor = clrBackground;
            control.ForeColor = clrForeground;

            // Also update the label
            foreach (Control child in control.Controls)
            {
                child.BackColor = control.BackColor;
                child.ForeColor = control.ForeColor;
            }
        }
    }
}
