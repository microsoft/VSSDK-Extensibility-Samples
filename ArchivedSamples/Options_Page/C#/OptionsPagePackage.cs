/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.IDE.OptionsPage
{
    /// <summary>
    /// This class implements a Visual Studio package that is registered for the Visual Studio IDE.
    /// The package class uses a number of registration attributes to specify integration parameters.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideOptionPageAttribute(typeof(OptionsPageGeneral), "My Options Page (C#)", "General", 100, 101, true, new string[] { "Change sample general options (C#)" })]
    [ProvideProfileAttribute(typeof(OptionsPageGeneral), "My Options Page (C#)", "General Options", 100, 101, true, DescriptionResourceID = 100)]
    [ProvideOptionPageAttribute(typeof(OptionsPageCustom), "My Options Page (C#)", "Custom", 100, 102, true, new string[] { "Change sample custom options (C#)" })]
    [InstalledProductRegistration("My Options Page (C#)", "My Options Page (C#) Sample", "1.0")]
    [Guid(GuidStrings.GuidPackage)]
    public class OptionsPagePackageCS : Package
    {
        /// <summary>
        /// Initialization of the package.  This is where you should put all initialization
        /// code that depends on VS services.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            // TODO: add initialization code here
        }

        public static OptionsPagePackageCS EnsurePackageIsLoaded()
        {
            IVsShell shell = Package.GetGlobalService(typeof(SVsShell)) as IVsShell;
            if (shell != null)
            {
                IVsPackage package;
                Guid guid = new Guid(GuidStrings.GuidPackage);
                
                if (ErrorHandler.Succeeded(shell.LoadPackage(ref guid, out package)))
                {
                    return package as OptionsPagePackageCS;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the OptionsPageGeneral
        /// </summary>
        /// <returns></returns>
        public static OptionsPageGeneral OptionsPageGeneral()
        {
            OptionsPagePackageCS package = EnsurePackageIsLoaded();
            return package?.GetDialogPage(typeof(OptionsPageGeneral)) as OptionsPageGeneral;
        }

        /// <summary>
        /// Get the OptionsPageCustom
        /// </summary>
        /// <returns></returns>
        public static OptionsPageCustom OptionsPageCustom()
        {
            OptionsPagePackageCS package = EnsurePackageIsLoaded();
            return package?.GetDialogPage(typeof(OptionsPageCustom)) as OptionsPageCustom;
        }
    }
}
