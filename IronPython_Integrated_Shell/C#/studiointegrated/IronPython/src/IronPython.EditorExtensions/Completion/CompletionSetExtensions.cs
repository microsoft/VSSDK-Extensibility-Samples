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
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace IronPython.EditorExtensions
{
    /// <summary>
    /// Provides extension methods for <see cref="CompletionSet"/>
    /// </summary>
    internal static class CompletionSetExtensions
    {
        private static CompletionMatchResult MatchCompletionList(this CompletionSet set, IList<Completion> completionList, CompletionMatchType matchType, bool caseSensitive)
        {
            if (set.ApplicableTo == null)
            {
                throw new InvalidOperationException("Cannot match completion set with no applicability span.");
            }
            
			ITextSnapshot currentSnapshot = set.ApplicableTo.TextBuffer.CurrentSnapshot;
            string text = set.ApplicableTo.GetText(currentSnapshot);
            if (text.Length != 0)
            {
                Completion bestMatch = null;
                int maxMatchPosition = -1;
                bool isUnique = false;
                bool isSelected = false;
                foreach (Completion currentCompletion in completionList)
                {
                    string displayText = string.Empty;
                    if (matchType == CompletionMatchType.MatchDisplayText)
                    {
                        displayText = currentCompletion.DisplayText;
                    }
                    else if (matchType == CompletionMatchType.MatchInsertionText)
                    {
                        displayText = currentCompletion.InsertionText;
                    }
                    int matchPositionCount = 0;
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (i >= displayText.Length)
                        {
                            break;
                        }
                        char textChar = text[i];
                        char displayTextChar = displayText[i];
                        if (!caseSensitive)
                        {
                            textChar = char.ToLowerInvariant(textChar);
                            displayTextChar = char.ToLowerInvariant(displayTextChar);
                        }
                        if (textChar != displayTextChar)
                        {
                            break;
                        }
                        matchPositionCount++;
                    }
                    if (matchPositionCount > maxMatchPosition)
                    {
                        maxMatchPosition = matchPositionCount;
                        bestMatch = currentCompletion;
                        isUnique = true;
                        if ((matchPositionCount == text.Length) && (maxMatchPosition > 0))
                        {
                            isSelected = true;
                        }
                    }
                    else if (matchPositionCount == maxMatchPosition)
                    {
                        isUnique = false;
                        if (isSelected)
                        {
                            break;
                        }
                    }
                }
                if (bestMatch != null)
                {
                    CompletionMatchResult result = new CompletionMatchResult();
                    result.SelectionStatus = new CompletionSelectionStatus(bestMatch, isSelected, isUnique);
                    result.CharsMatchedCount = (maxMatchPosition >= 0) ? maxMatchPosition : 0;
                    return result;
                }
            }
            return null;
        }

        internal static CompletionSelectionStatus SelectBestMatch(this CompletionSet set, CompletionMatchType matchType, bool caseSensitive)
        {
            CompletionMatchResult matchedCompletions = set.MatchCompletionList(set.Completions, matchType, caseSensitive);
            CompletionMatchResult matchedCompletionBuilders = set.MatchCompletionList(set.CompletionBuilders, matchType, caseSensitive);
            int completionBuilderCount = 0;
            if (matchedCompletionBuilders != null)
            {
                completionBuilderCount = (matchedCompletionBuilders.CharsMatchedCount + (matchedCompletionBuilders.SelectionStatus.IsSelected ? 1 : 0)) + (matchedCompletionBuilders.SelectionStatus.IsUnique ? 1 : 0);
            }
            int completionCount = 0;
            if (matchedCompletions != null)
            {
                completionCount = (matchedCompletions.CharsMatchedCount + (matchedCompletions.SelectionStatus.IsSelected ? 1 : 0)) + (matchedCompletions.SelectionStatus.IsUnique ? 1 : 0);
            }
            if ((completionBuilderCount > completionCount) && (matchedCompletionBuilders != null))
            {
                set.SelectionStatus = matchedCompletionBuilders.SelectionStatus;
            }
            else if (matchedCompletions != null)
            {
                set.SelectionStatus = matchedCompletions.SelectionStatus;
            }
            else if (set.Completions.Count > 0)
            {
                if (!set.Completions.Contains(set.SelectionStatus.Completion))
                {
                    set.SelectionStatus = new CompletionSelectionStatus(set.Completions[0], false, false);
                }
            }
            else if (set.CompletionBuilders.Count > 0)
            {
                if (!set.CompletionBuilders.Contains(set.SelectionStatus.Completion))
                {
                    set.SelectionStatus = new CompletionSelectionStatus(set.CompletionBuilders[0], false, false);
                }
            }
            else
            {
                set.SelectionStatus = new CompletionSelectionStatus(null, false, false);
            }

            return set.SelectionStatus;
        }

		class CompletionMatchResult
		{
			public int CharsMatchedCount { get; set; }
			public CompletionSelectionStatus SelectionStatus { get; set; }
		}
    }
}
