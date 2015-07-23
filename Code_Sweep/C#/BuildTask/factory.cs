/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System.Collections.Generic;
using System.Text;

namespace Microsoft.Samples.VisualStudio.CodeSweep.BuildTask
{
    /// <summary>
    /// Factory methods for creation of objects whose implementation is internal to the BuildTask project.
    /// </summary>
    static public class Factory
    {
        /// <summary>
        /// Creates an IIgnoreInstance implementation from the specified arguments.
        /// </summary>
        public static IIgnoreInstance GetIgnoreInstance(string file, string lineText, string term, int column)
        {
            return new IgnoreInstance(file, lineText, term, column);
        }

        /// <summary>
        /// Creates an IIgnoreInstance implementation from the specified arguments.
        /// </summary>
        public static IIgnoreInstance DeserializeIgnoreInstance(string serializedInstance, string projectFolderForRelativization)
        {
            return new IgnoreInstance(serializedInstance, projectFolderForRelativization);
        }

        /// <summary>
        /// Creates a collection of IIgnoreInstance implementations from the specified arguments.
        /// </summary>
        /// <remarks>
        /// This is the opposite of SerializeIgnoreInstances; you can pass its output to this method.
        /// </remarks>
        public static IEnumerable<IIgnoreInstance> DeserializeIgnoreInstances(string serializedInstances, string projectFolderForRelativization)
        {
            if (serializedInstances != null)
            {
                foreach (string ignoreInstance in Utilities.ParseEscaped(serializedInstances, ';'))
                {
                    if (ignoreInstance != null && ignoreInstance.Length > 0)
                    {
                        yield return DeserializeIgnoreInstance(ignoreInstance, projectFolderForRelativization);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a string representation of a collection of IIgnoreInstance objects.
        /// </summary>
        /// <remarks>
        /// This is the opposite of DeserializeIgnoreInstances; you can pass its output to this method.
        /// </remarks>
        public static string SerializeIgnoreInstances(IEnumerable<IIgnoreInstance> instances, string projectFolderForRelativization)
        {
            StringBuilder result = new StringBuilder();

            foreach (IIgnoreInstance instance in instances)
            {
                if (result.Length > 0)
                {
                    result.Append(';');
                }
                result.Append(Utilities.EscapeChar(instance.Serialize(projectFolderForRelativization), ';'));
            }

            return result.ToString();
        }
    }
}
