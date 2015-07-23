/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Microsoft.Samples.VisualStudio.CodeSweep.Scanner
{
    class Scanner : IScanner
    {
        /// <summary>
        /// See <c>IScanner</c> documentation.
        /// </summary>
        public IMultiFileScanResult Scan(IEnumerable<string> filePaths, IEnumerable<ITermTable> termTables)
        {
            return Scan(filePaths, termTables, null);
        }

        /// <summary>
        /// See <c>IScanner</c> documentation.
        /// </summary>
        public IMultiFileScanResult Scan(IEnumerable<string> filePaths, IEnumerable<ITermTable> termTables, FileScanCompleted callback)
        {
            return Scan(filePaths, termTables, callback, null);
        }

        /// <summary>
        /// See <c>IScanner</c> documentation.
        /// </summary>
        public IMultiFileScanResult Scan(IEnumerable<string> filePaths, IEnumerable<ITermTable> termTables, FileScanCompleted callback, FileContentGetter contentGetter)
        {
            return Scan(filePaths, termTables, callback, contentGetter, null);
        }

        /// <summary>
        /// See <c>IScanner</c> documentation.
        /// </summary>
        public IMultiFileScanResult Scan(IEnumerable<string> filePaths, IEnumerable<ITermTable> termTables, FileScanCompleted callback, FileContentGetter contentGetter, ScanStopper stopper)
        {
            if (filePaths == null)
            {
                throw new ArgumentNullException("filePaths");
            }
            if (termTables == null)
            {
                throw new ArgumentNullException("termTables");
            }

            MatchFinder finder = new MatchFinder(termTables);

            MultiFileScanResult allResults = new MultiFileScanResult();

            foreach (string filePath in filePaths)
            {
                if (stopper != null && stopper())
                {
                    break;
                }

                if (FileShouldBeScanned(filePath))
                {
                    IScanResult fileResult = ScanFile(filePath, finder, contentGetter, stopper);
                    allResults.Append(fileResult);
                    if (callback != null)
                    {
                        callback(fileResult);
                    }
                }
            }

            return allResults;
        }

        #region Private Members

        private static bool FileShouldBeScanned(string filePath)
        {
            string extension;

            try
            {
                extension = Path.GetExtension(filePath);
            }
            catch (ArgumentException)
            {
                // Path.GetExtension can't parse file paths that are invalid, but we still want to
                // send them to the scanner, so we'll try to manually parse the extension.
                int lastDot = filePath.LastIndexOf('.');
                int lastWhack = filePath.LastIndexOf('\\');

                if (lastDot < 0 || lastDot < lastWhack)
                {
                    extension = string.Empty;
                }
                else
                {
                    extension = filePath.Substring(lastDot, filePath.Length - lastDot);
                }
            }

            return GetAllowedExtensions().Any(
                item => string.Compare(extension, "." + item, StringComparison.OrdinalIgnoreCase) == 0);
        }

        private static List<string> GetAllowedExtensions()
        {
            XmlDocument document = new XmlDocument();
            document.Load(Globals.AllowedExtensionsPath);

            List<string> result = new List<string>();

            foreach (XmlNode node in document.SelectNodes("allowedextensions/extension"))
            {
                result.Add(node.InnerText);
            }

            return result;
        }

        private static IScanResult ScanFile(string filePath, MatchFinder finder, FileContentGetter contentGetter, ScanStopper stopper)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            // See if the content getter can give us the file contents.  If so, we'll scan that
            // string rather than loading the file from disk.
            if (contentGetter != null)
            {
                string content = contentGetter(filePath);
                if (content != null)
                {
                    return ScanResult.ScanOccurred(filePath, GetScanHits(filePath, content, finder, stopper));
                }
            }

            StreamReader reader = null;

            try
            {
                try
                {
                    reader = File.OpenText(filePath);
                }
                catch (Exception ex)
                {
                    if (ex is UnauthorizedAccessException ||
                        ex is ArgumentException ||
                        ex is ArgumentNullException ||
                        ex is PathTooLongException ||
                        ex is DirectoryNotFoundException ||
                        ex is FileNotFoundException ||
                        ex is NotSupportedException ||
                        ex is IOException)
                    {
                        return ScanResult.ScanNotPossible(filePath);
                    }
                    else
                    {
                        throw;
                    }
                }

                return ScanResult.ScanOccurred(filePath, GetScanHits(filePath, reader, finder, stopper));
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        private static IEnumerable<IScanHit> GetScanHits(string filePath, StreamReader reader, MatchFinder finder, ScanStopper stopper)
        {
            List<IScanHit> hits = new List<IScanHit>();

            MatchFoundCallback callback =
                (term, line, column, lineText, warning) => hits.Add(new ScanHit(filePath, line, column, lineText, term, warning));

            finder.Reset();

            while (!reader.EndOfStream)
            {
                if (stopper != null && stopper())
                {
                    break;
                }
                finder.AnalyzeNextCharacter((char)reader.Read(), callback);
            }

            finder.Finish(callback);

            return hits;
        }

        private static IEnumerable<IScanHit> GetScanHits(string filePath, string content, MatchFinder finder, ScanStopper stopper)
        {
            List<IScanHit> hits = new List<IScanHit>();

            MatchFoundCallback callback =
                (term, line, column, lineText, warning) => hits.Add(new ScanHit(filePath, line, column, lineText, term, warning));

            finder.Reset();

            foreach (char c in content)
            {
                if (stopper != null && stopper())
                {
                    break;
                }
                finder.AnalyzeNextCharacter(c, callback);
            }

            finder.Finish(callback);

            return hits;
        }

        #endregion Private Members
    }
}
