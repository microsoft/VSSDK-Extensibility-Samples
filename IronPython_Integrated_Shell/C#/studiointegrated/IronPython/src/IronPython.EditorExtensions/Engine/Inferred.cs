/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using IronPython.Compiler.Ast;
using IronPython.Runtime;

namespace Microsoft.VisualStudio.IronPythonInference
{
    public abstract class Inferred
    {
        public abstract bool IsInstance { get; }
        public abstract bool IsType { get; }
        public abstract bool IsCallable { get; }
        public abstract bool IsBound { get; }
        public abstract IEnumerable<SymbolId> Names { get; }

        public abstract IList<Inferred> InferName(SymbolId name, Engine engine);
        public abstract IList<FunctionInfo> InferMethods(SymbolId name);
        public abstract IList<Inferred> InferResult(Engine engine);
    }

    public class InferredClass : Inferred
    {
        private ClassScope cs;

        public InferredClass(ClassScope cs)
        {
            this.cs = cs;
        }

        public override bool IsInstance { get { return false; } }
        public override bool IsType { get { return true; } }
        public override bool IsCallable { get { return true; } }
        public override bool IsBound { get { return false; } }
        public override IEnumerable<SymbolId> Names { get { return cs.GetNamesCurrent(); } }

        public override IList<Inferred> InferName(SymbolId name, Engine engine)
        {
            return cs.ResolveCurrent(name, engine);
        }
        public override IList<FunctionInfo> InferMethods(SymbolId name)
        {
            return Engine.InferMethods(cs.Module, SymbolTable.Init, cs);
        }
        public override IList<Inferred> InferResult(Engine engine)
        {
            return Engine.MakeList<Inferred>(new InferredInstance(this));
        }
        public void Define(SymbolId name, Definition definition)
        {
            cs.Define(name, definition);
        }
    }

    public class InferredInstance : Inferred
    {
        Inferred type;

        public InferredInstance(Inferred type)
        {
            if (null == type)
            {
                throw new ArgumentNullException("type");
            }
            Debug.Assert(!type.IsInstance);
            this.type = type;
        }

        public override bool IsInstance { get { return true; } }
        public override bool IsType { get { return false; } }
        public override bool IsCallable { get { return false; } }
        public override bool IsBound { get { return false; } }
        public override IEnumerable<SymbolId> Names { get { return type.Names; } }
        public override IList<Inferred> InferName(SymbolId name, Engine engine)
        {
            IList<Inferred> inferred = type.InferName(name, engine);
            foreach (Inferred inf in inferred)
            {
                FunctionInfo fi = inf as FunctionInfo;
                if (fi != null)
                {
                    fi.Bind();
                }
            }
            return inferred;
        }
        public override IList<FunctionInfo> InferMethods(SymbolId name)
        {
            return type.InferMethods(name);
        }
        public override IList<Inferred> InferResult(Engine engine)
        {
            return type.InferResult(engine);
        }
    }

    public abstract class ReflectedMember : Inferred
    {
        public abstract Inferred Infer(Engine engine);

        public override bool IsInstance { get { return false; } }
        public override bool IsType { get { return false; } }
        public override bool IsCallable { get { return false; } }
        public override bool IsBound { get { return false; } }
        public override IEnumerable<SymbolId> Names { get { return null; } }

        public override IList<Inferred> InferName(SymbolId name, Engine engine)
        {
            return null;
        }
        public override IList<FunctionInfo> InferMethods(SymbolId name)
        {
            return null;
        }
        public override IList<Inferred> InferResult(Engine engine)
        {
            return null;
        }
    }

