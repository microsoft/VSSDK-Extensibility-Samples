/***************************************************************************
 
Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Microsoft.Samples.VisualStudio.CodeSweep.Scanner
{
    [Serializable]
    class ScanResult : IScanResult, ISerializable
    {
        const string FilePathKey = "FilePath";
        const string HitsKey = "Hits";
        const string ScannedKey = "Scanned";

        public static ScanResult ScanOccurred(string filePath, IEnumerable<IScanHit> hits)
        {
            return new ScanResult(filePath, hits, true);
        }

        public static ScanResult ScanNotPossible(string filePath)
        {
            return new ScanResult(filePath, null, false);
        }

        string _filePath;
        IEnumerable<IScanHit> _hits;
        bool _scanned;

        private ScanResult(string filePath, IEnumerable<IScanHit> hits, bool scanned)
        {
            _filePath = filePath;
            _hits = hits;
            _scanned = scanned;
        }

        protected ScanResult(SerializationInfo info, StreamingContext context)
        {
            _filePath = info.GetString(FilePathKey);
            _hits = (IEnumerable<IScanHit>)info.GetValue(HitsKey, typeof(IEnumerable<IScanHit>));
            _scanned = info.GetBoolean(ScannedKey);
        }

        #region IScanResult Members

        public string FilePath
        {
            get { return _filePath; }
        }

        public IEnumerable<IScanHit> Results
        {
            get { return _hits; }
        }

        public bool Scanned
        {
            get { return _scanned; }
        }

        public bool Passed
        {
            get
            {
                if (_scanned && _hits != null)
                {
                    // If there are any hits, it didn't pass.
                    return !_hits.Any();
                }
                return false;
            }
        }

        #endregion IScanResult Members

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(FilePathKey, _filePath);
            info.AddValue(HitsKey, _hits);
            info.AddValue(ScannedKey, _scanned);
        }

        #endregion ISerializable Members
    }
}
