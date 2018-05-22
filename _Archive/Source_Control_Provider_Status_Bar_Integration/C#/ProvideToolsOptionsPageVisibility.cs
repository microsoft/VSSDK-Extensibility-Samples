/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Globalization;
using MsVsShell = Microsoft.VisualStudio.Shell;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.SccProvider
{
	/// <summary>
	/// This attribute registers the visibility of a Tools/Options property page.
    /// While Microsoft.VisualStudio.Shell allow registering a tools options page 
    /// using the ProvideOptionPageAttribute attribute, currently there is no better way
    /// of declaring the options page visibility, so a custom attribute needs to be used.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ProvideToolsOptionsPageVisibility : MsVsShell.RegistrationAttribute
	{
        private string _categoryName = null;
        private string _pageName = null;
        private Guid _commandUIGuid;
        
        /// <summary>
		/// </summary>
        public ProvideToolsOptionsPageVisibility(string categoryName, string pageName, string commandUIGuid)
		{
            _categoryName = categoryName;
            _pageName = pageName;
            _commandUIGuid = new Guid(commandUIGuid);
    	}

        /// <summary>
        /// The programmatic name for this category (non localized).
        /// </summary>
        public string CategoryName
        {
            get { return _categoryName; }
        }

        /// <summary>
        /// The programmatic name for this page (non localized).
        /// </summary>
        public string PageName
        {
            get { return _pageName; }
        }

        /// <summary>
        /// Get the command UI guid controlling the visibility of the page.
        /// </summary>
        public Guid CommandUIGuid
        {
            get { return _commandUIGuid; }
        }

        private string RegistryPath
        {
            get { return string.Format(CultureInfo.InvariantCulture, "ToolsOptionsPages\\{0}\\{1}\\VisibilityCmdUIContexts", CategoryName, PageName); }
        }

        /// <summary>
		///     Called to register this attribute with the given context.  The context
		///     contains the location where the registration inforomation should be placed.
		///     It also contains other information such as the type being registered and path information.
		/// </summary>
        public override void Register(RegistrationContext context)
		{
            // Write to the context's log what we are about to do
            context.Log.WriteLine(String.Format(CultureInfo.CurrentCulture, "Opt.Page Visibility:\t{0}\\{1}, {2}\n", CategoryName, PageName, CommandUIGuid.ToString("B")));

            // Create the visibility key.
            using (Key childKey = context.CreateKey(RegistryPath))
            {
                // Set the value for the command UI guid.
                childKey.SetValue(CommandUIGuid.ToString("B"), 1);
            }
		}

        /// <summary>
        /// Unregister this visibility entry.
        /// </summary>
        public override void Unregister(RegistrationContext context)
        {
            context.RemoveValue(RegistryPath, CommandUIGuid.ToString("B"));
        }
    }
}
