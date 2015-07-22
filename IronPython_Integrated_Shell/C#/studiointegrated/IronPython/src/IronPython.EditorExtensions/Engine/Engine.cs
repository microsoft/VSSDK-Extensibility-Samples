/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using IronPython.Compiler.Ast;
using IronPython.Runtime;

namespace Microsoft.VisualStudio.IronPythonInference
{
    public class Engine
    {
        private const int MaxDepth = 5;

        private bool global;
        private int depth;
        private Module module;

        private Engine(Module module, bool global)
        {
            this.module = module;
            this.global = global;
        }

        public Inferred Import(SymbolId name)
        {
            if (name == SymbolTable.Empty)
            {
                return null;
            }
            Inferred inferred;
            if (global && module.TryImport(name.GetString(), out inferred))
            {
                return inferred;
            }
            else return null;
        }

        public Inferred InferType(Type type)
        {
            return module.InferType(type);
        }

        public static Engine Create(Module module, bool global)
        {
            return new Engine(module, global);
        }

        public static IList<Inferred> Infer(Module module, Node node, Scope scope)
        {
            return Infer(module, node, scope, true);
        }

        public static IList<Inferred> Infer(Module module, Node node, Scope scope, bool global)
        {
            return new Engine(module, global).Infer(node, scope);
        }

        public static IList<FunctionInfo> InferMethods(Module module, SymbolId name, Scope scope)
        {
            return new Engine(module, false).InferMethods(name, scope);
        }

        private IList<FunctionInfo> InferMethods(SymbolId name, Scope scope)
        {
            IList<Inferred> inferred = Infer(new NameExpression(name), scope);
            IList<FunctionInfo> methods = null;
            if (inferred != null)
            {
                foreach (Inferred inf in inferred)
                {
                    methods = Engine.Union(methods, inf.InferMethods(name));
                }
            }
            return methods;
        }

        private IList<Inferred> InferBuiltin(SymbolId name)
        {
            return module.InferBuiltin(name, this);
        }

