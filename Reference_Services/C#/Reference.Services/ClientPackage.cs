/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.ComponentModel.Design;
using System.Diagnostics;

using Microsoft.VisualStudio.Shell;
using Microsoft.Samples.VisualStudio.Services.Interfaces;

namespace Microsoft.Samples.VisualStudio.Services
{
	/// <summary>
	/// This is the second package created by this sample and is the client of the services exposed 
	/// by the ServicesPackage package.
	/// It will define three menu entries under the Tools menu and each command will try to use
	/// a different service.
	/// </summary>
	// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
	// a package.
	[PackageRegistration(UseManagedResourcesOnly = true)]
	// This attribute is used to register the information needed to show this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#110", "#111", "1.0", IconResourceID = 400)]
	// This attribute is needed to let the shell know that this package exposes some menus.
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[System.Runtime.InteropServices.Guid(GuidsList.guidClientPkgString)]
	public class ClientPackage : Package
	{
		/// <summary>
		/// This method is called by the base class during SetSite. At this point the service provider
		/// for the package is set and all the services are available.
		/// </summary>
		protected override void Initialize()
		{
			// Call the base implementation to finish the initialization of the package.
			base.Initialize();

            // Get the IMenuCommandService object to add the MenuCommand that will handle the command
            // defined by this package.
            IMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
			if (null == mcs)
			{
				// If we fail to get the IMenuCommandService, then we cannot add the handler
				// for the command, so we can exit now.
				Debug.WriteLine("Can not get the OleMenuCommandService from the base class.");
				return;
			}

			// Define the command and add it to the command service.
			CommandID id = new CommandID(GuidsList.guidClientCmdSet, ClientPkgCmdIDList.cmdidClientGetGlobalService);
			MenuCommand command = new MenuCommand(GetGlobalServiceCallback, id);
			mcs.AddCommand(command);

			// Add the command that will try to get the local server and that is expected to fail.
			id = new CommandID(GuidsList.guidClientCmdSet, ClientPkgCmdIDList.cmdidClientGetLocalService);
            //command = new MenuCommand(new EventHandler(GetLocalServiceCallback), id);
            command = new MenuCommand(GetLocalServiceCallback, id);
            mcs.AddCommand(command);

			// Add the command that will call the local service using the global one.
			id = new CommandID(GuidsList.guidClientCmdSet, ClientPkgCmdIDList.cmdidClientGetLocalUsingGlobal);
			command = new MenuCommand(GetLocalUsingGlobalCallback, id);
			mcs.AddCommand(command);
		}

		/// <summary>
		/// This function is the event handler for the command defined by this package and is the
		/// consumer of the service exposed by the ServicesPackage package.
		/// </summary>
		private void GetGlobalServiceCallback(object sender, EventArgs args)
		{
			// Get the service exposed by the other package. This the expected sequence of queries:
			// GetService will query the service provider implemented by the base class of this
			// package for SMyGlobalService; this service will be not found (it is not exposed by this
			// package), so the base class will forward the request to the service provider used during 
			// SetSite; this is the global service provider and it will find the service because
			// ServicesPackage has proffered it using the proffer service.
			IMyGlobalService service = GetService(typeof(SMyGlobalService)) as IMyGlobalService;
			if (null == service)
			{
				// If the service is not available we can exit now.
				Debug.WriteLine("Can not get the global service.");
				return;
			}
			// Call the function exposed by the global service. This function will write a message
			// on the output window and on the debug output so that it will be possible to verify
			// that it executed.
			service.GlobalServiceFunction();
		}

		/// <summary>
		/// This is the function that will try to get the local service exposed by the Services
		/// package. This function is expected to fail because this package has no access to the
		/// service provider implemented by the other package.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Microsoft.Samples.VisualStudio.Services.HelperFunctions.WriteOnOutputWindow(System.IServiceProvider,System.String)")]
		private void GetLocalServiceCallback(object sender, EventArgs args)
		{
			// Try to get the service. Notice that GetService will use the service provider
			// implemented by the base class and, in case of service not found, it will query
			// the service provider used during SetSite to get the service. Because the
			// service provider implemented by ServicesPackage is not inside this chain of
			// providers, this query must fail.
			IMyLocalService service = GetService(typeof(SMyLocalService)) as IMyLocalService;
			if (null != service)
			{
				// Something strange happened, write a message on the debug output and exit.
				Debug.WriteLine("GetService for the local service succeeded, but it should fail.");
				return;
			}

			// Write to output window that the call failed to get the service, as expected.
			string outputText = " ===============================================\n" +
								"\tGetLocalServiceCallback produces expected result.\n" +
								" ===============================================\n";
			// Write a message on the debug output.
			HelperFunctions.WriteOnOutputWindow(this, outputText);
		}

		/// <summary>
		/// This function will call the method of the global service that will get a reference and
		/// call a method of the local one.
		/// </summary>
		private void GetLocalUsingGlobalCallback(object sender, EventArgs args)
		{
			// Get a reference to the global service.
			IMyGlobalService service = GetService(typeof(SMyGlobalService)) as IMyGlobalService;
			if (null == service)
			{
				// The previous call failed, but we expected it to succeed.
				// Write a message on the debug output and exit.
				Debug.WriteLine("Can not get the global service.");
				return;
			}
			// Now call the method that will cause the call in the local service.
			service.CallLocalService();
		}
	}
}
