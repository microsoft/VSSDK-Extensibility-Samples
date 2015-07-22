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
    public class CodeDomCodeFunction : CodeDomCodeElement<CodeMemberMethod>, CodeFunction {
        private CodeElement parent;
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "kind")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        public CodeDomCodeFunction(DTE dte, CodeElement parent, string name, vsCMFunction kind, object type, vsCMAccess access)
            : base(dte, name) {
            this.parent = parent;

            this.CodeObject = new CodeMemberMethod();
            this.CodeObject.Name = name;
            this.CodeObject.ReturnType = new CodeTypeReference(ObjectToClassName(type));
            this.CodeObject.Attributes = VSAccessToMemberAccess(access);
        }

        public CodeDomCodeFunction(CodeElement parent, CodeMemberMethod method)
            : base((null==parent) ? null : parent.DTE, (null==method) ? null : method.Name) {
            this.parent = parent;
            CodeObject = method;
        }

        public override TextPoint StartPoint {
            get {
                TextDocument document = null;
                if (null != ProjectItem) {
                    if (null == ProjectItem.Document) {
                        ProjectItem.Open(Guid.Empty.ToString("B"));
                    }
                    document = (TextDocument)ProjectItem.Document.Object("TextDocument");
                }
                // functions always start at 1 so people inserting new lines
                // are less likely to throw off Python white space.
                return new CodeDomTextPoint(document,
                    1,
                    (int)CodeObject.UserData["Line"]);
            }
        }

        #region CodeFunction Members

        public vsCMAccess Access {
            get { return MemberAccessToVSAccess(CodeObject.Attributes); }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set { 
                CodeObject.Attributes = VSAccessToMemberAccess(value);
                CommitChanges();
            }
        }

        public CodeAttribute AddAttribute(string Name, string Value, object Position) {
            CodeAttribute ca = AddCustomAttribute(CodeObject.CustomAttributes, Name, Value, Position);

            CommitChanges();

            return ca;
        }

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
            CodeParameter res =AddParameter(CodeObject.Parameters, Name, Type, Position);

            CommitChanges();
            return res;
        }
      
        public bool CanOverride {
            get {
                return (CodeObject.Attributes & MemberAttributes.Final) != 0;
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                if (value) CodeObject.Attributes |= MemberAttributes.Final;
                else CodeObject.Attributes &= ~MemberAttributes.Final;

                CommitChanges();
            }
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

        public bool IsShared {
            get {
                return (CodeObject.Attributes & MemberAttributes.Static) != 0;
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                if (value) CodeObject.Attributes |= MemberAttributes.Static;
                else CodeObject.Attributes &= ~MemberAttributes.Static;

                CommitChanges();
            }
        }

        public bool MustImplement {
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

        public CodeElements Parameters {
            get {
                return GetParameters(CodeObject.Parameters);
            }
        }

        public dynamic Parent {
            get { return parent; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Element">Required. A CodeElement object or the name of one in the collection.</param>
        public void RemoveParameter(object Element) {
            RemoveParameter(CodeObject.Parameters, Element);

            CommitChanges();
        }

        /// <summary>
        /// Return type
        /// </summary>
        public CodeTypeRef Type {
            get {
                return CodeDomCodeTypeRef.FromCodeTypeReference(CodeObject.ReturnType);                
            }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set {
                CodeObject.ReturnType = CodeDomCodeTypeRef.ToCodeTypeReference(value);
            }
        }

        public CodeElements Attributes {
            get { return GetCustomAttributes(CodeObject.CustomAttributes); }
        }

        public vsCMFunction FunctionKind {
            get { throw new NotImplementedException(); }
        }

        public bool IsOverloaded {
            get { throw new NotImplementedException(); }
        }

        public CodeElements Overloads {
            get { throw new NotImplementedException(); }
        }

        public string get_Prototype(int Flags) {
            throw new NotImplementedException();
        }

        #endregion

        public override object ParentElement {
            get { return parent; }
        }

        public override CodeElements Children {
            get { return null; }
        }

        public override CodeElements Collection {
            get { return parent.Children; }
        }

        public override string FullName {
            get { return CodeObject.Name; }
        }

        public override vsCMElement Kind {
            get {
                return vsCMElement.vsCMElementFunction;
            }
        }

        public override ProjectItem ProjectItem {
            get { return parent.ProjectItem; }
        }
    }
}
