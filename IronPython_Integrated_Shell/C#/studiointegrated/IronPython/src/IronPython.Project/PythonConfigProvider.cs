/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio;

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{
	/// <summary>
	/// Enables the Any CPU Platform form name for Iron Python Projects
	/// </summary>
	[ComVisible(true), CLSCompliant(false)]
	public class PythonConfigProvider : ConfigProvider
	{
		#region ctors
		public PythonConfigProvider(ProjectNode manager)
			: base(manager)
		{
		}
		#endregion
		#region overridden methods
		public override int GetPlatformNames(uint celt, string[] names, uint[] actual)
		{
			if(names != null)
				names[0] = "Any CPU";

			if(actual != null)
				actual[0] = 1;

			return VSConstants.S_OK;
		}

		public override int GetSupportedPlatformNames(uint celt, string[] names, uint[] actual)
		{
			if(names != null)
				names[0] = "Any CPU";

			if(actual != null)
				actual[0] = 1;

			return VSConstants.S_OK;
		}
		#endregion
	}
}
