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
    public abstract class CodeDomCodeType<CodeTypeType> : CodeDomCodeElement<CodeTypeType>, CodeType 
        where CodeTypeType : CodeTypeDeclaration {
        private CodeElement parent;

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        protected CodeDomCodeType(DTE dte, CodeElement parent, string name)
            : base(dte, name) {
            this.parent = parent;
        }

        protected void Initialize(object bases, object interfaces, vsCMAccess access) {
            if (bases == System.Reflection.Missing.Value) {
                CodeObject.BaseTypes.Add(new CodeTypeReference("System.Object"));
            } else {
                object[] baseClasses = bases as object[];
                foreach (object baseCls in baseClasses) {
                    CodeObject.BaseTypes.Add(new CodeTypeReference(ObjectToClassName(baseCls)));
                }
            }

            if (interfaces != System.Reflection.Missing.Value) {
                object[] interfaceClasses = interfaces as object[];
                foreach (object baseCls in interfaceClasses) {
                    CodeObject.BaseTypes.Add(new CodeTypeReference(ObjectToClassName(baseCls)));
                }
            }

            CodeObject.Attributes = VSAccessToMemberAccess(access);
        }

        public override CodeElements Collection {
            get { return parent.Children; }
        }

        public override CodeElements Children {
            get { 
                //!!! and bases?
                return Members;
            }
        }

        #region CodeType Members

        public vsCMAccess Access {
            get { return MemberAccessToVSAccess(CodeObject.Attributes); }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set { CodeObject.Attributes = VSAccessToMemberAccess(value); }
        }

        public CodeAttribute AddAttribute(string Name, string Value, object Position) {
            return AddCustomAttribute(CodeObject.CustomAttributes, Name, Value, Position);
        }

        public CodeElement AddBase(object Base, object Position) {
            string clsName = ObjectToClassName(Base);
            CodeDomCodeTypeRef ctr = new CodeDomCodeTypeRef(DTE, clsName);

            CodeObject.BaseTypes.Insert(PositionToBaseIndex(Position), ctr.CodeObject);
            return ctr;
        }

        public CodeElements Attributes {
            get { return GetCustomAttributes(CodeObject.CustomAttributes); }
        }

        public CodeElements DerivedTypes {
            get { throw new NotImplementedException(); }
        }

        public EnvDTE.CodeNamespace Namespace {
            get {
                CodeDomCodeNamespace nsParent = parent as CodeDomCodeNamespace;
                if (nsParent != null) return nsParent;

                CodeType typeParent = parent as CodeType;
                if (typeParent != null) return typeParent.Namespace;

                return null;
            }
        }

        public bool get_IsDerivedFrom(string FullName) {
            throw new NotImplementedException();
        }

        public CodeElements Bases {
            get {
                CodeDomCodeElements res = new CodeDomCodeElements(DTE, this);
                foreach (CodeTypeReference baseType in CodeObject.BaseTypes) {
                    CodeElement ce = (CodeElement)baseType.UserData[CodeKey];
                    if (!(ce is CodeInterface))
                        res.Add(ce);
                }
                return res;
            }
        }

        public CodeElements ImplementedInterfaces {
            get {
                CodeDomCodeElements res = new CodeDomCodeElements(DTE, this);
                foreach (CodeTypeReference baseType in CodeObject.BaseTypes) {
                    CodeElement ce = (CodeElement)baseType.UserData[CodeKey];
                    if (ce is CodeInterface)
                        res.Add(ce);
                }
                return res;
            }
        }

        public string Comment {
            get {
                return GetComment(CodeObject.Comments, false);
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                ReplaceComment(CodeObject.Comments, value, false);

            }
        }
                
        public string DocComment {
            get {
                return GetComment(CodeObject.Comments, true);
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                ReplaceComment(CodeObject.Comments, value, true);
            }
        }

        public CodeElements Members {
            [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
            get {
                CodeDomCodeElements res = new CodeDomCodeElements(DTE, this);
                foreach (CodeTypeMember member in CodeObject.Members) {
                    if (member.UserData[CodeKey] == null) {
                        if (member is CodeMemberEvent) {
                            //member.UserData[CodeKey] = new CodeDomCode
                        } else if (member is CodeMemberField) {
                            CodeMemberField cmf = member as CodeMemberField;
                            member.UserData[CodeKey] = new CodeDomCodeVariable(this, cmf);
                        } else if (member is CodeMemberMethod) {
                            CodeMemberMethod cmm = member as CodeMemberMethod;
                            member.UserData[CodeKey] = new CodeDomCodeFunction(this, cmm);
                        } else if (member is CodeMemberProperty) {
                            CodeMemberProperty cmp = member as CodeMemberProperty;
                            member.UserData[CodeKey] = new CodeDomCodeProperty(this, cmp);
                        } else if (member is CodeSnippetTypeMember) {
                        } else if (member is CodeTypeDeclaration) {                            
                        }
                    }

                    res.Add((CodeElement)member.UserData[CodeKey]);
                }
                return res;
            }
        }

        public dynamic Parent {
            get { return parent; }
        }

        public void RemoveBase(object Element) {
            int index = ((CodeDomCodeElements)Bases).IndexOf((SimpleCodeElement)Element);

            CodeObject.BaseTypes.RemoveAt(index);
        }

        public void RemoveMember(object Element) {
            int index = ((CodeDomCodeElements)Members).IndexOf((SimpleCodeElement)Element);

            ((CodeDomCodeElements)Members).RemoveAt(index);
        }

        #endregion
        
        protected int PositionToIndex(object position) {
            ICodeDomElement icde = position as ICodeDomElement;
            if (icde != null) {
                return CodeObject.Members.IndexOf((CodeTypeMember)icde.UntypedCodeObject) + 1;
            }

            if (position == System.Reflection.Missing.Value) {
                return CodeObject.Members.Count;
            }

            int pos = (int)position;
            if (pos == -1) {
                return CodeObject.Members.Count;
            }
            return pos;
        }

        protected int PositionToInterfaceIndex(object position) {
            return PositionToInterfaceOrBaseIndex(position, true);
        }

        protected int PositionToBaseIndex(object position) {
            return PositionToInterfaceOrBaseIndex(position, false);
        }

        private int PositionToInterfaceOrBaseIndex(object Position, bool toInterface) {
            ICodeDomElement icde = Position as ICodeDomElement;
            int position = Int32.MaxValue;
            if (icde == null) {
                if (Position != System.Reflection.Missing.Value) {
                    position = (int)Position;
                }
            }

            int index = 0, realIndex = 0;
            foreach (CodeTypeReference baseType in CodeObject.BaseTypes) {                
                CodeElement ce = (CodeElement)baseType.UserData[CodeKey];
                if ((icde != null && ce == icde) || position == index + 1) {
                    return realIndex;
                }
                if (ce is CodeInterface == toInterface) index++;
                realIndex++;
            }

            return realIndex;
        }

        public override object ParentElement {
            get { return parent; }
        }

        public override string FullName {
            get {
                string parentFullName = string.Empty;
                if (null != parent) {
                    parentFullName = parent.FullName;
                }
                if (string.IsNullOrEmpty(parentFullName)) {
                    return CodeObject.Name;
                }
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}.{1}", parentFullName, CodeObject.Name);
            }
        }

        public override bool IsCodeType {
            get {
                return true;
            }
        }

        public override ProjectItem ProjectItem {
            get { return parent.ProjectItem; }
        }

    }
}
