1. Implement [IAsyncLoadablePackageInitialize](https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.shell.interop.iasyncloadablepackageinitialize?view=visualstudiosdk-2017) interface

2. In the pkgdef for your package, add the following:
```c#
[$RootKey$\Packages\{YOUR PACKAGE GUID}]
"InprocServer32"="Path to your package dll"
@="YOUR PACKAGE NAME"
"AllowsBackgroundLoad"=dword:00000001
```

3. If your package proffers services, and you want to make them async, then in the pkgdef of your service, add the following:
```c#
[$RootKey$\Services\{YOUR SERVICE GUID}]
"Name"="YOUR SERVICE NAME"
@="{YOUR PACKAGE GUID}"
"IsAsyncQueryable"=dword:00000001
```

4. For the autoload entries in your package, add the following:
```c#
[$RootKey$\AutoLoadPackages\$UICONTEXT_GUID}]
"$YOUR_PACKAGE_GUID”= dword:00000002
```

To handle solution events, you can take a look at the guidance here: https://github.com/madskristensen/SolutionLoadSample
