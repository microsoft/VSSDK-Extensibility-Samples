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
using System.Text;
using EnvDTE;

using System.Diagnostics;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {
    [ComVisible(true)]
    public abstract class CodeDomCodeElement<CodeTypeType> : SimpleCodeElement, ICodeDomElement 
        where CodeTypeType : CodeObject {
        

        private CodeTypeType codeObj;

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        protected CodeDomCodeElement(DTE dte, string name)
            : base(dte, name) {
        }

        public override TextPoint EndPoint {
            get {
                if (null == ProjectItem) {
                    return null;
                }
                if (!CodeObject.UserData.Contains("EndColumn") || !CodeObject.UserData.Contains("EndLine")) {
                    return null;
                }
                if (null == ProjectItem.Document) {
                    ProjectItem.Open(Guid.Empty.ToString("B"));
                }
                return new CodeDomTextPoint((TextDocument)ProjectItem.Document.Object("TextDocument"),
                    (int)CodeObject.UserData["EndColumn"],
                    (int)CodeObject.UserData["EndLine"]);
            }
        }

        public override TextPoint StartPoint {
            get {                
                return new CodeDomTextPoint((TextDocument)ProjectItem.Document.Object("TextDocument"),
                    (int)CodeObject.UserData["Column"],
                    (int)CodeObject.UserData["Line"]);
            }
        }

        public override TextPoint GetEndPoint(vsCMPart Part) {
            return EndPoint;
        }

        public override TextPoint GetStartPoint(vsCMPart Part) {
            return StartPoint;
        }

        public CodeTypeType CodeObject {
            get {
                return codeObj;
            }
            set {
                codeObj = value;
            }
        }

        #region ICodeDomElement Members

        public object UntypedCodeObject {
            get { return codeObj; }
        }

        public abstract object ParentElement {
            [SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces")]
            get;
        }

        #endregion

        #region Common protected helpers

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "collection")]
        protected CodeElements GetCustomAttributes(CodeAttributeDeclarationCollection collection) {
            CodeDomCodeElements res = new CodeDomCodeElements(DTE, this);
            //!!! not right
            return res;
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        protected CodeAttribute AddCustomAttribute(CodeAttributeDeclarationCollection collection, string name, string value, object position) {
            CodeDomCodeAttribute cdca = new CodeDomCodeAttribute(DTE, this, name);
            collection.Insert(AttributePositionToIndex(collection, position), cdca.CodeObject); 
                        
            return cdca;
        }

        protected string GetComment(CodeCommentStatementCollection collection, bool docComment) {
            StringBuilder res = new StringBuilder();
            foreach (CodeComment comment in collection) {
                if (comment.DocComment == docComment) {
                    res.AppendLine(comment.Text);
                }
            }

            return res.ToString();
        }

        protected void ReplaceComment(CodeCommentStatementCollection collection, string value, bool docComment) {
            int i = 0;
            while (i < collection.Count) {
                if (collection[i].Comment.DocComment != docComment) {
                    i++;
                } else {
                    collection.RemoveAt(i);
                }
            }

            string[] strings = value.Split('\n');
            for (i = 0; i < strings.Length; i++) {
                collection.Add(new CodeCommentStatement(new CodeComment(strings[i], docComment)));
            }
        }
       
        protected CodeParameter AddParameter(CodeParameterDeclarationExpressionCollection collection, string name, object type, object position) {
            CodeTypeRef typeRef = ObjectToTypeRef(type);
            CodeDomCodeParameter cdParam = new CodeDomCodeParameter(DTE, this, name, typeRef);

            collection.Insert(PositionToParameterIndex(collection, position), cdParam.CodeObject);
            return cdParam;
        }

        protected void RemoveParameter(CodeParameterDeclarationExpressionCollection collection, object element) {
            string strElement = element as string;
            int index = 0;
            foreach (CodeParameterDeclarationExpression param in collection) {
                if (strElement == null && CodeDomCodeParameter.GetCodeParameter(param) == element) {
                    collection.RemoveAt(index);
                    break;
                } else if (strElement != null && param.Name == strElement) {
                    collection.RemoveAt(index);
                    break;
                }

                index++;
            }
        }

        protected CodeElements GetParameters(CodeParameterDeclarationExpressionCollection collection) {
            CodeDomCodeElements res = new CodeDomCodeElements(DTE, this);
            foreach (CodeParameterDeclarationExpression param in collection) {
                if (param.UserData[CodeKey] == null) {
                    param.UserData[CodeKey] = new CodeDomCodeParameter(this, param);
                }
                res.Add(CodeDomCodeParameter.GetCodeParameter(param));
            }
            return res;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        protected void CommitChanges() {
            object curParent = ParentElement;
            while (!(curParent is CodeDomFileCodeModel)) {
                curParent = ((ICodeDomElement)curParent).ParentElement;
                CodeDomFileCodeModel fcm = curParent as CodeDomFileCodeModel;
                if (fcm != null) {
                    fcm.CommitChanges();
                    break;
                }

                if (curParent == null) {
                    Debug.Assert(false, "Not ICodeDomElement or CodeDomFileCodeModel in parent hierarchy");
                    break;
                }
            }
        }
        #endregion

        #region Private helpers

        private static int PositionToParameterIndex(CodeParameterDeclarationExpressionCollection collection, object Position) {
            ICodeDomElement icde = Position as ICodeDomElement;
            if (icde != null) {
                return collection.IndexOf((CodeParameterDeclarationExpression)icde.UntypedCodeObject) + 1;
            }

            if (Position == System.Reflection.Missing.Value) {
                return collection.Count;
            }

            int pos = (int)Position;
            if (pos == -1) {
                return collection.Count;
            }
            return pos-1;
        }


        private static int AttributePositionToIndex(CodeAttributeDeclarationCollection collection, object Position) {
            ICodeDomElement icde = Position as ICodeDomElement;
            if (icde != null) {
                return collection.IndexOf((CodeAttributeDeclaration)icde.UntypedCodeObject) + 1;
            }

            if (Position == System.Reflection.Missing.Value) {
                return collection.Count;
            }

            int pos = (int)Position;
            if (pos == -1) {
                return collection.Count;
            }
            return pos-1;
        }

        #endregion

    }
}
