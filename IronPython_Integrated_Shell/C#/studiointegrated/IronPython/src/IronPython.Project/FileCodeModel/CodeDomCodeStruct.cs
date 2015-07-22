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
    public class CodeDomCodeStruct : CodeDomCodeType<CodeTypeDeclaration>, CodeStruct {
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        public CodeDomCodeStruct(DTE dte, CodeElement parent, string name, object bases, object implementedInterfaces, vsCMAccess access)
            : base(dte, parent, name) {

            CodeObject = new CodeTypeDeclaration(name);
            CodeObject.IsStruct = true;
            CodeObject.UserData[CodeKey] = this;

            Initialize(bases, implementedInterfaces, access);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        public CodeDomCodeStruct(DTE dte, CodeElement parent, CodeTypeDeclaration declaration)
            : base(dte, parent, (null==declaration) ? null : declaration.Name) {
            
            CodeObject = declaration;
        }


        #region CodeStruct Members

        public CodeClass AddClass(string Name, object Position, object Bases, object ImplementedInterfaces, vsCMAccess Access) {
            CodeDomCodeClass codeClass = new CodeDomCodeClass(DTE, this, Name, Bases, ImplementedInterfaces, Access);

            CodeObject.Members.Insert(PositionToIndex(Position), codeClass.CodeObject);

            CommitChanges();

            return codeClass;
        }

        public CodeDelegate AddDelegate(string Name, object Type, object Position, vsCMAccess Access) {
            CodeDomCodeDelegate codeDelegate = new CodeDomCodeDelegate(DTE, this, Name, Type);

            CodeObject.Members.Insert(PositionToIndex(Position), codeDelegate.CodeObject);

            CommitChanges();

            return codeDelegate;
        }

        public CodeEnum AddEnum(string Name, object Position, object Bases, vsCMAccess Access) {
            CodeDomCodeEnum codeEnum = new CodeDomCodeEnum(DTE, this, Name, Bases, Access);

            CodeObject.Members.Insert(PositionToIndex(Position), codeEnum.CodeObject);

            CommitChanges();

            return codeEnum;
        }

        public CodeFunction AddFunction(string Name, vsCMFunction Kind, object Type, object Position, vsCMAccess Access, object Location) {
            CodeDomCodeFunction codeFunc = new CodeDomCodeFunction(DTE, this, Name, Kind, Type, Access);

            CodeObject.Members.Insert(PositionToIndex(Position), codeFunc.CodeObject);

            CommitChanges();

            return codeFunc;
        }

        public CodeInterface AddImplementedInterface(object Base, object Position) {
            throw new NotImplementedException();
        }

        public CodeProperty AddProperty(string GetterName, string PutterName, object Type, object Position, vsCMAccess Access, object Location) {
            //!!! parent
            CodeDomCodeProperty res = new CodeDomCodeProperty(DTE, null, GetterName, PutterName, Type, Access);

            CodeObject.Members.Insert(PositionToIndex(Position), res.CodeObject);

            CommitChanges();

            return res;
        }

        public CodeStruct AddStruct(string Name, object Position, object Bases, object ImplementedInterfaces, vsCMAccess Access) {
            CodeDomCodeStruct codeStruct = new CodeDomCodeStruct(DTE, this, Name, Bases, ImplementedInterfaces, Access);

            CodeObject.Members.Insert(PositionToIndex(Position), codeStruct.CodeObject);

            CommitChanges();

            return codeStruct;
        }

        public CodeVariable AddVariable(string Name, object Type, object Position, vsCMAccess Access, object Location) {
            CodeDomCodeVariable codeVar = new CodeDomCodeVariable(DTE, 
                                        this, 
                                        Name, 
                                        ObjectToTypeRef(Type), 
                                        Access);

            CodeObject.Members.Insert(PositionToIndex(Position), codeVar.CodeObject);

            CommitChanges();

            return codeVar;
        }

        public bool IsAbstract {
            get {
                return (CodeObject.Attributes & MemberAttributes.Abstract) != 0;
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                if (value) CodeObject.Attributes |= MemberAttributes.Abstract;
                else CodeObject.Attributes &= ~MemberAttributes.Abstract;

                CommitChanges();

            }
        }

        public void RemoveInterface(object Element) {
            int index = 0;
            foreach (CodeTypeReference typeRef in CodeObject.BaseTypes) {
                if (Element == ((CodeDomCodeTypeRef)typeRef.UserData[CodeKey]).CodeType) {
                    CodeObject.BaseTypes.RemoveAt(index);
                    break;
                }
                index++;
            }

            CommitChanges();
        }

        #endregion

        public override vsCMElement Kind {
            get {
                return vsCMElement.vsCMElementStruct;
            }
        }

    }

}
