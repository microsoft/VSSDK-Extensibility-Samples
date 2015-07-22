/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{
	/// <summary>
	/// Reference container node for Iron Python references.
	/// </summary>
	public class PythonReferenceContainerNode : ReferenceContainerNode
	{
		public PythonReferenceContainerNode(ProjectNode project)
			: base(project)
		{
		}

		protected override ProjectReferenceNode CreateProjectReferenceNode(ProjectElement element)
		{
			return new PythonProjectReferenceNode(this.ProjectMgr, element);
		}

		protected override ProjectReferenceNode CreateProjectReferenceNode(VSCOMPONENTSELECTORDATA selectorData)
		{
			return new PythonProjectReferenceNode(this.ProjectMgr, selectorData.bstrTitle, selectorData.bstrFile, selectorData.bstrProjRef);
		}
	}
}
