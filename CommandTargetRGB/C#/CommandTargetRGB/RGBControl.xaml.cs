/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System.Windows;
using System.Windows.Controls;

namespace Microsoft.CommandTargetRGB
{
    public enum RGBControlColor
    {
        Red,
        Green,
        Blue,
    }

    /// <summary>
    /// Interaction logic for RGBControl.xaml
    /// </summary>
    public partial class RGBControl : UserControl
    {
        public RGBControl()
        {
            InitializeComponent();
        }

        public static DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(RGBControlColor), typeof(RGBControl), new FrameworkPropertyMetadata(RGBControlColor.Red));

        public RGBControlColor Color
        {
            get
            {
                return ((RGBControlColor)(this.GetValue(RGBControl.ColorProperty)));
            }
            set
            {
                this.SetValue(RGBControl.ColorProperty, value);
            }
        }

        // Allow the tool window to create the toolbar tray.  Set its style and
        // add it to the grid.
        public void SetTray(ToolBarTray tray)
        {
            tray.Style = FindResource("ToolBarTrayStyle") as Style;
            grid.Children.Add(tray);
        }
    }
}