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

    /// <summary>
    /// Sqlite Visualizer exception
    /// </summary>
    internal class SqliteVisualizerException : Exception
    {
        /// <summary>
        /// Create a new <see cref="SqliteVisualizerException"/> instance.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="expression"></param>
        public SqliteVisualizerException(String message, String expression)
            : base(message)
        {
            Debug.Assert(!String.IsNullOrEmpty(message) && !String.IsNullOrEmpty(expression), "Arguments should not be null");
            this.Expression = expression;
        }

        /// <summary>
        /// Gets or sets the expression being evaluated when the failure occurred.
        /// </summary>
        public String Expression
        {
            get;
            private set;
        }
    }
}
