/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.Services
{
	/// <summary>
	/// This class is used to expose some utility functions used in this project.
	/// </summary>
	internal static class HelperFunctions
	{
		/// <summary>
		/// This function is used to write a string on the Output window of Visual Studio.
		/// </summary>
		/// <param name="provider">The service provider to query for SVsOutputWindow</param>
		/// <param name="text">The text to write</param>
		internal static void WriteOnOutputWindow(IServiceProvider provider, string text)
		{
			// At first write the text on the debug output.
			Debug.WriteLine(text);

			// Check if we have a provider
			if (null == provider)
			{
				// If there is no provider we can not do anything; exit now.
				Debug.WriteLine("No service provider passed to WriteOnOutputWindow.");
				return;
			}

			// Now get the SVsOutputWindow service from the service provider.
			IVsOutputWindow outputWindow = provider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
			if (null == outputWindow)
			{
				// If the provider doesn't expose the service there is nothing we can do.
				// Write a message on the debug output and exit.
				Debug.WriteLine("Can not get the SVsOutputWindow service.");
				return;
			}

			// We can not write on the Output window itself, but only on one of its panes.
			// Here we try to use the "General" pane.
			Guid guidGeneral = Microsoft.VisualStudio.VSConstants.GUID_OutWindowGeneralPane;
			IVsOutputWindowPane windowPane;
			if (Microsoft.VisualStudio.ErrorHandler.Failed(outputWindow.GetPane(ref guidGeneral, out windowPane)) ||
				(null == windowPane))
			{
                if (Microsoft.VisualStudio.ErrorHandler.Failed(outputWindow.CreatePane(ref guidGeneral, "General", 1, 0)))
                {
                    // Nothing to do here, just debug output and exit
                    Debug.WriteLine("Failed to create the Output window pane.");
                    return;
                }
				if (Microsoft.VisualStudio.ErrorHandler.Failed(outputWindow.GetPane(ref guidGeneral, out windowPane)) ||
				(null == windowPane))
				{
					// Again, there is nothing we can do to recover from this error, so write on the
					// debug output and exit.
					Debug.WriteLine("Failed to get the Output window pane.");
					return;
				}
                if (Microsoft.VisualStudio.ErrorHandler.Failed(windowPane.Activate()))
                {
                    Debug.WriteLine("Failed to activate the Output window pane.");
                    return;
                }
			}

			// Finally we can write on the window pane.
			if (Microsoft.VisualStudio.ErrorHandler.Failed(windowPane.OutputString(text)))
			{
				Debug.WriteLine("Failed to write on the Output window pane.");
			}
		}
	}
}
