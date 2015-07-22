/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Samples.VisualStudio.CodeDomCodeModel;

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{
	/// <summary>
	/// Add support for automation on py files.
	/// </summary>
	[ComVisible(true)]
	[Guid("CCD70EB5-E3FE-454f-AD14-C945E9F04250")]
	public class OAIronPythonFileItem : OAFileItem
	{
		#region variables
		private EnvDTE.FileCodeModel codeModel;
		#endregion

		#region ctors
		public OAIronPythonFileItem(OAProject project, FileNode node)
			: base(project, node)
		{
		}
		#endregion

		#region overridden methods
		public override EnvDTE.FileCodeModel FileCodeModel
		{
			get
			{
				if(null != codeModel)
				{
					return codeModel;
				}
				if((null == this.Node) || (null == this.Node.OleServiceProvider))
				{
					return null;
				}
				ServiceProvider sp = new ServiceProvider(this.Node.OleServiceProvider);
				IVSMDCodeDomProvider smdProvider = sp.GetService(typeof(SVSMDCodeDomProvider)) as IVSMDCodeDomProvider;
				if(null == smdProvider)
				{
					return null;
				}
				CodeDomProvider provider = smdProvider.CodeDomProvider as CodeDomProvider;
				codeModel = PythonCodeModelFactory.CreateFileCodeModel(this as EnvDTE.ProjectItem, provider, this.Node.Url);
				return codeModel;
			}
		}

		public override EnvDTE.Window Open(string viewKind)
		{
			if(string.Compare(viewKind, EnvDTE.Constants.vsViewKindPrimary) == 0)
			{
				// Get the subtype and decide the viewkind based on the result
				if(((PythonFileNode)this.Node).IsFormSubType)
				{
					return base.Open(EnvDTE.Constants.vsViewKindDesigner);
				}
			}
			return base.Open(viewKind);
		}
		#endregion
	}

	[ComVisible(true)]
	public class OAIronPythonProject : OAProject
	{
		public OAIronPythonProject(PythonProjectNode pythonProject)
			: base(pythonProject)
		{
		}

		public override EnvDTE.CodeModel CodeModel
		{
			get
			{
				return PythonCodeModelFactory.CreateProjectCodeModel(this);
			}
		}
	}

}
