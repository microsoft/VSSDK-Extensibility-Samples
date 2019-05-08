/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.SccProvider
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    internal sealed class ProvideSolutionProps : RegistrationAttribute
    {
        private string _propName;

        public ProvideSolutionProps(string propName)
        {
            _propName = propName;
        }

        public override void Register(RegistrationContext context)
        {
            context.Log.WriteLine(string.Format(CultureInfo.InvariantCulture, "ProvideSolutionProps: ({0} = {1})", context.ComponentType.GUID.ToString("B"), PropName));

            Key childKey = null;

            try
            {
                childKey = context.CreateKey(string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", "SolutionPersistence", PropName));

                childKey.SetValue(string.Empty, context.ComponentType.GUID.ToString("B"));
            }
            finally
            {
                if (childKey != null) childKey.Close();
            }
        }

        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey(string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", "SolutionPersistence", PropName));
        }

        public string PropName { get { return _propName; } }
    }
}
