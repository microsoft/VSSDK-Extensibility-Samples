/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using Microsoft.Samples.VisualStudio.CodeSweep.BuildTask.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Samples.VisualStudio.CodeSweep.BuildTask
{
    class IgnoreInstance : IIgnoreInstance
    {
        string _file;
        string _lineText;
        string _term;
        int _column;

        /// <summary>
        /// Creates an IgnoreInstance object.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>file</c>, <c>lineText</c>, or <c>term</c> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <c>file</c>, <c>lineText</c>, or <c>term</c> is empty; or if <c>column</c> is less than zero or greater than or equal to <c>lineText.Length</c>.</exception>
        public IgnoreInstance(string file, string lineText, string term, int column)
        {
            Init(file, lineText, term, column);
        }

        /// <summary>
        /// Creates an IgnoreInstance object from a serialized representation.
        /// </summary>
        /// <param name="serialization">The file, term, column, and line text, separated by commas.</param>
        /// <param name="projectFolderForDerelativization">The project folder used to convert the serialized relative file path to a rooted file path.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>serialization</c> or <c>projectFolderForDerelativization</c> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <c>serialization</c> does not contain four comma-delimited fields; if any of the string fields is empty; if the column field cannot be parsed; or if the column field is less than zero or greater than or equal to the line text length.</exception>
        public IgnoreInstance(string serialization, string projectFolderForDerelativization)
        {
            if (serialization == null)
            {
                throw new ArgumentNullException("serialization");
            }

            IList<string> fields = Utilities.ParseEscaped(serialization, ',');

            if (fields.Count != 4)
            {
                throw new ArgumentException(Resources.InvalidSerializationStringForIgnoreInstance);
            }

            if (fields[0].Length == 0)
            {
                throw new ArgumentException(Resources.EmptyFileField);
            }

            int column;
            if (int.TryParse(fields[2], out column))
            {
                Init(Utilities.AbsolutePathFromRelative(fields[0], projectFolderForDerelativization), fields[3], fields[1], column);
            }
            else
            {
                throw new ArgumentException(Resources.BadColumnField, "serialization");
            }
        }

        /// <summary>
        /// Returns a serialized representation of this object.
        /// </summary>
        /// <returns>A string containing four fields delimited by commas.  Commas within the fields are escaped with backslashes.</returns>
        public string Serialize(string projectFolderForRelativization)
        {
            string relativePath = Utilities.RelativePathFromAbsolute(FilePath, projectFolderForRelativization);

            return Utilities.EscapeChar(relativePath, ',') + ',' +
                Utilities.EscapeChar(IgnoredTerm, ',') + ',' +
                PositionOfIgnoredTerm.ToString(CultureInfo.InvariantCulture) + ',' +
                Utilities.EscapeChar(IgnoredLine, ',');
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            var otherInstance = (IgnoreInstance)obj;

            return
                FilePath == otherInstance.FilePath &&
                IgnoredLine == otherInstance.IgnoredLine &&
                IgnoredTerm == otherInstance.IgnoredTerm &&
                PositionOfIgnoredTerm == otherInstance.PositionOfIgnoredTerm;
        }

        public override int GetHashCode()
        {
            // Since these properties define the behavior of the Equals method, they must also
            // define the hash code.
            return
                (FilePath ?? string.Empty).GetHashCode() ^
                (IgnoredLine ?? string.Empty).GetHashCode() ^
                (IgnoredTerm ?? string.Empty).GetHashCode() ^
                PositionOfIgnoredTerm.GetHashCode();
        }

        #region IIgnoreInstance Members

        public string FilePath
        {
            get { return _file; }
        }

        public string IgnoredLine
        {
            get { return _lineText; }
        }

        public int PositionOfIgnoredTerm
        {
            get { return _column; }
        }

        public string IgnoredTerm
        {
            get { return _term; }
        }

        #endregion IIgnoreInstance Members

        #region Private Members

        private void Init(string file, string lineText, string term, int column)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            if (lineText == null)
            {
                throw new ArgumentNullException("lineText");
            }
            if (term == null)
            {
                throw new ArgumentNullException("term");
            }
            if (file.Length == 0)
            {
                throw new ArgumentException(Resources.EmptyString, "file");
            }
            column -= LeadingWhitespace(lineText);
            lineText = lineText.Trim();
            if (lineText.Length == 0)
            {
                throw new ArgumentException(Resources.EmptyString, "lineText");
            }
            if (term.Length == 0)
            {
                throw new ArgumentException(Resources.EmptyString, "term");
            }
            if (column < 0 || column >= lineText.Length)
            {
                throw new ArgumentOutOfRangeException("column", column, Resources.ColumnOutOfRange);
            }

            _file = file;
            _lineText = lineText;
            _term = term;
            _column = column;
        }

        private static int LeadingWhitespace(string text)
        {
            int count = 0;
            for (; count < text.Length && char.IsWhiteSpace(text[count]); ++count) { };
            return count;
        }

        #endregion Private Members
    }
}
