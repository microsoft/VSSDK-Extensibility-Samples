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
	/// This class is used to expose the list of the IDs of the commands implemented
	/// by the client package. This list of IDs must match the set of IDs defined inside the
	/// Buttons section of the VSCT file.
	/// </summary>
	static class CommandId
	{
		// Define the list a set of public static members.
		public const int cmdidPersistedWindow =    0x2001;
		public const int cmdidUiEventsWindow =     0x2002;
		public const int cmdidRefreshWindowsList = 0x2003;

		// Define the list of menus (these include toolbars)
		public const int IDM_MyToolbar =           0x0101;
	}
}
