/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Samples.VisualStudio.Services.Interfaces;

namespace Microsoft.Samples.VisualStudio.Services
{
	/// <summary>
	/// This is the class that implements the global service. All it needs to do is to implement 
	/// the interfaces exposed by this service (in this case IMyGlobalService).
	/// This class also needs to implement the SMyGlobalService interface in order to notify the 
	/// package that it is actually implementing this service.
	/// </summary>
	public class MyGlobalService : IMyGlobalService
	{
		// Store in this variable the service provider that will be used to query for other services.
		private IServiceProvider serviceProvider;
		public MyGlobalService(IServiceProvider sp)
		{
			Debug.WriteLine("Constructing a new instance of MyGlobalService");
			serviceProvider = sp;
		}

		#region IMyGlobalService Members
		/// <summary>
		/// Implementation of the function that does not access the local service.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Microsoft.Samples.VisualStudio.Services.HelperFunctions.WriteOnOutputWindow(System.IServiceProvider,System.String)")]
		public void GlobalServiceFunction()
		{
			string outputText = " ======================================\n" +
			                    "\tGlobalServiceFunction called.\n" +
			                    " ======================================\n";
			HelperFunctions.WriteOnOutputWindow(serviceProvider, outputText);
		}

		/// <summary>
		/// Implementation of the function that will call a method of the local service.
		/// Notice that this class will access the local service using as service provider the one
		/// implemented by ServicesPackage.
		/// </summary>
		public int CallLocalService()
		{
			// Query the service provider for the local service.
			// This object is supposed to be build by ServicesPackage and it pass its service provider
			// to the constructor, so the local service should be found.
			IMyLocalService localService = serviceProvider.GetService(typeof(SMyLocalService)) as IMyLocalService;
			if (null == localService)
			{
				// The local service was not found; write a message on the debug output and exit.
				Debug.WriteLine("Can not get the local service from the global one.");
				return -1;
			}

			// Now call the method of the local service. This will write a message on the output window.
			return localService.LocalServiceFunction();
		}
		#endregion
	}
}
