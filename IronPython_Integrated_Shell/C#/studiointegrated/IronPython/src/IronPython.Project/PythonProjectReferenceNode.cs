/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{
	/// <summary>
	/// Knows about special requirements for project to project references
	/// </summary>
	public class PythonProjectReferenceNode : ProjectReferenceNode
	{
		public PythonProjectReferenceNode(ProjectNode root, ProjectElement element)
			: base(root, element)
		{
		}

		public PythonProjectReferenceNode(ProjectNode project, string referencedProjectName, string projectPath, string projectReference)
			: base(project, referencedProjectName, projectPath, projectReference)
		{
		}

		/// <summary>
		/// Checks if a reference can be added to the project. 
		/// It calls base to see if the reference is not already there,
		/// and that it is not circular reference.
		/// If the target project is a a Python Project we can not add the project reference 
		/// because this scenario is not supported.
		/// </summary>
		/// <param name="errorHandler">The error handler delegate to return</param>
		/// <returns>false if reference cannot be added, otherwise true</returns>
		protected override bool CanAddReference(out CannotAddReferenceErrorMessage errorHandler)
		{
			//If the target project is a Python Project then show an error message
			string referencedProjectType = GetProjectType(this.ReferencedProjectGuid);
			if(string.Compare(this.ProjectMgr.ProjectType, referencedProjectType, StringComparison.OrdinalIgnoreCase) == 0)
			{
				errorHandler = new CannotAddReferenceErrorMessage(ShowProjectReferenceErrorMessage);
				return false;
			}

			//If source project has designer files of subtype form and if the target output (assembly) does not exists 
			//show a dialog that tells the user to build the target project before the project reference can be added
			if(!File.Exists(this.ReferencedProjectOutputPath) && HasFormItems())
			{
				errorHandler = new CannotAddReferenceErrorMessage(ShowProjectReferenceErrorMessage2);
				return false;
			}

			//finally we must evaluate the the rules applied on the base class
			if(!base.CanAddReference(out errorHandler))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Evaluates all pythonfilenode children of the project and returns true if anyone has subtype set to Form
		/// </summary>
		/// <returns>true if a pythonfilenode with subtype Form is found</returns>
		private bool HasFormItems()
		{
			//Get the first available py file in the project and update the MainFile property
			List<PythonFileNode> pythonFileNodes = new List<PythonFileNode>();
			this.ProjectMgr.FindNodesOfType<PythonFileNode>(pythonFileNodes);
			foreach(PythonFileNode node in pythonFileNodes)
			{
				if(node.IsFormSubType)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets a Project type string for a specified project instance guid
		/// </summary>
		/// <param name="projectGuid">Project instance guid.</param>
		/// <returns>The project type string</returns>
		private string GetProjectType(Guid projectGuid)
		{
			IVsHierarchy hierarchy = VsShellUtilities.GetHierarchy(this.ProjectMgr.Site, projectGuid);
			object projectType;
			ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_TypeName, out projectType));
			return projectType as string;
		}

		/// <summary>
		/// Shows Visual Studio message box with error message regarding project to project reference.
		/// The message box is not show in case the method has been called from automation
		/// </summary>
		private void ShowProjectReferenceErrorMessage()
		{
			if(!Utilities.IsInAutomationFunction(this.ProjectMgr.Site))
			{
				string message = SR.GetString(SR.ProjectReferenceError, CultureInfo.CurrentUICulture);
				string title = string.Empty;
				OLEMSGICON icon = OLEMSGICON.OLEMSGICON_CRITICAL;
				OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK;
				OLEMSGDEFBUTTON defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
				VsShellUtilities.ShowMessageBox(this.ProjectMgr.Site, title, message, icon, buttons, defaultButton);
			}
		}

		/// <summary>
		/// Shows Visual Studio message box with error message regarding project to project reference. Target Project must be built before
		/// adding the the project to project reference.
		/// The message box is not show in case the method has been called from automation
		/// </summary>
		private void ShowProjectReferenceErrorMessage2()
		{
			if(!Utilities.IsInAutomationFunction(this.ProjectMgr.Site))
			{
				string message = SR.GetString(SR.ProjectReferenceError2, CultureInfo.CurrentUICulture);
				string title = string.Empty;
				OLEMSGICON icon = OLEMSGICON.OLEMSGICON_CRITICAL;
				OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK;
				OLEMSGDEFBUTTON defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
				VsShellUtilities.ShowMessageBox(this.ProjectMgr.Site, title, message, icon, buttons, defaultButton);
			}
		}

	}
}
