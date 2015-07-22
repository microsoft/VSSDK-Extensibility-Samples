//***************************************************************************
// 
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//***************************************************************************

#if CODE_ANALYSIS_BASELINE
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Tagger", Scope="type", Target="HighlightWordSample.HighlightWordTagger")]
[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Tagger", Scope="type", Target="HighlightWordSample.HighlightWordTaggerProvider")]
[module: SuppressMessage("Microsoft.Design","CA1004:GenericMethodsShouldProvideTypeParameter", Scope="member", Target="HighlightWordSample.HighlightWordTaggerProvider.#CreateTagger`1(Microsoft.VisualStudio.Text.Editor.ITextView,Microsoft.VisualStudio.Text.ITextBuffer)")]
[module: SuppressMessage("Microsoft.MSInternal","CA904:DeclareTypesInMicrosoftOrSystemNamespace", Scope="namespace", Target="HighlightWordSample")]
[module: SuppressMessage("Microsoft.Design","CA1020:AvoidNamespacesWithFewTypes", Scope="namespace", Target="HighlightWordSample")]

[module: SuppressMessage("Microsoft.Performance","CA1811:AvoidUncalledPrivateCode", Scope="member", Target="HighlightWordSample.HighlightWordTaggerProvider.#set_TextSearchService(Microsoft.VisualStudio.Text.Operations.ITextSearchService)", Justification="MEF imports")]
[module: SuppressMessage("Microsoft.Performance","CA1811:AvoidUncalledPrivateCode", Scope="member", Target="HighlightWordSample.HighlightWordTaggerProvider.#set_TextStructureNavigatorSelector(Microsoft.VisualStudio.Text.Operations.ITextStructureNavigatorSelectorService)", Justification="MEF imports")]
#endif
