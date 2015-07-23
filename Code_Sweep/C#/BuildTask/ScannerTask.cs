/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Samples.VisualStudio.CodeSweep.BuildTask.Properties;
using Microsoft.Samples.VisualStudio.CodeSweep.Scanner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Microsoft.Samples.VisualStudio.CodeSweep.BuildTask
{
    /// <summary>
    /// MSBuild task which performs a CodeSweep scan across items in a project.
    /// </summary>
    [Description("CodeSweepTaskEntry")]
    public class ScannerTask : Task
    {
        /// <summary>
        /// Performs a scan over all files in the project.
        /// </summary>
        /// <returns>False if any violations are found, true otherwise.</returns>
        /// <remarks>
        /// If any violations are found, a message will be sent to standard output.  If this task
        /// is running the VS IDE with the CodeSweep package loaded, the message will also be
        /// placed in the task list.
        /// </remarks>
        public override bool Execute()
        {
            if (!string.IsNullOrEmpty(HostIdeProcId))
            {
                _host = GetHostObject(int.Parse(HostIdeProcId));
            }

            _ignoreInstancesList = new List<IIgnoreInstance>(Factory.DeserializeIgnoreInstances(IgnoreInstances, Path.GetDirectoryName(Project)));

            IMultiFileScanResult result = Scanner.Factory.GetScanner().Scan(GetFilesToScan(), GetTermTables(), OutputScanResults);

            if (SignalErrorIfTermsFound)
            {
                return result.PassedScan == result.Attempted;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the list of term table files, expressed as paths relative to the project
        /// folder, delimited by semicolons.
        /// </summary>
        [Required]
        public string TermTables
        {
            set { _termTables = (value != null) ? value.Split(';') : null; }
            get { return (_termTables != null) ? Utilities.Concatenate(_termTables, ";") : null; }
        }

        /// <summary>
        /// Gets or sets the list of files to scan.
        /// </summary>
        [Required]
        public ITaskItem[] FilesToScan { get; set; }

        /// <summary>
        /// Gets or sets the full path of the project file being built.
        /// </summary>
        [Required]
        public string Project { get; set; }

        /// <summary>
        /// Gets or sets the list of "ignore instances".
        /// </summary>
        public string IgnoreInstances { get; set; }

        /// <summary>
        /// Controls whether the task will indicate an error when it finds a search term.
        /// </summary>
        public bool SignalErrorIfTermsFound { get; set; }

        /// <summary>
        /// Sets the process ID of the host IDE.
        /// </summary>
        public string HostIdeProcId { get; set; }

        #region Private Members

        string[] _termTables;
        List<IIgnoreInstance> _ignoreInstancesList;
        readonly List<string> _duplicateTerms = new List<string>();
        IScannerHost _host;

        private void OutputScanResults(IScanResult result)
        {
            if (result.Scanned)
            {
                if (!result.Passed)
                {
                    foreach (IScanHit hit in result.Results)
                    {
                        IIgnoreInstance thisIgnoreInstance = Factory.GetIgnoreInstance(result.FilePath, hit.LineText, hit.Term.Text, hit.Column);

                        if (_ignoreInstancesList.Contains(thisIgnoreInstance))
                            continue;

                        if (hit.Warning != null)
                        {
                            if (!_duplicateTerms.Any(
                                item => string.Compare(item, hit.Term.Text, StringComparison.OrdinalIgnoreCase) == 0))
                            {
                                Log.LogWarning(hit.Warning);
                                _duplicateTerms.Add(hit.Term.Text);
                            }
                        }

                        string outputText;

                        if (hit.Term.RecommendedTerm != null && hit.Term.RecommendedTerm.Length > 0)
                        {
                            outputText = string.Format(CultureInfo.CurrentUICulture, Resources.HitFormatWithReplacement, result.FilePath, hit.Line + 1, hit.Column + 1, hit.Term.Text, hit.Term.Severity, hit.Term.Class, hit.Term.Comment, hit.Term.RecommendedTerm);
                        }
                        else
                        {
                            outputText = string.Format(CultureInfo.CurrentUICulture, Resources.HitFormat, result.FilePath, hit.Line + 1, hit.Column + 1, hit.Term.Text, hit.Term.Severity, hit.Term.Class, hit.Term.Comment);
                        }

                        if (_host != null)
                        {
                            // We're piping the results to the task list, so we don't want to use
                            // LogWarning, which would create an entry in the error list.
                            Log.LogMessage(MessageImportance.High, outputText);
                        }
                        else
                        {
                            Log.LogWarning(outputText);
                        }
                    } // foreach (IScanHit hit in result.Results)
                } // if (!result.Passed)
            } // if (result.Scanned)
            else
            {
                Log.LogWarning(string.Format(CultureInfo.CurrentUICulture, Resources.FileNotScannedError, result.FilePath));
            }

            if (_host != null)
            {
                _host.AddResult(result, Project);
            }
        }

        private IEnumerable<ITermTable> GetTermTables()
        {
            List<ITermTable> result = new List<ITermTable>();

            foreach (string file in _termTables)
            {
                try
                {
                    result.Add(Scanner.Factory.GetTermTable(file));
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException || ex is System.Xml.XmlException)
                    {
                        Log.LogWarning(string.Format(CultureInfo.CurrentUICulture, Resources.TermTableLoadFailed, file));
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return result;
        }

        private IEnumerable<string> GetFilesToScan()
        {
            return FilesToScan.Select(item => item.ItemSpec);
        }

        IScannerHost GetHostObject(int hostProcId)
        {
            try
            {
                ChannelServices.RegisterChannel(new TcpChannel(), ensureSecurity: false);
            }
            catch (RemotingException)
            {
                // The channel may already have been registered.
            }

            try
            {
                return (IScannerHost)Activator.GetObject(typeof(IScannerHost), Utilities.GetRemotingUri(hostProcId, includeLocalHostPrefix: true));
            }
            catch
            {
                return null;
            }
        }

        #endregion Private Members
    }
}