    public class ReflectedField : ReflectedMember
    {
        private FieldInfo info;
        public ReflectedField(FieldInfo info)
        {
            if (null == info)
            {
                throw new ArgumentNullException("info");
            }
            this.info = info;
        }
        public override Inferred Infer(Engine engine)
        {
            if (null == engine)
            {
                throw new ArgumentNullException("engine");
            }
            return engine.InferType(info.FieldType);
        }
    }
    public class ReflectedMethod : ReflectedMember
    {
        private MethodInfo info;
        public ReflectedMethod(MethodInfo info)
        {
            this.info = info;
        }
        public override bool IsCallable { get { return true; } }
        public override Inferred Infer(Engine engine)
        {
            return this;
        }
        public override IList<FunctionInfo> InferMethods(SymbolId name)
        {
            return Engine.MakeList<FunctionInfo>(new ReflectedMethodInfo(info));
        }
        public override IList<Inferred> InferResult(Engine engine)
        {
            Type type = info.ReturnType;
            if (type != typeof(void))
            {
                return Engine.MakeList(engine.InferType(info.ReturnType));
            }
            else return null;
        }
    }
    public class ReflectedProperty : ReflectedMember
    {
        private PropertyInfo info;
        public ReflectedProperty(PropertyInfo info)
        {
            this.info = info;
        }
        public override Inferred Infer(Engine engine)
        {
            if (null == engine)
            {
                throw new ArgumentNullException("engine");
            }
            return engine.InferType(info.PropertyType);
        }
    }
    public class ReflectedConstructor : ReflectedMember
    {
        private ConstructorInfo info;
        public ReflectedConstructor(ConstructorInfo info)
        {
            this.info = info;
        }
        public override bool IsCallable { get { return true; } }
        public override Inferred Infer(Engine engine)
        {
            return this;
        }
        public override IList<FunctionInfo> InferMethods(SymbolId name)
        {
            return Engine.MakeList<FunctionInfo>(new ReflectedConstructorInfo(info));
        }
        public override IList<Inferred> InferResult(Engine engine)
        {
            return Engine.MakeList(engine.InferType(info.DeclaringType));
        }
    }

    public class ReflectedType : Inferred
    {
        // Disable "DoNotDeclareVisibleInstanceFields" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051")]
        // Disable "DoNotNestGenericTypesInMemberSignatures" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006")]
        protected Dictionary<SymbolId, List<ReflectedMember>> members;
        // Disable "DoNotDeclareVisibleInstanceFields" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051")]
        protected readonly Type type;

        public ReflectedType(Type type)
        {
            this.type = type;
        }

        public override bool IsInstance { get { return false; } }
        public override bool IsType { get { return true; } }
        public override bool IsCallable { get { return true; } }
        public override bool IsBound { get { return false; } }

        public override IEnumerable<SymbolId> Names
        {
            get
            {
                Initialize();
                return members.Keys;
            }
        }

        public override IList<Inferred> InferName(SymbolId name, Engine engine)
        {
            Initialize();

            List<ReflectedMember> list;
            IList<Inferred> result = null;
            if (members.TryGetValue(name, out list))
            {
                foreach (ReflectedMember member in list)
                {
                    result = Engine.Append<Inferred>(result, member.Infer(engine));
                }
            }

            return result;
        }

        public override IList<FunctionInfo> InferMethods(SymbolId name)
        {
            Initialize();

            List<ReflectedMember> list;
            IList<FunctionInfo> result = null;
            if (members.TryGetValue(name, out list))
            {
                foreach (ReflectedMember member in list)
                {
                    ReflectedMethod method = member as ReflectedMethod;
                    if (null != method)
                    {
                        result = Engine.Union<FunctionInfo>(result, method.InferMethods(name));
                    }
                }
            }
            else if (type.Name == name.GetString())
            {
                members.TryGetValue(SymbolTable.StringToId(".ctor"), out list);
                foreach (ReflectedMember member in list)
                {
                    ReflectedConstructor constructor = member as ReflectedConstructor;
                    if (constructor != null)
                    {
                        result = Engine.Union<FunctionInfo>(result, constructor.InferMethods(name));
                    }
                }
            }

            return result;
        }

        public override IList<Inferred> InferResult(Engine engine)
        {
            return Engine.MakeList<Inferred>(this);
        }

        protected virtual void Initialize()
        {
            if (members != null) return;

            MemberInfo[] infos = type.GetMembers();

            members = new Dictionary<SymbolId, List<ReflectedMember>>();
            foreach (MemberInfo info in infos)
            {
                ReflectedMember md = CreateMemberDefinition(info);
                if (md != null)
                {
                    List<ReflectedMember> list;
                    SymbolId name = SymbolTable.StringToId(info.Name);
                    if (!members.TryGetValue(name, out list))
                    {
                        members[name] = list = new List<ReflectedMember>();
                    }
                    list.Add(md);
                }
            }
        }

        protected static ReflectedMember CreateMemberDefinition(MemberInfo info)
        {
            MethodInfo method;
            FieldInfo field;
            PropertyInfo property;
            ConstructorInfo constructor;

            ReflectedMember md;
            if ((method = info as MethodInfo) != null)
            {
                md = new ReflectedMethod(method);
            }
            else if ((field = info as FieldInfo) != null)
            {
                md = new ReflectedField(field);
            }
            else if ((property = info as PropertyInfo) != null)
            {
                md = new ReflectedProperty(property);
            }
            else if ((constructor = info as ConstructorInfo) != null)
            {
                md = new ReflectedConstructor(constructor);
            }
            else
            {
                md = null;
            }

            return md;
        }
    }

