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
    /// Factory for publicly visible types in the Scanner component.
    /// </summary>
    public static class Factory
    {
        /// <summary>
        /// Creates an IScanner object.
        /// </summary>
        public static IScanner GetScanner()
        {
            if (_scanner == null)
            {
                _scanner = new Scanner();
            }
            return _scanner;
        }

        /// <summary>
        /// Creates an ITermTable object for the term table stored in a given file.
        /// </summary>
        /// <param name="filePath">The file containing the XML specification for a term table.</param>
        public static ITermTable GetTermTable(string filePath)
        {
            return new TermTable(filePath);
        }

        #region Private Members

        static IScanner _scanner;

        #endregion Private Members
    }
}
