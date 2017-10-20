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
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.Debugger.Evaluation;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Threading;

    /// <summary>
    /// Visualizer view model
    /// </summary>
    internal class VisualizerWindowViewModel : IDisposable, INotifyPropertyChanged
    {
        private static readonly int MaxRowsPerRequest = 25;

        private readonly IDictionary<String, TableDetails> tableDetails = new Dictionary<String, TableDetails>();
        private readonly ModuleBuilder modBuilder;

        private IEnumerable<String> tables;
        private ObservableCollection<object> tableRows;
        private bool isDataLoaded = true;
        private bool showOverlay = false;
        private String overlayMessage = String.Empty;
        private String selectedTable;

        private bool isDisposed = false;
        private String sqliteInstanceName;
        private JoinableTask loadSelectedTableRowsTask;
        private DkmInspectionContext inspectionContext;
        private CancellationTokenSource cancelTokenSource;

        /// <summary>
        /// Create a new <see cref="SqliteVisualizerService"/> instance.
        /// </summary>
        /// <param name="modBuilder"><see cref="ModuleBuilder"/> instance for all dynamic types</param>
        public VisualizerWindowViewModel(ModuleBuilder modBuilder)
        {
            Debug.Assert(modBuilder != null, "Argument should not be null");
            this.modBuilder = modBuilder;

#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
            this.LoadNextCommand = new RelayCommand(async (o) =>
            {
                await this.LoadSelectedTableRowsAsync(newSelectedTable: false);
            });
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates

            this.CancelCommand = new RelayCommand((o) =>
            {
                // If the cancel call is initiated, the current table details should be already available
                var detailsTask = this.GetTableDetailsAsync(this.SelectedTable);
                Debug.Assert(detailsTask.IsCompleted, "Task should be complete");

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                TableDetails details = detailsTask.Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

                Debug.Assert(this.cancelTokenSource != null, "CancellationTokenSource should not be null");
                this.cancelTokenSource.Cancel();

                this.CancelCommand.Enable = false;
            });
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the available tables
        /// </summary>
        public IEnumerable<String> AvailableTables
        {
            get
            {
                return this.tables;
            }

            private set
            {
                Debug.Assert(value != null, "Tables should not be null");
                this.tables = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the currently selected table
        /// </summary>
        public String SelectedTable
        {
            get
            {
                return this.selectedTable;
            }

            set
            {
                if (!String.Equals(this.selectedTable, value))
                {
                    this.selectedTable = value;
                    this.loadSelectedTableRowsTask = this.LoadSelectedTableRowsAsync(newSelectedTable: true);
                }

                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets a command for loading the next rows from the table
        /// </summary>
        public RelayCommand LoadNextCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a command for cancelling the current query
        /// </summary>
        public RelayCommand CancelCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets an <see cref="ObservableCollection{T}"/> that contains the table's rows
        /// </summary>
        public ObservableCollection<object> TableRows
        {
            get
            {
                return this.tableRows;
            }

            set
            {
                Debug.Assert(value != null, "Table row collection should never be null");
                this.tableRows = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating if data is loaded
        /// </summary>
        public bool IsDataLoaded
        {
            get
            {
                return this.isDataLoaded;
            }

            set
            {
                this.isDataLoaded = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating is the overlay should be present
        /// </summary>
        public bool ShowOverlay
        {
            get
            {
                return this.showOverlay;
            }

            set
            {
                this.showOverlay = value;

                // Data loaded is the inverse of the overlay state
                this.IsDataLoaded = !this.showOverlay;

                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the overlay message
        /// </summary>
        public String OverlayMessage
        {
            get
            {
                return this.overlayMessage;
            }

            set
            {
                Debug.Assert(!string.IsNullOrEmpty(value), "Message should never be null");
                this.overlayMessage = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Visualize the property that represents a 'sqlite3 *'.
        /// </summary>
        /// <param name="result">Result of accessing property</param>
        /// <param name="sqliteInstanceName">Property name</param>
        public void VisualizeSqliteInstance(DkmSuccessEvaluationResult result)
        {
            Debug.Assert(result != null && !String.IsNullOrEmpty(result.FullName), "Arguments should not be null");

            this.sqliteInstanceName = result.FullName;
            this.ShowOverlay = true;
            this.LoadNextCommand.Enable = false;
            this.CancelCommand.Enable = false;

            bool dmpDebugging = result.InspectionContext.Thread.Process.LivePart == null;
            if (dmpDebugging)
            {
                this.OverlayMessage = Resources.ErrMsg_DmpDebuggingNotSupported;
            }
            else
            {
                this.OverlayMessage = Resources.Msg_LoadingTables;

                // Create a new inspection context for all subsequent func-evals in this visualizer
                this.inspectionContext = DkmInspectionContext.Create(
                    result.InspectionSession,
                    result.RuntimeInstance,
                    result.InspectionContext.Thread,
                    1000,
                    DkmEvaluationFlags.None,
                    DkmFuncEvalFlags.None,
                    10,
                    result.InspectionContext.Language,
                    null);

                // Get available tables
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    IList<string> tables;
                    try
                    {
                        tables = await LoadTablesAsync();
                    }
                    catch (Exception e)
                    {
                        this.OverlayMessage = String.Format(CultureInfo.CurrentCulture, Resources.ErrMsg_FailureGettingTables, e.Message);
                        return;
                    }

                // If tables do exist, prepare the table query
                if (tables.Count == 0)
                    {
                        this.OverlayMessage = Resources.ErrMsg_NoTablesFound;
                    }
                    else
                    {
                        this.AvailableTables = tables;
                        this.SelectedTable = tables[0];
                    }
                });
            }

            var window = new VisualizerWindow()
            {
                DataContext = this
            };

            window.ShowDialog();
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            // Clean up any outstanding query
            foreach (var details in this.tableDetails)
            {
                details.Value.Dispose();
            }

            this.isDisposed = true;
        }

        private void RaisePropertyChanged([CallerMemberName] String name = null)
        {
            var h = this.PropertyChanged;
            if (h != null)
            {
                h(this, new PropertyChangedEventArgs(name));
            }
        }

        private async Task<IList<string>> LoadTablesAsync()
        {
            using (var tableQuery = await SqliteFuncEvalQuery.CreateAsync(
                this.inspectionContext,
                this.sqliteInstanceName,
                "SELECT name FROM sqlite_master WHERE type='table'"))
            {
                var tables = new List<String>();
                foreach (var t in await tableQuery.ExecuteAsync(int.MaxValue, CancellationToken.None))
                {
                    Debug.Assert(t.Length != 0, "All rows should have at least 1 column");
                    tables.Add(t.First());
                }

                return tables;
            }
        }

        private JoinableTask LoadSelectedTableRowsAsync(bool newSelectedTable)
        {
            return ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                this.LoadNextCommand.Enable = false;

                this.ShowOverlay = true;
                this.OverlayMessage = Resources.Msg_GettingRows;

                try
                {
                    var details = await this.GetTableDetailsAsync(this.selectedTable);

                    // Cancellation is only available after table details have been aquired
                    this.cancelTokenSource = new CancellationTokenSource();
                    this.CancelCommand.Enable = true;
                    var rows = await this.GetTableRowsAsync(details, MaxRowsPerRequest, this.cancelTokenSource.Token);

                    int prevRowCount = details.VisibleRows.Count;

                    // Add rows to the visible row collection
                    foreach (var r in rows)
                    {
                        details.VisibleRows.Add(r);
                    }

                    // If this is a newly selected table, swap the collection property
                    if (newSelectedTable)
                    {
                        this.TableRows = details.VisibleRows;
                    }

                    // If the requested number of rows was returned, then there might be more.
                    // If the user made a cancel request, then enable loading.
                    this.LoadNextCommand.Enable = (details.VisibleRows.Count - prevRowCount) == MaxRowsPerRequest || !this.CancelCommand.Enable;
                    this.ShowOverlay = false;
                }
                catch (SqliteVisualizerException e)
                {
                    this.OverlayMessage = String.Format(CultureInfo.CurrentCulture, Resources.ErrMsg_VisualizerFailure, e.Message, e.Expression);
                }
                finally
                {
                    this.CancelCommand.Enable = false;
                    this.cancelTokenSource.Dispose();
                    this.cancelTokenSource = null;
                }
            });
        }

        private async Task<IEnumerable<object>> GetTableRowsAsync(TableDetails details, int maxRowCount, CancellationToken token)
        {
            var rows = await details.Query.ExecuteAsync(maxRowCount, token);

            var rowInstances = new List<object>();
            foreach (var r in rows)
            {
                var rowInst = details.CreateRowInstance(r);
                rowInstances.Add(rowInst);
            }

            return rowInstances;
        }

        private async Task<TableDetails> GetTableDetailsAsync(string table)
        {
            TableDetails details;
            if (!this.tableDetails.TryGetValue(table, out details))
            {
                // Escape string for sqlite - http://www.sqlite.org/lang_expr.html
                var escapedTableName = this.selectedTable.Replace("\"", "\\\"");
                escapedTableName = escapedTableName.Replace("'", "''");
                var query = await SqliteFuncEvalQuery.CreateAsync(
                    this.inspectionContext,
                    this.sqliteInstanceName,
                    $"SELECT * FROM '{escapedTableName}'");

                // Move off the main thread since we are about to do some code gen
                await TaskScheduler.Default;

                details = new TableDetails(this.selectedTable, query, this.modBuilder);
                this.tableDetails[table] = details;
            }

            return details;
        }

        private class TableDetails : IDisposable
        {
            private readonly ModuleBuilder modBuilder;
            private readonly String tableName;

            private bool isDisposed = false;
            private FieldInfo[] rowTypeFieldInfo;
            private Type dynamicRowType;

            /// <summary>
            /// Instaniate a new <see cref="TableDetails"/> instance.
            /// </summary>
            /// <param name="tableName">Name of the table</param>
            /// <param name="query">Associated <see cref="SqliteFuncEvalQuery"/> instance</param>
            /// <param name="modBuilder"><see cref="ModuleBuilder"/> instance for dynamic row type generation</param>
            public TableDetails(String tableName, SqliteFuncEvalQuery query, ModuleBuilder modBuilder)
            {
                Debug.Assert(query != null && !String.IsNullOrEmpty(tableName) && modBuilder != null, "Arugments should not be null");

                this.tableName = tableName;
                this.modBuilder = modBuilder;
                this.Query = query;
                this.VisibleRows = new ObservableCollection<object>();

                this.CreateRowType(this.Query.ColumnNames);
            }

            /// <summary>
            /// Gets the visible row collection
            /// </summary>
            public ObservableCollection<object> VisibleRows { get; private set; }

            /// <summary>
            /// Gets the query for the table
            /// </summary>
            public SqliteFuncEvalQuery Query { get; private set; }

            /// <summary>
            /// Create a new row instance
            /// </summary>
            /// <param name="members">Values on the new row</param>
            /// <returns>A new dynamic instance</returns>
            public object CreateRowInstance(String[] members)
            {
                Debug.Assert(members != null && members.Length == this.rowTypeFieldInfo.Length, "Invalid instance members");

                var row = Activator.CreateInstance(this.dynamicRowType);
                for (var i = 0; i < Math.Min(members.Length, this.rowTypeFieldInfo.Length); ++i)
                {
                    this.rowTypeFieldInfo[i].SetValue(row, members[i]);
                }

                return row;
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (this.isDisposed)
                {
                    return;
                }

                this.Query.Dispose();
                this.isDisposed = true;
            }

            private void CreateRowType(IEnumerable<String> fieldNames)
            {
                Debug.Assert(this.Query != null, "Query should not be null");
                TypeBuilder rowTypeBuilder = this.modBuilder.DefineType(this.tableName + "_Row_"  + this.Query.UniqueTableId, TypeAttributes.Public);

                // Add properties (getters) and backing fields to the new row type
                foreach (var f in fieldNames)
                {
                    // Backing field
                    FieldBuilder fb = rowTypeBuilder.DefineField("_" + f, typeof(String), FieldAttributes.Public);

                    // Property
                    PropertyBuilder pb = rowTypeBuilder.DefineProperty(f, PropertyAttributes.HasDefault, typeof(String), null);

                    // Property getter
                    MethodBuilder getter = rowTypeBuilder.DefineMethod("get_" + f, MethodAttributes.Public | MethodAttributes.HideBySig, typeof(String), Type.EmptyTypes);

                    // Create IL for getter
                    var getter_il = getter.GetILGenerator();
                    getter_il.Emit(OpCodes.Ldarg_0);    // Push 'this'
                    getter_il.Emit(OpCodes.Ldfld, fb);  // Load backing field
                    getter_il.Emit(OpCodes.Ret);

                    pb.SetGetMethod(getter);
                }

                // Create new type
                this.dynamicRowType = rowTypeBuilder.CreateType();

                // Get field info from new type
                var fieldInfos = new List<FieldInfo>();
                foreach (var f in fieldNames)
                {
                    // Backing fields have a '_' prefix
                    var fieldInfo = this.dynamicRowType.GetField("_" + f);
                    Debug.Assert(fieldInfo != null, "Field should not be null");
                    fieldInfos.Add(fieldInfo);
                }

                this.rowTypeFieldInfo = fieldInfos.ToArray();
            }
        }
    }
}
