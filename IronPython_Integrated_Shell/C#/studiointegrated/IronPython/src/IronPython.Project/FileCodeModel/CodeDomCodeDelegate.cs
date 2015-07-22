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
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class CodeDomCodeDelegate : CodeDomCodeType<CodeTypeDelegate>, CodeDelegate {
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private object type;
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        public CodeDomCodeDelegate(DTE dte, CodeElement parent, string name, object type)
            : base(dte, parent, name) {

            CodeObject = new CodeTypeDelegate(name);
            CodeObject.UserData[CodeKey] = this;

            this.type = type;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        public CodeDomCodeDelegate(DTE dte, CodeElement parent, CodeTypeDelegate delegateType)
            : base(dte, parent, (null==delegateType) ? null : delegateType.Name) {
            
            CodeObject = delegateType;
        }


        #region CodeDelegate Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">Required. The name of the parameter.</param>
        /// <param name="Type">Required. A vsCMTypeRef constant indicating the data type that the function returns. This can be a CodeTypeRef object, a vsCMTypeRef constant, or a fully qualified type name.</param>
        /// <param name="Position">Optional. Default = 0. The code element after which to add the new element. If the value is a CodeElement, then the new element is added immediately after it.
        /// 
        /// If the value is a Long, then AddParameter indicates the element after which to add the new element.
        /// 
        /// Because collections begin their count at 1, passing 0 indicates that the new element should be placed at the beginning of the collection. A value of -1 means the element should be placed at the end. 
        /// </param>
        /// <returns>A CodeParameter object. </returns>
        public CodeParameter AddParameter(string Name, object Type, object Position) {
            CodeParameter res = AddParameter(CodeObject.Parameters,
                Name,
                Type,
                Position);

            CommitChanges();
            return res;
        }

        public CodeClass BaseClass {
            get { throw new NotImplementedException(); }
        }

        public CodeElements Parameters {
            get {
                return GetParameters(CodeObject.Parameters);
            }
        }

        public void RemoveParameter(object Element) {
            RemoveParameter(CodeObject.Parameters, Element);

            CommitChanges();
        }

        public CodeTypeRef Type {
            get {
                return CodeDomCodeTypeRef.FromCodeTypeReference(CodeObject.ReturnType);
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                CodeObject.ReturnType = CodeDomCodeTypeRef.ToCodeTypeReference(value);
            }
        }

        public string get_Prototype(int Flags) {
            throw new NotImplementedException();
        }

        #endregion

        public override vsCMElement Kind {
            get {
                return vsCMElement.vsCMElementDelegate;
            }
        }

    }

}
