/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using IronPython.CodeDom;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {
    /// <summary>
    /// Merger class for performing merges into VS or on-disk
    /// </summary>
    class FileCodeMerger : IMergeDestination {
        private TextDocument doc;
        private ProjectItem parentItem;
        private bool hasMerged = false;

        public FileCodeMerger(ProjectItem parent) {
            parentItem = parent;
        }

        private TextDocument Document {
            get {
                if (null == doc) {
                    if (null == parentItem.Document) {
                        parentItem.Open(Guid.Empty.ToString("B"));
                    }
                    doc = (TextDocument)parentItem.Document.Object("TextDocument");
                }
                return doc;
            }
        }

        #region IMergeDestination Members

        public void InsertRange(int start, IList<string> lines) {
            EditPoint ep = Document.CreateEditPoint(new CodeDomTextPoint(Document, 1, start + 1));
            for (int i = 0; i < lines.Count; i++) {
                ep.Insert(lines[i]+"\r\n");
            }

            hasMerged = true;
        }

        public void RemoveRange(int start, int count) {
            EditPoint ep = Document.CreateEditPoint(new CodeDomTextPoint(Document, 1, start + 1));            
            for (int i = 0; i < count; i++) {
                ep.Delete(ep.LineLength+1);
            }

            hasMerged = true;
        }

        public int LineCount {
            get {
                return Document.EndPoint.Line - Document.StartPoint.Line;
            }
        }

        public bool HasMerged {
            get { return hasMerged; }
        }

        public string FinalText {
            get { 
                // return back modified text
                hasMerged = false;
                return null;
            }
        }

        #endregion
    }
}
