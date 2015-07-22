/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Runtime.InteropServices;

using IronPython.Hosting;

namespace Microsoft.Samples.VisualStudio.IronPython.Interfaces
{
    /// <summary>
    /// Interface implemented by an engine object.
    /// </summary>
    [Guid("89B8BBD7-DBC0-46cc-B43E-9E8D9CB724D3")]
    [ComVisible(true)]
    public interface IEngine
    {
        /// <summary>Gets the copyright information about the engine.</summary>
        string Copyright { get;}
        /// <summary>Evaluates an expression.</summary>
        object Evaluate(string expression);
        /// <summary>Executes a command.</summary>
        void Execute(string text);
        /// <summary>Executes the content of a file.</summary>
        void ExecuteFile(string fileName);
        /// <summary>Executes the command in console mode.</summary>
        void ExecuteToConsole(string text);
        /// <summary>Gets the value of a variable.</summary>
        object GetVariable(string name);
        /// <summary>Parse the text and finds if it can be executed.</summary>
        bool ParseInteractiveInput(string text, bool allowIncompleteStatement);
        /// <summary>Executes the commands in the console.</summary>
        int RunInteractive();
        /// <summary>Sets the value of a variable.</summary>
        void SetVariable(string name, object value);
        /// <summary>Sets the standard error for the engine.</summary>
        System.IO.Stream StdErr { get; set;}
        /// <summary>Sets the standard input for the engine.</summary>
        System.IO.Stream StdIn { get; set; }
        /// <summary>Sets the standard output for the engine.</summary>
        System.IO.Stream StdOut { get; set;}
        /// <summary>Gets the version of the engine.</summary>
        Version Version { get; }
    }

    /// <summary>
    /// This is the definition of the interface exposed by the service defined by the
    /// IronPython console package.
    /// </summary>
    [Guid("1106288e-9740-40ee-bab5-1e4e1c5f7252")]
    [ComVisible(true)]
    public interface IPythonEngineProvider
    {
        /// <summary>Gets the instance of the engine shared between different components.</summary>
        IEngine GetSharedEngine();
        /// <summary>Creates a new instance of the engine.</summary>
        IEngine CreateNewEngine();
    }

}