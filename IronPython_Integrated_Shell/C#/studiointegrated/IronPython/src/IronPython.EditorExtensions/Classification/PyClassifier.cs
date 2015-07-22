/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text;
using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.VisualStudio.IronPythonInference;

namespace IronPython.EditorExtensions
{
    /// <summary>
    /// Implements <see cref="IClassifier"/> in order to provide coloring
    /// </summary>
    internal class PyClassifier : IClassifier
    {
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
        private IClassificationTypeRegistryService classificationRegistryService;
        private ITextBuffer textBuffer;

        internal PyClassifier(ITextBuffer textBuffer, IClassificationTypeRegistryService classificationRegistryService)
        {
            this.textBuffer = textBuffer;
            this.classificationRegistryService = classificationRegistryService;
           
            this.textBuffer.ReadOnlyRegionsChanged += new EventHandler<SnapshotSpanEventArgs>(textBuffer_ReadOnlyRegionsChanged);
        }

        void textBuffer_ReadOnlyRegionsChanged(object sender, SnapshotSpanEventArgs e)
        {
            // We need to call this event when read-only regions are added, so they will be grayed out.
            OnClassificationChanged(new SnapshotSpan(textBuffer.CurrentSnapshot, e.Span));
        }

        private void OnClassificationChanged(SnapshotSpan changeSpan)
        {
            if (ClassificationChanged != null)
            {
                ClassificationChanged(this, new ClassificationChangedEventArgs(changeSpan));
            }
        }

        IList<ClassificationSpan> IClassifier.GetClassificationSpans(SnapshotSpan span)
        {
            var classifications = new List<ClassificationSpan>();
            
			using (var systemState = new SystemState())
            {
				int startIndex, endIndex;

                // Execute the IPy tokenizer
				var tokenizer = new Tokenizer(span.GetText().ToCharArray(), true, systemState, new CompilerContext(string.Empty, new QuietCompilerSink()));
                var token = tokenizer.Next();

                // Iterate the tokens
				while (token.Kind != TokenKind.EndOfFile)
                {
                    // Determine the bounds of the classfication span
                    startIndex = span.Snapshot.GetLineFromLineNumber(tokenizer.StartLocation.Line - 1 + span.Start.GetContainingLine().LineNumber).Start.Position + tokenizer.StartLocation.Column;
                    endIndex = span.Snapshot.GetLineFromLineNumber(tokenizer.EndLocation.Line - 1 + span.Start.GetContainingLine().LineNumber).Start.Position + tokenizer.EndLocation.Column;
                    
					if (endIndex > span.Snapshot.GetText().Length) 
						endIndex = span.Snapshot.GetText().Length;
                    
					if (endIndex > startIndex && !span.Snapshot.TextBuffer.IsReadOnly(new Span(startIndex, endIndex - startIndex)))
                    {
                        // Add the classfication span
                        classifications.Add(new ClassificationSpan(new SnapshotSpan(span.Snapshot, startIndex, endIndex - startIndex), GetClassificationType(token)));
                    }
                
                    // Get next token
					token = tokenizer.Next();
                }
            }

            foreach (var region in span.Snapshot.TextBuffer.GetReadOnlyExtents(span))
            {
                // Add classfication for read only regions
                classifications.Add(new ClassificationSpan(new SnapshotSpan(span.Snapshot, region), classificationRegistryService.GetClassificationType("PythonReadOnlyRegion")));
            }

            return classifications;
        }

        private IClassificationType GetClassificationType(Token token)
        {
            // Translate the token kind into a classfication type
            switch (token.Kind)
            {
                case TokenKind.Comment:
                    return classificationRegistryService.GetClassificationType(PyClassificationTypes.Comment);

                case TokenKind.Dot:
                case TokenKind.LeftParenthesis:
                case TokenKind.RightParenthesis:
                case TokenKind.LeftBracket:
                case TokenKind.RightBracket:
                case TokenKind.LeftBrace:
                case TokenKind.RightBrace:
                case TokenKind.Comma:
                case TokenKind.Colon:
                case TokenKind.BackQuote:
                case TokenKind.Semicolon:
                case TokenKind.Assign:
                case TokenKind.Twiddle:
                case TokenKind.LessThanGreaterThan:
					return classificationRegistryService.GetClassificationType(PyClassificationTypes.Delimiter);

                case TokenKind.Add:
                case TokenKind.AddEqual:
                case TokenKind.Subtract:
                case TokenKind.SubtractEqual:
                case TokenKind.Power:
                case TokenKind.PowerEqual:
                case TokenKind.Multiply:
                case TokenKind.MultiplyEqual:
                case TokenKind.FloorDivide:
                case TokenKind.FloorDivideEqual:
                case TokenKind.Divide:
                case TokenKind.DivEqual:
                case TokenKind.Mod:
                case TokenKind.ModEqual:
                case TokenKind.LeftShift:
                case TokenKind.LeftShiftEqual:
                case TokenKind.RightShift:
                case TokenKind.RightShiftEqual:
                case TokenKind.BitwiseAnd:
                case TokenKind.BitwiseAndEqual:
                case TokenKind.BitwiseOr:
                case TokenKind.BitwiseOrEqual:
                case TokenKind.Xor:
                case TokenKind.XorEqual:
                case TokenKind.LessThan:
                case TokenKind.GreaterThan:
                case TokenKind.LessThanOrEqual:
                case TokenKind.GreaterThanOrEqual:
                case TokenKind.Equal:
                case TokenKind.NotEqual:
					return classificationRegistryService.GetClassificationType(PyClassificationTypes.Operator);

                case TokenKind.KeywordAnd:
                case TokenKind.KeywordAssert:
                case TokenKind.KeywordBreak:
                case TokenKind.KeywordClass:
                case TokenKind.KeywordContinue:
                case TokenKind.KeywordDef:
                case TokenKind.KeywordDel:
                case TokenKind.KeywordElseIf:
                case TokenKind.KeywordElse:
                case TokenKind.KeywordExcept:
                case TokenKind.KeywordExec:
                case TokenKind.KeywordFinally:
                case TokenKind.KeywordFor:
                case TokenKind.KeywordFrom:
                case TokenKind.KeywordGlobal:
                case TokenKind.KeywordIf:
                case TokenKind.KeywordImport:
                case TokenKind.KeywordIn:
                case TokenKind.KeywordIs:
                case TokenKind.KeywordLambda:
                case TokenKind.KeywordNot:
                case TokenKind.KeywordOr:
                case TokenKind.KeywordPass:
                case TokenKind.KeywordPrint:
                case TokenKind.KeywordRaise:
                case TokenKind.KeywordReturn:
                case TokenKind.KeywordTry:
                case TokenKind.KeywordWhile:
                case TokenKind.KeywordYield:
					return classificationRegistryService.GetClassificationType(PyClassificationTypes.Keyword);

                case TokenKind.Name:
					return classificationRegistryService.GetClassificationType(PyClassificationTypes.Identifier);

                case TokenKind.Constant:
                    ConstantValueToken ctoken = (ConstantValueToken)token;
                    if (ctoken.Constant is string)
                    {
						return classificationRegistryService.GetClassificationType(PyClassificationTypes.String);
                    }
                    else
                    {
						return classificationRegistryService.GetClassificationType(PyClassificationTypes.Number);
                    }

                default:
					return classificationRegistryService.GetClassificationType(PyClassificationTypes.Unknown);
            }
        }
    }
}