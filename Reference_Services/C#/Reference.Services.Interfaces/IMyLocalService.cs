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
	/// This is the interface implemented by the local service.
	/// Notice that we have to define this interface as COM visible so that 
	/// it will be possible to query for it from the native version of IServiceProvider.
	/// </summary>
	[Guid("04079195-ce4d-4683-aec3-e2f2be23b934")]
	[ComVisible(true)]
	public interface IMyLocalService
	{
		int LocalServiceFunction();
	}

	/// <summary>
	/// This interface is used to define the Type or Guid that identifies the service.
	/// It is not strictly required because our service will implement only one interface,
	/// but in case of services that implement multiple interfaces it is good practice to define
	/// a different type to identify the service itself.
	/// </summary>
	[Guid("ed840427-1df8-4d3a-85eb-38847fba93f4")]
	public interface SMyLocalService
	{
	}
}
