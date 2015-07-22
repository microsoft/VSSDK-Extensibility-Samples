/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using EnvDTE;
using IronPython.CodeDom;
using Microsoft.VisualStudio.TextManager.Interop;

using CDCodeNamespace = System.CodeDom.CodeNamespace;
using VSCodeNamespace = EnvDTE.CodeNamespace;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {

    /// <summary>
    /// Provides a FileCodeModel based upon the representation of the program obtained via CodeDom.
    /// 
    /// There are 3 ways a document can be edited that the code model needs to handle:
    ///     1. The user edits the document inside of VS.  Here we don't need
    ///        to update the code model until the next call back to manipulate it.
    ///     2. A script runs which uses EditPoint's to update the text of the document.  
    ///     3. The user uses the FileCodeModel to add members to the document.
    /// 
    /// </summary>
    internal class CodeDomFileCodeModel : SimpleCodeElement, EnvDTE.FileCodeModel {        
        private ProjectItem parent;             // the project item for which we represent the FileCodeModel on
        private CodeCompileUnit ccu;            // the top-level CodeCompileUnit which stores our members
        private IMergableProvider provider;     // our code dom provider which we use for updating ccu
        private CodeDomCodeNamespace vsTopNamespace; // top-level CodeModel namespace
        private CDCodeNamespace topNamespace;   // top level CodeDom namespace
        private bool isDirty;                   // we are dirty after a user edit which means we need to re-parse before handing out CodeElement's.
        private bool committing;                // if we're updating the buffer ourself we ignore our own edits.
        private IVsTextLines textBuffer;        // The buffer storing the text.
        private IMergeDestination mergeDestination;

        public CodeDomFileCodeModel(DTE dte, ProjectItem parent, CodeDomProvider provider, string filename) : base(dte, filename) {
            this.parent = parent;
            this.provider = provider as IMergableProvider;
            if (provider == null) throw new ArgumentException("provider must implement IMergeableProvider interface");            
        }

        public CodeDomFileCodeModel(DTE dte, IVsTextLines buffer, CodeDomProvider provider, string moniker) : base(dte, moniker) {
            this.textBuffer = buffer;
            this.provider = provider as IMergableProvider;
            if (provider == null) throw new ArgumentException("provider must implement IMergeableProvider interface");            
        }

        /// <summary>
        /// Called when text is added to the page via either the user editing the page or 
        /// via using EditPoint's from FileCodeModel.  We go through and update any items
        /// after the current item so their current positions are correct.  We also mark the
        /// model as being dirty so that before we hand out any CodeElement's we can re-parse, but
        /// we avoid the expensive reparse if the user is simply doing multiple edits.  
        /// 
        /// Finally, when things go idle, we will also re-parse the document so we need not delay
        /// it until the user actually wants to do something.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "last")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "sender")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnLineChanged(object sender, TextLineChange[] changes, int last) {
            if (committing) return;

            CodeDomFileCodeModel model = parent.FileCodeModel as CodeDomFileCodeModel;
            Debug.Assert(model != null);

            for (int i = 0; i < changes.Length; i++) {
                UpdatePositions(changes[i].iStartLine, changes[i].iNewEndLine - changes[i].iOldEndLine, changes[i].iNewEndIndex - changes[i].iOldEndIndex);
            }

            isDirty = true;
        }

        /// <summary>
        /// Called when the editor is idle.  This is an ideal time to re-parse.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "lines")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void OnIdle(IVsTextLines lines) {
            Reparse();
        }

        internal IMergeDestination MergeDestination {
            get { return mergeDestination; }
            set { mergeDestination = value; }
        }

        /// <summary>
        /// performs lazy initialization to ensure our current code model is up-to-date.
        /// 
        /// If we haven't yet created our CodeDom backing we'll create it for the 1st time.  If we've
        /// created our backing, but some elements have been changed that we haven't yet reparsed
        /// then we'll reparse & merge any of the appropriate changes.
        /// </summary>
        private void Initialize() {
            if (ccu != null) {
                if (isDirty) {
                    Reparse();
                    isDirty = false;
                }
                return;
            }

            IMergeDestination merger = MergeDestination;
            if (null == textBuffer) {
                using (FileStream fs = new FileStream(Name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    if ((null == merger) && (null != parent)) {
                        merger = new FileCodeMerger(parent);
                    }
                    ccu = provider.ParseMergable(new StreamReader(fs),  merger);
                }
            } else {
                // Find the size of the buffer.
                int lastLine;
                int lastColumn;
                ErrorHandler.ThrowOnFailure(textBuffer.GetLastLineIndex(out lastLine, out lastColumn));
                // Get the text in the buffer.
                string text;
                ErrorHandler.ThrowOnFailure(textBuffer.GetLineText(0, 0, lastLine, lastColumn, out text));
                if (null == merger) {
                    merger = new TextBufferMerger(textBuffer);
                }
                ccu = provider.ParseMergable(text, Name, merger);
            }
        }

        #region FileCodeModel Members

        /// <summary>
        /// Adds a class to the top-level (empty) namespace 
        /// </summary>
        /// <param name="Name">The name of the class to add</param>
        /// <param name="Position">The position where the class should be added (1 based)</param>
        /// <param name="Bases">The bases the class dervies from</param>
        /// <param name="ImplementedInterfaces">the interfaces the class implements</param>
        /// <param name="Access">The classes protection level</param>
        public CodeClass AddClass(string Name, 
                                    object Position, 
                                    object Bases, 
                                    object ImplementedInterfaces, 
                                    vsCMAccess Access) {            
            Initialize();

            InitTopNamespace();

            CodeClass cs = vsTopNamespace.AddClass(Name, Position, Bases, ImplementedInterfaces, Access);
            
            CommitChanges();
            
            return cs;
        }

        /// <summary>
        /// Adds a function to the top-level namespace.  Currently adding functions to namespaces doesn't do anything.
        /// </summary>
        public CodeFunction AddFunction(string Name, vsCMFunction Kind, object Type, object Position, vsCMAccess Access) {
            Initialize();

            InitTopNamespace();

            CodeFunction cf = vsTopNamespace.AddFunction(Name, Kind, Type, Position, Access);

            CommitChanges();

            return cf;
        }

        /// <summary>
        /// Adds a namespace to the top level namespace.
        /// </summary>
        /// <param name="Name">Required. The name of the new namespace.</param>
        /// <param name="Position">Optional. Default = 0. The code element after which to add the new element. If the value is a CodeElement, then the new element is added immediately after it.</param>
        /// <returns></returns>
        public VSCodeNamespace AddNamespace(string Name, object Position) {
            Initialize();

            InitTopNamespace();

            CDCodeNamespace cn = new CDCodeNamespace(Name);
            EnsureNamespaceLinked(cn);

            VSCodeNamespace after = Position as VSCodeNamespace;
            if (after != null) {
                for (int i = 0; i < ccu.Namespaces.Count; i++) {
                    if (ccu.Namespaces[i].UserData[CodeKey] == after) {
                        ccu.Namespaces.Insert(i + 1, cn);
                    }
                }
            } else {
                int index = (int)Position - 1;
                ccu.Namespaces.Insert(index, cn);
            }
            

            CommitChanges();

            return (VSCodeNamespace)cn.UserData[CodeKey];
        }

        /// <summary>
        /// Given a point and an element type to search for returns the element of that type at that point
        /// or null if no element is found.
        /// </summary>
        public CodeElement CodeElementFromPoint(TextPoint Point, vsCMElement Scope) {
            Initialize();

            CodeElement res;
            foreach (CDCodeNamespace cn in ccu.Namespaces) {
                if (Scope == vsCMElement.vsCMElementNamespace) {
                    if (IsInBlockRange(cn, Point)) {
                        EnsureNamespaceLinked(cn);
                        return (CodeElement)cn.UserData[CodeKey];
                    }
                }

                if (Scope == vsCMElement.vsCMElementImportStmt) {
                    foreach (CodeNamespaceImport import in cn.Imports) {                        
                        if (IsInRange(import, Point)) {
                            return (CodeElement)import.UserData[CodeKey];
                        }
                    }

                    continue;
                }

                foreach(CodeTypeDeclaration ctd in cn.Types) {
                    res = CheckAttributes(Point, Scope, ctd.CustomAttributes);
                    if (res != null) return res;

                    if ((ctd.IsClass && Scope == vsCMElement.vsCMElementClass) ||
                        (ctd.IsEnum&& Scope == vsCMElement.vsCMElementEnum) ||
                        (ctd.IsInterface&& Scope == vsCMElement.vsCMElementInterface) ||
                        (ctd.IsStruct&& Scope == vsCMElement.vsCMElementStruct)) {

                        if (IsInBlockRange(ctd, Point)) return (CodeElement)ctd.UserData[CodeKey];

                        continue;
                    }

                    foreach (CodeTypeMember ctm in ctd.Members) {
                        res = CodeElementFromMember(Point, Scope, ctm);
                        if (res != null) return res;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all the CodeElements that live in the namespace
        /// </summary>
        public CodeElements CodeElements {
            get {
                Initialize();

                CodeDomCodeElements res = new CodeDomCodeElements(DTE, this);
                foreach (CDCodeNamespace member in ccu.Namespaces) {
                    EnsureNamespaceLinked(member);

                    res.Add((CodeElement)member.UserData[CodeKey]);
                }
                return res;
            }
        }

        private void EnsureNamespaceLinked(CDCodeNamespace member) {
            if (member.UserData[CodeKey] == null) {
                CodeDomCodeNamespace cdcn = new CodeDomCodeNamespace(DTE, member.Name, this);
                cdcn.CodeObject = member;
                member.UserData[CodeKey] = cdcn;
            }
        }

        public ProjectItem Parent {
            get { return parent; }
        }

        /// <summary>
        /// Removes an element from the namespace.
        /// </summary>
        /// <param name="Element"></param>
        public void Remove(object Element) {
            Initialize();

            int index = ((CodeDomCodeElements)CodeElements).IndexOf((SimpleCodeElement)Element);

            ccu.Namespaces.RemoveAt(index);
        }

        public CodeDelegate AddDelegate(string Name, object Type, object Position, vsCMAccess Access) {
            Initialize();

            InitTopNamespace();

            CodeDelegate cd = vsTopNamespace.AddDelegate(Name, Type, Position, Access);

            CommitChanges();

            return cd;
        }

        public CodeEnum AddEnum(string Name, object Position, object Bases, vsCMAccess Access) {
            Initialize();

            InitTopNamespace();

            CodeEnum ce = vsTopNamespace.AddEnum(Name, Position, Bases, Access);

            CommitChanges();

            return ce;
        }

        public CodeInterface AddInterface(string Name, object Position, object Bases, vsCMAccess Access) {
            Initialize();

            InitTopNamespace();

            CodeInterface ci = vsTopNamespace.AddInterface(Name, Position, Bases, Access);

            CommitChanges();

            return ci;
        }

        public CodeStruct AddStruct(string Name, object Position, object Bases, object ImplementedInterfaces, vsCMAccess Access) {
            Initialize();

            InitTopNamespace();

            CodeStruct cs = vsTopNamespace.AddStruct(Name, Position, Bases, ImplementedInterfaces, Access);

            CommitChanges();

            return cs;
        }

        #endregion

        public CodeVariable AddVariable(string Name, object Type, object Position, vsCMAccess Access) {
            throw new NotImplementedException();
        }

        public CodeAttribute AddAttribute(string Name, string Value, object Position) {
            throw new NotImplementedException();
        }

        internal void CommitChanges() {
            IMergableProvider prov = provider as IMergableProvider;
            if (prov == null) throw new InvalidOperationException("provider must be mergable");

            DTE.UndoContext.Open("Undo Code Merge", false);
            committing = true;
            try {
                prov.MergeCodeFromCompileUnit(ccu);
            } finally {
                committing = false;
                DTE.UndoContext.Close();
            }
        }

        #region Reparser
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void Reparse() {
            if(isDirty) {
                Debug.Assert(ccu != null);

                CodeCompileUnit newCcu = null;
                TextDocument td = ((TextDocument)ProjectItem.Document.Object("TextDocument"));
                
                Debug.Assert(td != null);
                
                EditPoint ep = td.CreateEditPoint(td.StartPoint);                
                string text = ep.GetText(td.EndPoint);

                try{
                    newCcu = provider.ParseMergable(text,
                        ProjectItem.Document.FullName,
                        new FileCodeMerger(parent));

                    MergeNewCompileUnit(newCcu);
                } catch {
                    // swallow parse errors.
                }                
                isDirty = false;
            }
        }

        private static object merged = new object();

        /// <summary>
        /// Merges two CodeCompileUnit's into one.  The new CCU's children will
        /// be added to our current CCU's children.  
        /// </summary>
        private void MergeNewCompileUnit(CodeCompileUnit newCcu) {
            int curIndex = 0;
            foreach (CDCodeNamespace cn in newCcu.Namespaces) {
                bool found = false;
                foreach (CDCodeNamespace oldCn in ccu.Namespaces) {
                    if (oldCn.Name == cn.Name) {
                        oldCn.UserData[merged] = merged;

                        found = true;

                        MergeNamespaces(cn, oldCn);
                        break;
                    }
                }

                // new namespace (or could be renamed), move it to 
                // our ccu
                if (!found) {
                    ccu.Namespaces.Insert(curIndex, cn);
                } 
                curIndex++;                
            }

            foreach (CDCodeNamespace cn in ccu.Namespaces) {
                if (cn.UserData[merged] == null) {
                    // namespace was removed or renamed
                    ccu.Namespaces.Remove(cn);
                }

                cn.UserData.Remove(merged);
            }
        }

        private void MergeNamespaces(CDCodeNamespace newNs, CDCodeNamespace oldNs) {
            CopyLineInfo(newNs, oldNs);

            int curIndex = 0;
            foreach (CodeTypeDeclaration ctd in newNs.Types) {
                bool found = false;
                foreach (CodeTypeDeclaration oldCtd in oldNs.Types) {
                    if (oldCtd.Name == ctd.Name) {
                        oldCtd.UserData[merged] = merged;

                        found = true;

                        MergeTypes(ctd, oldCtd);
                        break;
                    }
                }

                if (!found) {
                    oldNs.Types.Insert(curIndex, ctd);
                }
                curIndex++;
            }

            foreach (CodeTypeDeclaration ctd in oldNs.Types) {
                if (ctd.UserData[merged] == null) {
                    oldNs.Types.Remove(ctd);
                }

                ctd.UserData.Remove(merged);
            }
        }

        private static void CopyLineInfo(CodeObject newObject, CodeObject oldObject) {
            oldObject.UserData["Line"] = newObject.UserData["Line"];
            oldObject.UserData["Column"] = newObject.UserData["Column"];
            oldObject.UserData["EndLine"] = newObject.UserData["EndLine"];
            oldObject.UserData["EndColumn"] = newObject.UserData["EndColumn"];
        }

        private void MergeTypes(CodeTypeDeclaration newCtd, CodeTypeDeclaration oldCtd) {
            CopyLineInfo(newCtd, oldCtd);

            int curIndex = 0;
            foreach (CodeTypeMember ctm in newCtd.Members) {
                bool found = false;
                foreach (CodeTypeMember oldCtm in oldCtd.Members) {
                    if (oldCtm.Name == ctm.Name) {
                        oldCtm.UserData[merged] = merged;

                        found = true;

                        MergeTypeMember(ctm, oldCtm);
                        break;
                    }
                }

                if (!found) {
                    oldCtd.Members.Insert(curIndex, ctm);
                }
                curIndex++;
            }

            foreach (CodeTypeMember ctm in oldCtd.Members) {
                if (ctm.UserData[merged] == null) {
                    oldCtd.Members.Remove(ctm);
                }

                ctm.UserData.Remove(merged);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMethodsAsStatic")]
        private void MergeTypeMember(CodeTypeMember newMember, CodeTypeMember oldMember) {
            //!!! need to merge statements.
            CopyLineInfo(newMember, oldMember);
        }
        #endregion

        #region Position Updater
        /// <summary>
        /// Walks the code dom tree updating positions of items based upon an 
        /// edit the user made directly to the text document
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void UpdatePositions(int fromLine, int lines, int chars) {
            if(ccu == null) Initialize(); 

            foreach (CDCodeNamespace cn in ccu.Namespaces) {
                foreach (CodeTypeDeclaration ctd in cn.Types) {
                    AdjustTypeDeclaration(fromLine, lines, chars, ctd);
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static void AdjustTypeDeclaration(int fromLine, int lines, int chars, CodeTypeDeclaration ctd) {
            foreach (CodeTypeMember ctm in ctd.Members) {
                DoOneAdjust(fromLine, lines, chars, ctm);

                if (ctm is CodeTypeDeclaration) {
                    AdjustTypeDeclaration(fromLine, lines, chars, ctm as CodeTypeDeclaration);
                } else if (ctm is CodeMemberField) {
                    AdjustField(fromLine, lines, chars, ctm);
                } else if (ctm is CodeMemberMethod) {
                    AdjustMethod(fromLine, lines, chars, ctm);
                } else if (ctm is CodeMemberProperty) {
                    AdjustProperty(fromLine, lines, chars, ctm);
                } else if (ctm is CodeMemberEvent) {
                    // already adjusted the event, no submembers to adjust
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static void AdjustField(int fromLine, int lines, int chars, CodeTypeMember cmm) {
            CodeMemberField field = cmm as CodeMemberField;

            DoOneAdjust(fromLine, lines, chars, field.InitExpression);            
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static void AdjustProperty(int fromLine, int lines, int chars, CodeTypeMember ctm) {
            CodeMemberProperty cmp = ctm as CodeMemberProperty;

            AdjustParameters(fromLine, lines, chars, cmp.Parameters);

            if (cmp.HasGet) {
                AdjustStatements(fromLine, lines, chars, cmp.GetStatements);
            }

            if (cmp.HasSet) {
                AdjustStatements(fromLine, lines, chars, cmp.SetStatements);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static void AdjustMethod(int fromLine, int lines, int chars, CodeTypeMember ctm) {
            CodeMemberMethod cmm = ctm as CodeMemberMethod;
            AdjustParameters(fromLine, lines, chars, cmm.Parameters);

            foreach (CodeTypeParameter ctp in cmm.TypeParameters) {
                DoOneAdjust(fromLine, lines, chars, ctp);
            }

            AdjustStatements(fromLine, lines, chars, cmm.Statements);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static void AdjustStatements(int fromLine, int lines, int chars, CodeStatementCollection statements) {
            foreach (CodeStatement cs in statements) {
                DoOneAdjust(fromLine, lines, chars, cs);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static void AdjustParameters(int fromLine, int lines, int chars, CodeParameterDeclarationExpressionCollection parameters) {
            foreach (CodeParameterDeclarationExpression param in parameters) {
                DoOneAdjust(fromLine, lines, chars, param);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "chars")]
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static void DoOneAdjust(int fromLine, int lines, int chars, CodeObject co) {
            if (co.UserData["Line"] != null) {
                int curLine = (int)co.UserData["Line"];
                int curCol = (int)co.UserData["Column"];

                if (curLine == fromLine) {
                } else if ((lines > 0) == (curLine > fromLine)) {
                    // line needs to be adjusted
                    co.UserData["Line"] = curLine + lines;
                }

                if (co.UserData["EndLine"] != null) {
                    int endLine = (int)co.UserData["EndLine"];
                    int endCol = (int)co.UserData["EndColumn"];

                    if (curLine == fromLine) {
                    } else if ((lines > 0) == (curLine > fromLine)) {
                        // line needs to be adjusted
                        co.UserData["EndLine"] = curLine + lines;
                    }
                }
            }
        }
        #endregion

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static CodeElement CodeElementFromMember(TextPoint Point, vsCMElement Scope, CodeTypeMember ctm) {
            CodeElement res = CheckAttributes(Point, Scope, ctm.CustomAttributes);
            if (res != null) return res;

            CodeMemberMethod method = ctm as CodeMemberMethod;
            if (method != null) {
                if (Scope == vsCMElement.vsCMElementFunction) {
                    if (IsInBlockRange(method, Point)) return (CodeElement)method.UserData[CodeKey];
                }
                //!!! walk method
                if (Scope == vsCMElement.vsCMElementParameter || Scope == vsCMElement.vsCMElementAttribute) {
                    foreach (CodeParameterDeclarationExpression param in method.Parameters) {
                        if (IsInRange(param, Point)) return (CodeElement)method.UserData[CodeKey];

                        res = CheckAttributes(Point, Scope, param.CustomAttributes);
                        if (res != null) return res;
                    }

                    foreach (CodeTypeParameter ctp in method.TypeParameters) {
                        if (IsInRange(ctp, Point)) return (CodeElement)method.UserData[CodeKey];

                        res = CheckAttributes(Point, Scope, ctp.CustomAttributes);
                        if (res != null) return res;
                    }
                }

                res = CheckAttributes(Point, Scope, method.ReturnTypeCustomAttributes);
                if (res != null) return res;

                res = WalkStatements(Point, Scope, method.Statements);
                if (res != null) return res;
            } else if (ctm is CodeMemberEvent) {
                if (Scope == vsCMElement.vsCMElementEvent) {
                    if (IsInRange(ctm, Point)) return (CodeElement)ctm.UserData[CodeKey];
                }
            } else if (ctm is CodeMemberProperty) {
                CodeMemberProperty cmp = ctm as CodeMemberProperty;

                foreach (CodeParameterDeclarationExpression param in cmp.Parameters) {
                    if (IsInRange(param, Point)) return (CodeElement)method.UserData[CodeKey];

                    res = CheckAttributes(Point, Scope, param.CustomAttributes);
                    if (res != null) return res;
                }

                if (Scope == vsCMElement.vsCMElementProperty) {
                    if (IsInBlockRange(ctm, Point)) return (CodeElement)ctm.UserData[CodeKey];
                }

                if (cmp.HasGet) WalkStatements(Point, Scope, cmp.GetStatements);
                if (cmp.HasSet) WalkStatements(Point, Scope, cmp.SetStatements);
            } else if (ctm is CodeMemberField) {
                if (Scope == vsCMElement.vsCMElementVariable) {
                    if (IsInRange(ctm, Point)) return (CodeElement)ctm.UserData[CodeKey];
                }
            }
            return null;
        }

        private static CodeElement WalkStatements(TextPoint Point, vsCMElement Scope, CodeStatementCollection statements) {
            foreach (CodeStatement cs in statements) {
                if (Scope == vsCMElement.vsCMElementAssignmentStmt && cs is CodeAssignStatement) {
                    if (IsInRange(cs, Point)) return (CodeElement)cs.UserData[CodeKey];
                }

                if (Scope == vsCMElement.vsCMElementLocalDeclStmt && cs is CodeVariableDeclarationStatement) {
                    if (IsInRange(cs, Point)) return (CodeElement)cs.UserData[CodeKey];
                }

                CodeExpressionStatement ces = cs as CodeExpressionStatement;
                if (ces != null) {
                    if (Scope == vsCMElement.vsCMElementFunctionInvokeStmt && ces.Expression is CodeMethodInvokeExpression) {
                        if (IsInRange(cs, Point)) return (CodeElement)cs.UserData[CodeKey];
                    }
                    if (Scope == vsCMElement.vsCMElementPropertySetStmt && ces.Expression is CodePropertySetValueReferenceExpression) {
                        if (IsInRange(cs, Point)) return (CodeElement)cs.UserData[CodeKey];
                    }
                }

                if (Scope == vsCMElement.vsCMElementOther && IsInRange(cs, Point)) return (CodeElement)cs.UserData[CodeKey];
            }
            return null;
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "Point")]
        private static CodeElement CheckAttributes(TextPoint Point, vsCMElement Scope, CodeAttributeDeclarationCollection attributes) {
            if (Scope == vsCMElement.vsCMElementAttribute) {
                foreach (CodeAttributeDeclaration cad in attributes) {
                    //!!! no user data on attributes!
                    //if (IsInRange(cad, Point)) return (CodeElement)method.UserData[CodeKey];
                }
            }
            return null;
        }
        private static bool IsInBlockRange(CodeObject codeObject, TextPoint point) {
            Nullable<int> line = UserDataInt(codeObject, "Line");
            Nullable<int> endLine = UserDataInt(codeObject, "EndLine");

            // match in the middle of a block
            if (line <= point.Line && point.Line <= endLine) {
                return true;
            }
            return false;
        }
        private static bool IsInRange(CodeObject codeObject, TextPoint point) {
            Nullable<int> line = UserDataInt(codeObject, "Line");
            Nullable<int> endLine = UserDataInt(codeObject, "EndLine");

            // match in the middle of a block
            if (line < point.Line && point.Line < endLine) {
                return true;
            }

            if (line == point.Line || endLine == line) {
                // single line match, make sure the columns are right
                Nullable<int> col = UserDataInt(codeObject, "Column");
                Nullable<int> endCol = UserDataInt(codeObject, "EndColumn");

                if (line == point.Line &&
                    col <= point.LineCharOffset &&
                    point.LineCharOffset < endCol) {
                    return true;
                } else if (endLine != line &&
                    endLine == point.Line &&
                    endCol <= point.LineCharOffset &&
                    point.LineCharOffset < endCol) {
                    return true;
                }
            }

            return false;
        }

        private static Nullable<int> UserDataInt(CodeObject codeObject, string name) {
            object val = codeObject.UserData[name];
            if (val == null) return null;

            return (int)val;
        }

        public override TextPoint GetEndPoint(vsCMPart Part) {
            return EndPoint;
        }

        public override TextPoint GetStartPoint(vsCMPart Part) {
            return StartPoint;
        }

        public override TextPoint EndPoint {
            get { return ((TextDocument)this.ProjectItem.Document.Object("TextDocument")).EndPoint; }
        }

        public override TextPoint StartPoint {
            get { return ((TextDocument)this.ProjectItem.Document.Object("TextDocument")).StartPoint; }
        }

        public override CodeElements Children {
            get { return CodeElements; }
        }

        public override CodeElements Collection {
            get { return null; }
        }

        public override string FullName {
            get { return DTE.FullName; }
        }

        public override vsCMElement Kind {
            get { return vsCMElement.vsCMElementModule; }
        }

        public override ProjectItem ProjectItem {
            get { return parent; }
        }


        /// <summary>
        /// Ensures that we have a top-level namespace (cached in our topNamespace field)
        /// </summary>
        private void InitTopNamespace() {
            if (vsTopNamespace == null) {
                vsTopNamespace = new CodeDomCodeNamespace(DTE, String.Empty, this);

                foreach (CDCodeNamespace ns in ccu.Namespaces) {
                    if (String.IsNullOrEmpty(ns.Name)) {
                        topNamespace = ns;
                        break;
                    }
                }

                if (topNamespace == null) {
                    topNamespace = new CDCodeNamespace(String.Empty);
                    ccu.Namespaces.Add(topNamespace);
                    isDirty = true;
                }

                vsTopNamespace.CodeObject = topNamespace;
                topNamespace.UserData[CodeKey] = vsTopNamespace;
            }
        }


    }
}
