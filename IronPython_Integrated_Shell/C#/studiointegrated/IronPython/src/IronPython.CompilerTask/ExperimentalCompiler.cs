/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using System.CodeDom.Compiler;
using IronPython.CodeDom;
using CoreIronPython = IronPython;

namespace Microsoft.Samples.VisualStudio.IronPython.CompilerTasks
{
	/// <summary>
	/// This class is an alternate compiler to build Iron Python project.
	/// The reason we have it is that the standard compiler produces assemblies
	/// which are meant to be interpreted rather then ran directly, and WAP scenarios
	/// require real assemblies.
	/// </summary>
	internal class ExperimentalCompiler : ICompiler
	{
		#region fields
		private List<string> sourceFiles;
		private string outputAssembly;
        private CoreIronPython.Hosting.CompilerSink errorSink;
		private List<string> referencedAssemblies = new List<string>();
        private IList<CoreIronPython.Hosting.ResourceFile> resourceFiles = new List<CoreIronPython.Hosting.ResourceFile>();
		private string mainFile = null;
		private System.Reflection.Emit.PEFileKinds targetKind = System.Reflection.Emit.PEFileKinds.Dll;
		private bool includeDebugInformation = false;
		#endregion

		#region Constructors
		public ExperimentalCompiler(IList<string> sourcesFiles, string outputAssembly)
		{
			this.sourceFiles = (List<string>)sourcesFiles;
			this.outputAssembly = outputAssembly;
		}

		public ExperimentalCompiler(IList<string> sourcesFiles, string outputAssembly, CoreIronPython.Hosting.CompilerSink compilerSink)
		{
			this.sourceFiles = (List<string>)sourcesFiles;
			this.outputAssembly = outputAssembly;
			this.errorSink = compilerSink;
		}
		#endregion

		#region ICompiler Members

		public IList<string> SourceFiles
		{
			get
			{
				return this.sourceFiles;
			}
			set
			{
				this.sourceFiles = (List<string>)value;
			}
		}

		public string OutputAssembly
		{
			get
			{
				return this.outputAssembly;
			}
			set
			{
				this.outputAssembly = value;
			}
		}

		public IList<string> ReferencedAssemblies
		{
			get
			{
				return this.referencedAssemblies;
			}
			set
			{
				this.referencedAssemblies = (List<string>)value;
			}
		}

		public IList<CoreIronPython.Hosting.ResourceFile> ResourceFiles
		{
			get
			{
				return this.resourceFiles;
			}
			set
			{
				this.resourceFiles = value;
			}
		}

		public string MainFile
		{
			get
			{
				return mainFile;
			}
			set
			{
				this.mainFile = value;
			}
		}

		public System.Reflection.Emit.PEFileKinds TargetKind
		{
			get
			{
				return this.targetKind;
			}
			set
			{
				this.targetKind = value;
			}
		}

		public bool IncludeDebugInformation
		{
			get
			{
				return this.includeDebugInformation;
			}
			set
			{
				this.includeDebugInformation = value;
			}
		}

		public void Compile()
		{
			PythonProvider provider = new PythonProvider();
			CompilerParameters options = new CompilerParameters(referencedAssemblies.ToArray(), OutputAssembly, IncludeDebugInformation);
			options.MainClass = MainFile;
			foreach(CoreIronPython.Hosting.ResourceFile resourceInfo in resourceFiles)
			{
				// NOTE: with this approach we lack a way to control the name of the generated resource or if it is public
				string resource = resourceInfo.File;
				options.EmbeddedResources.Add(resource);
			}

			CompilerResults results = provider.CompileAssemblyFromFile(options, sourceFiles.ToArray());
			foreach (CompilerError error in results.Errors)
			{
				int errorNumber = 0;
				int.TryParse(error.ErrorNumber, out errorNumber);
				this.errorSink.AddError(error.FileName, error.ErrorText, String.Empty, new CoreIronPython.Hosting.CodeSpan(error.Line, error.Column, error.Line, error.Column+1), errorNumber, error.IsWarning ? CoreIronPython.Hosting.Severity.Warning : CoreIronPython.Hosting.Severity.Error);
			}
		}

		#endregion
	}
}
