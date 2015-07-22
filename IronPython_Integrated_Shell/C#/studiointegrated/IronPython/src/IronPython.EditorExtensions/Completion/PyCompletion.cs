/*****************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR
IMPLIED, INCLUDING ANY IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR PURPOSE,
MERCHANTABILITY, OR NON-INFRINGEMENT.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.IronPythonInference;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TextManager.Interop;

namespace IronPython.EditorExtensions
{
    /// <summary>
    /// Provides a completion for IronPython
    /// </summary>
    internal class PyCompletion : Completion, IComparable
    {
        /// <summary>
        /// Constructor used by IPy declarations retrieved from the IPy engine
        /// </summary>
        internal PyCompletion(Declaration declaration, IGlyphService glyphService)
            : base(declaration.Title)
        {
            this.InsertionText = declaration.Title;
            this.Description = declaration.Description;
            this.IconSource = glyphService.GetGlyph(GetGroupFromDeclaration(declaration), GetScopeFromDeclaration(declaration));
        }

        /// <summary>
        /// Constructor used by IPy snippets retrieved from the expansion manager
        /// </summary>
        internal PyCompletion(VsExpansion vsExpansion, IGlyphService glyphService)            
            : base(vsExpansion.title)
        {            
            this.InsertionText = vsExpansion.title;
            this.Description = vsExpansion.description;
            this.IconSource = glyphService.GetGlyph(StandardGlyphGroup.GlyphCSharpExpansion, StandardGlyphItem.GlyphItemPublic);

            this.VsExpansion = vsExpansion;
        }

        private StandardGlyphItem GetScopeFromDeclaration(Declaration declaration)
        {
            return StandardGlyphItem.GlyphItemPublic;
        }

        private StandardGlyphGroup GetGroupFromDeclaration(Declaration declaration)
        {
            switch (declaration.Type)
            {
                case Declaration.DeclarationType.Class:
                    return StandardGlyphGroup.GlyphGroupClass;
                case Declaration.DeclarationType.Function:
                    return StandardGlyphGroup.GlyphGroupMethod;
                case Declaration.DeclarationType.Snippet:
                    return StandardGlyphGroup.GlyphCSharpExpansion;
                default:
                    return StandardGlyphGroup.GlyphGroupClass;
            }
        }

        internal VsExpansion? VsExpansion { get; private set; }

        public int CompareTo(object other)
        {
            var otherCompletion = other as PyCompletion;
            return this.DisplayText.CompareTo(otherCompletion.DisplayText); 
        }
    }
}