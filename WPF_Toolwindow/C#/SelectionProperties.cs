/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
	/// <summary>
	/// This class holds the properties that will be displayed in the Properties
	/// window for the selected object.
	/// 
	/// While all these properties are read-only, defining the set method would
	/// make them writable.
	/// 
	/// We derive from CustomTypeDescriptor, which is an ICustomTypeDescriptor, and
	/// the only part that we overload is the ComponentName.
	/// </summary>
	internal class SelectionProperties : CustomTypeDescriptor
	{
		private string caption = string.Empty;
		private Guid persistanceGuid = Guid.Empty;
		private int index = -1;

		/// <summary>
		/// This class holds the properties for one item.
		/// The list of properties could be modified to display a different
		/// set of properties.
		/// </summary>
		/// <param name="caption">Window Title</param>
		/// <param name="persistence">Persistence Guid</param>
		public SelectionProperties(string caption, Guid persistence)
		{
			this.caption = caption;
			persistanceGuid = persistence;
		}

		/// <summary>
		/// Caption property that will be exposed in the Properties window.
		/// </summary>
		public string Caption
		{
			get { return caption; }
		}

		/// <summary>
		/// Guid corresponding to the tool window.
		/// Note that this property uses attributes to provide richer data
		/// to the Properties window.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DisplayName("Persistence GUID")]
		[Description("Guids used to uniquely identify the window type.")]
		[Category("Advanced")]
		public string PersistenceGuid
		{
			get { return persistanceGuid.ToString("B"); }
		}

		/// <summary>
		/// Index of the window in our list. We use this internally to avoid having to
		/// search the list of windows when the selection is changed from the Property
		/// window.
		/// This property will not be visible because we are using the Browsable(false) attribute
		/// </summary>
		[Browsable(false)]
		public int Index
		{
			get { return index; }
			set { index = value; }
		}

		/// <summary>
		/// String that will be displayed in the Properties window combo box.
		/// </summary>
		protected override string ComponentName
		{
			get { return Caption; }
		}
	}
}
