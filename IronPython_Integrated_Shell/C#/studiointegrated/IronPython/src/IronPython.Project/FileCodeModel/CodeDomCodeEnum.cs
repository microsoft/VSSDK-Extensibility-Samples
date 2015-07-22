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
    public class CodeDomCodeEnum : CodeDomCodeType<CodeTypeDeclaration>, CodeEnum {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "bases")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        public CodeDomCodeEnum(DTE dte, CodeElement parent, string name, object bases, vsCMAccess access)
            : base(dte, parent, name) {

            CodeObject = new CodeTypeDeclaration(name);
            CodeObject.IsEnum = true;
            CodeObject.UserData[CodeKey] = this;

            Initialize(Bases, System.Reflection.Missing.Value, access);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "2#decl")]
        public CodeDomCodeEnum(DTE dte, CodeElement parent, CodeTypeDeclaration decl)
            : base(dte, parent, (null==decl) ? null : decl.Name) {
            
            CodeObject = decl;
        }

        #region CodeEnum Members

        public CodeVariable AddMember(string Name, object Value, object Position) {
            CommitChanges();

            throw new NotImplementedException();
        }

        #endregion

        public override vsCMElement Kind {
            get {
                return vsCMElement.vsCMElementEnum;
            }
        }

    }

}
