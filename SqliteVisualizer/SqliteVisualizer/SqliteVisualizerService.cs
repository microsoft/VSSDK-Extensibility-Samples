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
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Debugger.Evaluation;
    using Microsoft.VisualStudio.Debugger.Interop;

    /// <summary>
    /// Sqlite visualizer service implementation
    /// </summary>
    internal class SqliteVisualizerService : SSqliteVisualizerService, IVsCppDebugUIVisualizer
    {
        private readonly ModuleBuilder modBuilder;

        /// <summary>
        /// Create a new <see cref="SqliteVisualizerService"> instance.
        /// </summary>
        public SqliteVisualizerService()
        {
            var thisAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var asmName = new AssemblyName(thisAssemblyName + ".Dynamic");

            AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndCollect);
            this.modBuilder = asmBuilder.DefineDynamicModule("TableRows");
        }

        /// <inheritdoc />
        int IVsCppDebugUIVisualizer.DisplayValue(uint ownerHwnd, uint visualizerId, IDebugProperty3 debugProperty)
        {
            try
            {
                var dkmEvalResult = DkmSuccessEvaluationResult.ExtractFromProperty(debugProperty);

                using (var viewModel = new VisualizerWindowViewModel(this.modBuilder))
                {
                    viewModel.VisualizeSqliteInstance(dkmEvalResult);
                }
            }
            catch (Exception e)
            {
                Debug.Fail("Visualization failed: " + e.Message);
                return e.HResult;
            }

            return VSConstants.S_OK;
        }
    }
}
