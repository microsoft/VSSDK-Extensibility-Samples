/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System.Collections.Generic;

namespace Microsoft.Samples.VisualStudio.CodeSweep.Scanner
{
    /// <summary>
    /// The result of a scan across zero or more files.
    /// </summary>
    public interface IMultiFileScanResult
    {
        /// <summary>
        /// Gets the number of files CodeSweep attempted to scan.
        /// </summary>
        int Attempted { get; }

        /// <summary>
        /// Gets the number of files for which no search hits were found.
        /// </summary>
        int PassedScan { get; }

        /// <summary>
        /// Gets the number of files for which one or more search hits were found.
        /// </summary>
        int FailedScan { get; }

        /// <summary>
        /// Gets the number of files which could not be opened for scanning.
        /// </summary>
        int UnableToScan { get; }

        /// <summary>
        /// Gets the search hits that were found by the scan.
        /// </summary>
        IEnumerable<IScanResult> Results { get; }
    }
}
