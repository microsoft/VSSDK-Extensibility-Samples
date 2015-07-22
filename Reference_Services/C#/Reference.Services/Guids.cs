/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;

namespace Microsoft.Samples.VisualStudio.Services
{
	/// <summary>
	/// This class is used only to expose the list of Guids used by this package.
	/// This list of guids must match the set of Guids used inside the VSCT file.
	/// </summary>
	internal static class GuidsList
	{
        public const string guidClientPkgString = "DF3ED918-375F-45B2-BAC0-2C31A0A8DA57";
        public const string guidClientCmdSetString = "36A0B180-F23F-4D96-A1A0-5928B6F7497D";

        public const string guidSevicesPkgString = "d695001c-f46a-407b-a1c9-54c35ef8ce87";
        
        // Now define the list of guids as public static members.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public static readonly Guid guidClientPkg = new Guid(guidClientPkgString);
        public static readonly Guid guidClientCmdSet = new Guid(guidClientCmdSetString);
	}
}
