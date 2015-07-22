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
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace IronPython.EditorExtensions
{
    /// <summary>
    /// Exports the iron python content type and file extension
    /// </summary>
    public sealed class PyContentTypeDefinition
    {
        public const string ContentType = "IronPython";
        public const string ConsoleContentType = "IronPythonConsole";

        /// <summary>
        /// Exports the IPy content type
        /// </summary>
        [Export(typeof(ContentTypeDefinition))]
        [Name(PyContentTypeDefinition.ContentType)]
        [BaseDefinition("code")]
        public ContentTypeDefinition IPyContentType { get; set; }

        /// <summary>
        /// Exports the IPy Console content type
        /// </summary>
        [Export(typeof(ContentTypeDefinition))]
        [Name(PyContentTypeDefinition.ConsoleContentType)]
        [BaseDefinition("code")]
        public ContentTypeDefinition IPyContentTypeConsole { get; set; }

        /// <summary>
        /// Exports the IPy file extension
        /// </summary>
		[Export(typeof(FileExtensionToContentTypeDefinition))]
		[ContentType(PyContentTypeDefinition.ContentType)]
        [FileExtension(".py")]
        public FileExtensionToContentTypeDefinition IPyFileExtension { get; set; }
    }
}