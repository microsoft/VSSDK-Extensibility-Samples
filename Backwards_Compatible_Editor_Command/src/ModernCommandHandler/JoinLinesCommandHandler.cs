/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using JoinLineCommandImplementation;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace ModernCommandHandler
{
    [Export(typeof(ICommandHandler))]
    [ContentType("text")]
    [Name(nameof(JoinLinesCommandHandler))]
    public class JoinLinesCommandHandler : ICommandHandler<JoinLinesCommandArgs>
    {
        public string DisplayName => "Join Selected Lines";

        public CommandState GetCommandState(JoinLinesCommandArgs args)
        {
            return args.TextView.Selection.IsEmpty ? CommandState.Unavailable : CommandState.Available;
        }

        public bool ExecuteCommand(JoinLinesCommandArgs args, CommandExecutionContext context)
        {
            using (context.OperationContext.AddScope(allowCancellation: false, description: "Joining selected lines"))
            {
                args.TextView.TextBuffer.Insert(0, "// Invoked from modern command handler\r\n");
                JoinLine.JoinSelectedLines(args.TextView);
            }

            return true;
        }
    }
}
