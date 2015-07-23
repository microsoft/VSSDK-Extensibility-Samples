/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;

namespace Microsoft.Samples.VisualStudio.CodeSweep.BuildTask
{
    /// <summary>
    /// A representation of a specific instance of a term which should be ignored.
    /// </summary>
    public interface IIgnoreInstance
    {
        /// <summary>
        /// Gets the full path of the file in which this instance occurs.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Gets the full text of the line on which this instance occurs.
        /// </summary>
        string IgnoredLine { get; }

        /// <summary>
        /// Gets the column at which the instance begins, relative to the first non-whitespace character on the line.
        /// </summary>
        int PositionOfIgnoredTerm { get; }

        /// <summary>
        /// Gets the text of the term which is ignored.
        /// </summary>
        string IgnoredTerm { get; }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <param name="projectFolderForRelativization">Used to convert the file path to a relative path.</param>
        string Serialize(string projectFolderForRelativization);
    }
}
