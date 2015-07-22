/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.IronPython.Project
{
	public class SelectionElementValueChangedListener : SelectionListener
	{
		#region fileds
		private ProjectNode projMgr;
		#endregion
		#region ctors
		public SelectionElementValueChangedListener(ServiceProvider serviceProvider, ProjectNode proj)
			: base(serviceProvider)
		{
			projMgr = proj;
		}
		#endregion

		#region overridden methods
		public override int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
		{
			int hr = VSConstants.S_OK;
			if(elementid == VSConstants.DocumentFrame)
			{

				IVsWindowFrame pWindowFrame = varValueOld as IVsWindowFrame;
				if(pWindowFrame != null)
				{
					object document;
					// Get the name of the document associated with the old window frame
					hr = pWindowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out document);
					if(ErrorHandler.Succeeded(hr))
					{
						uint itemid;
						IVsHierarchy hier = projMgr as IVsHierarchy;
						hr = hier.ParseCanonicalName((string)document, out itemid);
						PythonFileNode node = projMgr.NodeFromItemId(itemid) as PythonFileNode;
						if(null != node)
						{
							node.RunGenerator();
						}
					}
				}
			}

			return hr;
		}
		#endregion

	}
}
