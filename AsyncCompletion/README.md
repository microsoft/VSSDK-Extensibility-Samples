# Completion sample

**Applies to Visual Studio 2019 and newer**

Visual Studio 2019 introduces a new completion infrastructure. Under the new model, the extender does not need to handle user events and manage the lifetime of completion. The extensibility points are scoped to the following scenarios:

## Running the sample
Build and deploy the `AsyncCompletionSample` project. This will add completion capability to all code editors. Due to interference with other language services, this sample is best run in a .txt file.

## IAsyncCompletionSource

Constructed by the MEF part `IAsyncCompletionSourceProvider`, the `IAsyncCompletionSource` is responsible for
* Providing completion items
* Establishing location of completion
You extension does not need to fulfill both roles,
but both roles need to be fufilled to begin the completion session.

If you are extending an existing language, expect the language service to provide the exact location. 
You may simply provide additional completion items to include in the session.

### In this sample
`SampleCompletionSource` and `SampleCompletionSourceProvider` demonstrate this interface. 

**Suggestion mode**

This completion source typically shows the list of chemical elements, but when invoked after a colon (`:`), it shows completion items related to the chemical element named before the colon. After the colon, the suggestion mode is active. In suggestion mode, user may type an expression not present in the completion list, and the expression will not be expanded to the best match upon pressing one of the "commit characters", indicated by `SampleCompletionCommitManager`

**Filters**

Also, observe that completion filters are singletons. Each completion item may respond to multiple filters. In this example, _metalloid_ elements are shown by clicking on either _metal_ or _non metal_ filters.

## IAsyncCompletionCommitManager

Constructed by MEF part `IAsyncCompletionCommitManagerProvider`, the `IAsyncCompletionCommitManager` is responsible for
* Indicating which characters may complete the session (e.g. space, dot, parentheses)
* Providing custom commit behavior, including, moving the caret, replacing code in other location of the document, modifying the project, etc.

Your extension may, but does not need to provide any of these roles.
By default, editor inserts the text of the completion item at the location of the completion session.
Furthermore, Enter, Tab and double click completes the session with the selected item. 

### In this sample
`SampleCompletionCommitManager` and `SampleCompletionCommitManagerProvider` demonstrate this interface.

## IAsyncCompletionItemManager

Constructed by MEF part `IAsyncCompletionItemManagerProvider`, the `IAsyncCompletionItemManager` is responsible for
* One time sorting of the aggregated completion items
* Filtering the list of items when user types and interacts with filter buttons
Your extension does not need to implement this type, as the editor provides a default implementation.
This sample contains source code of this implementation.

### In this sample
`DefaultCompletionItemManager` and `DefaultCompletionItemManagerProvider` are copies of the default implementation.

## ICompletionPresenter

Constructed by MEF part `ICompletionPresenterProvider`, the `ICompletionPresenter` is responsible for
* Displaying completion items and completion filters
* Informing the editor of user interactions such as selecting the item and clicking filter buttons
Your extension does not need to implement this type, as the editor provides a default implementation. 


## Futher reading
See the comments in the source code of this sample for detailed explanations. The comments also include sample code which might be useful to you, yet is not exercised by this specific extension.

* [AsyncCompletion namespace](https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.language.intellisense.asynccompletion?view=visualstudiosdk-2017)
