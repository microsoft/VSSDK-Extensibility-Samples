/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using IronPython.Compiler.Ast;
using IronPython.Hosting;
using IronPython.Runtime;

namespace Microsoft.VisualStudio.IronPythonInference
{
    internal class QuietCompilerSink : CompilerSink
    {
        public override void AddError(string path, string message, string lineText, CodeSpan span, int errorCode, Severity severity)
        {
        }
    }

    // Disable the "IdentifiersShouldNotMatchKeywords" warning.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716")]
    public class Module
    {
        private ModuleScope module;
        private GlobalSuite global;
        private string name;
        private Modules references;
        private Dictionary<ScopeStatement, Scope> scopes;

        public Module(Modules references, string name, GlobalSuite global, Dictionary<ScopeStatement, Scope> scopes)
        {
            this.references = references;
            this.name = name;
            this.scopes = scopes;
            this.global = global;
        }

        internal ModuleScope ModuleScope
        {
            get { return module; }
            set { module = value; }
        }

        public Scope GlobalScope
        {
            get { return module; }
        }

        public InferredClass GetClass(IronPython.Compiler.Ast.ClassDefinition cls)
        {
            Debug.Assert(scopes.ContainsKey(cls));
            return new InferredClass(scopes[cls] as ClassScope);
        }

        // Disable the "VariableNamesShouldNotMatchFieldNames" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500", MessageId = "name")]
        public bool TryImport(string name, out Inferred inferred)
        {
            return references.TryImport(this.name, name, out inferred);
        }

        public Inferred InferType(Type type)
        {
            return references.InferType(type);
        }

        // Disable the "VariableNamesShouldNotMatchFieldNames" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500", MessageId = "name")]
        internal IList<Inferred> InferBuiltin(SymbolId name, Engine engine)
        {
            return references.InferBuiltin(name, engine);
        }

        // Disable the "VariableNamesShouldNotMatchFieldNames" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500")]
        public IList<Declaration> GetAttributesAt(int line, int column)
        {
            Node node;
            Scope scope;
            List<Declaration> attributes = new List<Declaration>();

            if (Locate(line, column, out node, out scope))
            {
                FieldExpression field = node as FieldExpression;
                if (null != field)
                {
                    IList<Inferred> result = Engine.Infer(this, field.Target, scope);
                    if (result != null)
                    {
                        foreach (Inferred s in result)
                        {
                            if (s.Names != null)
                            {
                                foreach (SymbolId name in s.Names)
                                {
                                    attributes.Add(new Declaration(name.GetString()));
                                }
                            }
                        }
                    }
                }
                else if (node != null && !(node is IronPython.Compiler.Ast.ConstantExpression))
                {
                    foreach (SymbolId name in scope.GetNamesCurrent())
                    {
                        attributes.Add(new Declaration(name.GetString()));
                    }
                    for (; ; )
                    {
                        scope = scope.Parent;
                        if (scope == null) break;
                        IEnumerable<SymbolId> namesOuter = scope.GetNamesOuter();
                        if (namesOuter != null)
                        {
                            foreach (SymbolId name in namesOuter)
                            {
                                attributes.Add(new Declaration(name.GetString()));
                            }
                        }
                    }
                    AddBuiltins(attributes);
                }
            }
            else
            {
                foreach (SymbolId name in module.GetNamesOuter())
                {
                    attributes.Add(new Declaration(name.GetString()));
                }
                AddBuiltins(attributes);
            }
            return attributes;
        }

        private void AddBuiltins(List<Declaration> attributes)
        {
            if (references.BuiltinNames != null)
            {
                foreach (SymbolId builtinName in references.BuiltinNames)
                {
                    attributes.Add(new Declaration(builtinName.GetString()));
                }
            }
        }

        // Disable the "ReviewUnusedParameters" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "name")]
        // Disable the "VariableNamesShouldNotMatchFieldNames" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500", MessageId = "name")]
        public IList<FunctionInfo> GetMethodsAt(int line, int column, string name)
        {
            Node node;
            Scope scope;
            Node context;
            IList<Inferred> methods = null;
            SymbolId nodeName = SymbolTable.Empty;
            if (Locate(typeof(CallExpression), line, column, out node, out scope, out context))
            {
                if (context != null)
                {
                    node = ((CallExpression)context).Target;
                }

                FieldExpression fe;
                NameExpression ne;

                if ((fe = node as FieldExpression) != null)
                {
                    nodeName = fe.Name;
                    methods = Engine.Infer(this, fe, scope);
                }
                else if ((ne = node as NameExpression) != null)
                {
                    nodeName = ne.Name;
                    methods = Engine.Infer(this, node, scope);
                }
            }
            if (methods != null)
            {
                IList<FunctionInfo> infos = null;
                foreach (Inferred inf in methods)
                {
                    infos = Engine.Union(infos, inf.InferMethods(nodeName));
                }
                return infos;
            }

            return null;
        }

