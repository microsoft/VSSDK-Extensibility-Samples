/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.IO;
using System.Reflection;

namespace Microsoft.Samples.VisualStudio.CodeSweep
{
    /// <summary>
    /// Global constants.
    /// </summary>
    public static class Globals
    {
        /// <summary>
        /// Gets the full path of the CodeSweep folder under the current user's ApplicationData
        /// folder where per-user files are stored.
        /// </summary>
        public static string AppDataFolder
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft\\CodeSweep");
            }
        }

        /// <summary>
        /// The file name (no path) of the allowed extensions list.
        /// </summary>
        public const string AllowedExtensionsFileName = "extensions.xml";

        /// <summary>
        /// Gets the full path of the allowed extensions list.
        /// </summary>
        public static string AllowedExtensionsPath
        {
            get
            {
                return Path.Combine(AppDataFolder, AllowedExtensionsFileName);
            }
        }

        /// <summary>
        /// The file name (no path) of the default term table.
        /// </summary>
        public const string DefaultTermTableFileName = "sample_term_table.xml";

        /// <summary>
        /// Gets the full path of the default term table.
        /// </summary>
        public static string DefaultTermTablePath
        {
            get
            {
                return Path.Combine(AppDataFolder, DefaultTermTableFileName);
            }
        }

        public static string DefaultTermTableInstallPath
        {
            get
            {
                string expected = Path.Combine(InstallationFolder, DefaultTermTableFileName);

                if (File.Exists(expected))
                {
                    return expected;
                }
                else
                {
                    // Maybe this is not the installed product, but rather the VS SDK project.
                    // Look for the dll two levels up from our current folder.
                    return Utilities.AbsolutePathFromRelative(Path.Combine("..\\..", DefaultTermTableFileName), InstallationFolder);
                }
            }
        }

        public static string AllowedExtensionsInstallPath
        {
            get
            {
                string expected = Path.Combine(InstallationFolder, AllowedExtensionsFileName);

                if (File.Exists(expected))
                {
                    return expected;
                }
                else
                {
                    // Maybe this is not the installed product, but rather the VS SDK project.
                    // Look for the dll two levels up from our current folder.
                    return Utilities.AbsolutePathFromRelative(Path.Combine("..\\..", AllowedExtensionsFileName), InstallationFolder);
                }
            }
        }

        #region Private Members

        private static string InstallationFolder
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetLoadedModules()[0].FullyQualifiedName);
            }
        }

        #endregion Private Members
    }
}
