/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.BasicSccProvider
{
    /// <summary>
    /// This class is used to expose the list of the IDs of the commands implemented
    /// by the client package. This list of IDs must match the set of IDs defined inside the
    /// VSCT file.
    /// </summary>
    static class CommandId
    {
        // Define the list a set of public static members.
        public const int icmdSccCommand = 0x101;
        public const int icmdViewToolWindow = 0x102;
        public const int icmdToolWindowToolbarCommand = 0x103;

        // Define the list of menus (these include toolbars)
        public const int imnuToolWindowToolbarMenu = 0x200;

        // Define the list of icons (use decimal numbers here, to match the resource IDs)
        public const int iiconProductIcon = 400;

        // Define the list of bitmaps (use decimal numbers here, to match the resource IDs)
        public const int ibmpToolWindowsImages = 501;

        // Glyph indexes in the bitmap used for toolwindows (ibmpToolWindowsImages)
        public const int iconSccProviderToolWindow = 0;
    }
}
