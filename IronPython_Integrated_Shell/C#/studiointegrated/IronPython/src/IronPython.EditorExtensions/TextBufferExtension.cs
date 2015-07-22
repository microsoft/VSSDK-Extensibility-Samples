/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text;

namespace IronPython.EditorExtensions
{
    /// <summary>
    /// Provides extensions for <see cref="ITextBuffer"/>
    /// </summary>
    internal static class ITextBufferExtension
    {
        /// <summary>
        /// Returns the filename of the text buffer
        /// </summary>
        /// <param name="textBuffer"></param>
        /// <returns></returns>
        internal static string GetFileName(this ITextBuffer textBuffer)
        {
            if (textBuffer.Properties.ContainsProperty(typeof(ITextDocument)))
            {
                return textBuffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument)).FilePath;
            }

            return string.Empty;
        }
    }
}