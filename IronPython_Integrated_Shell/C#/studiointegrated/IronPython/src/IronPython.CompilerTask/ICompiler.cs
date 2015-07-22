/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using CoreIronPython = IronPython;

namespace Microsoft.Samples.VisualStudio.IronPython.CompilerTasks
{
	/// <summary>
	/// This expose the same methods and properties
	/// as the actual engine, but gives us a good
	/// way to replace it with a mock object when
	/// unit testing.
	/// </summary>
	public interface ICompiler
	{
		IList<string> SourceFiles {get; set;}
		string OutputAssembly {get; set;}
		IList<string> ReferencedAssemblies {get; set;}
		IList<CoreIronPython.Hosting.ResourceFile> ResourceFiles { get; set; }
		string MainFile { get; set;}
		PEFileKinds TargetKind {get; set;}
		bool IncludeDebugInformation {get; set;}

		void Compile();
	}
}

