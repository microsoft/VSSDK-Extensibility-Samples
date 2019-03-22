# Autoload Restrictions

Visual Studio is moving to a new autoload behavior. In a future release, VS will start blocking packages that autol load synchronously. More details can be found in [this blog post](https://blogs.msdn.microsoft.com/visualstudio/2018/05/16/improving-the-responsiveness-of-critical-scenarios-by-updating-auto-load-behavior-for-extensions/).

## Moving to Async Autoload
1. Derive the package from [AsyncPackage](https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.asyncpackage?view=visualstudiosdk-2017)
2. Add the following to Package registration:
  
```c#
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]  
[ProvideAutoLoad(UIContextGuid, PackageAutoLoadFlags.BackgroundLoad)] 
```

For general guidance on how to move to [AsyncPackage](https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.asyncpackage?view=visualstudiosdk-2017), please refer to [this page](https://github.com/Microsoft/VSSDK-Extensibility-Samples/blob/master/AsyncPackageMigration/README.md).

For guidance on support of native packages, check [here](https://github.com/Microsoft/VSSDK-Extensibility-Samples/blob/master/AsyncPackageMigration/NativeProjectSupport).


## Testing that your package is async autoloaded
To check if your package is async autoloaded, you can install [this extension](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.PackageLoadExplorer).

## Things to watch out for to ensure your extension still works:
If an async package uses solution events, due to autoload packages are now loaded after solution load completes, the extension may not work as expected. 

In general, to use solution-related events, at package load time, the package can enumerate contents in the solution to find information needed for the feature to work
1. If you need to change your command visibility based on the solution content, an example can be found here: https://github.com/madskristensen/VisibilityConstraintsSample 
2. If you are using SolutionEvents in your package, check out this sample: https://github.com/madskristensen/SolutionLoadSample

## Exemptions
Serveral UI contexts are extempted from the async autoload restriction. However,  it is still encouraged for packages using these UI contexts to convert to async if possible.

1. Packages using UIContext_Debugging and need to provide information during debugger launch, will be exempted from the async autoload restrictions.

2. Source Code Control(SCC) Provider packages providing sync functionality and requiring to load before solution opens, will be exempted from the async autoload restrictions.

3. Packages that participate in Project/Solution upgrade and need to provide information while a project is upgrading, should use UICONTEXT_SolutionOrProjectUpgrading which will be exempted from the async autoload restrictions.
*Note*: If a package is using different UI contexts, e.g., UIContext_SolutionOpening and listens to project upgrade events, then the package should start using UICONTEXT_SolutionOrProjectUpgrading to load.

4. Sometimes packages support the functionality of async loading after solution open, but are required to be loaded before the solution is fully closed. For packages needing such feature, we added a new UI context UIContext_SolutionClosing (coming in VSSDK 2019) and exempted it from the async autoload restriction.
