/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.Samples.VisualStudio.IronPython.Interfaces;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Samples.VisualStudio.IronPython.Console
{
    /// <summary>
    /// This class implements the list of the declarations returned by the scope.
    /// </summary>
    internal class MethodDeclarations : Declarations
    {
        private List<string> methods;

        public MethodDeclarations()
        {
            methods = new List<string>();
        }

        public void AddMethod(string method)
        {
            methods.Add(method);
        }

        public override int GetCount()
        {
            return methods.Count;
        }

        public override string GetDescription(int index)
        {
            if ((index < 0) || (index >= methods.Count))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return "";
        }

        public override string GetDisplayText(int index)
        {
            return methods[index];
        }

        public override int GetGlyph(int index)
        {
            if ((index < 0) || (index >= methods.Count))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return 0;
        }

        public override string GetName(int index)
        {
            return methods[index];
        }
    }

    /// <summary>
    /// This class implements the AuthoringScope used inside the console. This scope uses the
    /// Python engine to get the informations about the objects as opposed to the standard scope
    /// that builds the informations from the content of a file.
    /// </summary>
    public class ConsoleAuthoringScope : AuthoringScope
    {
        /// <summary>
        /// Factory function for the scope.
        /// </summary>
        public static ConsoleAuthoringScope CreateScope(ParseRequest request)
        {
            if (null == request)
            {
                return null;
            }
            ConsoleAuthoringScope scope = null;
            if (request.Reason == ParseReason.MemberSelect ||
                request.Reason == ParseReason.DisplayMemberList || 
                request.Reason == ParseReason.CompleteWord)
            {
                scope = new ConsoleAuthoringScope();
            }
            return scope;
        }

        private static IConsoleText console;
        /// <summary>
        /// Get or Set the IronPython console window used by all the instances of the scope.
        /// </summary>
        public static IConsoleText PythonConsole
        {
            get { return console; }
            set { console = value; }
        }
        private static IEngine engine;
        /// <summary>
        /// Gets the instance of the engine used by all the instances of the scope.
        /// </summary>
        private static IEngine Engine
        {
            get
            {
                if (null == engine)
                {
                    IPythonEngineProvider provider = (IPythonEngineProvider)site.GetService(typeof(IPythonEngineProvider));
                    engine = provider.GetSharedEngine();
                }
                return engine;
            }
        }

        private static LanguageService language;
        /// <summary>
        /// Gets or Sets the instance of the language service used by all the instances of the scope.
        /// </summary>
        public static LanguageService Language
        {
            get { return language; }
            set { language = value; }
        }

        private static IServiceProvider site;
        /// <summary>
        /// Gets or Sets the Site for all the instances of the scope.
        /// </summary>
        public static IServiceProvider Site
        {
            get { return site; }
            set { site = value; }
        }

        /// <summary>
        /// Private constructor for the scope.
        /// This constructor is private so that the compiler will not create a public one, so the
        /// only way to create an instance of this class is to use the factory function.
        /// </summary>
        private ConsoleAuthoringScope()
        {
            // Do Nothing
        }

        public override string GetDataTipText(int line, int col, out TextSpan span)
        {
            // Not implemented.
            span = new TextSpan();
            return null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
        {
            // Check that the text view is not null.
            if (null == view)
            {
                throw new ArgumentNullException("view");
            }

            // In order to get the correct text for this line we have to figure out if this line
            // contains part of the read-only region of the buffer because this region is not
            // supposed to be used (it is the output of the engine).
            // Use the function exposed by the console window to get the text of the line
            // without the part inside the read-only region.
            string lineText = PythonConsole.TextOfLine(line, col, true);
            if (null == lineText)
            {
                // there is no text to parse, so there is no delaration to return.
                // Return an empty Declarations object.
                return new MethodDeclarations();
            }

            // Get the text buffer.
            IVsTextLines buffer;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(
                view.GetBuffer(out buffer));

            // Get the scanner from the language service.
            IScanner scanner = Language.GetScanner(buffer);
            scanner.SetSource(lineText, 0);

            // Now use the scanner to parse this line and build the list of the tokens.
            List<TokenInfo> tokens = new List<TokenInfo>();
            TokenInfo lastToken = null;
            TokenInfo currentToken = new TokenInfo();
            int state = 0;
            while (scanner.ScanTokenAndProvideInfoAboutIt(currentToken, ref state))
            {
                if ((null != lastToken) && (currentToken.StartIndex > lastToken.EndIndex + 1))
                {
                    tokens.Clear();
                }
                tokens.Add(currentToken);
                lastToken = currentToken;
                currentToken = new TokenInfo();
            }

            // Now that we have the tokens we can use them to find the text to pass to the
            // IronPython engine to evaluate the expression.
            if (0 == tokens.Count)
            {
                // If the list of tokens is empty, then return an emty set of declarations.
                return new MethodDeclarations();
            }

            // Check if the last token is the one that generated the parse request.
            if (tokens[tokens.Count - 1].Trigger == TokenTriggers.None)
            {
                tokens.RemoveAt(tokens.Count - 1);
                if (0 == tokens.Count)
                {
                    return new MethodDeclarations();
                }
            }

            // Remove the token that generated the request
            if (tokens[tokens.Count - 1].Trigger != TokenTriggers.None)
            {
                tokens.RemoveAt(tokens.Count - 1);
                if (0 == tokens.Count)
                {
                    return new MethodDeclarations();
                }
            }

            // Now build the string to pass to the engine.
            int startIndex = tokens[0].StartIndex;
            int len = tokens[tokens.Count - 1].EndIndex - startIndex + 1;
            string engineCommand = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "dir({0})",  
                lineText.Substring(startIndex, len));

            MethodDeclarations declarations = new MethodDeclarations();
            try
            {
                IEnumerable members = Engine.Evaluate(engineCommand) as IEnumerable;
                if (null != members)
                {
                    foreach (string member in members)
                    {
                        declarations.AddMethod(member);
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing
            }
            return declarations;
        }

        public override Methods GetMethods(int line, int col, string name)
        {
            // Not implemented.
            return null;
        }

        public override string Goto(Microsoft.VisualStudio.VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
        {
            // Not implemented.
            span = new TextSpan();
            return null;
        }
    }
}
