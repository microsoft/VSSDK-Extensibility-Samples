/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Samples.VisualStudio.Services.Interfaces
{
	/// <summary>
	/// This is the interface that will be implemented by the global service exposed
	/// by the package defined in Reference.Services.
	/// Notice that we have to define this interface as COM visible so that 
	/// it will be possible to query for it from the native version of IServiceProvider.
	/// </summary>
	[Guid("ba9fe7a3-e216-424e-87f9-dee001228d03")]
	[ComVisible(true)]
	public interface IMyGlobalService
	{
		void GlobalServiceFunction();
		int CallLocalService();
	}

	/// <summary>
	/// The goal of this interface is actually just to define a Type (or Guid from the native
	/// client's point of view) that will be used to identify the service.
	/// In theory, we could use the interface defined above, but it is a good practice to always
	/// define a new type as the service's identifier because a service can expose different interfaces.
	/// </summary>
	[Guid("fafafdfb-60f3-47e4-b38c-1bae05b44240")]
	public interface SMyGlobalService
	{
	}
}
