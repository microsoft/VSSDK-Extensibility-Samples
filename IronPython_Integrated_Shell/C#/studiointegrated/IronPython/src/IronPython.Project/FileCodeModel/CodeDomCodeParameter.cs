/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.CodeDom;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using EnvDTE;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {
    [ComVisible(true)]
    [SuppressMessage("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable")]
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    public class CodeDomCodeParameter : CodeDomCodeElement<CodeParameterDeclarationExpression>, CodeParameter {
        private CodeElement parent;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private CodeTypeRef type;

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        public CodeDomCodeParameter(DTE dte, CodeElement parent, string name, CodeTypeRef type)
            : base(dte, name) {
            this.parent = parent;
            this.type = type;

            CodeObject = new CodeParameterDeclarationExpression(
                CodeDomCodeTypeRef.ToCodeTypeReference(type),
                name);
            CodeObject.UserData[CodeKey] = this;
        }

        public CodeDomCodeParameter(CodeElement parent, CodeParameterDeclarationExpression parameter) 
            : base((null==parent) ? null : parent.DTE, (null==parameter) ? null : parameter.Name) {
            CodeObject = parameter;
        }

        [SuppressMessage("Microsoft.Interoperability", "CA1407:AvoidStaticMembersInComVisibleTypes")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static CodeDomCodeParameter GetCodeParameter(CodeParameterDeclarationExpression declaration) {
            if (null == declaration) {
                throw new ArgumentNullException("declaration");
            }
            return (CodeDomCodeParameter)declaration.UserData[CodeKey];
        }

        #region CodeParameter Members

        public CodeAttribute AddAttribute(string Name, string Value, object Position) {
            CodeAttribute res = AddCustomAttribute(CodeObject.CustomAttributes, Name, Value, Position);
            CommitChanges();
            return res;
        }

        public CodeElements Attributes {
            get { return GetCustomAttributes(CodeObject.CustomAttributes); }
        }

        public string DocComment {
            get {
                throw new NotImplementedException();
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                throw new NotImplementedException();
            }
        }

        public CodeElement Parent {
            get { return parent; }
        }

        public CodeTypeRef Type {
            get {
                return CodeDomCodeTypeRef.FromCodeTypeReference(CodeObject.Type);
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                CodeObject.Type = CodeDomCodeTypeRef.ToCodeTypeReference(value);

                CommitChanges();
            }
        }

        #endregion

        public override CodeElements Children {
            get { throw new NotImplementedException(); }
        }

        public override CodeElements Collection {
            get { return parent.Children; }
        }

        public override object ParentElement {
            get { return parent; }
        }

        public override string FullName {
            get { return CodeObject.Name; }
        }

        public override vsCMElement Kind {
            get {
                return vsCMElement.vsCMElementParameter;
            }
        }

        public override ProjectItem ProjectItem {
            get { return parent.ProjectItem; }
        }

    }
}

