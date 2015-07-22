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

using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace OokLanguage
{
    #region Format definition
    /// <summary>
    /// Defines the editor format for the ookExclamation classification type. Text is colored BlueViolet
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "ook!")]
    [Name("ook!")]
    //this should be visible to the end user
    [UserVisible(false)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class OokE : ClassificationFormatDefinition
    {
        /// <summary>
        /// Defines the visual format for the "exclamation" classification type
        /// </summary>
        public OokE()
        {
            DisplayName = "ook!"; //human readable version of the name
            ForegroundColor = Colors.BlueViolet;
        }
    }

    /// <summary>
    /// Defines the editor format for the ookQuestion classification type. Text is colored Green
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "ook?")]
    [Name("ook?")]
    //this should be visible to the end user
    [UserVisible(false)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class OokQ : ClassificationFormatDefinition
    {
        /// <summary>
        /// Defines the visual format for the "question" classification type
        /// </summary>
        public OokQ()
        {
            DisplayName = "ook?"; //human readable version of the name
            ForegroundColor = Colors.Green;
        }
    }

    /// <summary>
    /// Defines the editor format for the ookPeriod classification type. Text is colored Orange
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "ook.")]
    [Name("ook.")]
    //this should be visible to the end user
    [UserVisible(false)]
    //set the priority to be after the default classifiers
    [Order(Before = Priority.Default)]
    internal sealed class OokP : ClassificationFormatDefinition
    {
        /// <summary>
        /// Defines the visual format for the "period" classification type
        /// </summary>
        public OokP()
        {
            DisplayName = "ook."; //human readable version of the name
            ForegroundColor = Colors.Orange;
        }
    }
    #endregion //Format definition
}
