/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

namespace SqliteVisualizer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.Debugger;
    using Microsoft.VisualStudio.Debugger.Evaluation;

    /// <summary>
    /// Class wrapper for func-evals on a native sqlite instance.
    /// </summary>
    internal class SqliteFuncEvalQuery : IDisposable
    {
        private static readonly int PAGE_READWRITE = 0x4;
        private static readonly int MEM_COMMIT = 0x1000;
        private static readonly int MEM_RESERVE = 0x2000;
        private static readonly int MEM_RELEASE = 0x8000;

        private readonly DkmInspectionContext inspectionContext;
        private readonly string sqliteInstanceName;
        private readonly string query;
        private readonly int uniqueTableId;
        private readonly ulong procMemForQuery;

        private volatile bool queryComplete = false;
        private bool isDisposed;

        private SqliteFuncEvalQuery(
            DkmInspectionContext inspectionContext,
            string sqliteInstanceName,
            string query)
        {
            Debug.Assert(
                inspectionContext != null
                && !string.IsNullOrEmpty(sqliteInstanceName)
                && !string.IsNullOrEmpty(query), "Invalid arguments");

            this.inspectionContext = inspectionContext;
            this.sqliteInstanceName = sqliteInstanceName;
            this.query = query;

            DkmProcess process = this.inspectionContext.Thread.Process;
            this.procMemForQuery = process.AllocateVirtualMemory(0, 8, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            this.uniqueTableId = this.procMemForQuery.GetHashCode() ^ this.sqliteInstanceName.GetHashCode() ^ this.query.GetHashCode();
        }

        /// <summary>
        /// Gets the column names for the result of the query
        /// </summary>
        public IEnumerable<string> ColumnNames { get; private set; }

        /// <summary>
        /// Gets a unique ID for this table instance
        /// </summary>
        public int UniqueTableId => this.uniqueTableId;

        /// <summary>
        /// Create a new <see cref="SqliteFuncEvalQuery"/> instance.
        /// </summary>
        /// <param name="inspectionContext">Context for all function evaluations</param>
        /// <param name="sqliteInstanceName">Property name of a 'sqlite3 *' instance</param>
        /// <param name="query">Query to execute</param>
        /// <returns><see cref="SqliteFuncEvalQuery"/> instance</returns>
        public static async Task<SqliteFuncEvalQuery> CreateAsync(
            DkmInspectionContext inspectionContext,
            string sqliteInstanceName,
            string query)
        {
            // Initialize the query
            return await Task.Run(() =>
            {
                bool isAlreadyInit;
                DkmComponentManager.InitializeThread(DkmComponentManager.IdeComponentId, out isAlreadyInit);

                try
                {
                    var funcEval = new SqliteFuncEvalQuery(inspectionContext, sqliteInstanceName, query);

                    funcEval.PrepareQuery();
                    int columnCount = funcEval.QueryColumnCount();
                    funcEval.QueryColumnDetails(columnCount);

                    return funcEval;
                }
                finally
                {
                    if (!isAlreadyInit)
                    {
                        DkmComponentManager.UninitializeThread(DkmComponentManager.IdeComponentId);
                    }
                }
            });
        }

        /// <summary>
        /// Execute the query and return rows up to the maximum
        /// </summary>
        /// <param name="maxRows">Maximum number of rows to returns</param>
        /// <param name="token"><see cref="CancellationToken"/> instance</param>
        /// <returns><see cref="IEnumerable{T}"/> of row data</returns>
        public async Task<IEnumerable<string[]>> ExecuteAsync(int maxRows, CancellationToken token)
        {
            if (this.queryComplete)
            {
                return Enumerable.Empty<string[]>();
            }

            return await Task.Run(() =>
            {
                bool isAlreadyInit;
                DkmComponentManager.InitializeThread(DkmComponentManager.IdeComponentId, out isAlreadyInit);

                try
                {
                    return this.GetRows(maxRows, token);
                }
                finally
                {
                    if (!isAlreadyInit)
                    {
                        DkmComponentManager.UninitializeThread(DkmComponentManager.IdeComponentId);
                    }
                }
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            bool isAlreadyInit;
            DkmComponentManager.InitializeThread(DkmComponentManager.IdeComponentId, out isAlreadyInit);

            try
            {
                this.FinalizeQuery();

                DkmProcess process = this.inspectionContext.Thread.Process;
                process.FreeVirtualMemory(this.procMemForQuery, 0, MEM_RELEASE);
            }
            finally
            {
                if (!isAlreadyInit)
                {
                    DkmComponentManager.UninitializeThread(DkmComponentManager.IdeComponentId);
                }
            }

            this.isDisposed = true;
        }

        private bool VerifySuccess(DkmEvaluationResult result, out DkmSuccessEvaluationResult success)
        {
            // Only return the result if it was successful
            success = result as DkmSuccessEvaluationResult;
            if (success == null)
            {
                Debug.Fail("Query has failed");
                Debug.WriteLine($"Failure during {result.FullName} evaluation");
                return false;
            }

            return true;
        }

        private void PrepareQuery()
        {
            DkmLanguageExpression expr = null;

            try
            {
                SqliteVisualizerException ex = null;
                DkmWorkList workList = DkmWorkList.Create(null);
                expr = this.AddFuncEval(
                    workList,
                    $"sqlite3_prepare({sqliteInstanceName}, \"{query}\", {query.Length + 1}, (sqlite3_stmt **){this.procMemForQuery}, nullptr)",
                    (r) =>
                    {
                        DkmSuccessEvaluationResult suc;
                        if (!this.VerifySuccess(r, out suc))
                        {
                            ex = new SqliteVisualizerException(Resources.ErrMsg_PrepareFailed, r.FullName);
                            return;
                        }
                    });

                workList.Execute();

                if (ex != null)
                {
                    throw ex;
                }
            }
            finally
            {
                if (expr != null)
                {
                    expr.Close();
                }
            }
        }

        private int QueryColumnCount()
        {
            int columnCount = 0;
            DkmLanguageExpression expr = null;

            try
            {
                DkmWorkList workList = DkmWorkList.Create(null);
                SqliteVisualizerException ex = null;
                expr = this.AddFuncEval(
                    workList,
                    $"sqlite3_column_count(*(sqlite3_stmt **){this.procMemForQuery})",
                    (r) =>
                    {
                        DkmSuccessEvaluationResult suc;
                        if (!this.VerifySuccess(r, out suc))
                        {
                            ex = new SqliteVisualizerException(Resources.ErrMsg_FuncEvalFailed, r.FullName);
                            return;
                        }

                        columnCount = (int)suc.Address.Value;
                    });

                workList.Execute();

                if (ex != null)
                {
                    throw ex;
                }
            }
            finally
            {
                if (expr != null)
                {
                    expr.Close();
                }
            }

            return columnCount;
        }

        private void QueryColumnDetails(int columnCount)
        {
            var exprs = new List<DkmLanguageExpression>();

            try
            {
                DkmProcess process = this.inspectionContext.Thread.Process;
                DkmWorkList workList = DkmWorkList.Create(null);
                SqliteVisualizerException ex = null;
                var columnNamesLocal = new string[columnCount];
                for (int i = 0; i < columnCount; ++i)
                {
                    var i_local = i;
                    var colName = this.AddFuncEval(
                        workList,
                        $"sqlite3_column_name(*(sqlite3_stmt **){this.procMemForQuery}, {i})",
                        (r) =>
                        {
                            DkmSuccessEvaluationResult suc;
                            if (!this.VerifySuccess(r, out suc))
                            {
                                ex = ex ?? new SqliteVisualizerException(Resources.ErrMsg_FuncEvalFailed, r.FullName);
                                return;
                            }

                            ulong address = suc.Address.Value;
                            byte[] stringMaybe = process.ReadMemoryString(address, DkmReadMemoryFlags.None, 1, 1024);
                            int len = stringMaybe.Length;
                            if (len > 0)
                            {
                                // The debugger null terminates all strings, but encoding doesn't strip null when creating a string
                                len--;
                                columnNamesLocal[i_local] = Encoding.UTF8.GetString(stringMaybe, 0, len);
                            }
                        });

                    exprs.Add(colName);
                }

                workList.Execute();

                if (ex != null)
                {
                    throw ex;
                }

                this.ColumnNames = columnNamesLocal;
            }
            finally
            {
                foreach (var e in exprs)
                {
                    e.Close();
                }
            }
        }

        private IEnumerable<string[]> GetRows(int maxRows, CancellationToken token)
        {
            var rows = new List<string[]>();
            for (int i = 0; i < maxRows; ++i)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                string[] row;
                if (!TryGetRow(out row))
                {
                    this.queryComplete = true;
                    break;
                }

                rows.Add(row);
            }

            return rows;
        }

        private bool TryGetRow(out string[] row)
        {
            // Move to the next row
            bool moreRows = false;
            var exprs = new List<DkmLanguageExpression>();

            try
            {
                const int SQLITE_ROW = 100;
                SqliteVisualizerException ex = null;
                DkmWorkList workList = DkmWorkList.Create(null);
                DkmLanguageExpression expr = this.AddFuncEval(
                    workList,
                    $"sqlite3_step(*(sqlite3_stmt **){this.procMemForQuery})",
                    (r) =>
                    {
                        DkmSuccessEvaluationResult suc;
                        if (!this.VerifySuccess(r, out suc))
                        {
                            ex = new SqliteVisualizerException(Resources.ErrMsg_FuncEvalFailed, r.FullName);
                            return;
                        }

                        moreRows = SQLITE_ROW == (int)suc.Address.Value;
                    });

                exprs.Add(expr);
                workList.Execute();

                if (ex != null)
                {
                    throw ex;
                }
            }
            finally
            {
                foreach (var e in exprs)
                {
                    e.Close();
                }

                exprs.Clear();
            }

            if (!moreRows)
            {
                row = new string[0];
                return false;
            }

            // Read each column in the row
            var rowLocal = new string[ColumnNames.Count()];
            try
            {
                SqliteVisualizerException ex = null;
                DkmProcess process = this.inspectionContext.Thread.Process;
                DkmWorkList workList = DkmWorkList.Create(null);
                for (int i = 0; i < rowLocal.Length; i++)
                {
                    var i_local = i;
                    var e = this.AddFuncEval(
                        workList,
                        $"sqlite3_column_text(*(sqlite3_stmt **){this.procMemForQuery}, {i})",
                        (r) =>
                        {
                            DkmSuccessEvaluationResult suc;
                            if (!this.VerifySuccess(r, out suc))
                            {
                                ex = ex ?? new SqliteVisualizerException(Resources.ErrMsg_FuncEvalFailed, r.FullName);
                                return;
                            }

                            ulong address = suc.Address.Value;
                            byte[] stringMaybe = process.ReadMemoryString(address, DkmReadMemoryFlags.None, 1, 1024);
                            int len = stringMaybe.Length;
                            if (len > 0)
                            {
                                // The debugger null terminates all strings, but encoding doesn't strip null when creating a string
                                len--;
                                rowLocal[i_local] = Encoding.UTF8.GetString(stringMaybe, 0, len);
                            }
                        });

                    exprs.Add(e);
                }

                workList.Execute();

                if (ex != null)
                {
                    throw ex;
                }
            }
            finally
            {
                foreach (var e in exprs)
                {
                    e.Close();
                }
            }

            row = rowLocal;
            return true;
        }

        private void FinalizeQuery()
        {
            DkmLanguageExpression expr = null;

            try
            {
                SqliteVisualizerException ex = null;
                DkmWorkList workList = DkmWorkList.Create(null);
                expr = this.AddFuncEval(
                    workList,
                    $"sqlite3_finalize(*(sqlite3_stmt **){this.procMemForQuery})",
                    (r) =>
                    {
                        DkmSuccessEvaluationResult suc;
                        if (!this.VerifySuccess(r, out suc))
                        {
                            ex = new SqliteVisualizerException(Resources.ErrMsg_FuncEvalFailed, r.FullName);
                            return;
                        }
                    });

                workList.Execute();

                if (ex != null)
                {
                    throw ex;
                }
            }
            finally
            {
                if (expr != null)
                {
                    expr.Close();
                }
            }
        }

        private DkmLanguageExpression AddFuncEval(
            DkmWorkList workList,
            string funcEval,
            Action<DkmEvaluationResult> evalResult)
        {
            Debug.Assert(workList != null && !string.IsNullOrEmpty(funcEval), "Arguments should not be null");
            Debug.WriteLine($"Creating func-eval: '{funcEval}'");

            // Create the evaluation to add
            var langExpr = DkmLanguageExpression.Create(
                this.inspectionContext.Language,
                DkmEvaluationFlags.None,
                funcEval,
                null);

            this.inspectionContext.EvaluateExpression(
                workList,
                langExpr,
                this.inspectionContext.Thread.GetTopStackFrame(),
                delegate (DkmEvaluateExpressionAsyncResult asyncResult)
                {
                    evalResult(asyncResult.ResultObject);
                });

            return langExpr;
        }
    }
}
