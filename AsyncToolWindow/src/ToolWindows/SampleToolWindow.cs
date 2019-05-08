using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;

namespace AsyncToolWindowSample.ToolWindows
{
    [Guid(WindowGuidString)]
    public class SampleToolWindow : ToolWindowPane
    {
        public const string WindowGuidString = "e4e2ba26-a455-4c53-adb3-8225fb696f8b"; // Replace with new GUID in your own code
        public const string Title = "Sample Tool Window";

        // "state" parameter is the object returned from MyPackage.InitializeToolWindowAsync
        public SampleToolWindow(SampleToolWindowState state) : base()
        {
            Caption = Title;
            BitmapImageMoniker = KnownMonikers.ImageIcon;

            Content = new SampleToolWindowControl(state);
        }
    }
}
