# Solution Load sample

**Applies to Visual Studio 2015 and newer**

This example shows how to listen to solution events when the package might not be initialized until after the solution has loaded.

Clone the repo to test out the sample in Visual Studio 2017 yourself.

## What is the problem?
In Visual Studio 2017 Update 8, packages will no longer be auto-loaded immediately when the following is true:

* Package inherits from AsyncPackage
* Uses `ProvideAutoload` attribute
* Supports background load
* VS is starting up or solution is being loaded 

Instead, the package will be initialized **after** the startup or solution load depending on the `ProvideAutoload` context. This is done for performance reasons and is generally speaking a net benefit to users.

The consequence is that the solution might already have been loaded when your package initializes and no solution load events will be fired until the user opens another solution. 

[See full Pakcage class in the source](src/VSPackage.cs)

## The new pattern
...happens to be what was always considered a best practice. Here are the steps:

1. Specify the package to autoload when a solution is opened
2. Check if a solution is open when package initializes and act (**new**)
3. Add event handlers for solution open events

Step #2 has always been considered a best practice, but now it is a mandatory step.

It used to be ok to autoload when a solution opened and then hook up the event handler, similar to this:

```c#
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
public sealed class VSPackage : AsyncPackage
{
    protected override Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        SolutionEvents.OnAfterBackgroundSolutionLoadComplete += HandleOpenSolution;
        return base.InitializeAsync(cancellationToken, progress);
    }

    private void HandleOpenSolution(object sender = null, EventArgs e = null)
    {
        ...
    }
}
```

The issue in the above sample is that when the `SolutionEvents.OnAfterBackgroundSolutionLoadComplete` event handler is registered, a solution might already be open. So we need to make sure to check that first, like so:

```c#
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
public sealed class VSPackage : AsyncPackage
{
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        bool isSolutionLoaded = await IsSolutionLoadedAsync();

        if (isSolutionLoaded)
        {
            HandleOpenSolution();
        }

        SolutionEvents.OnAfterBackgroundSolutionLoadComplete += HandleOpenSolution;
    }

    private async Task<bool> IsSolutionLoadedAsync()
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync();
        var solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;

        ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));

        return value is bool isSolOpen && isSolOpen;
    }

    private void HandleOpenSolution(object sender = null, EventArgs e = null)
    {
        ...
    }
}
```

[See full Pakcage class in the source](src/VSPackage.cs)

This simple check for `IsSolutionLoadedAsync()` is all we have to do and we can now handle the solution open as usual.

## Further reading

* [How to use AsyncPackage with background load](https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-use-asyncpackage-to-load-vspackages-in-the-background)
* [Use Rule-based UI context for package load](https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-use-rule-based-ui-context-for-visual-studio-extensions)

## License
[Apache 2.0](LICENSE)
