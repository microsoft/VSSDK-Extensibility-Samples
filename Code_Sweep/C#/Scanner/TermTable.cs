/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;

namespace Microsoft.Samples.VisualStudio.CodeSweep.Scanner
{
    [Serializable]
    class TermTable : ITermTable, ISerializable
    {
        const string FilePathKey = "FilePath";
        const string TermsKey = "Terms";

        readonly string _filePath;
        readonly List<ISearchTerm> _terms;

        public TermTable(string filePath)
        {
            _filePath = filePath;
            _terms = new List<ISearchTerm>();

            XmlDocument document = new XmlDocument();
            document.Load(filePath);

            foreach (XmlNode node in document.SelectNodes("xmldata/PLCKTT/Lang/Term"))
            {
                string text = node.Attributes["Term"].InnerText;
                int severity = int.Parse(node.Attributes["Severity"].InnerText, CultureInfo.InvariantCulture);
                string termClass = node.Attributes["TermClass"].InnerText;
                string comment = node.SelectSingleNode("Comment").InnerText;

                string recommended = null;
                XmlNode recommendedNode = node.SelectSingleNode("RecommendedTerm");
                if (recommendedNode != null)
                {
                    recommended = recommendedNode.InnerText;
                }

                SearchTerm term = new SearchTerm(this, text, severity, termClass, comment, recommended);

                foreach (XmlNode exclusionNode in node.SelectNodes("Exclusion"))
                {
                    term.AddExclusion(exclusionNode.InnerText);
                }
                foreach (XmlNode exclusionNode in node.SelectNodes("ExclusionContext"))
                {
                    term.AddExclusion(exclusionNode.InnerText);
                }

                _terms.Add(term);
            }

            if (_terms.Count == 0)
            {
                throw new ArgumentException("The file did not specify a valid term table.", "filePath");
            }
        }

        protected TermTable(SerializationInfo info, StreamingContext context)
        {
            _filePath = info.GetString(FilePathKey);
            _terms = (List<ISearchTerm>)info.GetValue(TermsKey, typeof(List<ISearchTerm>));
        }

        #region ITermTable Members

        public string SourceFile
        {
            get { return _filePath; }
        }

        public IEnumerable<ISearchTerm> Terms
        {
            get { return _terms; }
        }

        #endregion ITermTable Members

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(FilePathKey, _filePath);
            info.AddValue(TermsKey, _terms);
        }

        #endregion ISerializable Members
    }
}
