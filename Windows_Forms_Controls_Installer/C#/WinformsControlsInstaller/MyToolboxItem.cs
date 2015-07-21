/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace Microsoft.Samples.VisualStudio.IDE.WinformsControlsInstaller
{
    /// <summary>
    /// This custom ToolboxItem displays a simple dialog asking whether to 
    /// initialize a certain value.
    /// </summary>
    [Serializable]
    class MyToolboxItem : ToolboxItem
    {
        public MyToolboxItem()
        {
        }

        private MyToolboxItem(SerializationInfo info, StreamingContext context)
        {
            Deserialize(info, context);
        }

        const int IDYES = 6;

        protected override IComponent[] CreateComponentsCore(IDesignerHost host, IDictionary defaultValues)
        {
            return RunWizard(host, base.CreateComponentsCore(host, defaultValues));
        }

        /// <summary>
        /// This method sets various values on the newly created component.
        /// </summary>
        private IComponent[] RunWizard(IDesignerHost host, IComponent[] comps)
        {
            DialogResult result = DialogResult.No;
            IVsUIShell uiShell = null;
            if (host != null)
            {
                uiShell = (IVsUIShell)host.GetService(typeof(IVsUIShell));
            }

            // Always use the UI shell service to show a messagebox if possible.
            // There are also some useful helper methods for this in VsShellUtilities.
            if (uiShell != null)
            {
                int nResult = 0;
                Guid emptyGuid = Guid.Empty;

                uiShell.ShowMessageBox(0, ref emptyGuid, "Question", "Do you want to set the Text property?", null, 0,
                        OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, OLEMSGICON.OLEMSGICON_QUERY, 0, out nResult);

                if (nResult == IDYES)
                {
                    result = DialogResult.Yes;
                }
            }
            else
            {
                result = MessageBox.Show("Do you want to set the Text property?", "Question", MessageBoxButtons.YesNo);
            }

            if (result == DialogResult.Yes)
            {
                if (comps.Length > 0)
                {
                    // Use Types from the ITypeResolutionService.  Do not use locally defined types.
                    ITypeResolutionService typeResolver = (ITypeResolutionService)host.GetService(typeof(ITypeResolutionService));
                    if (typeResolver != null)
                    {
                        Type t = typeResolver.GetType(typeof(MyCustomTextBoxWithPopup).FullName);

                        // Check to ensure we got the right Type.
                        if (t != null && comps[0].GetType().IsAssignableFrom(t))
                        {
                            // Use a property descriptor instead of direct property access.
                            // This will allow the change to appear in the undo stack and it will get
                            // serialized correctly.
                            PropertyDescriptor pd = TypeDescriptor.GetProperties(comps[0])["Text"];
                            if (pd != null)
                            {
                                pd.SetValue(comps[0], "Text Property was initialized!");
                            }
                        }
                    }
                }
            }
            return comps;
        }
    }
}
