//***************************************************************************
//
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//***************************************************************************

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace IntraTextAdornmentSample
{
    internal sealed class ColorAdornment : Button
    {
        private Rectangle rect;

        internal ColorAdornment(ColorTag colorTag)
        {
            rect = new Rectangle()
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Width = 20,
                Height = 10 
            };

            Update(colorTag);

            Content = rect;
        }

        private Brush MakeBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        internal void Update(ColorTag colorTag)
        {
            rect.Fill = MakeBrush(colorTag.Color);
        }
    }
}
