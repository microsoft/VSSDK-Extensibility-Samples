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
using System.Xml;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Microsoft.Samples.VisualStudio.GeneratorSample
{
    // In order to be compatible with this single file generator, the input file has to
    // follow the schema in XMLClassGeneratorSchema.xsd

    /// <summary>
    /// Generates source code based on a XML document
    /// </summary>
    public static class SourceCodeGenerator
    {
        /// <summary>
        /// Create a CodeCompileUnit based on the XmlDocument doc
        /// In order to be compatible with this single file generator, the input XmlDocument has to
        /// follow the schema in XMLClassGeneratorSchema.xsd
        /// </summary>
        /// <param name="doc">An XML document that contains the description of the code to be generated</param>
        /// <param name="namespaceName">If the root node of doc does not have a namespace attribute, use this instead</param>
        /// <returns>The generated CodeCompileUnit</returns>
        public static CodeCompileUnit CreateCodeCompileUnit(XmlDocument doc, string namespaceName)
        {
            XmlElement root = doc.DocumentElement;

            if (root.Name != "Types")
            {
                throw new ArgumentException(string.Format(Strings.InvalidRoot, root.Name));
            }

            if (root.ChildNodes.Count == 0)
            {
                throw new ArgumentException(Strings.NoTypes);
            }

            if (root.Attributes.GetNamedItem("namespace") != null)
            {
                namespaceName = root.Attributes.GetNamedItem("namespace").Value;
            }
            
            CodeCompileUnit code = new CodeCompileUnit();
            
            // Just for VB.NET:
            // Option Strict On (controls whether implicit type conversions are allowed)
            code.UserData.Add("AllowLateBound", false);
            // Option Explicit On (controls whether variable declarations are required)
            code.UserData.Add("RequireVariableDeclaration", true);

            CodeNamespace codeNamespace = new CodeNamespace(namespaceName);

            foreach (XmlNode node in root.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Class":
                        codeNamespace.Types.Add(CreateClass(node));
                        break;
                    case "Enum":
                        codeNamespace.Types.Add(CreateEnum(node));
                        break;
                    default:
                        throw new ArgumentException(string.Format(Strings.InvalidType, node.Name));
                }
            }

            code.Namespaces.Add(codeNamespace);
            return code;
        }

        private static CodeTypeDeclaration CreateClass(XmlNode node)
        {
            string className;
            string classAccess;
            bool isPartial;

            GetClassInfo(node, out className, out classAccess, out isPartial);
            
            CodeTypeDeclaration typeDeclaration = new CodeTypeDeclaration(className);

            TypeAttributes attrib;

            switch (classAccess)
            {
                case "public":
                    attrib = TypeAttributes.Public;
                    break;
                case "internal":
                    attrib = TypeAttributes.NotPublic;
                    break;
                default:
                    throw new ArgumentException(string.Format(Strings.BadTypeAccess, classAccess));
            }

            typeDeclaration.IsPartial = isPartial;
            typeDeclaration.IsClass = true;
            typeDeclaration.TypeAttributes = attrib | TypeAttributes.Class;

            foreach (XmlNode n in node.ChildNodes)
            {
                CodeMemberField field = CreateField(n);
                typeDeclaration.Members.Add(field);
            }

            return typeDeclaration;
        }

        private static void GetClassInfo(XmlNode node, out string className, out string classAccess, out bool isPartial)
        {
            if (node.Attributes != null && node.Attributes.GetNamedItem("name") != null && node.Attributes.GetNamedItem("name").Value != string.Empty)
            {
                className = node.Attributes.GetNamedItem("name").Value;
            }
            else
            {
                throw new ArgumentException(Strings.ClassNodeNoName);
            }

            if (node.Attributes.GetNamedItem("access") != null)
            {
                classAccess = node.Attributes.GetNamedItem("access").Value;
            }
            else
            {
                classAccess = "public";
            }

            if (node.Attributes.GetNamedItem("partial") != null)
            {
                isPartial = bool.Parse(node.Attributes.GetNamedItem("partial").Value);
            }
            else
            {
                isPartial = false;
            }
        }

        private static CodeMemberField CreateField(XmlNode n)
        {
            string fieldName;
            string fieldAccess;
            string fieldValue;
            string fieldType;
            bool fieldIsStatic;

            GetFieldInfo(n, out fieldName, out fieldAccess, out fieldValue, out fieldType, out fieldIsStatic);

            CodeMemberField field = new CodeMemberField(fieldType, fieldName);

            if (n.Name == "Constant")
            {
                // Set the correct attributes for a constant field  
                field.Attributes = (field.Attributes & ~MemberAttributes.AccessMask & ~MemberAttributes.ScopeMask) | MemberAttributes.Public | MemberAttributes.Const;

                if (fieldType == "System.String")
                {
                    field.InitExpression = new CodePrimitiveExpression(fieldValue);
                }
                else
                {
                    field.InitExpression = new CodeSnippetExpression(fieldValue);
                }
            }
            else
            {
                switch (fieldAccess)
                {
                    case "public":
                        field.Attributes = MemberAttributes.Public;
                        break;
                    case "protected":
                        field.Attributes = MemberAttributes.Family;
                        break;
                    case "private":
                        field.Attributes = MemberAttributes.Private;
                        break;
                    default:
                        throw new ArgumentException(string.Format(Strings.BadVariableAccess, fieldAccess));
                }

                if (fieldIsStatic)
                {
                    field.Attributes |= MemberAttributes.Static;
                }
            }

            return field;
        }

        private static void GetFieldInfo(XmlNode n, out string memberName, out string memberAccess, out string memberValue, out string memberType, out bool memberIsStatic)
        {
            if (n.Name != "Variable" && n.Name != "Constant")
            {
                throw new ArgumentException(string.Format(Strings.BadClassMemberName, n.Name));
            }

            if (n.Attributes != null && n.Attributes.GetNamedItem("name") != null && n.Attributes.GetNamedItem("name").Value != string.Empty)
            {
                memberName = n.Attributes.GetNamedItem("name").Value;
            }
            else
            {
                throw new ArgumentException(Strings.ClassMemberNoName);
            }

            if (n.Attributes.GetNamedItem("type") != null)
            {
                memberType = n.Attributes.GetNamedItem("type").Value;
            }
            else
            {
                throw new ArgumentException(Strings.ClassMemberNoType);
            }

            if (n.Attributes.GetNamedItem("value") != null)
            {
                memberValue = n.Attributes.GetNamedItem("value").Value;
            }
            else
            {
                if (n.Name == "Constant")
                {
                    throw new ArgumentException(Strings.ConstantNoValue);
                }
                else
                {
                    memberValue = null;
                }
            }

            if (n.Attributes.GetNamedItem("access") != null)
            {
                memberAccess = n.Attributes.GetNamedItem("access").Value;
            }
            else
            {
                memberAccess = "public";
            }

            if (n.Attributes.GetNamedItem("static") != null)
            {
                if (n.Name == "Constant")
                {
                    throw new ArgumentException(Strings.ConstantNoValue);
                }
                else
                {
                    memberIsStatic = bool.Parse(n.Attributes.GetNamedItem("static").Value);
                }
            }
            else
            {
                memberIsStatic = false;
            }
        }

        private static CodeTypeDeclaration CreateEnum(XmlNode node)
        {
            string enumName;
            string enumAccess;
            bool enumFlagsAttribute;

            GetEnumInfo(node, out enumName, out enumAccess, out enumFlagsAttribute);
            
            CodeTypeDeclaration typeDeclaration = new CodeTypeDeclaration(enumName);
            typeDeclaration.IsEnum = true;

            if (enumFlagsAttribute)
            {
                typeDeclaration.CustomAttributes.Add(new CodeAttributeDeclaration("System.FlagsAttribute"));
            }

            TypeAttributes attrib;
            switch (enumAccess)
            {
                case "public":
                    attrib = TypeAttributes.Public;
                    break;
                case "internal":
                    attrib = TypeAttributes.NotPublic;
                    break;
                default:
                    throw new ArgumentException(string.Format(Strings.BadTypeAccess, enumAccess));
            }

            typeDeclaration.TypeAttributes = attrib;

            foreach (XmlNode n in node.ChildNodes)
            {
                string memberName;
                int memberValue;
                bool hasValue;

                GetEnumMemberInfo(enumFlagsAttribute, n, out memberName, out memberValue, out hasValue);

                CodeMemberField field = new CodeMemberField("System.Int32", memberName);
                if (hasValue)
                {
                    field.InitExpression = new CodePrimitiveExpression(memberValue);
                }

                typeDeclaration.Members.Add(field);                
            }

            return typeDeclaration;
        }

        private static void GetEnumMemberInfo(bool enumFlagsAttribute, XmlNode n, out string memberName, out int memberValue, out bool hasValue)
        {
            if (n.Name != "EnumMember")
            {
                throw new ArgumentException(string.Format(Strings.EnumNodeNoName, n.Name));
            }

            if (n.Attributes != null && n.Attributes.GetNamedItem("name") != null && n.Attributes.GetNamedItem("name").Value != string.Empty)
            {
                memberName = n.Attributes.GetNamedItem("name").Value;
            }
            else
            {
                throw new ArgumentException(Strings.EnumMemberNodeNoName);
            }

            if (n.Attributes.GetNamedItem("value") != null)
            {
                memberValue = System.Int32.Parse(n.Attributes.GetNamedItem("value").Value);
                hasValue = true;
            }
            else
            {
                if (enumFlagsAttribute)
                {
                    throw new ArgumentException(Strings.EnumMemberValueMissing);
                }
                else
                {
                    memberValue = 0;
                    hasValue = false;
                }
            }
        }

        private static void GetEnumInfo(XmlNode node, out string enumName, out string enumAccess, out bool enumFlagsAttribute)
        {
            if (node.Attributes != null && node.Attributes.GetNamedItem("name") != null && node.Attributes.GetNamedItem("name").Value != string.Empty)
            {
                enumName = node.Attributes.GetNamedItem("name").Value;
            }
            else
            {
                throw new ArgumentException(Strings.EnumNodeNoName);
            }
            
            if (node.ChildNodes.Count == 0)
            {
                throw new ArgumentException(Strings.EnumNoMembers);
            }

            if (node.Attributes.GetNamedItem("access") != null)
            {
                enumAccess = node.Attributes.GetNamedItem("access").Value;
            }
            else
            {
                enumAccess = "public";
            }

            if (node.Attributes.GetNamedItem("flagsAttribute") != null)
            {
                enumFlagsAttribute = bool.Parse(node.Attributes.GetNamedItem("flagsAttribute").Value);
            }
            else
            {
                enumFlagsAttribute = false;
            }
        }
    }
}