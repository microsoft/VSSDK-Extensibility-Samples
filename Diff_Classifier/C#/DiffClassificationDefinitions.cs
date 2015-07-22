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

namespace DiffClassifier
{
    using System.ComponentModel.Composition;
    using System.Windows.Media;
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Utilities;

    internal static class DiffClassificationDefinitions
    {
        #region Content Type and File Extension Definitions

        [Export]
        [Name("diff")]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition diffContentTypeDefinition = null;

        [Export]
        [FileExtension(".diff")]
        [ContentType("diff")]
        internal static FileExtensionToContentTypeDefinition diffFileExtensionDefinition = null;

        [Export]
        [FileExtension(".patch")]
        [ContentType("diff")]
        internal static FileExtensionToContentTypeDefinition patchFileExtensionDefinition = null;

        #endregion

        #region Classification Type Definitions

        [Export]
        [Name("diff")]
        internal static ClassificationTypeDefinition diffClassificationDefinition = null;

        [Export]
        [Name("diff.added")]
        [BaseDefinition("diff")]
        internal static ClassificationTypeDefinition diffAddedDefinition = null;

        [Export]
        [Name("diff.removed")]
        [BaseDefinition("diff")]
        internal static ClassificationTypeDefinition diffRemovedDefinition = null;

        [Export]
        [Name("diff.changed")]
        [BaseDefinition("diff")]
        internal static ClassificationTypeDefinition diffChangedDefinition = null;

        [Export]
        [Name("diff.infoline")]
        [BaseDefinition("diff")]
        internal static ClassificationTypeDefinition diffInfolineDefinition = null;

        [Export]
        [Name("diff.patchline")]
        [BaseDefinition("diff")]
        internal static ClassificationTypeDefinition diffPatchLineDefinition = null;

        [Export]
        [Name("diff.header")]
        [BaseDefinition("diff")]
        internal static ClassificationTypeDefinition diffHeaderDefinition = null;

        #endregion

        #region Classification Format Productions

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "diff.added")]
        [Name("diff.added")]
        internal sealed class DiffAddedFormat : ClassificationFormatDefinition
        {
            public DiffAddedFormat()
            {
                ForegroundColor = Colors.Blue;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "diff.removed")]
        [Name("diff.removed")]
        internal sealed class DiffRemovedFormat : ClassificationFormatDefinition
        {
            public DiffRemovedFormat()
            {
                ForegroundColor = Colors.Red;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "diff.changed")]
        [Name("diff.changed")]
        internal sealed class DiffChangedFormat : ClassificationFormatDefinition
        {
            public DiffChangedFormat()
            {
                ForegroundColor = Color.FromRgb(0xCC, 0x60, 0x10);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "diff.infoline")]
        [Name("diff.infoline")]
        internal sealed class DiffInfolineFormat : ClassificationFormatDefinition
        {
            public DiffInfolineFormat()
            {
                ForegroundColor = Color.FromRgb(0x44, 0xBB, 0xBB);
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "diff.patchline")]
        [Name("diff.patchline")]
        internal sealed class DiffPatchLineFormat : ClassificationFormatDefinition
        {
            public DiffPatchLineFormat()
            {
                ForegroundColor = Colors.Goldenrod;
            }
        }

        [Export(typeof(EditorFormatDefinition))]
        [ClassificationType(ClassificationTypeNames = "diff.header")]
        [Name("diff.header")]
        internal sealed class DiffHeaderFormat : ClassificationFormatDefinition
        {
            public DiffHeaderFormat()
            {
                ForegroundColor = Color.FromRgb(0, 0xBB, 0);
            }
        }
        #endregion
    }
}
