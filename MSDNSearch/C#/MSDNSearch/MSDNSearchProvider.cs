/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Runtime.InteropServices;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.MSDNSearch
{
    /// <summary>
    ///  Search Provider for MSDN Library
    ///  GUID uniquely identifies and differentiates MSDN search from other Quick Launch searches
    ///  Also, the category Shortcut is a unique string identifier, allowing scoping the results only from this provider.
    /// </summary>
    //
    // A global search provider declared statically in the registry needs the following:
    // 1) a class implementing the IVsSearchProvider interface
    // 2) the provider class specifying a Guid attribute of the search provider (provider_identifier)
    // 3) the provider class type declared on the Package-derived class using the ProvideSearchProvider attribute
    // 4) the package must derive from ExtensionPointPackage for automatic extension creation.
    //    An alternate solution is for the package to implement IVsPackageExtensionProvider and create the search 
    //    provider when CreateExtensionPoint(typeof(IVsSearchProvider).GUID, provider_identifier) is called.
    //
    // Declare the search provider guid, to be used during registration 
    // and during the provider's automatic creation as an extension point
    [Guid(GuidList.guidMSDNSearchProvider)]
    public class MSDNSearchProvider : IVsSearchProvider
    {
        // Defines all string variables like Description(Hover over Search Heading), Search Heading text, Category Shortcut
        private const string CategoryShortcutString = "msdn";

        // Main Search method that calls MSDNSearchTask to create and execute search query
        public IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchProviderCallback pSearchCallback)
        {
            if (dwCookie == VSConstants.VSCOOKIE_NIL)
            {
                return null;
            }

            return new MSDNSearchTask(this, dwCookie, pSearchQuery, pSearchCallback);
        }

        // Verifies persistent data to populate MRU list with previously selected result
        public IVsSearchItemResult CreateItemResult(string lpszPersistenceData) 
        {
            return MSDNSearchResult.FromPersistenceData(lpszPersistenceData, this);
        }

        // Get the GUID that identifies this search provider
        public Guid Category
        {
            get 
            { 
                return GetType().GUID; 
            }
        }
        
        // MSDN Search Category Heading 
        public string DisplayText
        {
            get 
            { 
                return Resources.MSDNSearchProviderDisplayText; 
            }
        }
        
        // MSDN Search Description - shows as tooltip on hover over Search Category Heading
        public string Description
        {
            get 
            { 
                return Resources.MSDNSearchProviderDescription; 
            }
        }

        protected IVsUIObject _resultsIcon;
        /// <summary>
        /// Returns the icon for each result. In this case, the same icon is returned for each result, 
        /// so we'll use the same object to save memory and time creating the images.
        ///
        /// Helper classses in Microsoft.Internal.VisualStudio.PlatformUI can be used to construct an IVsUIObject of VsUIType.Icon type.
        /// Use Win32IconUIObject if you have an HICON, use WinFormsIconUIObject if you have a System.Drawing.Icon, or
        /// use WpfPropertyValue.CreateIconObject() if you have a WPF ImageSource.
        /// There are also similar classes and functions that can be used to create objects implementing IVsUIObject of type VsUIType.Bitmap
        /// starting from a bitmap image (e.g. Win32BitmapUIObject, WpfPropertyValue.CreateBitmapObject). 
        /// </summary>
        public IVsUIObject ResultsIcon
        {
            get
            {
                if (this._resultsIcon == null)
                {
                    // Create an IVsUIObject from the winforms icon object
                    this._resultsIcon = new WinFormsIconUIObject(Resources.ResultsIcon);
                }
                return this._resultsIcon;
            }
        }


        // MSDN Category shortcut to scope results to to show only from MSDN Library
        // This is a unique string, and should not be localized.
        public string Shortcut
        {
            get
            {
                return CategoryShortcutString;
            }
        }

        public string Tooltip
        {
            get { return null; } // No additional tooltip
        }

        
        public void ProvideSearchSettings(IVsUIDataSource pSearchOptions)
        {
            // This provider uses the default settings, there is no need to change the data source properties
        }
    }
}
