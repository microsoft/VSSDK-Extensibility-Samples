/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.BasicSccProvider
{
    /// <summary>
    /// Implementation of Sample Source Control Provider Service
    /// </summary>
    [Guid("ADC98052-1000-41D1-A6C3-704E6C1A3DE2")]
    public class SccProviderService : IVsSccProvider
    {
        private bool _active = false;
        private BasicSccProvider _sccProvider = null;

        public SccProviderService(BasicSccProvider sccProvider)
        {
            _sccProvider = sccProvider;
        }

        /// <summary>
        /// Returns whether this source control provider is the active scc provider.
        /// </summary>
        public bool Active
        {
            get { return _active; }
        }

        //--------------------------------------------------------------------------------
        // IVsSccProvider specific interfaces
        //--------------------------------------------------------------------------------
        
        /// <summary>
        /// Called by the scc manager when the provider is activated. Make 
        /// visible and enable if necessary scc related menu commands
        /// </summary>
        /// <returns>Returns S_OK if operation was successful</returns>
        public int SetActive()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentUICulture, "Scc Provider is now active"));

            _active = true;
            _sccProvider.OnActiveStateChange();

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by the scc manager when the provider is deactivated.
        /// Hides and disables scc related menu commands
        /// </summary>
        /// <returns>Returns S_OK if operation was successful</returns>
        public int SetInactive()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentUICulture, "Scc Provider is now inactive"));

            _active = false;
            _sccProvider.OnActiveStateChange();

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Called by the scc manager when the user wants to switch to a different source control provider.
        /// </summary>
        /// <param name="pfResult">The number of solutions under this source control. (Aways 0 in current implementation)</param>
        /// <returns>Returns S_OK if operation was successful</returns>
        public int AnyItemsUnderSourceControl(out int pfResult)
        {
            // Use this function to check if the user needs to be prompted when closing the current solution. 
            // (in case anything is under the control of this provider)
            pfResult = 0;
            return VSConstants.S_OK;
        }
    }
}