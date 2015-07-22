/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EnvDTE;

using System.Runtime.InteropServices;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {
    [ComVisible(true)]
    [SuppressMessage("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable")]
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class CodeDomCodeElements : List<CodeElement>, CodeElements {
        DTE dte;
        object parent;

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        public CodeDomCodeElements(DTE dte, object parent) {
            this.dte = dte;
            this.parent = parent;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void AddElement(CodeElement element) {
            Add(element);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void InsertElement(int index, CodeElement element) {
            Insert(index, element);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public void RemoveElement(CodeElement element) {
            Remove(element);
        }

        #region CodeElements Members

        public new int Count {
            get { return base.Count; }
        }

        public bool CreateUniqueID(string Prefix, ref string NewName) {
            NewName = Guid.NewGuid().ToString();
            return true;
        }

        public DTE DTE {
            get { return dte; }
        }

        public dynamic Parent {
            get { return parent; }
        }

        public new System.Collections.IEnumerator GetEnumerator() {
            return base.GetEnumerator();
        }

        public CodeElement Item(object index) {            
            return this[PositionToIndex(index)];
        }

        public void Reserved1(object Element) {
            throw new NotImplementedException();
        }

        #endregion

        private int PositionToIndex(object element) {
            CodeElement cde = element as CodeElement;
            if (cde != null) {
                return this.IndexOf(cde);
            }

            int pos = (int)element;
            if (pos == -1) {                
                return this.Count;
            } 
            
            return pos - 1;
        }

    }

}
