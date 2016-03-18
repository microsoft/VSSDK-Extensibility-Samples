/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

// SccProviderStorage.cs : The class implements a fake source control storage for the SccProvider package
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Microsoft.Samples.VisualStudio.SourceControlIntegration.SccProvider
{
    // This class defines basic source control status values
    public enum SourceControlStatus {
        scsUncontrolled = 0,
        scsCheckedIn,
        scsCheckedOut
    };

    public class SccProviderStorage
    {
        private static string _storageExtension = ".storage";
        private string _projectFile = null;
        private Hashtable _controlledFiles = null;

        public SccProviderStorage(string projectFile)
        {
            _projectFile = projectFile.ToLower();
            _controlledFiles = new Hashtable();

            // Read the storage file if it already exist
            ReadStorageFile();
        }

        /// <summary>
        /// Saves the list of the "controlled" files in a file with the same name as the project but with an extra ".storage" extension
        /// </summary>
        private void WriteStorageFile()
        {
            StreamWriter objWriter = null;

            try
            {
                string storageFile = _projectFile + _storageExtension;
                objWriter = new StreamWriter(storageFile, false, System.Text.Encoding.Unicode);
                foreach (string strFile in _controlledFiles.Keys)
                {
                    objWriter.Write(strFile);
                    objWriter.Write("\r\n");
                }
            }
            finally
            {
                if (objWriter != null)
                {
                    objWriter.Close();
                }
            }
        }

        /// <summary>
        /// Reads the list of "controlled" files in the current project.
        /// </summary>
        public void ReadStorageFile()
        {
            string storageFile = _projectFile + _storageExtension;
            if (File.Exists(storageFile))
            {
                StreamReader objReader = null;

                try
                {
                    objReader = new StreamReader(storageFile, System.Text.Encoding.Unicode, false);
                    String strLine;
                    while ((strLine = objReader.ReadLine()) != null)
                    {
                        strLine.Trim();

                        _controlledFiles[strLine.ToLower()] = null;
                    }
                }
                finally
                {
                    if (objReader != null)
                    {
                        objReader.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Adds files to source control by adding them to the list of "controlled" files in the current project
        /// and changing their attributes to reflect the "checked in" status.
        /// </summary>
        public void AddFilesToStorage(IList<string> files)
        {
            // Add the files to a hastable so we can easily check later which files are controlled
            foreach (string file in files)
            {
                _controlledFiles[file.ToLower()] = null;
            }

            // And save the storage file
            WriteStorageFile();

            // Adding the files to the store also makes the local files read only
            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    File.SetAttributes(file, FileAttributes.ReadOnly);
                }
            }
        }

        /// <summary>
        /// Renames a "controlled" file. If the project file is being renamed, rename the whole storage file
        /// </summary>
        public void RenameFileInStorage(string strOldName, string strNewName)
        {
            strOldName = strOldName.ToLower();
            strNewName = strNewName.ToLower();

            // Rename the file in the storage if it was controlled
            if (_controlledFiles.ContainsKey(strOldName))
            {
                _controlledFiles.Remove(strOldName);
                _controlledFiles[strNewName] = null;
            }

            // Save the storage file to reflect changes
            WriteStorageFile();

            // If the project file itself is being renamed, we have to rename the storage file itself
            if (_projectFile.CompareTo(strOldName) == 0)
            {
                string _storageOldFile = strOldName + _storageExtension;
                string _storageNewFile = strNewName + _storageExtension;
                File.Move(_storageOldFile, _storageNewFile);
            }
        }

        /// <summary>
        /// Returns a source control status inferred from the file's attributes on local disk
        /// </summary>
        public SourceControlStatus GetFileStatus(string filename)
        {
            if (!_controlledFiles.ContainsKey(filename.ToLower()))
            {
                return SourceControlStatus.scsUncontrolled;
            }

            // Once we know it's controlled, look at the attribute to see if it's "checked out" or not
            if (!File.Exists(filename))
            {
                // Consider a non-existent file checked in
                return SourceControlStatus.scsCheckedIn;
            }
            else
            {
                if ((File.GetAttributes(filename) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    return SourceControlStatus.scsCheckedIn;
                }
                else
                {
                    return SourceControlStatus.scsCheckedOut;
                }
            }
        }

        /// <summary>
        /// Checkin a file to store by making the file on disk read only
        /// </summary>
        public void CheckinFile(string filename)
        {
            if (File.Exists(filename))
            {
                File.SetAttributes(filename, FileAttributes.ReadOnly);
            }
        }

        /// <summary>
        /// Checkout a file from store by making the file on disk writable
        /// </summary>
        public void CheckoutFile(string filename)
        {
            if (File.Exists(filename))
            {
                File.SetAttributes(filename, FileAttributes.Normal);
            }
        }
    }
}
