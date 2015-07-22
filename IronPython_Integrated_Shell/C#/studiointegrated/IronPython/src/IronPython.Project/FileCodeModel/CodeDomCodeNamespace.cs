/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using EnvDTE;
using VSCodeNamespace = EnvDTE.CodeNamespace;
using CDCodeNamespace = System.CodeDom.CodeNamespace;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {

    [ComVisible(true)]
    [SuppressMessage("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable")]
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    public class CodeDomCodeNamespace : CodeDomCodeElement<CDCodeNamespace>, VSCodeNamespace {
        private CodeElement parent;

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        public CodeDomCodeNamespace(DTE dte, string name, CodeElement parent)
            : base(dte, name) {
            this.parent = parent;
        }

        #region VSCodeNamespace Members

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        protected int GetNewIndex(object position) {
            CodeElement ce;
            if (position is int || position is long) {
                int res = (position is long) ? (int)(long)position : (int)position;

                if (res == -1) return CodeObject.Types.Count;

                return res;
            } else if ((ce = position as CodeElement) != null) {
                for (int i = 0; i < CodeObject.Types.Count; i++) {
                    if (CodeObject.Types[i].UserData[CodeKey] == ce) return i;
                }
            }
            return 0;
        }

        public CodeClass AddClass(string Name, object Position, object Bases, object ImplementedInterfaces, vsCMAccess Access) {
            CodeDomCodeClass res = new CodeDomCodeClass(DTE, this, Name, Bases, ImplementedInterfaces, Access);
            AddItem<CodeTypeDeclaration>(res.CodeObject, Position);

            return res;
        }

        public CodeDelegate AddDelegate(string Name, object Type, object Position, vsCMAccess Access) {
            CodeDomCodeDelegate res = new CodeDomCodeDelegate(DTE, this, Name, Type);
            AddItem<CodeTypeDelegate>(res.CodeObject, Position);

            return res;
        }

        public CodeEnum AddEnum(string Name, object Position, object Bases, vsCMAccess Access) {
            CodeDomCodeEnum ce = new CodeDomCodeEnum(DTE, this, Name, Bases, Access);
            AddItem<CodeTypeDeclaration>(ce.CodeObject, Position);

            return ce;
        }

        public CodeInterface AddInterface(string Name, object Position, object Bases, vsCMAccess Access) {
            CodeDomCodeInterface ci = new CodeDomCodeInterface(DTE, this, Name, Bases, Access);
            AddItem<CodeTypeDeclaration>(ci.CodeObject, Position);

            return ci;
        }

        public CodeStruct AddStruct(string Name, object Position, object Bases, object ImplementedInterfaces, vsCMAccess Access) {
            CodeDomCodeStruct ce = new CodeDomCodeStruct(DTE, this, Name, Bases, ImplementedInterfaces, Access);

            AddItem<CodeTypeDeclaration>(ce.CodeObject, Position);
            return ce;
        }

        public EnvDTE.CodeNamespace AddNamespace(string Name, object Position) {
            throw new NotImplementedException();
        }

        public string Comment {
            get {
                return GetComment(CodeObject.Comments, false);
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                ReplaceComment(CodeObject.Comments, value, true);
                CommitChanges();
            }
        }

        public string DocComment {
            get {
                return GetComment(CodeObject.Comments, false);
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                ReplaceComment(CodeObject.Comments, value, true);
                CommitChanges();
            }
        }

        public CodeElements Members {
            [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
            get {
                Debug.Assert(CodeObject != null);

                CodeDomCodeElements elements = new CodeDomCodeElements(DTE, this);
                foreach (CodeTypeDeclaration ctd in CodeObject.Types) {
                    if (ctd.UserData[CodeKey] == null) {
                        if (ctd.IsClass) {
                            ctd.UserData[CodeDomFileCodeModel.CodeKey] = new CodeDomCodeClass(DTE, this, ctd);
                        } else if (ctd.IsInterface) {
                            ctd.UserData[CodeDomFileCodeModel.CodeKey] = new CodeDomCodeInterface(DTE, this, ctd);
                        } else if (ctd.IsEnum) {
                            ctd.UserData[CodeDomFileCodeModel.CodeKey] = new CodeDomCodeEnum(DTE, this, ctd);
                        } else if (ctd.IsStruct) {
                            ctd.UserData[CodeDomFileCodeModel.CodeKey] = new CodeDomCodeStruct(DTE, this, ctd);
                        } else if (ctd is CodeTypeDelegate) {
                            ctd.UserData[CodeDomFileCodeModel.CodeKey] = new CodeDomCodeDelegate(DTE, this, (CodeTypeDelegate)ctd);
                        }
                    }

                    elements.Add((CodeElement)ctd.UserData[CodeKey]);
                }

                return elements;
            }
        }

        public dynamic Parent {
            get { return parent; }
        }

        public void Remove(object Element) {
            foreach (CodeTypeDeclaration ctd in CodeObject.Types) {
                if (ctd.UserData[CodeKey] == Element) {
                    CodeObject.Types.Remove(ctd);
                    return;
                }
            }

            int index = ((int)Element) - 1;
            CodeObject.Types.RemoveAt(index);

            CommitChanges();
        }

        #endregion

        private void AddItem<T>(T ctd, object Position)
            where T : CodeTypeDeclaration
        {
            int position = GetNewIndex(Position);

            CodeObject.Types.Insert(position, ctd);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMethodsAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "access")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "kind")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "name")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "position")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "type")]
        public CodeFunction AddFunction(string name, vsCMFunction kind, object type, object position, vsCMAccess access) {
            return null;
        }

        public override CodeElements Children {
            get { return Members; }
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
                return vsCMElement.vsCMElementNamespace;
            }
        }

        public override ProjectItem ProjectItem {
            get { return parent.ProjectItem; }
        }
    }

}
