/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Diagnostics;
using System.Collections.Generic;
using IronPython.Compiler;
using IronPython.Compiler.Ast;

namespace Microsoft.VisualStudio.IronPythonInference
{
    public class Locator : IAstWalker
    {
        Location location;
        ScopeStatement current;
        Stack<Node> context;
        Type contextType;

        Node candidateNode;
        ScopeStatement candidateScope;
        Node candidateContext;

        public Locator(int line, int column)
        {
            location.Line = line;
            location.Column = column;
        }

        public Locator(Type contextType, int line, int column)
            : this(line, column)
        {
            this.contextType = contextType;
        }

        public Node Candidate
        {
            get { return candidateNode; }
        }
        public ScopeStatement Scope
        {
            get { return candidateScope; }
        }
        public Node Context
        {
            get { return candidateContext; }
        }

        private bool Process(Node node)
        {
            if (contextType != null && node.GetType() == contextType)
            {
                PushContext(node);
            }
            if (node.Start <= location && location < node.End)
            {
                SaveCandidate(node);
            }
            return true;
        }

        private void PostProcess(Node node)
        {
            if (contextType != null && node.GetType() == contextType)
            {
                PopContext(node);
            }
        }

        private void PushContext(Node n)
        {
            Debug.Assert(contextType != null && n.GetType() == contextType);
            if (context == null)
            {
                context = new Stack<Node>();
            }
            context.Push(n);
        }

        // Disable the "ReviewUnusedParameters" warning because n is used only in debug.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801", MessageId = "n")]
        private void PopContext(Node n)
        {
            Debug.Assert(contextType != null && n.GetType() == contextType);
            Debug.Assert(context != null && context.Count > 0);
            Node n2 = context.Pop();
            Debug.Assert((object)n2 == (object)n);
        }

        private void SaveCandidate(Node node)
        {
            if (candidateNode == null || Better(node, candidateNode))
            {
                Debug.Print("Candidate: {0} at {1}:{2}-{3}:{4} ({5}:{6})",
                    node, node.Start.Line, node.Start.Column, node.End.Line, node.End.Column,
                    location.Line, location.Column);

                candidateNode = node;
                candidateScope = current;
                candidateContext = context != null && context.Count > 0 ? context.Peek() : null;
            }
        }

