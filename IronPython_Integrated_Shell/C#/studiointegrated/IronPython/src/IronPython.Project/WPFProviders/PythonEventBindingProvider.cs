/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.Design.Host;
using System.CodeDom.Compiler;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using System.Text.RegularExpressions;
using System.CodeDom;
using Microsoft.VisualStudio.Shell.Design.Serialization;
using Microsoft.VisualStudio.Shell.Design.Serialization.CodeDom;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Drawing;
using Microsoft.VisualStudio;
using System.Globalization;

namespace Microsoft.Samples.VisualStudio.IronPython.Project.WPFProviders
{
    /// <summary>
    /// This class provides the event handler generation for the 
    /// WPF designer. Note that this object is NOT required for languages
    /// where the CodeDom is used for event handler generation. This is needed
    /// in the case of IronPython due to limitations in the static compiler 
    /// support.
    /// </summary>
    public class PythonEventBindingProvider : EventBindingProvider
    {
        private IVsProject3 project;
        private PythonFileNode pyFile;

        internal PythonEventBindingProvider(PythonFileNode pyFile)
        {
            this.pyFile = pyFile;
            this.project = pyFile.ProjectMgr;
        }

        public override bool RemoveHandlesForName(string elementName)
        {
            return false;
        }

        public override bool AddEventHandler(EventDescription eventDescription, string objectName, string methodName)
        {
            const string Init = "__init__";
            //This is not the most optimal solution for WPF since we will call FindLogicalNode for each event handler,
            //but it simplifies the code generation for now.

            CodeDomDocDataAdapter adapter = GetDocDataAdapterForPyFile();

            //Find the __init__ method
            CodeMemberMethod method = null;
            foreach(CodeTypeMember ctMember in adapter.TypeDeclaration.Members)
            {
                if (ctMember is CodeConstructor)
                {
                    if (ctMember.Name == Init)
                    {
                        method = ctMember as CodeMemberMethod;
                        break;
                    }
                }
            }
            if (method == null)
            {
                method = new CodeConstructor();
                method.Name = Init;
            }

            //Create a code statement which looks like: LogicalTreeHelper.FindLogicalNode(self.Root, "button1").Click += self.button1_Click
            CodeTypeReferenceExpression logicalTreeHelper = new CodeTypeReferenceExpression("LogicalTreeHelper");
            CodeMethodReferenceExpression findLogicalNodeMethod = new CodeMethodReferenceExpression(logicalTreeHelper, "FindLogicalNode");
            CodeFieldReferenceExpression selfWindow = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "Root");
            CodeMethodInvokeExpression findLogicalNodeInvoke = new CodeMethodInvokeExpression( findLogicalNodeMethod, selfWindow, new CodeSnippetExpression("\'" + objectName + "\'"));
            CodeDelegateCreateExpression createDelegateExpression = new CodeDelegateCreateExpression(new CodeTypeReference("System.EventHandler"), new CodeThisReferenceExpression(), methodName);
            CodeAttachEventStatement attachEvent = new CodeAttachEventStatement(findLogicalNodeInvoke, eventDescription.Name, createDelegateExpression);
            method.Statements.Add(attachEvent);
            adapter.Generate();
            return true;
        }

        public override bool AllowClassNameForMethodName()
        {
            return true;
        }

        public override bool CreateMethod(EventDescription eventDescription, string methodName, string initialStatements)
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = methodName;
            
