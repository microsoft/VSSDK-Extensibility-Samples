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

[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "DiffClassifier")]
[module: SuppressMessage("Microsoft.MSInternal", "CA904:DeclareTypesInMicrosoftOrSystemNamespace", Scope = "namespace", Target = "DiffClassifier")]
[module: SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member", Target = "DiffClassifier.DiffClassificationDefinitions.#diffAddedDefinition")]
[module: SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member", Target = "DiffClassifier.DiffClassificationDefinitions.#diffChangedDefinition")]
[module: SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member", Target = "DiffClassifier.DiffClassificationDefinitions.#diffClassificationDefinition")]
[module: SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member", Target = "DiffClassifier.DiffClassificationDefinitions.#diffContentTypeDefinition")]
[module: SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member", Target = "DiffClassifier.DiffClassificationDefinitions.#diffFileExtensionDefinition")]
[module: SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member", Target = "DiffClassifier.DiffClassificationDefinitions.#diffHeaderDefinition")]
[module: SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member", Target = "DiffClassifier.DiffClassificationDefinitions.#diffInfolineDefinition")]
[module: SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member", Target = "DiffClassifier.DiffClassificationDefinitions.#diffPatchLineDefinition")]
[module: SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member", Target = "DiffClassifier.DiffClassificationDefinitions.#diffRemovedDefinition")]
[module: SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope = "member", Target = "DiffClassifier.DiffClassificationDefinitions.#patchFileExtensionDefinition")]
[module: SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Scope = "type", Target = "DiffClassifier.DiffClassifier")]

#endif
