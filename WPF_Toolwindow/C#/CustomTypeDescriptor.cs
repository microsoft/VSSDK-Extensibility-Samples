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
	/// This base class forwards all the ICustomTypeDescriptor calls
	/// to the default TypeDescriptor, except for GetComponentName.
	/// This allows for a class to specify the name that will be
	/// displayed in the combo box of the Properties window.
	/// </summary>
	internal abstract class CustomTypeDescriptor : ICustomTypeDescriptor
	{
		/// <summary>
		/// Name of the component.
		/// When this class is used to expose property in the Properties
		/// window, this should be the name associated with this instance.
		/// </summary>
		protected abstract string ComponentName
		{
			get;
		}

		#region ICustomTypeDescriptor Members

		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return TypeDescriptor.GetAttributes(GetType());
		}

		string ICustomTypeDescriptor.GetClassName()
		{
			return TypeDescriptor.GetClassName(GetType());
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return ComponentName;
		}

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return TypeDescriptor.GetConverter(GetType());
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(GetType());
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			return TypeDescriptor.GetDefaultProperty(GetType());
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(GetType(), editorBaseType);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(GetType(), attributes);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents(GetType());
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			return TypeDescriptor.GetProperties(GetType(), attributes);
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return TypeDescriptor.GetProperties(GetType());
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}

		#endregion
	}
}
