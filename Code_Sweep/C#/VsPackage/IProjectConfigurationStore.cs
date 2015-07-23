/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System.Collections.Generic;

namespace Microsoft.Samples.VisualStudio.CodeSweep.VSPackage
{
    interface IProjectConfigurationStore
    {
        ICollection<string> TermTableFiles { get; }
        ICollection<BuildTask.IIgnoreInstance> IgnoreInstances { get; }
        bool RunWithBuild { get; set; }
        bool HasConfiguration { get; }
        void CreateDefaultConfiguration();
    }
}
