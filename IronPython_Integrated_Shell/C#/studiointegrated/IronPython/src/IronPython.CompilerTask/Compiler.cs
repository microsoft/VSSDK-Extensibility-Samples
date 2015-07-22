/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using CoreIronPython = IronPython;

namespace Microsoft.Samples.VisualStudio.IronPython.CompilerTasks
{
	/// <summary>
	/// The main purpose of this class is to associate the PythonCompiler
	/// class with the ICompiler interface.
	/// </summary>
	public class Compiler : CoreIronPython.Hosting.PythonCompiler, ICompiler
	{
		public Compiler(IList<string> sourcesFiles, string OutputAssembly)
			: base(sourcesFiles, OutputAssembly)
		{
		}

        public Compiler(IList<string> sourcesFiles, string OutputAssembly, CoreIronPython.Hosting.CompilerSink compilerSink)
			: base(sourcesFiles, OutputAssembly, compilerSink)
		{
		}
	}
}