            foreach(EventParameter param in eventDescription.Parameters)
            {
                method.Parameters.Add(new CodeParameterDeclarationExpression(param.TypeName, param.Name));
            }
            //Finally, add the new method to the class
            CodeDomDocDataAdapter adapter = GetDocDataAdapterForPyFile();
            adapter.TypeDeclaration.Members.Add(method);
            adapter.Generate();
            return true;
        }

        public override string CreateUniqueMethodName(string objectName, EventDescription eventDescription)
        {
            string originalMethodName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", objectName, eventDescription.Name);
            string methodName = originalMethodName;

            List<CodeTypeMember> methods = GetHandlersFromActivePyFile(string.Format(CultureInfo.InvariantCulture, "{0}_{1}", objectName, eventDescription.Name));
            while (methods.Count > 0)
            {
                //Try to append a _# at the end until we find an unused method name
                Match match = Regex.Match(methodName, @"_\d+$");
                if (!match.Success)
                {
                    methodName = originalMethodName + "_1";
                }
                else
                {
                    int nextValue = Int32.Parse(match.Value.Substring(1)) + 1;
                    methodName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", originalMethodName, nextValue);
                }
                methods = GetHandlersFromActivePyFile(methodName);
            }
            return methodName;
        }

        public override IEnumerable<string> GetCompatibleMethods(EventDescription eventDescription)
        {
            throw new NotImplementedException();
        }

        private List<CodeTypeMember> GetHandlersFromActivePyFile(string methodName)
        {
            List<CodeTypeMember> methods = new List<CodeTypeMember>();
            //We expect that py files that contain the event wiring for XAML files contain a namespace
            //and a class.
            foreach (CodeTypeMember member in GetCodeDomForPyFile().Members)
            {
                //We just match on the element name here (e.g. button1_Click), not on parameters
                if (member.Name == methodName)
                    methods.Add(member);
            }
            return methods;
        }

        public override IEnumerable<string> GetMethodHandlers(EventDescription eventDescription, string objectName)
        {
            List<string> methodHandlers = new List<string>();

            foreach (CodeTypeMember member in GetCodeDomForPyFile().Members)
            {
                if (member is CodeConstructor)
                {
                    CodeConstructor constructor = (CodeConstructor)member;
                    foreach (CodeStatement statement in constructor.Statements)
                    {
                        if (statement is CodeAttachEventStatement)
                        {
                            CodeAttachEventStatement codeAttach = (CodeAttachEventStatement)statement;
                            if (codeAttach.Event.EventName != eventDescription.Name)
                            {
                                //This is a code attach, but not for the event that the designer is looking for.
                                //Go to the next one.
                                continue;
                            }
                            if (codeAttach.Event.TargetObject is CodeMethodInvokeExpression)
                            {
                                CodeMethodInvokeExpression findLogNode = (CodeMethodInvokeExpression)codeAttach.Event.TargetObject;
                                if (findLogNode.Parameters.Count >= 2)
                                {
                                    if (findLogNode.Parameters[1] is CodePrimitiveExpression)
                                    {
                                        string targetObjectName = ((CodePrimitiveExpression)findLogNode.Parameters[1]).Value.ToString().Trim('"');
                                        if(targetObjectName.Equals(objectName, StringComparison.Ordinal))
                                        {
                                            if (codeAttach.Listener is CodeDelegateCreateExpression)
                                            {
                                                methodHandlers.Add(((CodeDelegateCreateExpression)codeAttach.Listener).MethodName);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return methodHandlers;
        }

        public override bool IsExistingMethodName(EventDescription eventDescription, string methodName)
        {
            List<CodeTypeMember> elements = GetHandlersFromActivePyFile(methodName);
            return elements.Count != 0;
        }

        public override bool RemoveEventHandler(EventDescription eventDescription, string objectName, string methodName)
        {
            throw new NotImplementedException();
        }

        public override bool RemoveMethod(EventDescription eventDescription, string methodName)
        {
            throw new NotImplementedException();
        }

        public override void SetClassName(string className)
        {
            return;
        }

        public override bool ShowMethod(EventDescription eventDescription, string methodName)
        {
            CodeDomDocDataAdapter adapter = GetDocDataAdapterForPyFile();
            List<CodeTypeMember> methodsToShow = GetHandlersFromActivePyFile(methodName);
            if (methodsToShow == null || methodsToShow.Count < 1)
                return false;

            Point point = new Point();
            if (methodsToShow[0] != null)
            {
                //We can't navigate to every method, so just take the first one in the list.
                object pt = methodsToShow[0].UserData[typeof(Point)];
                if (pt != null)
                {
                    point = (Point)pt;
                }
            }
            //Get IVsTextManager to navigate to the code
            IVsTextManager mgr = Package.GetGlobalService(typeof(VsTextManagerClass)) as IVsTextManager;
            Guid logViewCode = VSConstants.LOGVIEWID_Code;
            return ErrorHandler.Succeeded(mgr.NavigateToLineAndColumn(adapter.DocData.Buffer, ref logViewCode, point.Y - 1, point.X, point.Y - 1, point.X));
        }

        public override void ValidateMethodName(EventDescription eventDescription, string methodName)
        {
            return;
        }

        private static EnvDTE.DTE dte;
        private static EnvDTE.DTE DTE
        {
            get
            {
                if (dte == null)
                {
                    dte = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
                }
                return dte;
            }
        }

        /// <summary>
        /// This method will get the CodeDomDocDataAdapter corresponding to the active XAML file in
        /// the designer.
        /// </summary>
        /// <returns>The CodeDomDocDataAdapter for the .py file that corresponds to the active xaml file</returns>
        public CodeDomDocDataAdapter GetDocDataAdapterForPyFile()
        {
            IVSMDCodeDomProvider codeDom = (new ServiceProvider(pyFile.OleServiceProvider, true)).GetService(typeof(SVSMDCodeDomProvider)) as IVSMDCodeDomProvider;
            DocData data = new DocData((project as PythonProjectNode).ProjectMgr.Site, pyFile.Url);
            CodeDomDocDataAdapter cdDocDataAdapter = new CodeDomDocDataAdapter((project as PythonProjectNode).ProjectMgr.Site, data);
            return cdDocDataAdapter;
        }

        /// <summary>
        /// This method will get the CodeTypeDeclaration corresponding to the active XAML file in
        /// the designer.
        /// </summary>
        /// <returns>The CodeTypeDeclaration for the .py file that corresponds to the active xaml file</returns>
        public CodeTypeDeclaration GetCodeDomForPyFile()
        {
            return GetDocDataAdapterForPyFile().TypeDeclaration;
        }

        public override void AppendStatements(EventDescription eventDescription, string methodName, string statements, int relativePosition)
        {
            throw new NotImplementedException();
        }

        public override string CodeProviderLanguage
        {
            get { throw new NotImplementedException(); }
        }
    }
}
