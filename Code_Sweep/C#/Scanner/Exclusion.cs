/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.VisualStudio.CodeSweep.Scanner
{
    [Serializable]
    class Exclusion : IExclusion, ISerializable
    {
        const string TextKey = "Text";
        const string TermKey = "Term";

        readonly string _text;
        readonly ISearchTerm _term;

        public Exclusion(string text, ISearchTerm term)
        {
            _text = text;
            _term = term;
        }

        protected Exclusion(SerializationInfo info, StreamingContext context)
        {
            _text = info.GetString(TextKey);
            _term = (ISearchTerm)info.GetValue(TermKey, typeof(ISearchTerm));
        }

        #region IExclusion Members

        public string Text
        {
            get { return _text; }
        }

        public ISearchTerm Term
        {
            get { return _term; }
        }

        #endregion IExclusion Members

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(TextKey, _text);
            info.AddValue(TermKey, _term);
        }

        #endregion ISerializable Members
    }
}
