/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

namespace Microsoft.Samples.VisualStudio.CodeSweep.Scanner
{
    /// <summary>
    /// A single hit on a specific term found during a scan.
    /// </summary>
    public interface IScanHit
    {
        /// <summary>
        /// Gets the full path of the file in which the term was found.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Gets the zero-based line number of the line on which the term was found.
        /// </summary>
        int Line { get; }

        /// <summary>
        /// Gets the zero-based column number of the character at which the term begins.
        /// </summary>
        int Column { get; }

        /// <summary>
        /// Gets the full text of the line on which the term was found.
        /// </summary>
        string LineText { get; }

        /// <summary>
        /// Gets the search term that was found.
        /// </summary>
        ISearchTerm Term { get; }

        /// <summary>
        /// Gets the warning associated with this search hit, or null if there is no warning.
        /// </summary>
        string Warning { get; }
    }
}
