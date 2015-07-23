/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Samples.VisualStudio.CodeSweep.Scanner
{
    [Serializable]
    class SearchTerm : ISearchTerm, ISerializable
    {
        const string TextKey = "Text";
        const string TableKey = "Table";
        const string ClassKey = "Class";
        const string SeverityKey = "Severity";
        const string CommentKey = "Comment";
        const string RecommendedKey = "Recommended";
        const string ExclusionsKey = "Exclusions";

        readonly string _text = string.Empty;
        readonly ITermTable _table;
        readonly string _class;
        readonly int _severity;
        readonly string _comment;
        readonly string _recommended;
        readonly List<IExclusion> _exclusions;

        /// <summary>
        /// Initializes the search term with the specified text.
        /// </summary>
        /// <param name="table">The table to which this term belongs.</param>
        /// <param name="text">The text to search for.</param>
        /// <param name="severity">The severity of the term, normally between 1 and 3 inclusive.</param>
        /// <param name="termClass">The class of the term, such as "Geopolitical".</param>
        /// <param name="comment">A descriptive comment for the term.</param>
        /// <param name="recommendedTerm">The recommended replacement; may be null.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>text</c> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <c>text</c> is an empty string.</exception>
        public SearchTerm(ITermTable table, string text, int severity, string termClass, string comment, string recommendedTerm)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            if (text.Length == 0)
            {
                throw new ArgumentException("Empty string not allowed", "text");
            }

            _table = table;
            _text = text;
            _severity = severity;
            _class = termClass;
            _comment = comment;
            _recommended = recommendedTerm;
            _exclusions = new List<IExclusion>();
        }

        protected SearchTerm(SerializationInfo info, StreamingContext context)
        {
            _text = info.GetString(TextKey);
            _table = (ITermTable)info.GetValue(TableKey, typeof(ITermTable));
            _class = info.GetString(ClassKey);
            _severity = info.GetInt32(SeverityKey);
            _comment = info.GetString(CommentKey);
            _recommended = info.GetString(RecommendedKey);
            _exclusions = (List<IExclusion>)info.GetValue(ExclusionsKey, typeof(List<IExclusion>));
        }

        public void AddExclusion(string text)
        {
            _exclusions.Add(new Exclusion(text, this));
        }

        #region ISearchTerm Members

        /// <summary>
        /// Gets the text to search for.
        /// </summary>
        public string Text
        {
            get { return _text; }
        }

        public int Severity
        {
            get { return _severity; }
        }

        public string Class
        {
            get { return _class; }
        }

        public string Comment
        {
            get { return _comment; }
        }

        public string RecommendedTerm
        {
            get { return _recommended; }
        }

        /// <summary>
        /// Gets the list of phrases containing this term which should be excluded from the results.
        /// </summary>
        public IEnumerable<IExclusion> Exclusions
        {
            get { return _exclusions; }
        }

        public ITermTable Table
        {
            get { return _table; }
        }

        #endregion ISearchTerm Members

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(TextKey, _text);
            info.AddValue(TableKey, _table);
            info.AddValue(ClassKey, _class);
            info.AddValue(SeverityKey, _severity);
            info.AddValue(CommentKey, _comment);
            info.AddValue(RecommendedKey, _recommended);
            info.AddValue(ExclusionsKey, _exclusions);
        }

        #endregion ISerializable Members
    }
}
