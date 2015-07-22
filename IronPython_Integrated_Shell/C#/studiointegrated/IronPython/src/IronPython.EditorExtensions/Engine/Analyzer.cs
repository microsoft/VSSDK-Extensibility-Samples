/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using IronPython.Compiler;
using IronPython.Compiler.Ast;
using IronPython.Hosting;
using IronPython.Runtime;

namespace Microsoft.VisualStudio.IronPythonInference
{
    internal abstract class DefineAnalyzer : AstWalkerNonRecursive
    {
        protected Analyzer analyzer;

        protected DefineAnalyzer(Analyzer analyzer)
        {
            this.analyzer = analyzer;
        }

        protected abstract void Define(SymbolId name);

        #region Overriden AstWalkerFalse methods

        public override bool Walk(NameExpression node)
        {
            if (null == node)
            {
                throw new ArgumentNullException("node");
            }
            Define(node.Name);
            return false;
        }

        public override bool Walk(ParenthesisExpression node)
        {
            return true;
        }

        public override bool Walk(TupleExpression node)
        {
            return true;
        }

        #endregion
    }

    internal class ArgumentAnalyzer : DefineAnalyzer
    {
        private IronPython.Compiler.Ast.FunctionDefinition function;
        private int argument;

        public ArgumentAnalyzer(Analyzer analyzer)
            : base(analyzer)
        {
        }

        protected override void Define(SymbolId name)
        {
            analyzer.Define(name, new ArgumentDefinition(name, function, argument));
        }

        // Disable the "VariableNamesShouldNotMatchFieldNames" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500")]
        public void Analyze(IronPython.Compiler.Ast.FunctionDefinition function)
        {
            this.argument = 0;
            this.function = function;

            foreach (Expression param in function.Parameters)
            {
                param.Walk(this);
                argument++;
            }
        }
    }

    internal class AssignmentAnalyzer : DefineAnalyzer
    {
        private AssignStatement assignment;

        public AssignmentAnalyzer(Analyzer analyzer)
            : base(analyzer)
        {
        }

        protected override void Define(SymbolId name)
        {
            analyzer.Define(name, new AssignmentDefinition(assignment));
        }

        // Disable the "VariableNamesShouldNotMatchFieldNames" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500")]
        public void Analyze(AssignStatement assignment)
        {
            this.assignment = assignment;
            foreach (Expression e in assignment.Left)
            {
                FieldExpression field = e as FieldExpression;
                if (null != field)
                {
                    analyzer.SaveFieldExpressionession(field, assignment.Right);
                }
                e.Walk(this);
            }
        }
    }

    internal class TryAnalyzer : DefineAnalyzer
    {
        private TryStatement tryStatement;
        private TryStatementHandler tryHandler;

        public TryAnalyzer(Analyzer analyzer)
            : base(analyzer)
        {
        }

        protected override void Define(SymbolId name)
        {
            analyzer.Define(name, new TryDefinition(tryStatement, tryHandler));
        }

        public void Analyze(TryStatement ts)
        {
            this.tryStatement = ts;
            foreach (TryStatementHandler tsh in tryStatement.Handlers)
            {
                this.tryHandler = tsh;
                if (tsh.Target != null)
                {
                    tsh.Target.Walk(this);
                }
            }
        }
    }

    internal class ForAnalyzer : DefineAnalyzer
    {
        private ForStatement forStatement;

        public ForAnalyzer(Analyzer analyzer)
            : base(analyzer)
        {
        }

        protected override void Define(SymbolId name)
        {
            analyzer.Define(name, new ForDefinition(forStatement));
        }

        public void Analyze(ForStatement fs)
        {
            forStatement = fs;
            forStatement.Left.Walk(this);
        }
    }

    internal class DelAnalyzer : DefineAnalyzer
    {
        private DelStatement delStatement;
        public DelAnalyzer(Analyzer analyzer)
            : base(analyzer)
        {
        }

        protected override void Define(SymbolId name)
        {
            analyzer.Define(name, new DelDefinition(delStatement));
        }

        public void Analyze(DelStatement ds)
        {
            delStatement = ds;
            foreach (Expression e in delStatement.Expressions)
            {
                e.Walk(this);
            }
        }
    }

