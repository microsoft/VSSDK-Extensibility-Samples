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

namespace CaretFisheye
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Formatting;
    using Microsoft.VisualStudio.Utilities;

    /// <summary>
    /// This class implements a connector that produces the CaretFisheye LineTransformSourceProvider.
    /// </summary>
    [Export(typeof(ILineTransformSourceProvider))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class CaretFisheyeLineTransformSourceProvider : ILineTransformSourceProvider
    {
        public ILineTransformSource Create(IWpfTextView textView)
        {
            return CaretFisheyeLineTransformSource.Create(textView);
        }
    }
}

