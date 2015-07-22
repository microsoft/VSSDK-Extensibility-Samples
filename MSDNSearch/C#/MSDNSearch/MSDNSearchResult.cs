/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.MSDNSearch
{
    public class MSDNSearchResult : IVsSearchItemResult
    {
        public MSDNSearchResult(string displaytext, string url, string description, MSDNSearchProvider provider)
        {
            this.DisplayText = displaytext;  // Stores the text of the link
            // We'll use the description as tooltip - because it's pretty long and all items have it, 
            // returning it as part of Description will overload the QL popup with too much information
            this.Tooltip = description;
            // All items use the same icon
            this.Icon = provider.ResultsIcon;

            this.SearchProvider = provider;
            this.Url = url;
        }

        public static MSDNSearchResult FromPersistenceData(string persistenceData, MSDNSearchProvider provider)
        {
            string[] strArr = persistenceData.Split(new string[] { Separator }, StringSplitOptions.None);

            // Let's validate the string, to avoid crashing if someone is messing up with registry values
            if (strArr.Length != 3)
                return null;

            string displayText = UnescapePersistenceString(strArr[0]);
            string url = UnescapePersistenceString(strArr[1]);
            string description = UnescapePersistenceString(strArr[2]);

            if (string.IsNullOrEmpty(displayText) || string.IsNullOrEmpty(url))
                return null;

            return new MSDNSearchResult(displayText, url, description, provider);
        }

        const string Separator = "|";
        const string Escape = "&";
        const string EscapedSeparator = "&#124";
        const string EscapedEscape = "&amp;";

        static string EscapePersistenceString(string text)
        {
            if (text == null)
            {
                return String.Empty;
            }
            StringBuilder textBuilder = new StringBuilder(text);
            textBuilder.Replace(Escape, EscapedEscape);
            textBuilder.Replace(Separator, EscapedSeparator);
            return textBuilder.ToString();
        }

        static string UnescapePersistenceString(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return null;
            }
            StringBuilder textBuilder = new StringBuilder(text);
            textBuilder.Replace(EscapedSeparator, Separator);
            textBuilder.Replace(EscapedEscape, Escape);
            return textBuilder.ToString();
        }

        // The URL to use for invoking the item
        private string Url { get; set; }

        // Action to be performed on execution of result from result list
        public void InvokeAction()
        {
            Process.Start(this.Url);
        }

        public string Description
        {
            get;
            private set;
        }

        public string DisplayText
        {
            get;
            private set;
        }

        public IVsUIObject Icon
        {
            get;
            private set;
        }        

        // Retrieves persistence data for this result
        public string PersistenceData
        {
            get
            {
                // This is used for the MRU list.  We need to be able to fully recreate the result data.
                return String.Join( Separator,
                                    EscapePersistenceString(this.DisplayText),
                                    EscapePersistenceString(this.Url),
                                    EscapePersistenceString(this.Tooltip));
            }
        }

        public IVsSearchProvider SearchProvider
        {
            get;
            private set;
        }

        public string Tooltip
        {
            get;
            private set;
        }
    }
}
