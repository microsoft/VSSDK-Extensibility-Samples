/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using System.ComponentModel;


namespace MyCompany.RdtEventExplorer
{
    /// <summary>
    /// Interfact that describes the options that control the RDT Explorer grid display.
    /// </summary>
    public interface IOptions
    {
        #region IVsRunningDocTableEvents# Properties
        bool OptAfterAttributeChange
        {
            get;
            set;
        }
        bool OptAfterDocumentWindowHide
        {
            get;
            set;
        }
        bool OptAfterFirstDocumentLock
        {
            get;
            set;
        }
        bool OptAfterSave
        {
            get;
            set;
        }
        bool OptBeforeDocumentWindowShow
        {
            get;
            set;
        }
        bool OptBeforeLastDocumentUnlock
        {
            get;
            set;
        }
        bool OptAfterAttributeChangeEx
        {
            get;
            set;
        }
        bool OptBeforeSave
        {
            get;
            set;
        }
        bool OptAfterLastDocumentUnlock
        {
            get;
            set;
        }
        bool OptAfterSaveAll
        {
            get;
            set;
        }
        bool OptBeforeFirstDocumentLock
        {
            get;
            set;
        }
        #endregion
        // Return a [this] pointer so that options can be passed as a group via automation.
        Options ContainedOptions
        {
            get;
        }
    }

    /// <summary>
    /// Implementation of IOptions.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("8ACA7448-B10D-4534-B6B6-234331DE58A1")]
    public class Options : IOptions
    {
        public Options()
        {
        }
        #region IVsRunningDocTableEvents# Properties

        private bool optAfterAttributeChange = true;
        private bool optAfterDocumentWindowHide = true;
        private bool optAfterFirstDocumentLock = true;
        private bool optAfterSave = true;
        private bool optBeforeDocumentWindowShow = true;
        private bool optBeforeLastDocumentUnlock = true;
        private bool optAfterAttributeChangeEx = true;
        private bool optBeforeSave = true;
        private bool optAfterLastDocumentUnlock = true;
        private bool optAfterSaveAll = true;
        private bool optBeforeFirstDocumentLock = true;

        public bool OptAfterAttributeChange
        {
            get { return optAfterAttributeChange; }
            set { optAfterAttributeChange = value; }
        }
        public bool OptAfterDocumentWindowHide
        {
            get { return optAfterDocumentWindowHide; }
            set { optAfterDocumentWindowHide = value; }
        }
        public bool OptAfterFirstDocumentLock
        {
            get { return optAfterFirstDocumentLock; }
            set { optAfterFirstDocumentLock = value; }
        }
        public bool OptAfterSave
        {
            get { return optAfterSave; }
            set { optAfterSave = value; }
        }
        public bool OptBeforeDocumentWindowShow
        {
            get { return optBeforeDocumentWindowShow; }
            set { optBeforeDocumentWindowShow = value; }
        }
        public bool OptBeforeLastDocumentUnlock
        {
            get { return optBeforeLastDocumentUnlock; }
            set { optBeforeLastDocumentUnlock = value; }
        }
        public bool OptAfterAttributeChangeEx
        {
            get { return optAfterAttributeChangeEx; }
            set { optAfterAttributeChangeEx = value; }
        }
        public bool OptBeforeSave
        {
            get { return optBeforeSave; }
            set { optBeforeSave = value; }
        }
        public bool OptAfterLastDocumentUnlock
        {
            get { return optAfterLastDocumentUnlock; }
            set { optAfterLastDocumentUnlock = value; }
        }
        public bool OptAfterSaveAll
        {
            get { return optAfterSaveAll; }
            set { optAfterSaveAll = value; }
        }
        public bool OptBeforeFirstDocumentLock
        {
            get { return optBeforeFirstDocumentLock; }
            set { optBeforeFirstDocumentLock = value; }
        }
        #endregion
        // Return a [this] pointer so that options can be passed as a group via automation.
        public Options ContainedOptions
        {
            get {
                return this;
            }
        }
    }
}
