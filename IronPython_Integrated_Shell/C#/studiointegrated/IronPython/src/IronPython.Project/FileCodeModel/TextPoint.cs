/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using EnvDTE;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {
    public class CodeDomTextPoint : TextPoint {
        int x, y;
        TextDocument parent;

        public CodeDomTextPoint(TextDocument parent, int column, int row) {
            x = column;
            y = row;
            this.parent = parent;
        }

        #region TextPoint Members

        public int AbsoluteCharOffset {
            get { throw new NotImplementedException(); }
        }

        public bool AtEndOfDocument {
            get { throw new NotImplementedException(); }
        }

        public bool AtEndOfLine {
            get { throw new NotImplementedException(); }
        }

        public bool AtStartOfDocument {
            get { return x == 1 && y == 1; }
        }

        public bool AtStartOfLine {
            get { return x == 1; }
        }

        public EditPoint CreateEditPoint() {
            return parent.CreateEditPoint(this);
            //return new CodeDomEditPoint(parent, this);
        }

        public DTE DTE {
            get { return parent.DTE; }
        }

        public int DisplayColumn {
            get { return x; }
        }

        public bool EqualTo(TextPoint Point) {
            CodeDomTextPoint tp = Point as CodeDomTextPoint;
            if (tp == null) return false;

            return tp.x == x && tp.y == y;
        }

        public bool GreaterThan(TextPoint Point) {
            CodeDomTextPoint tp = Point as CodeDomTextPoint;
            if (tp == null) return false;

            return tp.y < y || (tp.y == y && tp.x < x);
        }

        public bool LessThan(TextPoint Point) {
            CodeDomTextPoint tp = Point as CodeDomTextPoint;
            if (tp == null) return false;

            return tp.y > y || (tp.y == y && tp.x > x);
        }

        public int Line {
            get { return y; }
        }

        public int LineCharOffset {
            get { return x; }
        }

        public int LineLength {
            get { throw new NotImplementedException(); }
        }

        public TextDocument Parent {
            get { return parent; }
        }

        public bool TryToShow(vsPaneShowHow How, object PointOrCount) {
            throw new NotImplementedException();
        }

        public CodeElement get_CodeElement(vsCMElement Scope) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
