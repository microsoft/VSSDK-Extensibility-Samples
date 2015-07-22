/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

using IronPython.Compiler.Ast;
using IronPython.Runtime;

namespace Microsoft.VisualStudio.IronPythonInference
{
    // Disable the "IdentifiersShouldDifferByMoreThanCase" warning.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1708")]
    public abstract class Scope
    {
        private Module module;
        private Scope parent;
        // Disable the "DoNotDeclareVisibleInstanceFields" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051")]
        // Disable the "DoNotNestGenericTypesInMemberSignatures" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006")]
        protected Dictionary<SymbolId, List<Definition>> definitions = new Dictionary<SymbolId, List<Definition>>();

        protected Scope(Module module, Scope parent)
        {
            this.module = module;
            this.parent = parent;
        }

        public Scope Parent
        {
            get { return parent; }
        }

        public Module Module
        {
            get { return module; }
        }

        public abstract ScopeStatement Statement { get; }

        // Disable the "UsePropertiesWhereAppropriate" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024")]
        public IEnumerable<SymbolId> GetNamesCurrent()
        {
            return definitions.Keys;
        }
        // Disable the "UsePropertiesWhereAppropriate" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024")]
        public abstract IEnumerable<SymbolId> GetNamesOuter();

        public void Define(SymbolId name, Definition definition)
        {
            List<Definition> list;

            if (!definitions.TryGetValue(name, out list))
            {
                list = new List<Definition>();
                definitions[name] = list;
            }

            list.Add(definition);
        }

        public IList<Inferred> ResolveCurrent(SymbolId name, Engine engine)
        {
            List<Definition> defs;
            IList<Inferred> inferred = null;
            if (definitions.TryGetValue(name, out defs))
            {
                foreach (Definition definition in defs)
                {
                    inferred = Engine.Union<Inferred>(inferred, definition.Resolve(engine, this));
                }
            }
            return inferred;
        }

        public abstract IList<Inferred> ResolveOuter(SymbolId name, Engine engine);
    }


    public class FunctionScope : Scope
    {
        private IronPython.Compiler.Ast.FunctionDefinition statement;

        public FunctionScope(Module module, Scope parent, IronPython.Compiler.Ast.FunctionDefinition statement)
            : base(module, parent)
        {
            this.statement = statement;
        }

        public override ScopeStatement Statement
        {
            get { return statement; }
        }

        public override IEnumerable<SymbolId> GetNamesOuter()
        {
            return GetNamesCurrent();
        }

        public override IList<Inferred> ResolveOuter(SymbolId name, Engine engine)
        {
            return ResolveCurrent(name, engine);
        }
    }

    public class ClassScope : Scope
    {
        private IronPython.Compiler.Ast.ClassDefinition statement;

        public ClassScope(Module module, Scope parent, IronPython.Compiler.Ast.ClassDefinition statement)
            : base(module, parent)
        {
            this.statement = statement;
        }

        public override ScopeStatement Statement
        {
            get { return statement; }
        }

        public override IEnumerable<SymbolId> GetNamesOuter()
        {
            return null;
        }

        public override IList<Inferred> ResolveOuter(SymbolId name, Engine engine)
        {
            return null;
        }
    }

    public class ModuleScope : Scope
    {
        private GlobalSuite statement;

        public ModuleScope(Module module, Scope parent, GlobalSuite statement)
            : base(module, parent)
        {
            this.statement = statement;
        }

        public override ScopeStatement Statement
        {
            get { return statement; }
        }

        public override IEnumerable<SymbolId> GetNamesOuter()
        {
            return GetNamesCurrent();
        }

        public override IList<Inferred> ResolveOuter(SymbolId name, Engine engine)
        {
            return ResolveCurrent(name, engine);
        }
    }
}
