/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Shell.Interop;
using MsVsShell = Microsoft.VisualStudio.Shell;
using VsConstants = Microsoft.VisualStudio.VSConstants;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.Samples.VisualStudio.IDE.ToolWindow
{
	/// <summary>
	/// This class is responsible for retrieving and keeping
	/// the list of tool windows.
	/// This cache the result of the last refresh.
	/// </summary>
	class WindowList
	{
		// List of tool window frames, current as of the last refresh
		private IList<IVsWindowFrame> framesList = null;
		// Names of the tool windows
		private IList<string> toolWindowNames = null;

		/// <summary>
		/// Get the IVsWindowFrame for the specified index
		/// </summary>
		/// <param name="index">Index in the list of windows</param>
		/// <returns>frame of the window</returns>
		public IVsWindowFrame this[int index]
		{
			get { return framesList[index]; }
		}

		/// <summary>
		/// The names of the existing tool windows.
		/// This gets updated when RefreshList is updated
		/// </summary>
		public IList<string> WindowNames
		{
			get { return toolWindowNames; }
		}

		/// <summary>
		/// Update the content of the list by asking VS
		/// </summary>
		/// <returns></returns>
		public void RefreshList()
		{
			framesList = new List<IVsWindowFrame>();
			toolWindowNames = new List<string>();

			// Get the UI Shell service
            IVsUIShell4 uiShell = (IVsUIShell4)MsVsShell.Package.GetGlobalService(typeof(SVsUIShell));
			// Get the tool windows enumerator
			IEnumWindowFrames windowEnumerator;

            uint flags = unchecked(((uint)__WindowFrameTypeFlags.WINDOWFRAMETYPE_Tool |(uint)__WindowFrameTypeFlags.WINDOWFRAMETYPE_Uninitialized));
            ErrorHandler.ThrowOnFailure(uiShell.GetWindowEnum(flags, out windowEnumerator));
           
			IVsWindowFrame[] frame = new IVsWindowFrame[1];
			uint fetched = 0;
			int hr = VsConstants.S_OK;
			// Note that we get S_FALSE when there is no more item, so only loop while we are getting S_OK
			while (hr == VsConstants.S_OK)
			{
				// For each tool window, add it to the list
				hr = windowEnumerator.Next(1, frame, out fetched);
				ErrorHandler.ThrowOnFailure(hr);
				if (fetched == 1)
				{
                    if (frame[0].IsVisible() == VsConstants.S_OK)
                    {
                        // We successfully retrieved a window frame, update our lists
                        string caption = (string)GetProperty(frame[0], (int)__VSFPROPID.VSFPROPID_Caption);
                        toolWindowNames.Add(caption);
                        framesList.Add(frame[0]);
                    }
				}
			}
		}

		/// <summary>
		/// This wraps the call to IVsWindowFrame.GetProperty
		/// </summary>
		/// <param name="frame">Window frame for which we want the property</param>
		/// <param name="propertyID">ID of the property to retrieve</param>
		/// <returns>The value of the property</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
		internal object GetProperty(IVsWindowFrame frame, int propertyID)
		{
			object result = null;
			ErrorHandler.ThrowOnFailure(frame.GetProperty(propertyID, out result));
			return result;
		}

		/// <summary>
		/// This wraps the call to IVsWindowFrame.GetGuidProperty
		/// </summary>
		/// <param name="frame">Window frame for which we want the property</param>
		/// <param name="propertyID">ID of the property to retrieve</param>
		/// <returns>The value of the property</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
		internal Guid GetGuidProperty(IVsWindowFrame frame, int propertyID)
		{
			Guid result = Guid.Empty;
			ErrorHandler.ThrowOnFailure(frame.GetGuidProperty(propertyID, out result));
			return result;
		}

		/// <summary>
		/// Returns a list of SelectionProperties items (1 per tool window listed)
		/// </summary>
		internal ArrayList WindowsProperties
		{
			get
			{
				int index = 0;
				ArrayList properties = new ArrayList();
				foreach (IVsWindowFrame frame in framesList)
				{
					// Get the properties for this frame
					SelectionProperties property = GetFrameProperties(frame);
					property.Index = index;
					properties.Add(property);
					++index;
				}
				return properties;
			}
		}

		/// <summary>
		/// Provides the properties object corresponding to the window frame
		/// </summary>
		/// <param name="frame">Window frame to return properties for</param>
		/// <returns>Properties object</returns>
		internal SelectionProperties GetFrameProperties(IVsWindowFrame frame)
		{
			// Get the caption and Guid for the current tool window
			string caption = (string)GetProperty(frame, (int)__VSFPROPID.VSFPROPID_Caption);
			Guid persistenceGuid = GetGuidProperty(frame, (int)__VSFPROPID.VSFPROPID_GuidPersistenceSlot); ;
			// Create the property object based on this and add it to the list
			SelectionProperties property = new SelectionProperties(caption, persistenceGuid);
			return property;
		}
	}
}
