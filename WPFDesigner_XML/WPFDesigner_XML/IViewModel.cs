/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.VsTemplateDesigner
{
    public interface IViewModel
    {
        VSTemplateTemplateData TemplateData { get; }
        VSTemplateTemplateContent TemplateContent { get; }

        string Name { get; set; }
        string Description { get; set; }
        string Icon { get; set; }
        string ProjectType { get; set; }
        string ProjectSubType { get; set; }
        string DefaultName { get; set; }
        string TemplateID { get; set; }
        string GroupID { get; set; }
        string SortOrder { get; set; }
        VSTemplateTemplateDataLocationField LocationField { get; set; }
        string LocationFieldMRUPrefix { get; set; }
        string PreviewImage { get; set; }
        string WizardAssembly { get; set; }
        string WizardClassName { get; set; }
        string WizardData { get; set; }

        bool ProvideDefaultName { get; set; }
        bool CreateNewFolder { get; set; }
        bool PromptForSaveOnCreation { get; set; }
        bool Hidden { get; set; }
        bool SupportsMasterPage { get; set; }
        bool SupportsCodeSeparation { get; set; }
        bool SupportsLanguageDropDown { get; set; }

        bool DesignerDirty { get; set; }
        bool IsNameEnabled { get; }
        bool IsDescriptionEnabled { get; }
        bool IsIconEnabled { get; }
        bool IsLocationFieldSpecified { get; }
        
        event EventHandler ViewModelChanged;
        void DoIdle();
        void Close();

        void OnSelectChanged(object p);
    }
}