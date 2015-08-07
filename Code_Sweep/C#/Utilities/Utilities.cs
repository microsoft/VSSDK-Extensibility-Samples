/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.Samples.VisualStudio.CodeSweep.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Samples.VisualStudio.CodeSweep
{
    /// <summary>
    /// General utility methods.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Concatenates a collection of strings.
        /// </summary>
        /// <param name="inputs">The strings to concatenate.</param>
        /// <param name="separator">The separator text that will be placed in between the individual strings.</param>
        static public string Concatenate(IEnumerable<string> inputs, string separator)
        {
            StringBuilder result = new StringBuilder();

            foreach (string input in inputs)
            {
                if (result.Length > 0)
                {
                    result.Append(separator);
                }
                result.Append(input);
            }

            return result.ToString();
        }

        /// <summary>
        /// "Escapes" all instances of the specified character by inserting backslashes before
        /// them.  In addition, backslashes are transformed to double-backslashes.
        /// </summary>
        public static string EscapeChar(string text, char toEscape)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            StringBuilder result = new StringBuilder();

            int spanStart = 0;

            char[] chars = new char[] { toEscape, '\\' };

            for (int spanStop = text.IndexOfAny(chars, spanStart); spanStop >= 0; spanStop = text.IndexOfAny(chars, spanStart))
            {
                result.Append(text.Substring(spanStart, spanStop - spanStart));
                result.Append("\\");
                result.Append(text[spanStop]);

                spanStart = spanStop + 1;
            }

            result.Append(text.Substring(spanStart));

            return result.ToString();
        }

        /// <summary>
        /// Splits a string into several fields.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        /// <remarks>
        /// Instances of <c>separator</c> alone are treated as field separators.  Escaped instances
        /// of <c>separator</c> (prefixed by backslashes) are unescaped, as are double-backslashes.
        /// </remarks>
        public static IList<string> ParseEscaped(string text, char separator)
        {
            List<string> result = new List<string>();

            StringBuilder current = new StringBuilder();

            char[] chars = new char[] { separator, '\\' };

            int spanStart = 0;

            for (int spanStop = text.IndexOfAny(chars, spanStart); spanStop >= 0; spanStop = text.IndexOfAny(chars, spanStart))
            {
                current.Append(text.Substring(spanStart, spanStop - spanStart));
                if (text[spanStop] == separator)
                {
                    // This is a separator on its own, since it would already have been dealt with
                    // if it had been preceeded by an escape operator.
                    result.Add(current.ToString());
                    current.Length = 0;
                }
                else
                {
                    // We found an instance of the escape operator, '\'
                    if (spanStop + 1 < text.Length)
                    {
                        if (text[spanStop + 1] == separator)
                        {
                            // An escaped separator is transformed into a non-escaped separator.
                            current.Append(separator);
                            ++spanStop;
                        }
                        else if (text[spanStop + 1] == '\\')
                        {
                            // A double-escape is transformed into the escape operator.
                            current.Append('\\');
                            ++spanStop;
                        }
                    }
                }

                spanStart = spanStop + 1;
            }

            if (spanStart < text.Length)
            {
                current.Append(text.Substring(spanStart));
            }

            if (current.Length > 0)
            {
                result.Add(current.ToString());
            }

            return result;
        }

        /// <summary>
        /// Transforms a relative path to an absolute one based on a specified base folder.
        /// </summary>
        static public string AbsolutePathFromRelative(string relativePath, string baseFolderForDerelativization)
        {
            if (relativePath == null)
            {
                throw new ArgumentNullException("relativePath");
            }
            if (baseFolderForDerelativization == null)
            {
                throw new ArgumentNullException("baseFolderForDerelativization");
            }
            if (Path.IsPathRooted(relativePath))
            {
                throw new ArgumentException(Resources.PathNotRelative, "relativePath");
            }
            if (!Path.IsPathRooted(baseFolderForDerelativization))
            {
                throw new ArgumentException(Resources.BaseFolderMustBeRooted, "baseFolderForDerelativization");
            }

            StringBuilder result = new StringBuilder(baseFolderForDerelativization);

            if (result[result.Length - 1] != Path.DirectorySeparatorChar)
            {
                result.Append(Path.DirectorySeparatorChar);
            }

            int spanStart = 0;

            while (spanStart < relativePath.Length)
            {
                int spanStop = relativePath.IndexOf(Path.DirectorySeparatorChar, spanStart);

                if (spanStop == -1)
                {
                    spanStop = relativePath.Length;
                }

                string span = relativePath.Substring(spanStart, spanStop - spanStart);

                if (span == "..")
                {
                    // The result string should end with a directory separator at this point.  We
                    // want to search for the one previous to that, which is why we subtract 2.
                    int previousSeparator;
                    if (result.Length < 2 || (previousSeparator = result.ToString().LastIndexOf(Path.DirectorySeparatorChar, result.Length - 2)) == -1)
                    {
                        throw new ArgumentException(Resources.BackTooFar);
                    }
                    result.Remove(previousSeparator + 1, result.Length - previousSeparator - 1);
                }
                else if (span != ".")
                {
                    // Ignore "." because it means the current direcotry
                    result.Append(span);

                    if (spanStop < relativePath.Length)
                    {
                        result.Append(Path.DirectorySeparatorChar);
                    }

                }

                spanStart = spanStop + 1;
            }

            return result.ToString();
        }

        /// <summary>
        /// Enumerates over a collection of rooted file paths, creating a new collection containing the relative versions.
        /// </summary>
        /// <remarks>
        /// If any of the paths cannot be relativized (because it does not have the same root as
        /// the base path), the absolute version is added to the collection that's returned.
        /// </remarks>
        public static List<string> RelativizePathsIfPossible(IEnumerable<string> absolutePaths, string basePath)
        {
            List<string> relativePaths = new List<string>();

            foreach (string absolutePath in absolutePaths)
            {
                if (CanRelativize(absolutePath, basePath))
                {
                    relativePaths.Add(RelativePathFromAbsolute(absolutePath, basePath));
                }
                else
                {
                    relativePaths.Add(absolutePath);
                }
            }

            return relativePaths;
        }

        private static bool CanRelativize(string absolutePath, string basePath)
        {
            if (absolutePath == null)
            {
                throw new ArgumentNullException("pathToRelativize");
            }
            if (basePath == null)
            {
                throw new ArgumentNullException("basePath");
            }

            if (!Path.IsPathRooted(absolutePath) || !Path.IsPathRooted(basePath))
            {
                throw new ArgumentException(Resources.BothMustBeRooted);
            }

            return string.Compare(Path.GetPathRoot(absolutePath), Path.GetPathRoot(basePath), StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Transforms an absolute path to a relative one based on a specified base folder.
        /// </summary>
        public static string RelativePathFromAbsolute(string pathToRelativize, string basePath)
        {
            if (pathToRelativize == null)
            {
                throw new ArgumentNullException("pathToRelativize");
            }
            if (basePath == null)
            {
                throw new ArgumentNullException("basePath");
            }

            if (!Path.IsPathRooted(pathToRelativize) || !Path.IsPathRooted(basePath))
            {
                throw new ArgumentException(Resources.BothMustBeRooted);
            }

            if (string.Compare(Path.GetPathRoot(pathToRelativize), Path.GetPathRoot(basePath), StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new ArgumentException(Resources.BothMustHaveSameRoot);
            }

            // remove the ending "\" to simplify the algorithm below
            basePath = basePath.TrimEnd(Path.DirectorySeparatorChar);

            string commonBase = FindCommonBasePath(pathToRelativize, basePath, true);

            if (commonBase.Length == basePath.Length)
            {
                string result = pathToRelativize.Substring(commonBase.Length);

                if (result[0] == Path.DirectorySeparatorChar)
                {
                    result = result.Substring(1, result.Length - 1);
                }
                return result;
            }
            else
            {
                int backOutCount = CountInstances(basePath.Substring(commonBase.Length), Path.DirectorySeparatorChar);
                string result = Duplicate(".." + Path.DirectorySeparatorChar, backOutCount) + pathToRelativize.Substring(commonBase.Length + 1);
                return result;
            }
        }

        /// <summary>
        /// Duplicates a specified string a specified number of times.
        /// </summary>
        public static string Duplicate(string text, int count)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            StringBuilder result = new StringBuilder(text.Length * count);

            for (int i = 0; i < count; ++i)
            {
                result.Append(text);
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns the number of instances of a given character in a string.
        /// </summary>
        public static int CountInstances(string text, char toFind)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            int result = 0;

            foreach (char c in text)
            {
                if (c == toFind)
                {
                    ++result;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the longest string <c>first</c> and <c>second</c> have in common beginning at index 0.
        /// </summary>
        public static string FindCommonBasePath(string first, string second, bool ignoreCase)
        {
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }

            string[] parts1 = first.Split(new char[] { Path.DirectorySeparatorChar });
            string[] parts2 = second.Split(new char[] { Path.DirectorySeparatorChar });

            int length = 0;

            for (; length < parts1.Length && length < parts2.Length; ++length)
            {
                if (ignoreCase)
                {
                    if (string.Compare(parts1[length], parts2[length], StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        break;
                    }
                }
                else
                {
                    if (string.Compare(parts1[length], parts2[length], StringComparison.Ordinal) != 0)
                    {
                        break;
                    }
                }
            }

            if (length == 0)
            {
                // nothing in common
                return string.Empty;
            }

            return string.Join(char.ToString(Path.DirectorySeparatorChar), parts1, startIndex: 0, count: length);
        }

        public static bool UnorderedCollectionsAreEqual<T>(ICollection<T> first, ICollection<T> second)
        {
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }

            if (first.Count != second.Count)
            {
                return false;
            }

            foreach (T item in first)
            {
                if (!second.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool OrderedCollectionsAreEqual<T>(IList<T> first, IList<T> second)
        {
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }

            if (first.Count != second.Count)
            {
                return false;
            }

            for (int i = 0; i < first.Count; ++i)
            {
                if (second.IndexOf(first[i]) != i)
                {
                    return false;
                }
            }

            return true;
        }

        public static string EncodeProgramFilesVar(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            if (path.StartsWith(programFiles, StringComparison.OrdinalIgnoreCase))
            {
                return "$(ProgramFiles)" + path.Substring(programFiles.Length);
            }
            else
            {
                return path;
            }
        }

        public const int RemotingChannel = 9000;

        public static string GetRemotingUri(int procId, bool includeLocalHostPrefix)
        {
            if (includeLocalHostPrefix)
            {
                return string.Format("tcp://localhost:{0}/ScannerHost-{1}", RemotingChannel, procId);
            }
            else
            {
                return string.Format("ScannerHost-{0}", procId);
            }
        }
    }
}