    internal class ListCompForAnalyzer : DefineAnalyzer
    {
        private ListComprehensionFor lcf;

        public ListCompForAnalyzer(Analyzer analyzer)
            : base(analyzer)
        {
        }

        protected override void Define(SymbolId name)
        {
            analyzer.Define(name, new ListCompForDefinition(lcf));
        }

        // Disable the "VariableNamesShouldNotMatchFieldNames" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1500")]
        public void Analyze(ListComprehensionFor lcf)
        {
            this.lcf = lcf;
            lcf.Left.Walk(this);
        }
    }

    internal class FieldAssignment
    {
        private readonly FieldExpression fe;
        private readonly Expression rhs;
        private readonly Scope anchor;

        public FieldAssignment(FieldExpression fe, Expression rhs, Scope anchor)
        {
            this.fe = fe;
            this.rhs = rhs;
            this.anchor = anchor;
        }

        public void Infer(Module module)
        {
            Engine e = Engine.Create(module, false);

            IList<Inferred> left = e.Infer(fe.Target, anchor);

            if (left != null)
            {
                foreach (Inferred inf in left)
                {
                    InferredClass ic = inf as InferredClass;
                    if (ic != null)
                    {
                        ic.Define(fe.Name, new IndirectDefinition(rhs, anchor));
                    }
                }
            }
        }
    }

    public class Analyzer : AstWalker
    {
        private Module module;
        private Scope current;

        private Dictionary<ScopeStatement, Scope> scopes = new Dictionary<ScopeStatement, Scope>();
        private List<FieldAssignment> fields = new List<FieldAssignment>();
        private static SystemState state;

        #region Recursive analyzers

        private ArgumentAnalyzer argumentAnalyzer;
        private AssignmentAnalyzer assignmentAnalyzer;
        private TryAnalyzer tryAnalyzer;
        private ForAnalyzer forAnalyzer;
        private DelAnalyzer delAnalyzer;
        private ListCompForAnalyzer lcfAnalyzer;

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static Analyzer()
        {
            state = new SystemState();
            PythonFile empty = new PythonFile(System.IO.Stream.Null, System.Text.Encoding.Default, "rw");
            state.__stderr__ = empty;
            state.__stdin__ = empty;
            state.__stdout__ = empty;
        }

        private Analyzer()
        {
            argumentAnalyzer = new ArgumentAnalyzer(this);
            assignmentAnalyzer = new AssignmentAnalyzer(this);
            tryAnalyzer = new TryAnalyzer(this);
            forAnalyzer = new ForAnalyzer(this);
            delAnalyzer = new DelAnalyzer(this);
            lcfAnalyzer = new ListCompForAnalyzer(this);
        }

        public static Module Analyze(Modules modules, CompilerSink sink, string name, string text)
        {
            CompilerContext context = new CompilerContext(name, sink);
            Parser parser = Parser.FromString(state, context, text);
            Statement Statement = parser.ParseFileInput();

            Analyzer analyzer = new Analyzer();
            return analyzer.DoAnalyze(modules, name, Statement);
        }

        private Module DoAnalyze(Modules modules, string name, Statement root)
        {
            GlobalSuite global = new GlobalSuite(root);
            module = new Module(modules, name, global, scopes);

            ModuleScope modsc;
            module.ModuleScope = modsc = new ModuleScope(module, null, global);

            PushScope(modsc);

            root.Walk(this);

            foreach (FieldAssignment fer in this.fields)
            {
                fer.Infer(module);
            }
            return module;
        }

        #region AstWalker Method Overrides

        // NameExpression
        public override bool Walk(NameExpression node)
        {
            if (null == node)
            {
                throw new ArgumentNullException("node");
            }
            Reference(node.Name);
            return true;
        }

        // AssignStatement
        public override bool Walk(AssignStatement node)
        {
            assignmentAnalyzer.Analyze(node);
            return true;
        }