        private bool Locate(Type contextType, int line, int column, out Node node, out Scope scope, out Node context)
        {
            Locator locator = new Locator(contextType, line, column);
            global.Walk(locator);
            node = locator.Candidate;
            scope = locator.Scope != null ? scopes[locator.Scope] : null;
            context = locator.Context;

#if DEBUG
            if (node != null)
            {
                Debug.Print("Located {0} in {1} at {2}:{3}-{4}:{5}",
                    node, context != null ? (object)context : (object)"<unknown>",
                    node.Start.Line, node.Start.Column,
                    node.End.Line, node.End.Column
                );
            }
#endif

            return node != null && context != null;
        }

        // Disable the "AvoidOutParameters" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021")]
        public bool Locate(int line, int column, out Node node, out Scope scope)
        {
            Locator locator = new Locator(line, column);
            global.Walk(locator);

            node = locator.Candidate;
            scope = locator.Scope != null ? scopes[locator.Scope] : null;

#if DEBUG
            if (node != null)
            {
                Debug.Print("Located {0} at {1}:{2}-{3}:{4}",
                    node,
                    node.Start.Line, node.Start.Column,
                    node.End.Line, node.End.Column
                );
            }
#endif

            return node != null;
        }
    }

    public class Modules
    {
        Dictionary<string, Module> modules = new Dictionary<string, Module>();
        ReflectedModule global = new ReflectedModule("<global>");
        Dictionary<Type, ReflectedType> reflected = new Dictionary<Type, ReflectedType>();

        InferredPythonType builtins = new InferredPythonType(typeof(IronPython.Modules.Builtin));

        InferredPythonType inferredInt = new InferredPythonType(typeof(IronPython.Runtime.Operations.IntOps));
        InferredPythonType inferredBgi = new InferredPythonType(typeof(IronPython.Runtime.Operations.LongOps));
        InferredPythonType inferredCpx = new InferredPythonType(typeof(IronPython.Runtime.Operations.ComplexOps));
        InferredPythonType inferredDbl = new InferredPythonType(typeof(IronPython.Runtime.Operations.FloatOps));
        InferredPythonType inferredStr = new InferredPythonType(typeof(IronPython.Runtime.Operations.StringOps));
        InferredPythonType inferredTpl = new InferredPythonType(typeof(IronPython.Runtime.Tuple));
        InferredPythonType inferredLst = new InferredPythonType(typeof(IronPython.Runtime.List));
        InferredPythonType inferredDct = new InferredPythonType(typeof(IronPython.Runtime.Dict));

        public Modules()
        {
            modules = new Dictionary<string, Module>();
            LoadTypes(typeof(string).Assembly);
            LoadTypes(typeof(System.Diagnostics.Debug).Assembly);
            LoadBuiltins();
        }

        public Module AnalyzeModule(CompilerSink sink, string name, string text)
        {
            Module module = Analyzer.Analyze(this, sink, name, text);
            return module;
        }

        // Disable the "VariableNamesShouldNotMatchFieldNames" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500", MessageId = "name")]
        internal bool TryImport(string baseName, string name, out Inferred inferred)
        {
            if (TryImportExternal(baseName, name, out inferred))
            {
                return true;
            }

            ReflectedModule reflectedScope;
            if (global.TryGetNamespace(name, out reflectedScope))
            {
                inferred = reflectedScope;
                return true;
            }

            ReflectedType reflectedType;
            if (global.TryGetBuiltin(name, out reflectedType))
            {
                inferred = reflectedType;
                return true;
            }

            inferred = null;
            return false;
        }

        // Disable the "DoNotCatchGeneralExceptionTypes" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031")]
        private bool TryImportExternal(string baseName, string name, out Inferred inferred)
        {
            string path = Path.GetDirectoryName(baseName);
            string file = Path.Combine(path, name + ".py");

            Module module;
            if (!modules.TryGetValue(file, out module))
            {
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(file);
                }
                catch
                {
                    inferred = null;
                    return false;
                }

                using (sr)
                {
                    string text = sr.ReadToEnd();
                    module = AnalyzeModule(new QuietCompilerSink(), file, text);
                }
            }