        // Disable the "AvoidExcessiveComplexity" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502")]
        // Disable the "DoNotCastUnnecessarily" warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800")]
        public IList<Inferred> Infer(Node node, Scope scope)
        {
            if (depth++ > MaxDepth)
            {
                return null;
            }

            IList<Inferred> inferred = null;
            if (node is AndExpression)
            {
                inferred = InferAndExpression((AndExpression)node, scope);
            }
            else if (node is AssertStatement)
            {
                inferred = InferAssertStatement((AssertStatement)node, scope);
            }
            else if (node is AssignStatement)
            {
                inferred = InferAssignStatement((AssignStatement)node, scope);
            }
            else if (node is AugAssignStatement)
            {
                inferred = InferAugAssignStatement((AugAssignStatement)node, scope);
            }
            else if (node is BackQuoteExpression)
            {
                inferred = InferBackQuoteExpression((BackQuoteExpression)node, scope);
            }
            else if (node is BinaryExpression)
            {
                inferred = InferBinaryExpression((BinaryExpression)node, scope);
            }
            else if (node is BreakStatement)
            {
                inferred = InferBreakStatement((BreakStatement)node, scope);
            }
            else if (node is CallExpression)
            {
                inferred = InferCallExpression((CallExpression)node, scope);
            }
            else if (node is IronPython.Compiler.Ast.ClassDefinition)
            {
                inferred = InferClassDef((IronPython.Compiler.Ast.ClassDefinition)node, scope);
            }
            else if (node is ConstantExpression)
            {
                inferred = InferConstantExpression((ConstantExpression)node, scope);
            }
            else if (node is ContinueStatement)
            {
                inferred = InferContinueStatement((ContinueStatement)node, scope);
            }
            else if (node is DelStatement)
            {
                inferred = InferDelStatement((DelStatement)node, scope);
            }
            else if (node is DictionaryExpression)
            {
                inferred = InferDictionaryExpression((DictionaryExpression)node, scope);
            }
            else if (node is ErrorExpression)
            {
                inferred = InferErrorExpression((ErrorExpression)node, scope);
            }
            else if (node is ExecStatement)
            {
                inferred = InferExecStatement((ExecStatement)node, scope);
            }
            else if (node is ExpressionStatement)
            {
                inferred = InferExpressionStatement((ExpressionStatement)node, scope);
            }
            else if (node is FieldExpression)
            {
                inferred = InferFieldExpression((FieldExpression)node, scope);
            }
            else if (node is ForStatement)
            {
                inferred = InferForStatement((ForStatement)node, scope);
            }
            else if (node is FromImportStatement)
            {
                inferred = InferFromImportStatement((FromImportStatement)node, scope);
            }
            else if (node is IronPython.Compiler.Ast.FunctionDefinition)
            {
                inferred = InferFuncDef((IronPython.Compiler.Ast.FunctionDefinition)node, scope);
            }
            else if (node is GeneratorExpression)
            {
                inferred = InferGeneratorExpression((GeneratorExpression)node, scope);
            }
            else if (node is GlobalSuite)
            {
                inferred = InferGlobalScope((GlobalSuite)node, scope);
            }
            else if (node is GlobalStatement)
            {
                inferred = InferGlobalStatement((GlobalStatement)node, scope);
            }
            else if (node is IfStatement)
            {
                inferred = InferIfStatement((IfStatement)node, scope);
            }
            else if (node is ImportStatement)
            {
                inferred = InferImportStatement((ImportStatement)node, scope);
            }
            else if (node is IndexExpression)
            {
                inferred = InferIndexExpression((IndexExpression)node, scope);
            }
            else if (node is LambdaExpression)
            {
                inferred = InferLambdaExpression((LambdaExpression)node, scope);
            }
            else if (node is ListComprehension)
            {
                inferred = InferListComp((ListComprehension)node, scope);
            }
            else if (node is ListExpression)
            {
                inferred = InferListExpression((ListExpression)node, scope);
            }
            else if (node is NameExpression)
            {
                inferred = InferNameExpression((NameExpression)node, scope);
            }
            else if (node is OrExpression)
            {
                inferred = InferOrExpression((OrExpression)node, scope);
            }
            else if (node is ParenthesisExpression)
            {
                inferred = InfeRightParenthesisExpression((ParenthesisExpression)node, scope);
            }
            else if (node is PassStatement)
            {
                inferred = InferPassStatement((PassStatement)node, scope);
            }
            else if (node is PrintStatement)
            {
                inferred = InferPrintStatement((PrintStatement)node, scope);
            }
            else if (node is RaiseStatement)
            {
                inferred = InferRaiseStatement((RaiseStatement)node, scope);
            }
            else if (node is ReturnStatement)
            {
                inferred = InferReturnStatement((ReturnStatement)node, scope);
            }
            else if (node is ScopeStatement)
            {
                inferred = InferScopeStatement((ScopeStatement)node, scope);
            }
            else if (node is SliceExpression)
            {
                inferred = InferSliceExpression((SliceExpression)node, scope);
            }
            else if (node is SuiteStatement)
            {
                inferred = InferSuiteStatement((SuiteStatement)node, scope);
            }
            else if (node is TryFinallyStatement)
            {
                inferred = InferTryFinallyStatement((TryFinallyStatement)node, scope);
            }
            else if (node is TryStatement)
            {
                inferred = InferTryStatement((TryStatement)node, scope);
            }
            else if (node is TupleExpression)
            {
                inferred = InferTupleExpression((TupleExpression)node, scope);
            }
            else if (node is UnaryExpression)
            {
                inferred = InferUnaryExpression((UnaryExpression)node, scope);
            }
            else if (node is WhileStatement)
            {
                inferred = InferWhileStatement((WhileStatement)node, scope);
            }
            else if (node is YieldStatement)
            {
                inferred = InferYieldStatement((YieldStatement)node, scope);
            }
            else
            {
                Debug.Fail("invalid node");
                inferred = null;
            }
            depth--;
            return inferred;
        }


        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private IList<Inferred> InferAndExpression(AndExpression node, Scope scope)
        {
            return Union(Infer(node.Left, scope), Infer(node.Right, scope));
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private IList<Inferred> InferAssertStatement(AssertStatement node, Scope scope)
        {
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private IList<Inferred> InferAssignStatement(AssignStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferAssignStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private IList<Inferred> InferAugAssignStatement(AugAssignStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferAugAssignStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private IList<Inferred> InferBackQuoteExpression(BackQuoteExpression node, Scope scope)
        {
            Debug.Print("Not implemented: InferBackQuoteExpressionIn");
            return null;
        }

        private IList<Inferred> InferBinaryExpression(BinaryExpression node, Scope scope)
        {
            return Union(Infer(node.Left, scope), Infer(node.Right, scope));
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private IList<Inferred> InferBreakStatement(BreakStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferBreakStatementIn");
            return null;
        }

        private IList<Inferred> InferCallExpression(CallExpression node, Scope scope)
        {
            IList<Inferred> targets = Infer(node.Target, scope);
            IList<Inferred> results = null;
            if (targets != null)
            {
                foreach (Inferred inf in targets)
                {
                    if (inf.IsCallable)
                    {
                        results = Engine.Union(results, inf.InferResult(this));
                    }
                }
            }
            return results;
        }

        // Disable the warning about the unused parameter.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferClassDef(IronPython.Compiler.Ast.ClassDefinition node, Scope scope)
        {
            return Engine.MakeList<Inferred>(module.GetClass(node));
        }

        // Disable the warning about the unused parameter.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferConstantExpression(ConstantExpression node, Scope scope)
        {
            if (node.Value != null)
            {
                return MakeList(module.InferType(node.Value.GetType()));
            }
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferContinueStatement(ContinueStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferContinueStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferDelStatement(DelStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferDelStatementIn");
            return null;
        }

        // Disable the warnings about unused parameters.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferDictionaryExpression(DictionaryExpression node, Scope scope)
        {
            return MakeList(module.InferType(typeof(IronPython.Runtime.Dict)));
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferErrorExpression(ErrorExpression node, Scope scope)
        {
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferExecStatement(ExecStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferExecStatementIn");
            return null;
        }

        private IList<Inferred> InferExpressionStatement(ExpressionStatement node, Scope scope)
        {
            return Infer(node.Expression, scope);
        }

        private IList<Inferred> InferFieldExpression(FieldExpression node, Scope scope)
        {
            IList<Inferred> list = Infer(node.Target, scope);
            IList<Inferred> result = null;
            if (list != null)
            {
                foreach (Inferred i in list)
                {
                    IList<Inferred> l2 = i.InferName(node.Name, this);
                    result = Union(result, l2);
                }
            }
            return result;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferForStatement(ForStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferForStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferFromImportStatement(FromImportStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferFromImportStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferFuncDef(IronPython.Compiler.Ast.FunctionDefinition node, Scope scope)
        {
            Debug.Print("Not implemented: InferFuncDefIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferGeneratorExpression(GeneratorExpression node, Scope scope)
        {
            Debug.Print("Not implemented: InferGeneratorExpressionIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferGlobalScope(GlobalSuite node, Scope scope)
        {
            Debug.Print("Not implemented: InferGlobalScopeIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferGlobalStatement(GlobalStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferGlobalStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferIfStatement(IfStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferIfStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferImportStatement(ImportStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferImportStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferIndexExpression(IndexExpression node, Scope scope)
        {
            Debug.Print("Not implemented: InferIndexExpressionIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferLambdaExpression(LambdaExpression node, Scope scope)
        {
            Debug.Print("Not implemented: InferLambdaExpressionIn");
            return null;
        }

        // Disable the warning about the unused parameters.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferListComp(ListComprehension node, Scope scope)
        {
            return MakeList(module.InferType(typeof(IronPython.Runtime.List)));
        }

        // Disable the warning about the unused parameters.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferListExpression(ListExpression node, Scope scope)
        {
            return MakeList(module.InferType(typeof(IronPython.Runtime.List)));
        }

        private IList<Inferred> InferNameExpression(NameExpression node, Scope scope)
        {
            IList<Inferred> inferred = scope.ResolveCurrent(node.Name, this);
            while (inferred == null || inferred.Count == 0)
            {
                scope = scope.Parent;
                if (scope == null) break;
                inferred = scope.ResolveOuter(node.Name, this);
            }

            if (inferred == null || inferred.Count == 0)
            {
                inferred = InferBuiltin(node.Name);
            }

            return inferred;
        }

        private IList<Inferred> InferOrExpression(OrExpression node, Scope scope)
        {
            return Union(Infer(node.Left, scope), Infer(node.Right, scope));
        }

        private IList<Inferred> InfeRightParenthesisExpression(ParenthesisExpression node, Scope scope)
        {
            return Infer(node.Expression, scope);
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferPassStatement(PassStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferPassStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferPrintStatement(PrintStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferPrintStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferRaiseStatement(RaiseStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferRaiseStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferReturnStatement(ReturnStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferReturnStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferScopeStatement(ScopeStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferScopeStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferSliceExpression(SliceExpression node, Scope scope)
        {
            Debug.Print("Not implemented: InferSliceExpressionIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferSuiteStatement(SuiteStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferSuiteStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferTryFinallyStatement(TryFinallyStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferTryFinallyStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferTryStatement(TryStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferTryStatementIn");
            return null;
        }

        // Disable the warning about the unused parameters.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferTupleExpression(TupleExpression node, Scope scope)
        {
            return MakeList(module.InferType(typeof(IronPython.Runtime.Tuple)));
        }

        private IList<Inferred> InferUnaryExpression(UnaryExpression node, Scope scope)
        {
            return Infer(node.Expression, scope);
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferWhileStatement(WhileStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferWhileStatementIn");
            return null;
        }

        // Disable the "MarkMethodsAsStatic" warning because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        // Disable the warning about the unused parameters because this method is not implemented.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "node")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "scope")]
        private List<Inferred> InferYieldStatement(YieldStatement node, Scope scope)
        {
            Debug.Print("Not implemented: InferYieldStatementIn");
            return null;
        }

        internal static IList<T> Union<T>(IList<T> a, IList<T> b)
        {
            if (a == null) return b;
            if (b == null) return a;

            List<T> union = new List<T>(a);
            union.AddRange(b);
            return union;
        }

        internal static List<T> MakeList<T>(T o)
        {
            List<T> list = null;
            if (o != null)
            {
                list = new List<T>(1);
                list.Add(o);
            }
            return list;
        }

        internal static IList<T> Append<T>(IList<T> list, T o)
        {
            if (o != null)
            {
                if (list == null)
                {
                    list = new List<T>();
                }
                list.Add(o);
            }
            return list;
        }
    }
}
