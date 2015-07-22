/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;

using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Samples.VisualStudio.IronPython.Project.Library
{
    /// <summary>
    /// Class used to identify a module. The module is identify using the hierarchy that
    /// contains it and its item id inside the hierarchy.
    /// </summary>
    internal sealed class ModuleId {
        private IVsHierarchy ownerHierarchy;
        private uint itemId;
        public ModuleId(IVsHierarchy owner, uint id) {
            this.ownerHierarchy = owner;
            this.itemId = id;
        }
        public IVsHierarchy Hierarchy {
            get { return ownerHierarchy; }
        }
        public uint ItemID {
            get { return itemId; }
        }
        public override int GetHashCode() {
            int hash = 0;
            if (null != ownerHierarchy) {
                hash = ownerHierarchy.GetHashCode();
            }
            hash = hash ^ (int)itemId;
            return hash;
        }
        public override bool Equals(object obj) {
            ModuleId other = obj as ModuleId;
            if (null == obj) {
                return false;
            }
            if (!ownerHierarchy.Equals(other.ownerHierarchy)) {
                return false;
            }
            return (itemId == other.itemId);
        }
    }
}