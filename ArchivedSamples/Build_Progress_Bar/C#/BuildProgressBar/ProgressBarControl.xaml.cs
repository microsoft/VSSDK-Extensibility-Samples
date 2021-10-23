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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Microsoft.BuildProgressBar
{
    /// <summary>
    /// Interaction logic for ProgressBarControl.xaml
    /// </summary>
    public partial class ProgressBarControl : UserControl
    {
        private static Color brightGreen = Color.FromArgb(0xFF, 0x01, 0xD3, 0x28);

        private bool animateColor = false;

        public ProgressBarControl()
        {
            InitializeComponent();

            // Animate from Blue to bright green by default
            StartColor = Colors.Blue;
            EndColor = brightGreen;
        }

        public Color StartColor { get; set; }
        public Color EndColor { get; set; }

        public bool AnimateColor
        {
            get
            {
                return animateColor;
            }
            set
            {
                animateColor = value;

                // Create or remove a drop shadow effect
                if (animateColor && progressBar.Effect == null)
                    progressBar.Effect = new DropShadowEffect();
                else if (!animateColor && progressBar.Effect != null)
                    progressBar.Effect = null;
            }
        }

        // Get/set the progress bar value
        public double Value
        {
            get
            {
                return progressBar.Value;
            }
            set
            {
                if (AnimateColor)
                {
                    progressBar.Foreground = new SolidColorBrush(Lerp(StartColor, EndColor, value));
                }
                else
                {
                    progressBar.Foreground = new SolidColorBrush(brightGreen);
                }

                progressBar.Value = value;
            }
        }

        // Get/set the progress bar text
        public string Text
        {
            get
            {
                return barText.Text;
            }
            set
            {
                barText.Text = value;
            }
        }

        // Calculate a linear interpolation between two colors
        private Color Lerp(Color first, Color second, double percentage)
        {
            byte a = (byte)((second.A - first.A) * percentage + first.A);
            byte r = (byte)((second.R - first.R) * percentage + first.R);
            byte g = (byte)((second.G - first.G) * percentage + first.G);
            byte b = (byte)((second.B - first.B) * percentage + first.B);

            return Color.FromArgb(a, r, g, b);
        }

        private void ProgressToolWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustSize();
        }

        private void ProgressToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustSize();
        }

        /// <summary>
        /// Adjust width and height to match tool window
        /// </summary>
        private void AdjustSize()
        {
            progressBar.Width = ActualWidth - 24;
            progressBar.Height = Math.Max(10, Math.Min(48, ActualHeight - 24));
            viewbox.Width = progressBar.Width;
        }
    }
}