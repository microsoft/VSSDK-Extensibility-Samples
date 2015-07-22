/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using IronPython.Compiler.Ast;
using IronPython.Runtime;

namespace Microsoft.VisualStudio.IronPythonInference
{
    public abstract class Definition
    {
        public abstract IList<Inferred> Resolve(Engine engine, Scope scope);
    }

    public class ArgumentDefinition : Definition
    {
        private IronPython.Compiler.Ast.FunctionDefinition function;
        private int argument;
        private SymbolId name;

        public ArgumentDefinition(SymbolId name, IronPython.Compiler.Ast.FunctionDefinition function, int argument)
        {
            this.name = name;
            this.function = function;
            this.argument = argument;
        }

        public override IList<Inferred> Resolve(Engine engine, Scope scope)
        {
            if (argument == 0 && name == SymbolTable.StringToId("self"))
            {
                ScopeStatement parent = function.Parent;
                if (parent != null)
                {
                    return engine.Infer(parent, scope);
                }
            }
            return null;
        }
    }

    public class AssignmentDefinition : Definition
    {
        private AssignStatement assignment;
        public AssignmentDefinition(AssignStatement assignment)
        {
            this.assignment = assignment;
        }

        public override IList<Inferred> Resolve(Engine engine, Scope scope)
        {
            return engine.Infer(assignment.Right, scope);
        }
    }

    public class ForDefinition : Definition
    {
        // Disable the "AvoidUnusedPrivateFields" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823")]
        private ForStatement forStatement;
        public ForDefinition(ForStatement forStatement)
        {
            this.forStatement = forStatement;
        }
        public override IList<Inferred> Resolve(Engine engine, Scope scope)
        {
            return null;
        }
    }

    public class ListCompForDefinition : Definition
    {
        // Disable the "AvoidUnusedPrivateFields" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823")]
        private ListComprehensionFor listCompFor;
        public ListCompForDefinition(ListComprehensionFor listCompFor)
        {
            this.listCompFor = listCompFor;
        }

        public override IList<Inferred> Resolve(Engine engine, Scope scope)
        {
            return null;
        }
    }

    public class ClassDefinition : Definition
    {
        private IronPython.Compiler.Ast.ClassDefinition @class;
        public ClassDefinition(IronPython.Compiler.Ast.ClassDefinition @class)
        {
            this.@class = @class;
        }

        public override IList<Inferred> Resolve(Engine engine, Scope scope)
        {
            return engine.Infer(@class, scope);
        }
    }

    public class FunctionDefinition : Definition
    {
        private IronPython.Compiler.Ast.FunctionDefinition function;
        public FunctionDefinition(IronPython.Compiler.Ast.FunctionDefinition function)
        {
            this.function = function;
        }

        internal IronPython.Compiler.Ast.FunctionDefinition Function
        {
            // Disable the "AvoidUncalledPrivateCode" warning.
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811")]
            get { return function; }
        }

        public override IList<Inferred> Resolve(Engine engine, Scope scope)
        {
            return Engine.MakeList<Inferred>(new FunctionDefinitionInfo(function));
        }
    }

    public class DelDefinition : Definition
    {
        // Disable the "AvoidUnusedPrivateFields" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823")]
        private DelStatement delStatement;
        public DelDefinition(DelStatement delStatement)
        {
            this.delStatement = delStatement;
        }

        public override IList<Inferred> Resolve(Engine engine, Scope scope)
        {
            return null;
        }
    }

    public class TryDefinition : Definition
    {
        // Disable the "AvoidUnusedPrivateFields" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823")]
        private TryStatement tryStatement;
        // Disable the "AvoidUnusedPrivateFields" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823")]
        private TryStatementHandler tryHandler;
        public TryDefinition(TryStatement tryStatement, TryStatementHandler tryHandler)
        {
            this.tryStatement = tryStatement;
            this.tryHandler = tryHandler;
        }
        public override IList<Inferred> Resolve(Engine engine, Scope scope)
        {
            return null;
        }
    }

    public class GlobalDefinition : Definition
    {
        public GlobalDefinition()
        {
        }
        public override IList<Inferred> Resolve(Engine engine, Scope scope)
        {
            return null;
        }
    }

    public class ImportDefinition : Definition
    {
        private DottedName name;
        // Disable the "AvoidUncalledPrivateCode" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823")]
        private ImportStatement importStatement;
        public ImportDefinition(DottedName name, ImportStatement import)
        {
            this.name = name;
            this.importStatement = import;
        }

        public override IList<Inferred> Resolve(Engine engine, Scope scope)
        {
            Inferred import = engine.Import(name.Names[0]);
            IList<Inferred> previous = null;

            if (import != null)
            {
                previous = Engine.MakeList(import);

                for (int i = 1; i < name.Names.Count; i++)
                {
                    IList<Inferred> next = null;
                    foreach (Inferred inf in previous)
                    {
                        IList<Inferred> n2 = inf.InferName(name.Names[i], engine);
                        next = Engine.Union(next, n2);
                    }
                    previous = next;
                }
            }
            return previous;
        }
    }

    public class FromImportDefinition : Definition
    {
        // Disable the "AvoidUnusedPrivateFields" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823")]
        private FromImportStatement from;
        public FromImportDefinition(FromImportStatement from)
        {
            this.from = from;
        }
        public override IList<Inferred> Resolve(Engine engine, Scope scope)
        {
            return null;
        }
    }

    public class DirectDefinition : Definition
    {
        private Type type;
        public DirectDefinition(Type type)
        {
            this.type = type;
        }
        public override IList<Inferred> Resolve(Engine engine, Scope scope)
        {
            return Engine.MakeList(engine.InferType(type));
        }
    }

    public class IndirectDefinition : Definition
    {
        private Expression expression;
        private Scope scope;

        public IndirectDefinition(Expression expression, Scope scope)
        {
            this.expression = expression;
            this.scope = scope;
        }

        // Disable the "VariableNamesShouldNotMatchFieldNames" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500")]
        public override IList<Inferred> Resolve(Engine engine, Scope scope)
        {
            return engine.Infer(this.expression, this.scope);
        }
    }
}
