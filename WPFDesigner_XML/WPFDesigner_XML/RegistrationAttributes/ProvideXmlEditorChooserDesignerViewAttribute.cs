/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Text;
using System.IO;

namespace Microsoft.VisualStudio.Shell
{
    /// <summary>
    /// Used to provide registration information to the XML Chooser
    /// for a custom XML designer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ProvideXmlEditorChooserDesignerViewAttribute : RegistrationAttribute
    {
        const string XmlChooserFactory = "XmlChooserFactory";
        const string XmlChooserEditorExtensionsKeyPath = @"Editors\{32CC8DFA-2D70-49b2-94CD-22D57349B778}\Extensions";
        const string XmlEditorFactoryGuid = "{FA3CD31E-987B-443A-9B81-186104E8DAC1}";


        private string name;
        private string extension;
        private Guid defaultLogicalView;
        private int xmlChooserPriority;

        /// <summary>
        /// Constructor for ProvideXmlEditorChooserDesignerViewAttribute.
        /// </summary>
        /// <param name="name">The registry keyName for your XML editor. For example "RESX", "Silverlight", "Workflow", etc...</param>
        /// <param name="extension">The file extension for your custom XML type (e.g. "xaml", "resx", "xsd").</param>
        /// <param name="defaultLogicalViewEditorFactory">A Type, Guid, or String object representing the editor factory for the default logical view.</param>
        /// <param name="xmlChooserPriority">The priority of the extension in the XML Chooser. This value must be greater than the extension's priority value for the XML designer's EditorFactory.</param>
        public ProvideXmlEditorChooserDesignerViewAttribute(string name, string extension, object defaultLogicalViewEditorFactory, int xmlChooserPriority)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Editor description cannot be null or empty.", "editorDescription");
            }
            if (string.IsNullOrWhiteSpace(extension))
            {
                throw new ArgumentException("Extension cannot be null or empty.", "extension");
            }
            if (defaultLogicalViewEditorFactory == null)
            {
                throw new ArgumentNullException("defaultLogicalViewEditorFactory");
            }

            this.name = name;
            this.extension = extension;
            this.defaultLogicalView = TryGetGuidFromObject(defaultLogicalViewEditorFactory);
            this.xmlChooserPriority = xmlChooserPriority;

            this.CodeLogicalViewEditor = XmlEditorFactoryGuid;
            this.DebuggingLogicalViewEditor = XmlEditorFactoryGuid;
            this.DesignerLogicalViewEditor = XmlEditorFactoryGuid;
            this.TextLogicalViewEditor = XmlEditorFactoryGuid;
        }

        public override void Register(RegistrationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            using (Key xmlChooserExtensions = context.CreateKey(XmlChooserEditorExtensionsKeyPath))
            {
                xmlChooserExtensions.SetValue(extension, xmlChooserPriority);
            }

            using (Key key = context.CreateKey(GetKeyName()))
            {
                key.SetValue("DefaultLogicalView", defaultLogicalView.ToString("B").ToUpperInvariant());
                key.SetValue("Extension", extension);
                
                if (!string.IsNullOrWhiteSpace(Namespace))
                {
                    key.SetValue("Namespace", Namespace);
                }

                if (MatchExtensionAndNamespace)
                {
                    key.SetValue("Match", "both");
                }

                if (IsDataSet.HasValue)
                {
                    key.SetValue("IsDataSet", Convert.ToInt32(IsDataSet.Value));
                }

                SetLogicalViewMapping(key, VSConstants.LOGVIEWID_Debugging, DebuggingLogicalViewEditor);
                SetLogicalViewMapping(key, VSConstants.LOGVIEWID_Code, CodeLogicalViewEditor);
                SetLogicalViewMapping(key, VSConstants.LOGVIEWID_Designer, DesignerLogicalViewEditor);
                SetLogicalViewMapping(key, VSConstants.LOGVIEWID_TextView, TextLogicalViewEditor);
            }
        }

        private void SetLogicalViewMapping(Key key, Guid logicalView, object editorFactory)
        {
            if (editorFactory != null)
            {
                key.SetValue(logicalView.ToString("B").ToUpperInvariant(), TryGetGuidFromObject(editorFactory).ToString("B").ToUpperInvariant());
            }
        }

        private Guid TryGetGuidFromObject(object guidObject)
        {
            // figure out what type of object they passed in and get the GUID from it
            if (guidObject is string)
                return new Guid((string)guidObject);
            else if (guidObject is Type)
                return ((Type)guidObject).GUID;
            else if (guidObject is Guid)
                return (Guid)guidObject;
            else
                throw new ArgumentException("Could not determine Guid from supplied object.", "guidObject");
        }

        public override void Unregister(RegistrationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            context.RemoveKey(GetKeyName());

            context.RemoveValue(XmlChooserEditorExtensionsKeyPath, extension);
            context.RemoveKeyIfEmpty(XmlChooserEditorExtensionsKeyPath);
        }

        private string GetKeyName()
        {
            return Path.Combine(XmlChooserFactory, name);
        }

        /// <summary>
        /// The XML Namespace used in documents that this editor supports.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Boolean value indicating whether the XML chooser should match on both the file extension
        /// and the Namespace. If false, the XML chooser will match on either the extension or the 
        /// Namespace.
        /// </summary>
        public bool MatchExtensionAndNamespace { get; set; }

        /// <summary>
        /// Special value used only by the DataSet designer.
        /// </summary>
        public bool? IsDataSet { get; set; }

        /// <summary>
        /// The editor factory to associate with the debugging logical view
        /// </summary>
        public object DebuggingLogicalViewEditor { get; set; }

        /// <summary>
        /// The editor factory to associate with the code logical view
        /// </summary>
        public object CodeLogicalViewEditor { get; set; }

        /// <summary>
        /// The editor factory to associate with the designer logical view
        /// </summary>
        public object DesignerLogicalViewEditor { get; set; }

        /// <summary>
        /// The editor factory to associate with the text logical view
        /// </summary>
        public object TextLogicalViewEditor { get; set; }
    }
}
