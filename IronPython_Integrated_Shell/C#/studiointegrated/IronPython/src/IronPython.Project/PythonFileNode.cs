/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.Samples.VisualStudio.IronPython.Project.WPFProviders;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Windows.Design.Host;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VSConstants = Microsoft.VisualStudio.VSConstants;

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{
	class PythonFileNode : FileNode
	{
		#region fields
		private OAVSProjectItem vsProjectItem;
		private SelectionElementValueChangedListener selectionChangedListener;
		private OAIronPythonFileItem automationObject;
		private DesignerContext designerContext;
		#endregion

		#region properties
		/// <summary>
		/// Returns bool indicating whether this node is of subtype "Form"
		/// </summary>
		public bool IsFormSubType
		{
			get
			{
				string result = this.ItemNode.GetMetadata(ProjectFileConstants.SubType);
				if(!String.IsNullOrEmpty(result) && string.Compare(result, ProjectFileAttributeValue.Form, true, CultureInfo.InvariantCulture) == 0)
					return true;
				else
					return false;
			}
		}
		/// <summary>
		/// Returns the SubType of an Iron Python FileNode. It is 
		/// </summary>
		public string SubType
		{
			get
			{
				return this.ItemNode.GetMetadata(ProjectFileConstants.SubType);
			}
			set
			{
				this.ItemNode.SetMetadata(ProjectFileConstants.SubType, value);
			}
		}

		protected internal VSLangProj.VSProjectItem VSProjectItem
		{
			get
			{
				if(null == vsProjectItem)
				{
					vsProjectItem = new OAVSProjectItem(this);
				}
				return vsProjectItem;
			}
		}

		protected internal Microsoft.Windows.Design.Host.DesignerContext DesignerContext
		{
			get
			{
				if(designerContext == null)
				{
					designerContext = new DesignerContext();
					//Set the EventBindingProvider for this XAML file so the designer will call it
					//when event handlers need to be generated
					designerContext.EventBindingProvider = new PythonEventBindingProvider(this.Parent.FindChild(this.Url.Replace(".xaml", ".py")) as PythonFileNode);
				}
				return designerContext;
			}
		}
		#endregion

		#region ctors
		internal PythonFileNode(ProjectNode root, ProjectElement e)
			: base(root, e)
		{
			selectionChangedListener = new SelectionElementValueChangedListener(new ServiceProvider((IOleServiceProvider)root.GetService(typeof(IOleServiceProvider))), root);
			selectionChangedListener.Init();

		}
		#endregion

		#region overridden properties

		internal override object Object
		{
			get
			{
				return this.VSProjectItem;
			}
		}
		#endregion

		#region overridden methods
		protected override NodeProperties CreatePropertiesObject()
		{
			PythonFileNodeProperties properties = new PythonFileNodeProperties(this);
			properties.OnCustomToolChanged += new EventHandler<HierarchyNodeEventArgs>(OnCustomToolChanged);
			properties.OnCustomToolNameSpaceChanged += new EventHandler<HierarchyNodeEventArgs>(OnCustomToolNameSpaceChanged);
			return properties;
		}

		public override int Close()
		{
			if(selectionChangedListener != null)
				selectionChangedListener.Dispose();
			return base.Close();
		}

		/// <summary>
		/// Returs an Iron Python FileNode specific object implmenting DTE.ProjectItem
		/// </summary>
		/// <returns></returns>
		public override object GetAutomationObject()
		{
			if(null == automationObject)
			{
				automationObject = new OAIronPythonFileItem(this.ProjectMgr.GetAutomationObject() as OAProject, this);
			}
			return automationObject;
		}

		public override int ImageIndex
		{
			get
			{
				if(IsFormSubType)
				{
					return (int)ProjectNode.ImageName.WindowsForm;
				}
				if(this.FileName.ToLower().EndsWith(".py"))
				{
					return PythonProjectNode.ImageOffset + (int)PythonProjectNode.PythonImageName.PyFile;
				}
				return base.ImageIndex;
			}
		}

		/// <summary>
		/// Open a file depending on the SubType property associated with the file item in the project file
		/// </summary>
		protected override void DoDefaultAction()
		{
			FileDocumentManager manager = this.GetDocumentManager() as FileDocumentManager;
			Debug.Assert(manager != null, "Could not get the FileDocumentManager");

			Guid viewGuid = (IsFormSubType ? VSConstants.LOGVIEWID_Designer : VSConstants.LOGVIEWID_Code);
			IVsWindowFrame frame;
			manager.Open(false, false, viewGuid, out frame, WindowFrameShowAction.Show);
		}

		protected override int ExecCommandOnNode(Guid guidCmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
		{
			Debug.Assert(this.ProjectMgr != null, "The PythonFileNode has no project manager");

			if(this.ProjectMgr == null)
			{
				throw new InvalidOperationException();
			}

			if(guidCmdGroup == PythonMenus.guidIronPythonProjectCmdSet)
			{
				if(cmd == (uint)PythonMenus.SetAsMain.ID)
				{
					// Set the MainFile project property to the Filename of this Node
					((PythonProjectNode)this.ProjectMgr).SetProjectProperty(PythonProjectFileConstants.MainFile, this.GetRelativePath());
					return VSConstants.S_OK;
				}
			}
			return base.ExecCommandOnNode(guidCmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
		}

		/// <summary>
		/// Handles the menuitems
		/// </summary>		
		protected override int QueryStatusOnNode(Guid guidCmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
		{
			if(guidCmdGroup == Microsoft.VisualStudio.Shell.VsMenus.guidStandardCommandSet97)
			{
				switch((VsCommands)cmd)
				{
					case VsCommands.AddNewItem:
					case VsCommands.AddExistingItem:
					case VsCommands.ViewCode:
						result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
						return VSConstants.S_OK;
					case VsCommands.ViewForm:
						if(IsFormSubType)
							result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
						return VSConstants.S_OK;
				}
			}

			else if(guidCmdGroup == PythonMenus.guidIronPythonProjectCmdSet)
			{
				if(cmd == (uint)PythonMenus.SetAsMain.ID)
				{
					result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
					return VSConstants.S_OK;
				}
			}
			return base.QueryStatusOnNode(guidCmdGroup, cmd, pCmdText, ref result);
		}

		#endregion

		#region methods
		public string GetRelativePath()
		{
			string relativePath = Path.GetFileName(this.ItemNode.GetMetadata(ProjectFileConstants.Include));
			HierarchyNode parent = this.Parent;
			while(parent != null && !(parent is ProjectNode))
			{
				relativePath = Path.Combine(parent.Caption, relativePath);
				parent = parent.Parent;
			}
			return relativePath;
		}

		internal OleServiceProvider.ServiceCreatorCallback ServiceCreator
		{
			get { return new OleServiceProvider.ServiceCreatorCallback(this.CreateServices); }
		}

		private object CreateServices(Type serviceType)
		{
			object service = null;
			if(typeof(EnvDTE.ProjectItem) == serviceType)
			{
				service = GetAutomationObject();
			}
			else if(typeof(DesignerContext) == serviceType)
			{
				service = this.DesignerContext;
			}
			return service;
		}
		#endregion
	}
}