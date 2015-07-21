/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/
using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    /// <summary>
    /// This DynamicWindowPane demonstrate the following features:
    ///	 - Hosting a user control in a tool window
    ///	 - Dynamic visibility control by a UI context
    ///	 - Window events
    /// 
    /// Tool windows are composed of a frame (provided by Visual Studio) and a
    /// pane (provided by the package implementer). The frame implements
    /// IVsWindowFrame while the pane implements IVsWindowPane.
    /// 
    /// DynamicWindowPane inherits the IVsWindowPane implementation from its
    /// base class (ToolWindowPane). DynamicWindowPane will host a .NET
    /// UserControl (DynamicWindowControl). The Package base class will
    /// get the user control by asking for the Window property on this class.
    /// </summary>
    [Guid("F0E1E9A1-9860-484d-AD5D-367D79AABF55")]
    class DynamicWindowPane : ToolWindowPane
    {
        // Control that will be hosted in the tool window
        private DynamicWindowWPFControl control = null;
        // Caching our output window pane
        private IVsOutputWindowPane outputWindowPane = null;

        /// <summary>
        /// Constructor for ToolWindowPane.
        /// Initialization that depends on the package or that requires access
        /// to VS services should be done in OnToolWindowCreated.
        /// </summary>
        public DynamicWindowPane()
            : base(null)
        {
            // Set the image that will appear on the tab of the window frame when docked with another window.
            // KnownMonikers is a set of image monkiers that are globablly recognized by VS. These images can be
            // used in any project without needing to include the source image.
            BitmapImageMoniker = Microsoft.VisualStudio.Imaging.KnownMonikers.Search;

            // Creating the user control that will be displayed in the window
            control = new DynamicWindowWPFControl();
            Content = control;
        }

        /// <summary>
        /// This is called after our control has been created and sited.
        /// This is a good place to initialize the control with data gathered
        /// from Visual Studio services.
        /// </summary>
        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            PackageToolWindow package = (PackageToolWindow)Package;

            // Set the text that will appear in the title bar of the tool window.
            // Note that because we need access to the package for localization,
            // we have to wait to do this here. If we used a constant string,
            // we could do this in the constructor.
            this.Caption = package.GetResourceString("@110");

            // Register to the window events
            WindowStatus windowFrameEventsHandler = new WindowStatus(OutputWindowPane, Frame as IVsWindowFrame);
            ErrorHandler.ThrowOnFailure(((IVsWindowFrame)Frame).SetProperty((int)__VSFPROPID.VSFPROPID_ViewHelper, windowFrameEventsHandler));
            // Let our control have access to the window state
            control.CurrentState = windowFrameEventsHandler;

            DisplayInfoBar();
        }
        
        /// <summary>
        /// Retrieve the pane that should be used to output information.
        /// </summary>
        private IVsOutputWindowPane OutputWindowPane
        {
            get
            {
                if (outputWindowPane == null)
                {
                    // First make sure the output window is visible
                    IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                    // Get the frame of the output window
                    Guid outputWindowGuid = GuidsList.guidOutputWindowFrame;
                    IVsWindowFrame outputWindowFrame = null;
                    ErrorHandler.ThrowOnFailure(uiShell.FindToolWindow((uint)__VSCREATETOOLWIN.CTW_fForceCreate, ref outputWindowGuid, out outputWindowFrame));
                    // Show the output window
                    if (outputWindowFrame != null)
                        outputWindowFrame.Show();

                    // Get the output window service
                    IVsOutputWindow outputWindow = (IVsOutputWindow)GetService(typeof(SVsOutputWindow));
                    // The following GUID is a randomly generated one. This is to uniquely identify our output pane.
                    // It is best to change it to something else to avoid sharing it with someone else.
                    // If the goal is to share, then the same guid should be used, and the pane should only
                    // be created if it does not already exist.
                    Guid paneGuid = new Guid("{E6E69B7B-C898-4b0a-AEAB-C1961BC9E54E}");
                    // Create the pane
                    PackageToolWindow package = (PackageToolWindow)Package;
                    string paneName = package.GetResourceString("@111");
                    ErrorHandler.ThrowOnFailure(outputWindow.CreatePane(ref paneGuid, paneName, 1 /*visible=true*/, 0 /*clearWithSolution=false*/));
                    // Retrieve the pane
                    ErrorHandler.ThrowOnFailure(outputWindow.GetPane(ref paneGuid, out outputWindowPane));
                }

                return outputWindowPane;
            }
        }

        public void DisplayInfoBar()
        {
            InfoBarTextSpan textSpan1 = new InfoBarTextSpan("This is a sample info bar ");
            InfoBarHyperlink link1 = new InfoBarHyperlink("sample link1 ", Resources.InfoBarLinkActionContext1);
            InfoBarHyperlink link2 = new InfoBarHyperlink("sample link2 ", Resources.InfoBarLinkActionContext2);
            InfoBarButton button1 = new InfoBarButton("sample button1", Resources.InfoBarButtonActionContext1);
            InfoBarButton button2 = new InfoBarButton("sample button2", Resources.InfoBarButtonActionContext2);
            InfoBarTextSpan[] textSpanCollection = new InfoBarTextSpan[] { textSpan1, link1, link2 };
            InfoBarActionItem[] actionItemCollection = new InfoBarActionItem[] { button1, button2 };
            InfoBarModel infoBarModel = new InfoBarModel(textSpanCollection, actionItemCollection,
                KnownMonikers.StatusInformation, isCloseButtonVisible: true);
            
            this.AddInfoBar(infoBarModel);
            SubscribeToInfoBarEvents();
        }

        private void OnInfoBarClosed(object sender, InfoBarEventArgs args)
        {
            MessageBox.Show(string.Format("Closed"));
            UnsubscribeFromInfoBarEvents();
        }

        private void OnInfoBarActionItemClicked(object sender, InfoBarActionItemEventArgs args)
        {
            if (args.ActionItem.ActionContext == Resources.InfoBarButtonActionContext1 || args.ActionItem.ActionContext == Resources.InfoBarButtonActionContext2)
            {
                MessageBox.Show(string.Format("Button '{0}' is clicked", args.ActionItem.Text));
            }
            else if (args.ActionItem.ActionContext == Resources.InfoBarLinkActionContext1 || args.ActionItem.ActionContext == Resources.InfoBarLinkActionContext2)
            {
                MessageBox.Show(string.Format("Link '{0}' is clicked", args.ActionItem.Text));
            }
            else
            {
                MessageBox.Show("Unknow action");
            }
        }

        private void SubscribeToInfoBarEvents()
        {
            this.InfoBarActionItemClicked += OnInfoBarActionItemClicked;
            this.InfoBarClosed += OnInfoBarClosed;
        }

        private void UnsubscribeFromInfoBarEvents()
        {
            this.InfoBarActionItemClicked -= OnInfoBarActionItemClicked;
            this.InfoBarClosed -= OnInfoBarClosed;
        }
    }
}
