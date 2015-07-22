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
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Samples.VisualStudio.Services.Interfaces;

namespace Microsoft.Samples.VisualStudio.Services
{
	/// <summary>
	/// This is the class that implements the local service. It implements IMyLocalService
	/// because this is the interface that we want to use, but it also implements the empty
	/// interface SMyLocalService in order to notify the service creator that it actually
	/// implements this service.
	/// </summary>
	public class MyLocalService : IMyLocalService, SMyLocalService
	{
		// Store a reference to the service provider that will be used to access the shell's services
		private IServiceProvider provider;
		/// <summary>
		/// Public constructor of this service. This will use a reference to a service provider to
		/// access the services provided by the shell.
		/// </summary>
		public MyLocalService(IServiceProvider sp)
		{
			Debug.WriteLine("Constructing a new instance of MyLocalService");
			provider = sp;
		}
		#region IMyLocalService Members
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "Microsoft.Samples.VisualStudio.Services.HelperFunctions.WriteOnOutputWindow(System.IServiceProvider,System.String)")]
		public int LocalServiceFunction()
		{
			string outputText = " ======================================\n" +
								"\tLocalServiceFunction called.\n" +
								" ======================================\n";
			HelperFunctions.WriteOnOutputWindow(provider, outputText);
			return 0;
		}
		#endregion
	}
}
