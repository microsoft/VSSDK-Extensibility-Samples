# VisibilityConstraints example

[![Build status](https://ci.appveyor.com/api/projects/status/k9x55sgxjyjsay0a?svg=true)](https://ci.appveyor.com/project/madskristensen/visibilityconstraintssample)

**Applies to Visual Studio 2015 and newer**

This sample shows how to use the `<VisibilityConstraints>` element in a Visual Studio extension to remove the need to use the `ProvideAutoload` attribute on a package class.

Clone the repo to test out the sample in Visual Studio 2017 yourself.

## What is VisibilityConstraints?
Often a package is being loaded to run a `BeforeQueryStatus` method for a button to determine its visibility. With `<VisibilityConstraints>` we can determine the visibility of a button without running `BeforeQueryStatus` and therefore don't need to load the package before the user clicks the button.

It is a best practice to never load a package before it is needed, and `<VisibilityConstraints>` allow us to load the package only when requested and not before.

## Limit use of ProvideAutoload
It is very common to autoload a package when Visual Studio starts up or when a solution is being loaded. It is done by putting an attribute on the `Package` or `AsyncPackage` class like so:

```c#
[ProvideAutoLoad(UIContextGuids80.SolutionExists)]  
```

This is often a bad practice because the features in the package usually only applies to certain file or project types. It is much better to only load the package when those files or projects are loaded and not before.

The general rule of thumb is to only load the package when aboslutely needed and never before. There is [great documentation][uicontext] on how to specify auto loading rules that causes loading to happen only when needed.

Some extensions that today use the `ProvideAutoload` attribute don't actually need it at all, since `<VisibilityConstraints>` can toggle the visibility of commands/buttons without loading the package. 

If you have to use `ProvideAutoload`, make sure you do so in the background using an `AsyncPackage` as [documented here][asyncpackage].

## Let's get started
First we must specify a rule for when a button should be visible. In this example, the rule is that the button should be visible when the user right-clicks a .cs or .vb file in Solution Explorer. We can express that in an attribute on the `Package` or `AsyncPackage` class like so:

```csharp
[ProvideUIContextRule(_uiContextSupportedFiles,
    name: "Supported Files",
    expression: "CSharp | VisualBasic",
    termNames: new[] { "CSharp", "VisualBasic" },
    termValues: new[] { "HierSingleSelectionName:.cs$", "HierSingleSelectionName:.vb$" })]
```

See [sample package class](src/MyPackage.cs) and more info about using the [ProvideUXContextRule][uicontext] attribute.

Then we must register a `<VisibilityConstraint`> based on that rule in the .vsct file like so:

```xml
<VisibilityConstraints>
  <VisibilityItem guid="guidPackageCmdSet" id="MyButtonId" context="uiContextSupportedFiles" />
</VisibilityConstraints>
```

...and remember to mark the button itself as dynamic visible:
 
```xml
<CommandFlag>DynamicVisibility</CommandFlag>
```

[See sample .vsct file](src/VsCommandTable.vsct)

That's it. The project in the `/src/` folder shows a working example of how this all fits together.

## Further reading
Read the docs for all the details surrounding these scenarios.

* [Use Rule-based UI Context for Visual Studio Extensions][uicontext]
* [Use AsyncPackage to Load VSPackages in the Background][asyncpackage]
* [VisibilityItem element][visibilityitem]

[uicontext]: https://docs.microsoft.com/visualstudio/extensibility/how-to-use-rule-based-ui-context-for-visual-studio-extensions
[asyncpackage]: https://docs.microsoft.com/visualstudio/extensibility/how-to-use-asyncpackage-to-load-vspackages-in-the-background
[visibilityitem]: https://docs.microsoft.com/en-us/visualstudio/extensibility/visibilityitem-element