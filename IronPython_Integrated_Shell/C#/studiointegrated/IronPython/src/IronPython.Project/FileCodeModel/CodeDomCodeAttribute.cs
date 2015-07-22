/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using EnvDTE;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {
    [ComVisible(true)]
    [SuppressMessage("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable")]
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class CodeDomCodeAttribute : SimpleCodeElement, ICodeDomElement, CodeAttribute {
        private CodeElement parent;
        private string attrValue;
        private CodeAttributeDeclaration attr;

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        public CodeDomCodeAttribute(DTE dte, CodeElement parent, string name)
            : base(dte, name) {
            this.parent = parent;
            CodeAttributeDeclaration cad = new CodeAttributeDeclaration();
            // !!! name, value

            CodeObject = cad;
        }

        public override TextPoint EndPoint {
            get { throw new NotImplementedException(); }
        }

        public override TextPoint StartPoint {
            get { throw new NotImplementedException(); }
        }

        public override TextPoint GetEndPoint(vsCMPart Part) {
            throw new NotImplementedException();
        }

        public override TextPoint GetStartPoint(vsCMPart Part) {
            throw new NotImplementedException();
        }


        #region CodeAttribute Members


        public void Delete() {
            throw new NotImplementedException();
        }

        public dynamic Parent {
            get {
                return parent;
            }
        }

        public string Value {
            get {
                return attrValue;
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                this.attrValue = value;
            }
        }

        #endregion

        #region ICodeDomElement Members

        public object UntypedCodeObject {
            get { return attr; }
        }

        public object ParentElement {
            get {
                return parent;
            }
        }

        #endregion

        public CodeAttributeDeclaration CodeObject {
            get {
                return attr;
            }
            set {
                attr = value;
            }
        }

        public override CodeElements Children {
            get { throw new System.NotImplementedException(); }
        }

        public override CodeElements Collection {
            get { return parent.Children; }
        }


        public override string FullName {
            get { return CodeObject.AttributeType.BaseType; }
        }

        public override vsCMElement Kind {
            get {
                return vsCMElement.vsCMElementAttribute;
            }
        }

        public override ProjectItem ProjectItem {
            get { return parent.ProjectItem; }
        }

    }

}
