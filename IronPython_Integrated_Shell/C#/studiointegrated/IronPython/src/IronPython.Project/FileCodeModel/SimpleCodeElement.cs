/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;

namespace Microsoft.Samples.VisualStudio.CodeDomCodeModel {
    interface ICodeDomElement {
        object UntypedCodeObject {
            get;
        }

        object ParentElement {
            get;
        }
    }

    [ComVisible(true)]
    [SuppressMessage("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable")]
    public abstract class SimpleCodeElement : CodeElement, CodeElement2 {
        private static Dictionary<string, CodeTypeRef> systemTypes;
        private string name;
        private DTE dte;
        private static object codeKey = new object();
        private string id;

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "0#dte")]
        protected SimpleCodeElement(DTE dte, string name) {
            this.dte = dte;
            this.name = name;
        }
        
        #region CodeElement Members

        public abstract CodeElements Children {
            get;
        }

        public abstract CodeElements Collection {
            get;
        }

        public abstract vsCMElement Kind {
            get;
        }

        public DTE DTE {
            get { return dte; }
        }

        public abstract TextPoint EndPoint {
            get;
        }

        public abstract string FullName {
            get;
        }

        public string ExtenderCATID {
            get { return String.Empty; }
        }

        public dynamic ExtenderNames {
            get { return new object[0]; }
        }

        public abstract TextPoint GetEndPoint(vsCMPart Part);

        public abstract TextPoint GetStartPoint(vsCMPart Part);

        public virtual vsCMInfoLocation InfoLocation {
            get { return vsCMInfoLocation.vsCMInfoLocationProject; }
        }

        public virtual bool IsCodeType {
            get { return false; }
        }

        public string Language {
            get { return "IronPython"; }
        }

        public string Name {
            get { return name; }
            [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
            set { name = value; }
        }

        public abstract ProjectItem ProjectItem {
            get;
        }

        public abstract TextPoint StartPoint {
            get;
        }

        public object get_Extender(string ExtenderName) {
            throw new NotImplementedException();
        }

        #endregion

        #region Common protected helpers
        protected static object CodeKey {
            get { return codeKey; }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
        protected CodeTypeRef ObjectToTypeRef(object type) {
            if (null == type) {
                throw new ArgumentNullException("type");
            }
            CodeTypeRef ctr = type as CodeTypeRef;
            if (ctr != null) return ctr;

            if(type is int){
                type = (vsCMTypeRef)(int)type;
            }

            if (type is vsCMTypeRef) {
                vsCMTypeRef typeRef = (vsCMTypeRef)type;
                switch (typeRef) {
                    case vsCMTypeRef.vsCMTypeRefVoid: return GetSystemType("System.Void"); 
                    case vsCMTypeRef.vsCMTypeRefString: return GetSystemType("System.String"); 
                    case vsCMTypeRef.vsCMTypeRefShort: return GetSystemType("System.Int16"); 
                    case vsCMTypeRef.vsCMTypeRefObject: return GetSystemType("System.Object"); 
                    case vsCMTypeRef.vsCMTypeRefLong: return GetSystemType("System.Int64"); 
                    case vsCMTypeRef.vsCMTypeRefInt: return GetSystemType("System.Int32"); 
                    case vsCMTypeRef.vsCMTypeRefFloat: return GetSystemType("System.Single"); 
                    case vsCMTypeRef.vsCMTypeRefDouble: return GetSystemType("System.Double"); 
                    case vsCMTypeRef.vsCMTypeRefDecimal: return GetSystemType("System.Decimal"); 
                    case vsCMTypeRef.vsCMTypeRefCodeType: return GetSystemType("System.Type"); 
                    case vsCMTypeRef.vsCMTypeRefChar: return GetSystemType("System.Char"); 
                    case vsCMTypeRef.vsCMTypeRefByte: return GetSystemType("System.Byte"); 
                    case vsCMTypeRef.vsCMTypeRefBool: return GetSystemType("System.Boolean"); 
                    case vsCMTypeRef.vsCMTypeRefArray: return GetSystemType("System.Array"); 
                    case vsCMTypeRef.vsCMTypeRefVariant:
                    case vsCMTypeRef.vsCMTypeRefPointer:
                    case vsCMTypeRef.vsCMTypeRefOther:
                        throw new NotImplementedException(String.Format("Unknown system type: {0}", type));
                }
            }

            string stringType = type as string;
            if (stringType != null) {
                return new CodeDomCodeTypeRef(dte, stringType);
            }

            throw new InvalidOperationException(String.Format("unknown type to get type from: {0} ({1})", type.GetType(), type));
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        protected static TypeAttributes VSAccessToCodeAccess(vsCMAccess access) {
            switch (access) {
                case vsCMAccess.vsCMAccessAssemblyOrFamily: return TypeAttributes.NestedFamORAssem;
                case vsCMAccess.vsCMAccessDefault: return TypeAttributes.NotPublic;
                case vsCMAccess.vsCMAccessPrivate: return TypeAttributes.NotPublic;
                case vsCMAccess.vsCMAccessProject: return TypeAttributes.NestedFamANDAssem;
                case vsCMAccess.vsCMAccessProjectOrProtected: return TypeAttributes.NestedFamily;
                case vsCMAccess.vsCMAccessProtected: return TypeAttributes.NestedAssembly;
                case vsCMAccess.vsCMAccessPublic: return TypeAttributes.Public;
                case vsCMAccess.vsCMAccessWithEvents: return TypeAttributes.NestedPublic;
            }
            throw new NotImplementedException();
        }

        protected static MemberAttributes VSAccessToMemberAccess(vsCMAccess access) {
            switch (access) {
                case vsCMAccess.vsCMAccessAssemblyOrFamily: return MemberAttributes.FamilyOrAssembly;
                case vsCMAccess.vsCMAccessPrivate: return MemberAttributes.Private;
                case vsCMAccess.vsCMAccessProject: return MemberAttributes.Assembly;
                case vsCMAccess.vsCMAccessProjectOrProtected: return MemberAttributes.FamilyOrAssembly;
                case vsCMAccess.vsCMAccessProtected: return MemberAttributes.Family;
                case vsCMAccess.vsCMAccessPublic: return MemberAttributes.Public;
                case vsCMAccess.vsCMAccessDefault:
                case vsCMAccess.vsCMAccessWithEvents:
                default:
                    return (MemberAttributes)0;
            }
        }

        protected static vsCMAccess MemberAccessToVSAccess(MemberAttributes attributes) {
            switch (attributes & MemberAttributes.AccessMask) {
                case MemberAttributes.Public: return vsCMAccess.vsCMAccessPublic;
                case MemberAttributes.Private: return vsCMAccess.vsCMAccessPrivate;
                case MemberAttributes.Family: return vsCMAccess.vsCMAccessProject;
                case MemberAttributes.FamilyOrAssembly: return vsCMAccess.vsCMAccessProjectOrProtected;
                case MemberAttributes.FamilyAndAssembly: return vsCMAccess.vsCMAccessProject;
                default:
                    return vsCMAccess.vsCMAccessDefault;
            }

        }

        protected string ObjectToClassName(object baseCls) {
            string strBase = baseCls as string;
            if (strBase != null) return strBase;

            CodeClass cs = baseCls as CodeClass;
            if (cs != null) return cs.FullName;

            CodeInterface ci = baseCls as CodeInterface;
            if (ci != null) return ci.FullName;

            return ObjectToTypeRef(baseCls).AsFullName;
        }

        #endregion

        #region Private Implementation details

        private CodeTypeRef GetSystemType(string typeName) {
            if (systemTypes == null) systemTypes = new Dictionary<string, CodeTypeRef>();

            CodeTypeRef res;
            if (systemTypes.TryGetValue(typeName, out res)) return res;

            res = new CodeDomCodeTypeRef(dte, typeName);
            systemTypes[typeName] = res;

            return res;
        }

        #endregion

        #region CodeElement2 Members


        public string ElementID {
            get { if (id == null) id = new Guid().ToString(); return id; }
        }

        public void RenameSymbol(string NewName) {
            this.name = NewName;            
        }

        #endregion
    }
}