    public class ReflectedModule : Inferred
    {
        // Disable the "AvoidUnusedPrivateFields" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823")]
        private string full;

        private Dictionary<SymbolId, ReflectedType> types;
        private Dictionary<SymbolId, ReflectedModule> namespaces;

        public ReflectedModule(string full)
        {
            this.full = full;
            namespaces = new Dictionary<SymbolId, ReflectedModule>();
        }

        public override bool IsInstance { get { return false; } }
        public override bool IsType { get { return false; } }
        public override bool IsCallable { get { return false; } }
        public override bool IsBound { get { return false; } }
        public override IEnumerable<SymbolId> Names
        {
            get
            {
                return Engine.Union(new List<SymbolId>(types.Keys), new List<SymbolId>(namespaces.Keys));
            }
        }

        public override IList<Inferred> InferName(SymbolId name, Engine engine)
        {
            IList<Inferred> result = null;

            ReflectedModule rs;
            if (namespaces.TryGetValue(name, out rs))
            {
                result = Engine.MakeList<Inferred>(rs);
            }

            ReflectedType rt;
            if (types.TryGetValue(name, out rt))
            {
                result = Engine.Append<Inferred>(result, rt);
            }

            return result;
        }

        public override IList<FunctionInfo> InferMethods(SymbolId name)
        {
            return null;
        }

        public override IList<Inferred> InferResult(Engine engine)
        {
            return null;
        }

        // Disable the "VariableNamesShouldNotMatchFieldNames" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500", MessageId = "full")]
        public ReflectedModule EnsureNamespace(string full, SymbolId name)
        {
            ReflectedModule nested;
            if (!namespaces.TryGetValue(name, out nested))
            {
                nested = new ReflectedModule(full);
                namespaces[name] = nested;
            }
            return nested;
        }

        internal void AddType(string name, ReflectedType rt)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (null == rt)
            {
                throw new ArgumentNullException("rt");
            }
            if (types == null)
            {
                types = new Dictionary<SymbolId, ReflectedType>();
            }
            types[SymbolTable.StringToId(name)] = rt;
        }

        public void AddType(Type type)
        {
            if (null == type)
            {
                throw new ArgumentNullException("type");
            }
            AddType(type.Name, new ReflectedType(type));
        }

        public void AddPythonType(string name, Type type)
        {
            AddType(name, new InferredPythonType(type));
        }

        public bool TryGetNamespace(string name, out ReflectedModule scope)
        {
            return namespaces.TryGetValue(SymbolTable.StringToId(name), out scope);
        }

