/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{
	[ComVisible(true), CLSCompliant(false)]
	[Guid("BF389FD8-F382-41b1-B502-63CB11254137")]
	public class PythonFileNodeProperties : SingleFileGeneratorNodeProperties
	{
		#region ctors
		public PythonFileNodeProperties(HierarchyNode node)
			: base(node)
		{
		}
		#endregion

		#region properties
		[Browsable(false)]
		public string URL
		{
			get
			{
				return "file:///" + this.Node.Url;
			}
		}
		[Browsable(false)]
		public string SubType
		{
			get
			{
				return ((PythonFileNode)this.Node).SubType;
			}
			set
			{
				((PythonFileNode)this.Node).SubType = value;
			}
		}

		[Microsoft.VisualStudio.Project.SRCategoryAttribute(Microsoft.VisualStudio.Project.SR.Advanced)]
		[Microsoft.VisualStudio.Project.LocDisplayName(Microsoft.VisualStudio.Project.SR.BuildAction)]
		[Microsoft.VisualStudio.Project.SRDescriptionAttribute(Microsoft.VisualStudio.Project.SR.BuildActionDescription)]
		public virtual PythonBuildAction PythonBuildAction
		{
			get
			{
				string value = this.Node.ItemNode.ItemName;
				if(value == null || value.Length == 0)
				{
					return PythonBuildAction.None;
				}
				return (PythonBuildAction)Enum.Parse(typeof(PythonBuildAction), value);
			}
			set
			{
				this.Node.ItemNode.ItemName = value.ToString();
			}
		}

		[Browsable(false)]
		public override BuildAction BuildAction
		{
			get
			{
				switch(this.PythonBuildAction)
				{
					case PythonBuildAction.ApplicationDefinition:
					case PythonBuildAction.Page:
					case PythonBuildAction.Resource:
						return BuildAction.Compile;
					default:
						return (BuildAction)Enum.Parse(typeof(BuildAction), this.PythonBuildAction.ToString());
				}
			}
			set
			{
				this.PythonBuildAction = (PythonBuildAction)Enum.Parse(typeof(PythonBuildAction), value.ToString());
			}
		}
		#endregion
	}

	public enum PythonBuildAction { None, Compile, Content, EmbeddedResource, ApplicationDefinition, Page, Resource };
}
