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
    /// Delegate type for a callback from <c>IScanner.Scan</c> which is called after each file scan
    /// completes.
    /// </summary>
    /// <param name="result">The result of the scan of a single file.</param>
    public delegate void FileScanCompleted(IScanResult result);

    /// <summary>
    /// Delegate type for an argument to <c>IScanner.Scan</c> which is called to get the text of a
    /// file instead of reading it from disk.
    /// </summary>
    /// <param name="filePath">The full path of the file to get.</param>
    /// <returns>The text of the file, or null if it is not provided by this delegate and should be read from disk.</returns>
    public delegate string FileContentGetter(string filePath);

    /// <summary>
    /// Delegate type for an argument to <c>IScanner.Scan</c> which is called to determine whether
    /// the scan should be aborted before it is finished.
    /// </summary>
    /// <returns>True if the scan should be aborted immediately, false otherwise.</returns>
    public delegate bool ScanStopper();

    /// <summary>
    /// Performs the scanning process.
    /// </summary>
    public interface IScanner
    {
        /// <summary>
        /// Scans a collection of files for terms defined in a collection of term tables.
        /// </summary>
        /// <param name="filePaths">The full paths of the files to scan.</param>
        /// <param name="termTables">The term tables containing the search terms to scan for.</param>
        /// <returns>The result of the scan.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>filePaths</c> or <c>termTables</c> is null.</exception>
        IMultiFileScanResult Scan(IEnumerable<string> filePaths, IEnumerable<ITermTable> termTables);

        /// <summary>
        /// Scans a collection of files for terms defined in a collection of term tables, and calls a callback delegate after each file is scanned.
        /// </summary>
        /// <param name="filePaths">The full paths of the files to scan.</param>
        /// <param name="termTables">The term tables containing the search terms to scan for.</param>
        /// <param name="callback">The delegate to be called after each file is scanned; may be null.</param>
        /// <returns>The result of the scan.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>filePaths</c> or <c>termTables</c> is null.</exception>
        IMultiFileScanResult Scan(IEnumerable<string> filePaths, IEnumerable<ITermTable> termTables, FileScanCompleted callback);

        /// <summary>
        /// Scans a collection of files for terms defined in a collection of term tables, and calls a callback delegate after each file is scanned.
        /// </summary>
        /// <param name="filePaths">The full paths of the files to scan.</param>
        /// <param name="termTables">The term tables containing the search terms to scan for.</param>
        /// <param name="callback">The delegate to be called after each file is scanned; may be null.</param>
        /// <param name="contentGetter">The delegate which may be called to provide the text of a file instead of reading it from disk; may be null.</param>
        /// <returns>The result of the scan.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>filePaths</c> or <c>termTables</c> is null.</exception>
        IMultiFileScanResult Scan(IEnumerable<string> filePaths, IEnumerable<ITermTable> termTables, FileScanCompleted callback, FileContentGetter contentGetter);

        /// <summary>
        /// Scans a collection of files for terms defined in a collection of term tables, and calls a callback delegate after each file is scanned.
        /// </summary>
        /// <param name="filePaths">The full paths of the files to scan.</param>
        /// <param name="termTables">The term tables containing the search terms to scan for.</param>
        /// <param name="callback">The delegate to be called after each file is scanned; may be null.</param>
        /// <param name="contentGetter">The delegate which may be called to provide the text of a file instead of reading it from disk; may be null.</param>
        /// <param name="stopper">The delegate which is called frequently during processing to determine if the scan should be aborted; may be null.</param>
        /// <returns>The result of the scan.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>filePaths</c> or <c>termTables</c> is null.</exception>
        IMultiFileScanResult Scan(IEnumerable<string> filePaths, IEnumerable<ITermTable> termTables, FileScanCompleted callback, FileContentGetter contentGetter, ScanStopper stopper);
    }
}
