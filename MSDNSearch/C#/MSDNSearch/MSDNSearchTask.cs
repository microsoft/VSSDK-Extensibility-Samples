/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Net;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;


namespace Microsoft.Samples.VisualStudio.MSDNSearch
{
    public class MSDNSearchTask : VsSearchTask
    {
        private MSDNSearchProvider provider;

        public MSDNSearchTask(MSDNSearchProvider provider, uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchProviderCallback pSearchCallback)
            : base(dwCookie, pSearchQuery, pSearchCallback)
        {
            this.provider = provider;            
        }

        // The web client used to perform the web query
        WebClient WebClient { get; set; }

        // Starts the search by sending Query to MSDN. This function is called on a background thread.
        protected override void OnStartSearch()
        {
            try
            {
                this.WebClient = new WebClient();
                Uri webQuery = new Uri( String.Format("http://social.msdn.microsoft.com/search/en-US/feed?query={0}&format=RSS", Uri.EscapeDataString(this.SearchQuery.SearchString)));

                // Don't use WebClient.DownloadXXXX synchronous functions because they can only be called on one thread at a time.
                // After starting a search in Quick Launch the user may type a different string and start a different search, 
                // in which case a different MSDNSearchTask will be created (possibly on other thread) and will be used to start 
                // a new web request - therefore we need to use async functions to do the online query.
                this.WebClient.DownloadDataCompleted += RSSDownloadComplete;
                this.WebClient.DownloadDataAsync(webQuery);
                
                // The search task will be marked complete when the completion event is signaled.
            }
            catch (WebException ex)
            {
                // Failed to download the RSS feed; remember the error code and set the task status
                this.ErrorCode = ex.HResult;
                this.SetTaskStatus(VSConstants.VsSearchTaskStatus.Error);
                // Report completion
                this.SearchCallback.ReportComplete(this, this.SearchResults);
            }
        }

        protected override void OnStopSearch()
        {
            // If the search is stopped and we have an async download in progress, stop it
            if (this.WebClient != null)
            {
                this.WebClient.CancelAsync();
            }
        }

        void RSSDownloadComplete(object sender, DownloadDataCompletedEventArgs e)
        {
            // If the request was cancelled because the search was cancelled/abandoned, there is nothing else to do here
            // The task completion was already notified to the search callback and the task status is already set.
            if (e.Cancelled)
            {
                return;
            }

            // If the request threw an exception, remember the code and set the task status
            if (e.Error != null)
            {
                this.ErrorCode = e.Error.HResult;
                this.SetTaskStatus(VSConstants.VsSearchTaskStatus.Error);
            }
            else
            {
                try
                {
                    // Parser code to parse through RSS results
                    var xmlDocument = new XmlDocument();

                    // The result is UTF-8 encoded, so make sure to decode it correctly
                    string resultXml = Encoding.UTF8.GetString(e.Result);
                    xmlDocument.LoadXml(resultXml);

                    var root = xmlDocument.DocumentElement;

                    // Each item/entry is a unique result
                    var entries = root.GetElementsByTagName("item");
                    if (entries.Count == 0)
                        entries = root.GetElementsByTagName("entry");

                    foreach (var node in entries)
                    {
                        // As we prepare the results, periodically check if the search was canceled
                        if (this.TaskStatus == VSConstants.VsSearchTaskStatus.Stopped)
                        {
                            // The completion was already notified by the base.OnStopSearch, there is nothing else to do
                            return;
                        }

                        var entry = node as XmlElement;
                        if (entry != null)
                        {
                            string title = null;
                            string url = null;
                            string description = null;

                            // Title tag provides result title
                            var titleNodes = entry.GetElementsByTagName("title");
                            if (titleNodes.Count > 0)
                            {
                                title = (titleNodes[0] as XmlElement).InnerText;
                            }

                            // Get the item's description as well
                            var descriptionNode = entry.GetElementsByTagName("description");
                            if (descriptionNode.Count > 0)
                            {
                                description = (descriptionNode[0] as XmlElement).InnerText;
                            }

                            // Link / URL / ID tag provides the URL linking the result string to its page
                            var linkNodes = entry.GetElementsByTagName("link");
                            if (linkNodes.Count == 0)
                                linkNodes = entry.GetElementsByTagName("url");
                            if (linkNodes.Count == 0)
                                linkNodes = entry.GetElementsByTagName("id");

                            if (linkNodes.Count > 0)
                            {
                                url = (linkNodes[0] as XmlElement).InnerText;
                            }

                            if (title != null && url != null)
                            {
                                // Create the results and then have the task report the result
                                var result = new MSDNSearchResult(title, url, description, this.provider);
                                this.SearchCallback.ReportResult(this, result);
                                // Increment the number of results reported by the provider
                                this.SearchResults++;
                            }
                        }
                    }

                    // Mark the task completed
                    this.SetTaskStatus(VSConstants.VsSearchTaskStatus.Completed);
                }
                catch (XmlException ex)
                {
                    // Remember the error code and set correct task status but otherwise don't report xml parsing errors, in case MSDN RSS format changes
                    this.ErrorCode = ex.HResult;
                    this.SetTaskStatus(VSConstants.VsSearchTaskStatus.Error);
                }
            }

            // Report completion of the search (with error or success)
            this.SearchCallback.ReportComplete(this, this.SearchResults);
        }
               
        protected new IVsSearchProviderCallback SearchCallback
        {
            get
            {
                return (IVsSearchProviderCallback)base.SearchCallback;
            }
        }
    }
}
