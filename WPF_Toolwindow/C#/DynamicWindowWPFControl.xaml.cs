/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
    /// <summary>
    /// Interaction logic for DynamicWindowWPFControl.xaml
    /// </summary>
    public partial class DynamicWindowWPFControl : UserControl
    {
        public DynamicWindowWPFControl()
        {
            InitializeComponent();
        }

        private WindowStatus currentState = null;
		/// <summary>
		/// This is the object that will keep track of the state of the IVsWindowFrame
		/// that is hosting this control. The pane should set this property once
		/// the frame is created to enable us to stay up to date.
		/// </summary>
		public WindowStatus CurrentState
		{
			get { return currentState; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");
				currentState = value;
				// Subscribe to the change notification so we can update our UI
				currentState.StatusChange += new EventHandler<EventArgs>(RefreshValues);
				// Update the display now
				RefreshValues(this, null);
			}
		}

		/// <summary>
		/// This method is the call back for state changes events
		/// </summary>
		/// <param name="sender">Event senders</param>
		/// <param name="arguments">Event arguments</param>
		private void RefreshValues(object sender, EventArgs arguments)
		{
            xText.Text = currentState.X.ToString(CultureInfo.CurrentCulture);
            yText.Text = currentState.Y.ToString(CultureInfo.CurrentCulture);
            widthText.Text = currentState.Width.ToString(CultureInfo.CurrentCulture);
            heightText.Text = currentState.Height.ToString(CultureInfo.CurrentCulture);
			dockedCheckBox.IsChecked = currentState.IsDockable;
            InvalidateVisual();
		}

        private void InvertColors(object sender, RoutedEventArgs e)
        {
            Brush temp;
            temp = Background;
            Background = Foreground;
            Foreground = temp;
        }
    }
}
