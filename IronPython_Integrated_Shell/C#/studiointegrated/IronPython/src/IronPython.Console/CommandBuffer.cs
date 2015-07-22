/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

*****************************************************************************/

using System;
using System.Text;

using Microsoft.Samples.VisualStudio.IronPython.Interfaces;

namespace Microsoft.Samples.VisualStudio.IronPython.Console
{
    /// <summary>
    /// This class is the one responsible to build the command text that the engine will execute.
    /// The client of the class (the console window) can add a line of text using the Add method,
    /// then this class will check with the engine to see if the text built so far can be executed;
    /// if the text can be executed, then it will call the engine to run the command and will
    /// empty the buffer, otherwise it will add the text to the buffer and wait for the next
    /// Add to try to execute it.
    /// </summary>
    public class CommandBuffer
    {
        private static readonly string singleLinePrompt = Resources.DefaultConsolePrompt;
        private static readonly string multiLinePrompt = Resources.MultiLineConsolePrompt;

        private IEngine engine;
        private StringBuilder textSoFar;

        public CommandBuffer(IEngine engine)
        {
            // Validate the engine.
            if (null == engine)
            {
                throw new ArgumentNullException("engine");
            }
            // Initialize the internal variables.
            this.engine = engine;
            this.textSoFar = new StringBuilder();

            // Write the command prompt.
            Write(singleLinePrompt);
        }

        public void Add(string text)
        {
            // This function is called to add a line, so write a new line delimeter to the output.
            Write(System.Environment.NewLine);

            // Add the text to the current buffer.
            if (textSoFar.Length > 0)
            {
                // We assume that Add is called to add a line, so if there is
                // previous text we have to create a new line.
                textSoFar.AppendLine();
            }
            textSoFar.Append(text);

            // Check with the engine if we can execute the text.
            bool allowIncomplete = !(string.IsNullOrEmpty(text) || (text.Trim().Length == 0));
            bool canExecute = engine.ParseInteractiveInput(textSoFar.ToString(), allowIncomplete);
            if (canExecute)
            {
                // If the text can be execute, then execute it and reset the text.
                try
                {
                    engine.ExecuteToConsole(textSoFar.ToString());
                }
                finally
                {
                    textSoFar.Length = 0;
                    Write(singleLinePrompt);
                }
            }
            else
            {
                // If the command is not executed, then it is a multi-line command, so
                // we have to write the correct prompt to the output.
                Write(multiLinePrompt);
            }
        }

        public string Text
        {
            get { return textSoFar.ToString(); }
        }

        private void Write(string text)
        {
            if ((null != engine) && (null != engine.StdOut))
            {
                // Note that we don't dispose the writer because otherwise it will
                // dispose also the stream.
                System.IO.StreamWriter writer = new System.IO.StreamWriter(engine.StdOut);
                writer.Write(text);
                writer.Flush();
            }
        }
    }
}