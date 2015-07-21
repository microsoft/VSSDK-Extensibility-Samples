/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
	/// <summary>
	/// This class is used only to expose the list of Guids used by this package.
	/// This list of guids must match the set of Guids used inside the VSCT file.
	/// </summary>
	static class GuidsList
	{
		// Now define the list of guids as public static members.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		public static readonly Guid guidClientPkg = new Guid("{01069CDD-95CE-4620-AC21-DDFF6C57F012}");
		public static readonly Guid guidClientCmdSet = new Guid("{1227033A-2F60-4bd6-8208-B43EC8C12510}");

		/// <summary>
		/// This Guid is the persistence guid for the output window.
		/// It can be found by running this sample, bringing up the output window,
		/// selecting it in the Persisted window and then looking in the Properties
		/// window.
		/// </summary>
		public static readonly Guid guidOutputWindowFrame = new Guid("{34e76e81-ee4a-11d0-ae2e-00a0c90fffc3}");
	}
}