        // ClassDefinition
        public override bool Walk(IronPython.Compiler.Ast.ClassDefinition node)
        {
            Define(node.Name, new ClassDefinition(node));

            // Base references are in the outer scope
            foreach (Expression b in node.Bases) b.Walk(this);

            // And so is the __name__ reference
            Reference(SymbolTable.Name);

            PushScope(node);

            // define the __doc__
            SymbolId doc = SymbolTable.Doc;
            Define(doc, new DirectDefinition(typeof(string)));

            // Walk the body
            node.Body.Walk(this);
            return false;
        }
        public override void PostWalk(IronPython.Compiler.Ast.ClassDefinition node)
        {
            PopScope();
        }

        // DelStatement
        public override bool Walk(DelStatement node)
        {
            delAnalyzer.Analyze(node);
            return true;
        }

        // ForStatement
        public override bool Walk(ForStatement node)
        {
            forAnalyzer.Analyze(node);
            return true;
        }

        // FromImportStatement
        public override bool Walk(FromImportStatement node)
        {
            if (node.Names != FromImportStatement.Star)
            {
                for (int i = 0; i < node.Names.Count; i++)
                {
                    SymbolId name = node.AsNames[i] != SymbolTable.Empty ? node.AsNames[i] : node.Names[i];
                    Define(name, new FromImportDefinition(node));
                }
            }
            return true;
        }

        // FunctionDefinition
        public override bool Walk(IronPython.Compiler.Ast.FunctionDefinition node)
        {
            // Name is defined in the enclosing scope
            Define(node.Name, new FunctionDefinition(node));

            // process the default arg values in the outer scope
            foreach (Expression e in node.Defaults)
            {
                e.Walk(this);
            }
            // process the decorators in the outer scope
            if (node.Decorators != null)
            {
                node.Decorators.Walk(this);
            }

            PushScope(node);

            argumentAnalyzer.Analyze(node);
            return true;
        }
        public override void PostWalk(IronPython.Compiler.Ast.FunctionDefinition node)
        {
            PopScope();
        }

        // GlobalStatement
        public override bool Walk(GlobalStatement node)
        {
            foreach (SymbolId n in node.Names)
            {
                Global(n);
            }
            return true;
        }

        // GlobalSuite
        public override void PostWalk(GlobalSuite node)
        {
            PopScope();
        }

        // ImportStatement
        public override bool Walk(ImportStatement node)
        {
            for (int i = 0; i < node.Names.Count; i++)
            {
                SymbolId name = node.AsNames[i] != SymbolTable.Empty ? node.AsNames[i] : node.Names[i].Names[0];
                Define(name, new ImportDefinition(node.Names[i], node));
            }
            return true;
        }

        // TryStatement
        public override bool Walk(TryStatement node)
        {
            tryAnalyzer.Analyze(node);
            return true;
        }

        // DottedName
        public override bool Walk(DottedName node)
        {
            Reference(node.Names[0]);
            return true;
        }

        // ListCompFor
        public override bool Walk(ListComprehensionFor node)
        {
            lcfAnalyzer.Analyze(node);
            return true;
        }

        #endregion

        internal void SaveFieldExpressionession(FieldExpression fieldExpression, Expression rhs)
        {
            fields.Add(new FieldAssignment(fieldExpression, rhs, current));
        }

        private void PushScope(IronPython.Compiler.Ast.FunctionDefinition func)
        {
            PushScope(new FunctionScope(module, current, func));
        }
        private void PushScope(IronPython.Compiler.Ast.ClassDefinition cls)
        {
            PushScope(new ClassScope(module, current, cls));
        }

        private void PushScope(Scope scope)
        {
            Debug.Assert(!scopes.ContainsKey(scope.Statement));
            scope.Statement.Parent = current != null ? current.Statement : null;
            scopes[scope.Statement] = scope;
            current = scope;
        }

        private void PopScope()
        {
            current = current.Parent;
            Debug.Assert(scopes.ContainsKey(current.Statement));
        }

        public void Define(SymbolId name, Definition definition)
        {
            if (name != SymbolTable.Empty)
            {
                current.Define(name, definition);
            }
        }

        // Disable the "MarkMethodsAsStatic" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "name")]
        public void Reference(SymbolId name)
        {
        }

        // Disable the "MarkMethodsAsStatic" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "name")]
        public void Global(SymbolId name)
        {
        }
    }
}
