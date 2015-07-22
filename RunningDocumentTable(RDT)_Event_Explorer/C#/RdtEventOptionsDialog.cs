/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using System.ComponentModel;


namespace MyCompany.RdtEventExplorer
{
    /// <summary>
    /// The Tool/Options dialog page that filters the RDT event display.
    /// This class owns the singleton instance of the options (rdteOptions)
    /// available via automation.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("7F389730-D552-414a-9C43-161B07CBFED4")]
    public class RdtEventOptionsDialog : DialogPage
    {
        // Dialog pages are cached singleton instances.
        private Options rdteOptions = new Options();

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IOptions RdteOptions
        {
            get 
            {
                return rdteOptions;
            }
        }
        /// <include file='doc\DialogPage.uex' path='docs/doc[@for="DialogPage.AutomationObject"]' />
        /// <devdoc>
        ///     The object the dialog page is going to browse.  The
        ///     default returns "this", but you can change it to
        ///     browse any object you want.
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override object AutomationObject {
            get 
            {
                 return RdteOptions;
            }
        }
    }
}
