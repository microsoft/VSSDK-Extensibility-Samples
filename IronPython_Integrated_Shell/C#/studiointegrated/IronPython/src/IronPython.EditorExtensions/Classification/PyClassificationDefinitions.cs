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
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using System.Windows.Media;

namespace IronPython.EditorExtensions
{
    /// <summary>
    /// Classification definitions
    /// </summary>
    internal class PyClassificationDefinitions
    {
        [Name(PyClassificationTypes.ReadOnlyRegion), Export]
        internal ClassificationTypeDefinition PythonReadOnlyRegionClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = PyClassificationTypes.ReadOnlyRegion)]
        [Name("PythonReadOnlyRegionFormatDefinition")]
        [Order]
        internal sealed class PythonReadOnlyRegionClassificationFormat : ClassificationFormatDefinition
        {
            internal PythonReadOnlyRegionClassificationFormat()
            {
                BackgroundColor = Colors.LightGray;
                this.DisplayName = "Python Read Only Region";
            }
        }

        [Name(PyClassificationTypes.Comment), Export]
        internal ClassificationTypeDefinition CommentClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = PyClassificationTypes.Comment)]
        [Name("PythonCommentFormatDefinition")]
        [Order]
        internal sealed class CommentClassificationFormat : ClassificationFormatDefinition
        {
            internal CommentClassificationFormat()
            {
                ForegroundColor = Colors.Green;
                this.DisplayName = "Python Comment";
            }
        }

        [Name(PyClassificationTypes.Delimiter), Export]
        internal ClassificationTypeDefinition DelimiterClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = PyClassificationTypes.Delimiter)]
        [Name("PythonDelimiterFormatDefinition")]
        [Order]
        internal sealed class DelimiterClassificationFormat : ClassificationFormatDefinition
        {
            public DelimiterClassificationFormat()
            {
                this.DisplayName = "Python Delimiter";
            }
        }

        [Name(PyClassificationTypes.Operator), Export]
        internal ClassificationTypeDefinition OperatorClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = PyClassificationTypes.Operator)]
        [Name("PythonOperatorFormatDefinition")]
        [Order]
        internal sealed class OperatorClassificationFormat : ClassificationFormatDefinition
        {
            public OperatorClassificationFormat()
            {
                this.DisplayName = "Python Operator";
            }
        }

        [Name(PyClassificationTypes.Keyword), Export]
        internal ClassificationTypeDefinition KeywordClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = PyClassificationTypes.Keyword)]
        [Name("PythonKeywordFormatDefinition")]
        [Order]
        internal sealed class KeywordClassificationFormat : ClassificationFormatDefinition
        {
            internal KeywordClassificationFormat()
            {
                ForegroundColor = Colors.Blue;
                this.DisplayName = "Python Keyword";
            }
        }

        [Name(PyClassificationTypes.Identifier), Export]
        internal ClassificationTypeDefinition IdentifierClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = PyClassificationTypes.Identifier)]
        [Name("PythonIdentifierFormatDefinition")]
        [Order]
        internal sealed class IdentifierClassificationFormat : ClassificationFormatDefinition
        {
            public IdentifierClassificationFormat()
            {
                this.DisplayName = "Python Identifier";
            }
        }

        [Name(PyClassificationTypes.String), Export]
        internal ClassificationTypeDefinition StringClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = PyClassificationTypes.String)]
        [Name("PythonStringFormatDefinition")]
        [Order]
        internal sealed class StringClassificationFormat : ClassificationFormatDefinition
        {
            internal StringClassificationFormat()
            {
                ForegroundColor = Colors.Brown;
                this.DisplayName = "Python String";
            }
        }

        [Name(PyClassificationTypes.Number), Export]
        internal ClassificationTypeDefinition NumberClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = PyClassificationTypes.Number)]
        [Name("PythonNumberFormatDefinition")]
        [Order]
        internal sealed class NumberClassificationFormat : ClassificationFormatDefinition
        {
            public NumberClassificationFormat()
            {
                this.DisplayName = "Python Number";
            }
        }

        [Name(PyClassificationTypes.Unknown), Export]
        internal ClassificationTypeDefinition UnknownClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = PyClassificationTypes.Number)]
        [Name("PythonUnknownFormatDefinition")]
        [Order]
        internal sealed class UnknownClassificationFormat : ClassificationFormatDefinition
        {
            public UnknownClassificationFormat()
            {
                this.DisplayName = "Python Unknown";
            }
        }
    }
}