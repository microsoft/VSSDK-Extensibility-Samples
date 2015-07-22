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
using EnvDTE;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {
    [ComVisible(true)]
    [SuppressMessage("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable")]
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    public class CodeDomCodeClass : CodeDomCodeType<CodeTypeDeclaration>, CodeClass {
        //!!! need to deal w/ indexing of interfaces & bases - we combine them in CodeDom but not here.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        public CodeDomCodeClass(DTE dte, CodeElement parent, string name, object bases, object interfaces, vsCMAccess access)
            : base(dte, parent, name) {
            
            CodeObject = new CodeTypeDeclaration(name);
            CodeObject.UserData[CodeKey] = this;
            CodeObject.IsClass = true;

            Initialize(bases, interfaces, access);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        public CodeDomCodeClass(DTE dte, CodeElement parent, CodeTypeDeclaration declaration)
            : base(dte, parent, (null==declaration) ? null : declaration.Name) {
            
            CodeObject = declaration;
        }


        #region CodeClass Members

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

        /// <summary>
        /// AddImplementedInterface adds a reference to an interface that the CodeClass promises to implement. AddImplementedInterface does not insert method stubs for the interface members.
        /// </summary>
        /// <param name="Base">Required. The interface the class will implement. This is either a CodeInterface or a fully qualified type name.</param>
        /// <param name="Position">Optional. Default = 0. The code element after which to add the new element. If the value is a CodeElement, then the new element is added immediately after it.
        /// 
        /// If the value is a Long data type, then AddImplementedInterface indicates the element after which to add the new element.
        /// 
        /// Because collections begin their count at 1, passing 0 indicates that the new element should be placed at the beginning of the collection. A value of -1 means the element should be placed at the end. 
        /// </param>
        /// <returns>A CodeInterface object. </returns>
        public CodeInterface AddImplementedInterface(object Base, object Position) {
            CodeTypeReference ctr;
            CodeDomCodeInterface iface = Base as CodeDomCodeInterface;

            if (iface != null)
                ctr = new CodeTypeReference(iface.FullName);
            else
                ctr = new CodeTypeReference((string)Base);
                        
            CodeObject.BaseTypes.Insert(PositionToInterfaceIndex(Position), ctr);

            CommitChanges();

            return new CodeDomCodeInterface(DTE, this, ctr.BaseType, System.Reflection.Missing.Value, vsCMAccess.vsCMAccessDefault);
        }

        public CodeProperty AddProperty(string GetterName, string PutterName, object Type, object Position, vsCMAccess Access, object Location) {                        
            CodeDomCodeProperty res = new CodeDomCodeProperty(DTE, this, GetterName, PutterName, Type, Access);
            
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

        /// <summary>
        /// Creates a new variable code construct and inserts the code in the correct location. 
        /// </summary>
        /// <param name="Name">Required. The name of the new variable.</param>
        /// <param name="Type">Required. A vsCMTypeRef constant indicating the data type that the function returns. This can be a CodeTypeRef object, a vsCMTypeRef constant, or a fully qualified type name.</param>
        /// <param name="Position">Optional. Default = 0. The code element after which to add the new element. If the value is a CodeElement, then the new element is added immediately after it.
        /// 
        /// If the value is a Long, then AddVariable indicates the element after which to add the new element.
        ///
        /// Because collections begin their count at 1, passing 0 indicates that the new element should be placed at the beginning of the collection. A value of -1 means the element should be placed at the end. 
        /// </param>
        /// <param name="Access">Optional. A vsCMAccess constant.</param>
        /// <param name="Location">Optional. The path and file name for the new variable definition. Depending on the language, the file name is either relative or absolute to the project file. The file is added to the project if it is not already a project item. If the file cannot be created and added to the project, then AddVariable fails.</param>
        /// <returns></returns>
        public CodeVariable AddVariable(string Name, object Type, object Position, vsCMAccess Access, object Location) {
            CodeDomCodeVariable codeVar = new CodeDomCodeVariable(DTE, this, Name, ObjectToTypeRef(Type), Access);

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
                else CodeObject.Attributes &= ~(MemberAttributes.Abstract);

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
                return vsCMElement.vsCMElementClass;
            }
        }

    }

}
