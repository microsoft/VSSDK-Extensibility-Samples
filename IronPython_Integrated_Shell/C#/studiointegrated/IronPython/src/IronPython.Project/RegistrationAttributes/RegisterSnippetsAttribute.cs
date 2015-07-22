/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{
    using System;
    using System.Globalization;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    ///     This attribute registers code snippets for a package.  The attributes on a 
    ///     package do not control the behavior of the package, but they can be used by registration 
    ///     tools to register the proper information with Visual Studio.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    // Disable the "IdentifiersShouldNotHaveIncorrectSuffix" warning.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711")]
    public sealed class RegisterSnippetsAttribute : RegistrationAttribute
    {

        private Guid _languageGuid;
        private bool _showRoots;
        private short _displayName;
        private string _languageStringId;
        private string _indexPath;
        private string _forceCreateDirectories;
        private string _paths;

        /// <summary>
        ///     Creates a new RegisterSnippetsAttribute.
        /// </summary>
        // Disable the "AvoidTypeNamesInParameters" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720")]
        public RegisterSnippetsAttribute(string languageGuid, bool showRoots, short displayName,
                                          string languageStringId, string indexPath,
                                          string forceCreateDirectories, string paths)
        {
            _languageGuid = new Guid(languageGuid);
            _showRoots = showRoots;
            _displayName = displayName;
            _languageStringId = languageStringId;
            _indexPath = indexPath;
            _forceCreateDirectories = forceCreateDirectories;
            _paths = paths;
        }

        /// <summary>
        ///     Returns the language guid.
        /// </summary>
        public string LanguageGuid
        {
            get { return _languageGuid.ToString("B"); }
        }

        /// <summary>
        ///     Returns true if roots are shown.
        /// </summary>
        public bool ShowRoots
        {
            get { return _showRoots; }
        }

        /// <summary>
        ///     Returns string ID corresponding to the language name.
        /// </summary>
        public short DisplayName
        {
            get { return _displayName; }
        }

        /// <summary>
        ///     Returns the string to use for the language name.
        /// </summary>
        public string LanguageStringId
        {
            get { return _languageStringId; }
        }

        /// <summary>
        ///     Returns the relative path to the snippet index file.
        /// </summary>
        public string IndexPath
        {
            get { return _indexPath; }
        }

        /// <summary>
        ///     Returns the paths to create.
        /// </summary>
        public string ForceCreateDirectories
        {
            get { return _forceCreateDirectories; }
        }

        /// <summary>
        ///     Returns the paths to look for snippets.
        /// </summary>
        public string Paths
        {
            get { return _paths; }
        }

        /// <summary>
        ///        The reg key name of the project.
        /// </summary>
        private string LanguageName()
        {
            return string.Format(CultureInfo.InvariantCulture, "Languages\\CodeExpansions\\{0}", LanguageStringId);
        }

        /// <summary>
        ///     Called to register this attribute with the given context.
        /// </summary>
        /// <param name="context">
        ///     Contains the location where the registration information should be placed.
        ///     It also contains other informations as the type being registered and path information.
        /// </param>
        // Disable the "NormalizeStringsToUppercase" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308")]
        public override void Register(RegistrationContext context)
        {
            if (context == null)
            {
                return;
            }
            using (Key childKey = context.CreateKey(LanguageName()))
            {
                childKey.SetValue("", LanguageGuid);

                string snippetIndexPath = context.ComponentPath;
                snippetIndexPath = System.IO.Path.Combine(snippetIndexPath, IndexPath);
                snippetIndexPath = context.EscapePath(System.IO.Path.GetFullPath(snippetIndexPath));

                childKey.SetValue("DisplayName", DisplayName.ToString(CultureInfo.InvariantCulture));
                childKey.SetValue("IndexPath", snippetIndexPath);
                childKey.SetValue("LangStringId", LanguageStringId.ToLowerInvariant());
                childKey.SetValue("Package", context.ComponentType.GUID.ToString("B"));
                childKey.SetValue("ShowRoots", ShowRoots ? 1 : 0);

                string snippetPaths = context.ComponentPath;
                snippetPaths = System.IO.Path.Combine(snippetPaths, ForceCreateDirectories);
                snippetPaths = context.EscapePath(System.IO.Path.GetFullPath(snippetPaths));

                //The following enables VS to look into a user directory for more user-created snippets
                string myDocumentsPath = @";%MyDocs%\Code Snippets\" + "Python" + @"\My Code Snippets\";
                using (Key forceSubKey = childKey.CreateSubkey("ForceCreateDirs"))
                {
                    forceSubKey.SetValue(LanguageStringId, snippetPaths + myDocumentsPath);
                }

                using (Key pathsSubKey = childKey.CreateSubkey("Paths"))
                {
                    pathsSubKey.SetValue(LanguageStringId, snippetPaths + myDocumentsPath);
                }
            }
        }

        /// <summary>
        ///     Called to unregister this attribute with the given context.
        /// </summary>
        /// <param name="context">
        ///     Contains the location where the registration information should be placed.
        ///     It also contains other informations as the type being registered and path information.
        /// </param>
        public override void Unregister(RegistrationContext context)
        {
            if (context != null)
            {
                context.RemoveKey(LanguageName());
            }
        }
    }
}

