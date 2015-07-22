/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using EnvDTE;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {
    [ComVisible(true)]
    [SuppressMessage("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable")]
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    public class CodeDomCodeInterface : CodeDomCodeType<CodeTypeDeclaration>, CodeInterface {
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        public CodeDomCodeInterface(DTE dte, CodeElement parent, string name, object baseType, vsCMAccess access)
            : base(dte, parent, name) {

            CodeObject = new CodeTypeDeclaration(name);
            CodeObject.IsInterface = true;
            CodeObject.UserData[CodeKey] = this;

            if (baseType != System.Reflection.Missing.Value) {
                baseType = new object[] { baseType };
            }

            Initialize(System.Reflection.Missing.Value, baseType, access);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "2#decl")]
        public CodeDomCodeInterface(DTE dte, CodeElement parent, CodeTypeDeclaration decl)
            : base(dte, parent, (decl==null) ? null : decl.Name) {
            
            CodeObject = decl;
        }

        #region CodeInterface Members

        public CodeFunction AddFunction(string Name, vsCMFunction Kind, object Type, object Position, vsCMAccess Access) {
            CodeDomCodeFunction codeFunc = new CodeDomCodeFunction(DTE, this, Name, Kind, Type, Access);

            CodeObject.Members.Insert(PositionToIndex(Position), codeFunc.CodeObject);


            CommitChanges();
            return codeFunc;
        }

        public CodeProperty AddProperty(string GetterName, string PutterName, object Type, object Position, vsCMAccess Access, object Location) {
            //!!! parent
            CodeDomCodeProperty res = new CodeDomCodeProperty(DTE, null, GetterName, PutterName, Type, Access);

            CodeObject.Members.Insert(PositionToIndex(Position), res.CodeObject);

            CommitChanges();
            return res;
        }

        #endregion

        public override vsCMElement Kind {
            get {
                return vsCMElement.vsCMElementInterface;
            }
        }

    }

}
