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
    public class CodeDomCodeProperty : CodeDomCodeElement<CodeMemberProperty>, CodeProperty {
        private CodeClass parent;
        private CodeFunction getter;
        private CodeFunction setter;

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "putName")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        public CodeDomCodeProperty(DTE dte, CodeClass parent, string name, string putName, object type, vsCMAccess access)
            : base(dte, name) {
            this.parent = parent;

            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Name = name;
            prop.UserData[CodeKey] = this;
            CodeObject = prop;

            prop.Type = CodeDomCodeTypeRef.ToCodeTypeReference(ObjectToTypeRef(type));
            prop.Attributes = VSAccessToMemberAccess(access);
        }

        public CodeDomCodeProperty(CodeType parent, CodeMemberProperty property)
            : base((null==parent) ? null : parent.DTE, (null==property) ? null : property.Name) {
            //!!! need to set parent
            CodeObject = property;
        }

        #region CodeProperty Members

        public vsCMAccess Access {
            get {
                return MemberAccessToVSAccess(CodeObject.Attributes);
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                CodeObject.Attributes = VSAccessToMemberAccess(value);
            }
        }

        public CodeAttribute AddAttribute(string Name, string Value, object Position) {
            return AddCustomAttribute(CodeObject.CustomAttributes, Name, Value, Position);
        }

        public CodeElements Attributes {
            get { return GetCustomAttributes(CodeObject.CustomAttributes); }
        }

        public string Comment {
            get {
                return GetComment(CodeObject.Comments, false);
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                ReplaceComment(CodeObject.Comments, value, false);

                CommitChanges();
            }
        }

        public string DocComment {
            get {
                return GetComment(CodeObject.Comments, true);
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                ReplaceComment(CodeObject.Comments, value, true);

                CommitChanges();
            }
        }

        public CodeFunction Getter {
            get {
                if (getter == null) {
                    if (CodeObject.HasGet) {
                        getter = new CodeDomCodeFunction(DTE,
                            (CodeElement)parent,
                            "get_" + Name,
                            vsCMFunction.vsCMFunctionPropertyGet,
                            CodeDomCodeTypeRef.FromCodeTypeReference(CodeObject.Type),
                            MemberAccessToVSAccess(CodeObject.Attributes));
                    }
                }
                return getter;
            }
            [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.ArgumentException.#ctor(System.String)")]
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                CodeDomCodeFunction cdcf = value as CodeDomCodeFunction;
                if (cdcf == null && value != null) {
                    throw new ArgumentException("value must be CodeDomCodeFunction");
                }

                if (value == null) {
                    CodeObject.HasGet = false;
                } else {
                    CodeObject.GetStatements.AddRange(cdcf.CodeObject.Statements);
                    CodeObject.HasGet = true;
                }
            }
        }

        public CodeFunction Setter {
            get {
                if (setter == null) {
                    if (CodeObject.HasSet) {
                        setter = new CodeDomCodeFunction(DTE,
                            (CodeElement)parent,
                            "set_" + Name,
                            vsCMFunction.vsCMFunctionPropertyGet,
                            CodeDomCodeTypeRef.FromCodeTypeReference(CodeObject.Type),
                            MemberAccessToVSAccess(CodeObject.Attributes));
                    }
                }
                return setter;
            }
            [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.ArgumentException.#ctor(System.String)")]
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                CodeDomCodeFunction cdcf = value as CodeDomCodeFunction;
                if (cdcf == null && value != null) {
                    throw new ArgumentException("value must be CodeDomCodeFunction");
                }

                if (value == null) {
                    CodeObject.HasSet = false;
                } else {                    
                    CodeObject.SetStatements.AddRange(cdcf.CodeObject.Statements);
                    CodeObject.HasSet = true;
                }
            }
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

        public CodeClass Parent {
            get { return parent; }
        }

        public string get_Prototype(int Flags) {
            throw new NotImplementedException();
        }

        #endregion

        public override object ParentElement {
            get { return parent; }
        }

        public override CodeElements Children {
            get { throw new NotImplementedException(); }
        }

        public override CodeElements Collection {
            get { return parent.Children; }
        }

        public override string FullName {
            get { return CodeObject.Name; }
        }

        public override vsCMElement Kind {
            get {
                return vsCMElement.vsCMElementProperty;
            }
        }

        public override ProjectItem ProjectItem {
            get { return parent.ProjectItem; }
        }
    }
}
