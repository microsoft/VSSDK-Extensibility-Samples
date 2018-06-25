# Migrate to AsyncPackage

Shows the simplest way to migrate a Visual Studio extension from using the [Package](https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.package?view=visualstudiosdk-2017) base class to using the [AsyncPackage](https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.asyncpackage?view=visualstudiosdk-2017) with background load enabled.

This work for Visual Studio extensions targeting **Visual Studio 2015** and newer.

## The *Package* class
Here is an example of a simple *Package* class that we want to convert to an *AsyncPackage*. It registers a service and a command.

```c#
[PackageRegistration(UseManagedResourcesOnly = true)]
[InstalledProductRegistration("My Synchronous Package", "Loads synchronously", "1.0")]
[ProvideService(typeof(MyService))]
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]
[Guid("d71fec50-1ce3-40d5-9e4e-3f5d3ed397b0")]
public class MyPackage : Package
{
    protected override void Initialize()
    {
        // Long running synchronous method call that blocks the UI thread 
        Thread.Sleep(5000);

        // Adds a service synchronosly on the UI thread
        var callback = new ServiceCreatorCallback(CreateMyService);
        ((IServiceContainer)this).AddService(typeof(MyService), callback);
        
        // Synchronously requesting a service on the UI thread
        var dte = GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

        // Initializes the command synchronously on the UI thread
        MyCommand.Initialize(this, dte);
    }

    private object CreateMyService(IServiceContainer container, Type serviceType)
    {
        if ( typeof(MyService) == serviceType)
        {
            var svc = new MyService();
            svc.Initialize(this);
            return svc;
        }

        return null;
    }
}
```

See the full [MyPackage.cs](src/MyPackage.cs) class.

## The *AsyncPackage* class
The async version of the above class looks very similar with some noticable differences.

```c#
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[InstalledProductRegistration("My Asynchronous Package", "Loads asynchronously", "1.0")]
[ProvideService(typeof(MyService), IsAsyncQueryable = true)]
[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
[Guid("d71fec50-1ce3-40d5-9e4e-3f5d3ed397b0")]
public sealed class MyAsyncPackage : AsyncPackage
{
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        // runs in the background thread and doesn't affect the responsiveness of the UI thread.
        await Task.Delay(5000);

        // Adds a service on the background thread
        AddService(typeof(MyService), CreateMyServiceAsync);

        // Switches to the UI thread in order to consume some services used in command initialization
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        // Query service asynchronously from the UI thread
        var dte = await GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

        // Initializes the command asynchronously now on the UI thread
        await MyCommand.InitializeAsync(this, dte);
    }

    private async Task<object> CreateMyServiceAsync(IAsyncServiceContainer container, CancellationToken cancellationToken, Type serviceType)
    {
        var svc = new MyService();
        await svc.InitializeAsync(this, cancellationToken);
        return svc;
    }
}
```

See the full [MyAsyncPackage.cs](src/MyAsyncPackage.cs) class.

**First of all**, the `PackageRegistration` attribute has the `AllowsBackgroundLoad` parameter set to true to indicate that this package supports background load.

**Secondly** the `ProvideAutoload` attribute has `PackageAutoLoadFlags.BackgroundLoad` set to indicate that the package should be loaded in the background for the specified UI Context.

**And thirdly**, we switch to the UI thread before initializing the *MyCommand* since some service calls still require to run on the UI thread.

## Points of interest

1. Even if switching to the main thread is the first thing you do in the `InitializeAsync` method, the package is still loaded in a way that makes VS more responsive.

2. As a rule of thumb, do as much work as possible before switching to the main thread. 

3. Use the [Visual Studio SDK analyzer NuGet package](https://www.nuget.org/packages/microsoft.visualstudio.sdk.analyzers) to help make sure everything is correctly hooked up.

## Further reading

* [Use AsyncPackage to load VSPackages in the background](https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-use-asyncpackage-to-load-vspackages-in-the-background)
* [How to provide an async Visual Studio service](https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-provide-an-asynchronous-visual-studio-service)
* [Use VisibilityConstraints instead of ProvideAutoload](https://github.com/Microsoft/VSSDK-Extensibility-Samples/tree/master/VisibilityConstraints)