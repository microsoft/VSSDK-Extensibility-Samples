/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System.Collections.Generic;

namespace Microsoft.Samples.VisualStudio.CodeSweep.Scanner
{
    /// <summary>
    /// A term to search for.
    /// </summary>
    public interface ISearchTerm
    {
        /// <summary>
        /// Gets the case-insensitive text of the term.
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Gets the term class, such as "Geopolitical".
        /// </summary>
        string Class { get; }

        /// <summary>
        /// Gets the term severity, normally ranging from 1 (most severe) to 3 (least severe).
        /// </summary>
        int Severity { get; }

        /// <summary>
        /// Gets the comment explaining why the term is undesirable and what to do about it.
        /// </summary>
        string Comment { get; }

        /// <summary>
        /// Gets the recommended replacement term; this may be null if there is no recommended
        /// replacement.
        /// </summary>
        string RecommendedTerm { get; }

        /// <summary>
        /// Gets the exclusions that apply to this term.
        /// </summary>
        IEnumerable<IExclusion> Exclusions { get; }

        /// <summary>
        /// Gets the term table to which this term belongs.
        /// </summary>
        ITermTable Table { get; }
    }
}
