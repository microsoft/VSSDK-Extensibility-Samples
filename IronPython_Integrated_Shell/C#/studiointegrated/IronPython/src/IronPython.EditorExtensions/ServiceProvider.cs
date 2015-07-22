/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using OleInterop = Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace IronPython10.MEF
{
    /// <summary>
    /// Provides helper functionality for Visual Studio
    /// </summary>
    internal class ServiceProvider : System.IServiceProvider
    {
        private System.IServiceProvider serviceProvider;

        internal ServiceProvider(ITextView view)
            : this(view.TextBuffer)
        { }

        internal ServiceProvider(ITextBuffer textBuffer)
        {
            this.serviceProvider = GetServiceProviderFromTextBuffer(textBuffer);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000")]
        private static System.IServiceProvider GetServiceProviderFromTextBuffer(ITextBuffer textBuffer)
        {
            if (textBuffer.Properties.ContainsProperty(typeof(IVsTextBuffer)))
            {
                IObjectWithSite objectWithSite = textBuffer.Properties.GetProperty<IObjectWithSite>(typeof(IVsTextBuffer));
                if (objectWithSite != null)
                {
                    Guid serviceProviderGuid = typeof(Microsoft.VisualStudio.OLE.Interop.IServiceProvider).GUID;
                    IntPtr ppServiceProvider = IntPtr.Zero;
                    // Get the service provider pointer using the Guid of the OleInterop ServiceProvider
                    objectWithSite.GetSite(ref serviceProviderGuid, out ppServiceProvider);

                    if (ppServiceProvider != IntPtr.Zero)
                    {
                        // Create a System.ServiceProvider with the OleInterop ServiceProvider
                        OleInterop.IServiceProvider oleInteropServiceProvider = (OleInterop.IServiceProvider)Marshal.GetObjectForIUnknown(ppServiceProvider);
                        return new Microsoft.VisualStudio.Shell.ServiceProvider(oleInteropServiceProvider);
                    }
                }
            }

            return null; 
        }

        public object GetService(Type serviceType)
        {
            return serviceProvider != null ? serviceProvider.GetService(serviceType) : null;
        }
    }
}