/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Runtime.Serialization;

namespace Microsoft.Samples.VisualStudio.IDE.EditorWithToolbox
{

    /// <summary>
    /// This class implements data to be stored in ToolboxItem.
    /// This class needs to be serializable in order to be passed to the toolbox
    /// and back.
    /// 
    /// Moreover, this assembly path is required to be on VS probing paths to make
    /// deserialization successful. See ToolboxItemData.pkgdef.
    /// </summary>
    [Serializable()]
    public class ToolboxItemData : ISerializable 
    {
        #region Fields
        private string content;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="sentence">Sentence value.</param>
        public ToolboxItemData(string sentence)
        {
            content = sentence;
        }
        #endregion Constructors

        #region Properties
        /// <summary>
        /// Gets the ToolboxItemData Content.
        /// </summary>
        public string Content
        {
            get { return content; }
        }
        #endregion Properties

        internal ToolboxItemData(SerializationInfo info, StreamingContext context)
        {
            content = info.GetValue("Content", typeof(string)) as string;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                info.AddValue("Content",Content);    
            }
        }
    }
}