        // Disable the "IdentifiersShouldBeSpelledCorrectly" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704")]
        public bool TryGetBuiltin(string name, out ReflectedType scope)
        {
            return types.TryGetValue(SymbolTable.StringToId(name), out scope);
        }
    }

    public class InferredPythonType : ReflectedType
    {
        public InferredPythonType(Type type)
            : base(type)
        {
        }

        protected override void Initialize()
        {
            if (members != null) return;

            members = new Dictionary<SymbolId, List<ReflectedMember>>();

            MemberInfo[] infos = type.GetMembers();
            foreach (MemberInfo info in infos)
            {
                object[] attrs = info.GetCustomAttributes(typeof(IronPython.Runtime.PythonNameAttribute), false);
                if (attrs == null || attrs.Length == 0) continue;
                IronPython.Runtime.PythonNameAttribute attr = attrs[0] as IronPython.Runtime.PythonNameAttribute;

                ReflectedMember md = CreateMemberDefinition(info);
                if (md != null)
                {
                    SymbolId name = SymbolTable.StringToId(attr.name);
                    List<ReflectedMember> list;
                    if (!members.TryGetValue(name, out list))
                    {
                        members[name] = list = new List<ReflectedMember>();
                    }
                    list.Add(md);
                }
            }
        }
    }

    public abstract class FunctionInfo : Inferred
    {
        private bool bound;

        public abstract string Name { get; }
        public abstract string Type { get; }
        public abstract string Description { get; }
        public abstract int ParameterCount { get; }
        // Disable the "AvoidOutParameters" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021")]
        public abstract void GetParameterInfo(int parameter, out string name, out string display, out string description);

        public override bool IsInstance { get { return false; } }
        public override bool IsType { get { return false; } }
        public override bool IsCallable { get { return true; } }
        public override bool IsBound { get { return bound; } }
        public override IEnumerable<SymbolId> Names { get { return null; } }

        public void Bind()
        {
            bound = true;
        }
        public override IList<Inferred> InferName(SymbolId name, Engine engine)
        {
            return null;
        }
        public override IList<FunctionInfo> InferMethods(SymbolId name)
        {
            return Engine.MakeList<FunctionInfo>(this);
        }
        public override IList<Inferred> InferResult(Engine engine)
        {
            return null;
        }
    }

    public class FunctionDefinitionInfo : FunctionInfo
    {
        private IronPython.Compiler.Ast.FunctionDefinition function;

        public FunctionDefinitionInfo(IronPython.Compiler.Ast.FunctionDefinition function)
        {
            this.function = function;
        }
        public override string Name
        {
            get { return function.Name.GetString(); }
        }
        public override string Type
        {
            get { return string.Empty; }
        }
        public override string Description
        {
            get
            {
                string description = function.Documentation;
                return (description != null && description.Length > 0) ? description : "Function " + Name;
            }
        }
        public override int ParameterCount
        {
            get
            {
                int count = function.Parameters.Count;
                if (count > 0 && IsBound)
                {
                    count--;
                }
                return count;
            }
        }
        public override void GetParameterInfo(int parameter, out string name, out string display, out string description)
        {
            if (IsBound && (parameter < int.MaxValue)) parameter++;

            Expression parm = function.Parameters[parameter];
            NameExpression nameExpression = parm as NameExpression;
            if (null != nameExpression)
            {
                name = nameExpression.Name.GetString();
                display = name;
                description = name;
            }
            else
            {
                name = display = description = String.Empty;
            }
        }
    }

    public class ReflectedMethodInfo : FunctionInfo
    {
        private System.Reflection.MethodInfo method;
        private System.Reflection.ParameterInfo[] parameters;

        public ReflectedMethodInfo(MethodInfo method)
        {
            this.method = method;
        }

        public override string Name
        {
            get { return method.Name; }
        }
        public override string Type
        {
            get { return method.ReturnType.Name; }
        }
        public override string Description
        {
            get { return method.Name; }
        }
        public override int ParameterCount
        {
            get { EnsureParameters(); return parameters.Length; }
        }
        public override void GetParameterInfo(int parameter, out string name, out string display, out string description)
        {
            EnsureParameters();
            System.Reflection.ParameterInfo param = parameters[parameter];
            name = param.Name;
            display = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1}", param.ParameterType.Name, param.Name);
            description = param.Name;
        }
        private void EnsureParameters()
        {
            if (parameters == null)
            {
                parameters = method.GetParameters();
            }
        }
    }

    public class ReflectedConstructorInfo : FunctionInfo
    {
        private System.Reflection.ConstructorInfo constructor;
        private System.Reflection.ParameterInfo[] parameters;

        public ReflectedConstructorInfo(ConstructorInfo constructor)
        {
            this.constructor = constructor;
        }
        public override string Name
        {
            get { return constructor.DeclaringType.Name; }
        }
        public override string Description
        {
            get { return Name; }
        }
        public override string Type
        {
            get { return constructor.DeclaringType.Name; }
        }
        public override int ParameterCount
        {
            get { EnsureParameters(); return parameters.Length; }
        }
        public override void GetParameterInfo(int parameter, out string name, out string display, out string description)
        {
            EnsureParameters();
            System.Reflection.ParameterInfo param = parameters[parameter];
            name = param.Name;
            display = string.Format(System.Globalization.CultureInfo.InstalledUICulture, "{0} {1}", param.ParameterType.Name, param.Name);
            description = param.Name;
        }
        private void EnsureParameters()
        {
            if (parameters == null)
            {
                parameters = constructor.GetParameters();
            }
        }
    }

    public class InferredModule : Inferred
    {
        private ModuleScope module;

        public InferredModule(ModuleScope module)
        {
            this.module = module;
        }
        public override bool IsInstance { get { return false; } }
        public override bool IsType { get { return false; } }
        public override bool IsCallable { get { return false; } }
        public override bool IsBound { get { return false; } }
        public override IEnumerable<SymbolId> Names
        {
            get { return module.GetNamesCurrent(); }
        }
        public override IList<Inferred> InferName(SymbolId name, Engine engine)
        {
            return module.ResolveCurrent(name, engine);
        }
        public override IList<FunctionInfo> InferMethods(SymbolId name)
        {
            return null;
        }
        public override IList<Inferred> InferResult(Engine engine)
        {
            return null;
        }
    }
}