            inferred = new InferredModule(module.ModuleScope);
            return true;
        }

        internal Inferred InferType(Type type)
        {
            if (type == typeof(int))
            {
                return inferredInt;
            }
            else if (type == typeof(IronMath.BigInteger))
            {
                return inferredBgi;
            }
            else if (type == typeof(IronMath.Complex64))
            {
                return inferredCpx;
            }
            else if (type == typeof(double))
            {
                return inferredDbl;
            }
            else if (type == typeof(string))
            {
                return inferredStr;
            }
            else if (type == typeof(IronPython.Runtime.Tuple))
            {
                return inferredTpl;
            }
            else if (type == typeof(IronPython.Runtime.List))
            {
                return inferredLst;
            }
            else if (type == typeof(IronPython.Runtime.Dict))
            {
                return inferredDct;
            }
            else
            {
                ReflectedType rt;
                if (!reflected.TryGetValue(type, out rt))
                {
                    rt = new ReflectedType(type);
                    reflected[type] = rt;
                }
                return rt;
            }
        }

        internal IList<Inferred> InferBuiltin(SymbolId name, Engine engine)
        {
            return builtins.InferName(name, engine);
        }

        internal IEnumerable<SymbolId> BuiltinNames
        {
            get
            {
                return builtins.Names;
            }
        }

        private void LoadTypes(Assembly assembly)
        {
            Type[] types = assembly.GetExportedTypes();

            foreach (Type type in types)
            {
                ReflectedModule scope = global;
                string[] ns = type.Namespace.Split('.');
                string full = "";
                bool dot = false;
                foreach (string n in ns)
                {
                    full = dot ? full + "." + n : n;
                    dot = true;
                    scope = scope.EnsureNamespace(full, SymbolTable.StringToId(n));
                }
                scope.AddType(type);
            }
        }

        private void LoadBuiltins()
        {
            Assembly asm = typeof(IronPython.Hosting.PythonEngine).Assembly;
            object[] attributes = asm.GetCustomAttributes(typeof(PythonModuleAttribute), false);
            foreach (PythonModuleAttribute pma in attributes)
            {
                if (pma.type == typeof(IronPython.Modules.Builtin))
                {
                    global.AddType(pma.name, builtins);
                }
                else
                {
                    global.AddPythonType(pma.name, pma.type);
                }
            }
            global.AddPythonType("sys", typeof(IronPython.Runtime.SystemState));
            global.AddPythonType("clr", typeof(IronPython.Modules.ClrModule));
        }
    }

    public class Declaration : IComparable
    {
        public enum DeclarationType
        {
            Snippet,
            Class,
            Function,
            Unknown
        }
        public Declaration(string title)
        {
            this.Type = DeclarationType.Unknown;
            this.shortcut = "";
            this.title = title;
            this.description = "";
        }

        public Declaration(string shortcut, string title, DeclarationType type, string description)
        {
            this.Type = type;
            this.shortcut = shortcut;
            this.title = title;
            this.description = description;
        }


        protected Declaration()
        {
        }

        public int CompareTo(object obj)
        {
            Declaration decl = (Declaration)obj;
            return (this.title.CompareTo(decl.title));
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is Declaration))
                return false;
            return (this.CompareTo(obj) == 0);
        }

        public override int GetHashCode()
        {
            return this.title.GetHashCode();
        }

        // Disable the "IdentifiersShouldNotMatchKeywords" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062")]
        public static bool operator ==(Declaration d1, Declaration d2)
        {
            return d1.Equals(d2);
        }

        // Disable the "IdentifiersShouldNotMatchKeywords" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062")]
        public static bool operator !=(Declaration d1, Declaration d2)
        {
            return !(d1 == d2);
        }

        // Disable the "IdentifiersShouldNotMatchKeywords" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062")]
        public static bool operator <(Declaration d1, Declaration d2)
        {
            return (d1.CompareTo(d2) < 0);
        }

        // Disable the "IdentifiersShouldNotMatchKeywords" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062")]
        public static bool operator >(Declaration d1, Declaration d2)
        {
            return (d1.CompareTo(d2) > 0);
        }

        public DeclarationType Type
        {
            get { return type; }
            set { type = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public string Shortcut
        {
            get { return shortcut; }
            set { shortcut = value; }
        }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        private DeclarationType type;
        private string description;
        private string shortcut;
        private string title;
    }
}