        private static bool Smaller(Node one, Node two)
        {
            int oneL = one.End.Line - one.Start.Line;
            int twoL = two.End.Line - two.Start.Line;

            if (oneL < twoL)
            {
                return true;
            }
            else if (oneL > twoL)
            {
                return false;
            }
            else
            {
                int oneC = one.End.Column - one.Start.Column;
                int twoC = one.End.Column - one.Start.Column;

                if (oneC < twoC)
                {
                    return true;
                }
                else if (oneC > twoC)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private static bool Better(Node one, Node two)
        {
            if (one.Start > two.Start)
            {
                if (one.End > two.End)
                {
                    return Smaller(one, two);
                }
                else if (one.End < two.End)
                {
                    return true;
                }
                else
                {
                    return true;
                }
            }
            else if (one.Start < two.Start)
            {
                if (one.End > two.End)
                {
                    return false;
                }
                else if (one.End < two.End)
                {
                    return Smaller(one, two);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (one.End > two.End)
                {
                    return false;
                }
                else if (one.End < two.End)
                {
                    return true;
                }
                else
                {
                    return true;
                }
            }
        }

        #region IWalker Members

        // AndExpression
        public bool Walk(AndExpression node) { return Process(node); }
        public void PostWalk(AndExpression node) { PostProcess(node); }

        // BackQuoteExpression
        public bool Walk(BackQuoteExpression node) { return Process(node); }
        public void PostWalk(BackQuoteExpression node) { PostProcess(node); }

        // BinaryExpression
        public bool Walk(BinaryExpression node) { return Process(node); }
        public void PostWalk(BinaryExpression node) { PostProcess(node); }

        // CallExpression
        public bool Walk(CallExpression node) { return Process(node); }
        public void PostWalk(CallExpression node) { PostProcess(node); }

        // ConditionalExpression
        public bool Walk(ConditionalExpression node) { return Process(node); }
        public void PostWalk(ConditionalExpression node) { PostProcess(node); }

        // ConstantExpression
        public bool Walk(ConstantExpression node) { return Process(node); }
        public void PostWalk(ConstantExpression node) { PostProcess(node); }

        // DictionaryExpression
        public bool Walk(DictionaryExpression node) { return Process(node); }
        public void PostWalk(DictionaryExpression node) { PostProcess(node); }

        // ErrorExpression
        public bool Walk(ErrorExpression node) { return Process(node); }
        public void PostWalk(ErrorExpression node) { PostProcess(node); }

        // FieldExpression
        public bool Walk(FieldExpression node) { return Process(node); }
        public void PostWalk(FieldExpression node) { PostProcess(node); }

        // GeneratorExpression
        public bool Walk(GeneratorExpression node) { return Process(node); }
        public void PostWalk(GeneratorExpression node) { PostProcess(node); }

        // IndexExpression
        public bool Walk(IndexExpression node) { return Process(node); }
        public void PostWalk(IndexExpression node) { PostProcess(node); }

        // LambdaExpression
        public bool Walk(LambdaExpression node) { return Process(node); }
        public void PostWalk(LambdaExpression node) { PostProcess(node); }

        // ListComprehension
        public bool Walk(ListComprehension node) { return Process(node); }
        public void PostWalk(ListComprehension node) { PostProcess(node); }

        // ListExpression
        public bool Walk(ListExpression node) { return Process(node); }
        public void PostWalk(ListExpression node) { PostProcess(node); }

        // NameExpression
        public bool Walk(NameExpression node) { return Process(node); }
        public void PostWalk(NameExpression node) { PostProcess(node); }

        // OrExpression
        public bool Walk(OrExpression node) { return Process(node); }
        public void PostWalk(OrExpression node) { PostProcess(node); }

        // ParenthesisExpression
        public bool Walk(ParenthesisExpression node) { return Process(node); }
        public void PostWalk(ParenthesisExpression node) { PostProcess(node); }

        // SliceExpression
        public bool Walk(SliceExpression node) { return Process(node); }
        public void PostWalk(SliceExpression node) { PostProcess(node); }

        // TupleExpression
        public bool Walk(TupleExpression node) { return Process(node); }
        public void PostWalk(TupleExpression node) { PostProcess(node); }

        // UnaryExpression
        public bool Walk(UnaryExpression node) { return Process(node); }
        public void PostWalk(UnaryExpression node) { PostProcess(node); }

        // AssertStatement
        public bool Walk(AssertStatement node) { return Process(node); }
        public void PostWalk(AssertStatement node) { PostProcess(node); }

        // AssignStatement
        public bool Walk(AssignStatement node) { return Process(node); }
        public void PostWalk(AssignStatement node) { PostProcess(node); }

        // AugAssignStatement
        public bool Walk(AugAssignStatement node) { return Process(node); }
        public void PostWalk(AugAssignStatement node) { PostProcess(node); }

        // BreakStatement
        public bool Walk(BreakStatement node) { return Process(node); }
        public void PostWalk(BreakStatement node) { PostProcess(node); }

        // ClassDefinition
        public bool Walk(IronPython.Compiler.Ast.ClassDefinition node) { current = node; return Process(node); }
        public void PostWalk(IronPython.Compiler.Ast.ClassDefinition node) { AssertCurrent(node); current = current.Parent; }

        // ContinueStatement
        public bool Walk(ContinueStatement node) { return Process(node); }
        public void PostWalk(ContinueStatement node) { PostProcess(node); }

        // DelStatement
        public bool Walk(DelStatement node) { return Process(node); }
        public void PostWalk(DelStatement node) { PostProcess(node); }

        // ExecStatement
        public bool Walk(ExecStatement node) { return Process(node); }
        public void PostWalk(ExecStatement node) { PostProcess(node); }

        // ExpressionStatement
        public bool Walk(ExpressionStatement node) { return Process(node); }
        public void PostWalk(ExpressionStatement node) { PostProcess(node); }

        // ForStatement
        public bool Walk(ForStatement node) { return Process(node); }
        public void PostWalk(ForStatement node) { PostProcess(node); }

        // FromImportStatement
        public bool Walk(FromImportStatement node) { return Process(node); }
        public void PostWalk(FromImportStatement node) { PostProcess(node); }

        // FunctionDefinition
        public bool Walk(IronPython.Compiler.Ast.FunctionDefinition node) { current = node; return Process(node); }
        public void PostWalk(IronPython.Compiler.Ast.FunctionDefinition node) { AssertCurrent(node); current = current.Parent; }

        // GlobalStatement
        public bool Walk(GlobalStatement node) { return Process(node); }
        public void PostWalk(GlobalStatement node) { PostProcess(node); }

        // GlobalSuite
        public bool Walk(GlobalSuite node) { current = node; return Process(node); }
        public void PostWalk(GlobalSuite node) { AssertCurrent(node); current = current.Parent; }

        // IfStatement
        public bool Walk(IfStatement node) { return Process(node); }
        public void PostWalk(IfStatement node) { PostProcess(node); }

        // ImportStatement
        public bool Walk(ImportStatement node) { return Process(node); }
        public void PostWalk(ImportStatement node) { PostProcess(node); }

        // PassStatement
        public bool Walk(PassStatement node) { return Process(node); }
        public void PostWalk(PassStatement node) { PostProcess(node); }

        // PrintStatement
        public bool Walk(PrintStatement node) { return Process(node); }
        public void PostWalk(PrintStatement node) { PostProcess(node); }

        // RaiseStatement
        public bool Walk(RaiseStatement node) { return Process(node); }
        public void PostWalk(RaiseStatement node) { PostProcess(node); }

        // ReturnStatement
        public bool Walk(ReturnStatement node) { return Process(node); }
        public void PostWalk(ReturnStatement node) { PostProcess(node); }

        // SuiteStatement
        public bool Walk(SuiteStatement node) { return Process(node); }
        public void PostWalk(SuiteStatement node) { PostProcess(node); }

        // TryFinallyStatement
        public bool Walk(TryFinallyStatement node) { return Process(node); }
        public void PostWalk(TryFinallyStatement node) { PostProcess(node); }

        // TryStatement
        public bool Walk(TryStatement node) { return Process(node); }
        public void PostWalk(TryStatement node) { PostProcess(node); }

        // WhileStatement
        public bool Walk(WhileStatement node) { return Process(node); }
        public void PostWalk(WhileStatement node) { PostProcess(node); }

        // YieldStatement
        public bool Walk(YieldStatement node) { return Process(node); }
        public void PostWalk(YieldStatement node) { PostProcess(node); }

        // Arg
        public bool Walk(Arg node) { return Process(node); }
        public void PostWalk(Arg node) { PostProcess(node); }

        // DottedName
        public bool Walk(DottedName node) { return Process(node); }
        public void PostWalk(DottedName node) { PostProcess(node); }

        // IfStatementTest
        public bool Walk(IfStatementTest node) { return Process(node); }
        public void PostWalk(IfStatementTest node) { PostProcess(node); }

        // ListComprehensionFor
        public bool Walk(ListComprehensionFor node) { return Process(node); }
        public void PostWalk(ListComprehensionFor node) { PostProcess(node); }

        // ListComprehensionIf
        public bool Walk(ListComprehensionIf node) { return Process(node); }
        public void PostWalk(ListComprehensionIf node) { PostProcess(node); }

        // TryStatementHandler
        public bool Walk(TryStatementHandler node) { return Process(node); }
        public void PostWalk(TryStatementHandler node) { PostProcess(node); }

        // WithStatement
        public bool Walk(WithStatement node) { return Process(node); }
        public void PostWalk(WithStatement node) { PostProcess(node); }

        #endregion

        [Conditional("DEBUG")]
        // Disable the "MarkMethodsAsStatic" warning because current is used in debug.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822")]
        private void AssertCurrent(ScopeStatement node)
        {
            Debug.Assert((object)node == (object)current);
        }
    }
}
