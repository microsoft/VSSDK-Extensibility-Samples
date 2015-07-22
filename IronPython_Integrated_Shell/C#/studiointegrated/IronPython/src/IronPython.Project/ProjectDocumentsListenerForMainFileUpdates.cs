/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{
	/// <summary>
	/// Updates the python project property called MainFile 
	/// </summary>
	class ProjectDocumentsListenerForMainFileUpdates : ProjectDocumentsListener
	{
		#region fields
		/// <summary>
		/// The python project who adviced for TrackProjectDocumentsEvents
		/// </summary>
		private PythonProjectNode project;
		#endregion

		#region ctors
		public ProjectDocumentsListenerForMainFileUpdates(ServiceProvider serviceProvider, PythonProjectNode project)
			: base(serviceProvider)
		{
			this.project = project;
		}
		#endregion

		#region overriden methods
		public override int OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] projects, int[] firstIndices, string[] oldFileNames, string[] newFileNames, VSRENAMEFILEFLAGS[] flags)
		{
			//Get the current value of the MainFile Property
			string currentMainFile = this.project.GetProjectProperty(PythonProjectFileConstants.MainFile, true);
			string fullPathToMainFile = Path.Combine(Path.GetDirectoryName(this.project.BaseURI.Uri.LocalPath), currentMainFile);

			//Investigate all of the oldFileNames if they belong to the current project and if they are equal to the current MainFile
			int index = 0;
			foreach(string oldfile in oldFileNames)
			{
				//Compare this project with the project that the old file belongs to
				IVsProject belongsToProject = projects[firstIndices[index]];
				if(Utilities.IsSameComObject(belongsToProject, this.project))
				{
					//Compare the files and update the MainFile Property if the currentMainFile is an old file
					if(NativeMethods.IsSamePath(oldfile, fullPathToMainFile))
					{
						//Get the newfilename and update the MainFile property
						string newfilename = newFileNames[index];
						PythonFileNode node = this.project.FindChild(newfilename) as PythonFileNode;
						if(node == null)
							throw new InvalidOperationException("Could not find the PythonFileNode object");
						this.project.SetProjectProperty(PythonProjectFileConstants.MainFile, node.GetRelativePath());
						break;
					}
				}

				index++;
			}

			return VSConstants.S_OK;
		}

		public override int OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] projects, int[] firstIndices, string[] oldFileNames, VSREMOVEFILEFLAGS[] flags)
		{
			//Get the current value of the MainFile Property
			string currentMainFile = this.project.GetProjectProperty(PythonProjectFileConstants.MainFile, true);
			string fullPathToMainFile = Path.Combine(Path.GetDirectoryName(this.project.BaseURI.Uri.LocalPath), currentMainFile);

			//Investigate all of the oldFileNames if they belong to the current project and if they are equal to the current MainFile
			int index = 0;
			foreach(string oldfile in oldFileNames)
			{
				//Compare this project with the project that the old file belongs to
				IVsProject belongsToProject = projects[firstIndices[index]];
				if(Utilities.IsSameComObject(belongsToProject, this.project))
				{
					//Compare the files and update the MainFile Property if the currentMainFile is an old file
					if(NativeMethods.IsSamePath(oldfile, fullPathToMainFile))
					{
						//Get the first available py file in the project and update the MainFile property
						List<PythonFileNode> pythonFileNodes = new List<PythonFileNode>();
						this.project.FindNodesOfType<PythonFileNode>(pythonFileNodes);
						string newMainFile = string.Empty;
						if(pythonFileNodes.Count > 0)
						{
							newMainFile = pythonFileNodes[0].GetRelativePath();
						}
						this.project.SetProjectProperty(PythonProjectFileConstants.MainFile, newMainFile);
						break;
					}
				}

				index++;
			}

			return VSConstants.S_OK;
		}

		public override int OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] projects, int[] firstIndices, string[] newFileNames, VSADDFILEFLAGS[] flags)
		{
			//Get the current value of the MainFile Property
			string currentMainFile = this.project.GetProjectProperty(PythonProjectFileConstants.MainFile, true);
			if(!string.IsNullOrEmpty(currentMainFile))
				//No need for further operation since MainFile is already set
				return VSConstants.S_OK;

			string fullPathToMainFile = Path.Combine(Path.GetDirectoryName(this.project.BaseURI.Uri.LocalPath), currentMainFile);

			//Investigate all of the newFileNames if they belong to the current project and set the first pythonFileNode found equal to MainFile
			int index = 0;
			foreach(string newfile in newFileNames)
			{
				//Compare this project with the project that the new file belongs to
				IVsProject belongsToProject = projects[firstIndices[index]];
				if(Utilities.IsSameComObject(belongsToProject, this.project))
				{
					//If the newfile is a python filenode we willl map this file to the MainFile property
					PythonFileNode filenode = project.FindChild(newfile) as PythonFileNode;
					if(filenode != null)
					{
						this.project.SetProjectProperty(PythonProjectFileConstants.MainFile, filenode.GetRelativePath());
						break;
					}
				}

				index++;
			}

			return VSConstants.S_OK;
		}
		#endregion

	}
}
