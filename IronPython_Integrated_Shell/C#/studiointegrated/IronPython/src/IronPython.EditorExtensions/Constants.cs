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

namespace IronPython.EditorExtensions
{
    internal static class Constants
    {
        /// <summary>
        /// The Guid of the IPy language service
        /// </summary>
        internal static readonly Guid IronPythonLanguageServiceGuid = new Guid("{ae8ce01a-b3ff-4c19-8c80-54669c197f2c}");

        /// <summary>
        /// Word separators chars
        /// </summary>
        internal static char[] Separators = new[] { '\n', '\r', '\t', ' ', ':', '(', ')', '[', ']', '{', '}', '?', '/', '+', '-', ';', '=', '*', '!', ',', '<', '>' };

        /// <summary>
        /// Word separators chars including dot
        /// </summary>
        internal static char[] SeparatorsPlusDot = Separators.Union(new[] {'.'}).ToArray();
    }
}
