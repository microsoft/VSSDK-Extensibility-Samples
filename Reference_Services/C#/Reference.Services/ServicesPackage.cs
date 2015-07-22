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
	/// This is the package that exposes the Visual Studio services.
	/// In order to expose a service a package must implement the IServiceProvider interface (the one 
	/// defined in the Microsoft.VisualStudio.OLE.Interop.dll interop assembly, not the one defined in the
	/// .NET Framework) and notify the shell that it is exposing the services.
	/// The implementation of the interface can be somewhat difficult and error prone because it is not 
	/// designed for managed clients, but using the Managed Package Framework (MPF) we don’t really need
	/// to write any code: if our package derives from the Package class, then it will get for free the 
	/// implementation of IServiceProvider from the base class.
	/// The notification to the shell about the exported service is done using the IProfferService interface
	/// exposed by the SProfferService service; this service keeps a list of the services exposed globally 
	/// by the loaded packages and allows the shell to find the service even if the service provider that 
	/// exposes it is not inside the currently active chain of providers. Register the service and package 
    /// inside the services section of the registry, the service will available for all clients. VS will 
    /// automatically load the package when the service is requested. The MPF exposes the 
	/// ProvideServiceAttribute registration attribute to add the information needed inside the registry, 
	/// so that all we have to do is to use it in the definition of the class that implements the package.
	/// </summary>
	// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
	// a package.
	[PackageRegistration(UseManagedResourcesOnly = true)]
	// This attribute is used to register the informations needed to show the this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#112", "#113", "1.0", IconResourceID = 400)]
	[ProvideService(typeof(SMyGlobalService))]
	[System.Runtime.InteropServices.Guid(GuidsList.guidSevicesPkgString)]
	public sealed class ServicesPackage : Package
	{
		/// <summary>
		/// Standard constructor for the package.
		/// </summary>
		public ServicesPackage()
		{
			// Here we update the list of the provided services with the ones specific for this package.
			// Notice that we set to true the boolean flag about the service promotion for the global:
			// to promote the service is actually to proffer it globally using the SProfferService service.
			// For performance reasons we don’t want to instantiate the services now, but only when and 
			// if some client asks for them, so we here define only the type of the service and a function
			// that will be called the first time the package will receive a request for the service. 
			// This callback function is the one responsible for creating the instance of the service 
			// object.
			IServiceContainer serviceContainer = this;
            /// <param name="promote"> a 'true' boolean value promotes this request to any parent service 
            /// containers </param>
			serviceContainer.AddService(typeof(SMyGlobalService), CreateService, true);
			serviceContainer.AddService(typeof(SMyLocalService), CreateService);
		}

		/// <summary>
		/// This is the function that will create a new instance of the services the first time a client
		/// will ask for a specific service type. It is called by the base class's implementation of
		/// IServiceProvider.
		/// </summary>
		/// <param name="container">The IServiceContainer that needs a new instance of the service.
		///                         This must be this package.</param>
		/// <param name="serviceType">The type of service to create.</param>
		/// <returns>The instance of the service.</returns>
		private object CreateService(IServiceContainer container, Type serviceType)
		{
			// Check if the IServiceContainer is this package.
			if (container != this)
			{
				Debug.WriteLine("ServicesPackage.CreateService called from an unexpected service container.");
				return null;
			}

			// Find the type of the requested service and create it.
			if (typeof(SMyGlobalService).IsEquivalentTo(serviceType))
			{
				// Build the global service using this package as its service provider.
				return new MyGlobalService(this);
			}
			if (typeof(SMyLocalService).IsEquivalentTo(serviceType))
			{
				// Build the local service using this package as its service provider.
				return new MyLocalService(this);
			}

			// If we are here the service type is unknown, so write a message on the debug output
			// and return null.
			Debug.WriteLine("ServicesPackage.CreateService called for an unknown service type.");
			return null;
		}
	}
}
