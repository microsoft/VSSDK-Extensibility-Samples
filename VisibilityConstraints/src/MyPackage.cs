using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace VisibilityConstraintsSample
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]       
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid("f76ae46c-c4e7-43cf-ac05-f6fd3cd699f1")]
    // Read more about ProvideUIContextRule and VisibilityConstraints here
    // https://docs.microsoft.com/visualstudio/extensibility/how-to-use-rule-based-ui-context-for-visual-studio-extensions
    [ProvideUIContextRule(_uiContextSupportedFiles,
        name: "Supported Files",
        expression: "CSharp | VisualBasic",
        termNames: new[] { "CSharp", "VisualBasic" },
        termValues: new[] { "HierSingleSelectionName:.cs$", "HierSingleSelectionName:.vb$" })]
    public sealed class MyPackage : AsyncPackage
    {
        private const string _uiContextSupportedFiles = "24551deb-f034-43e9-a279-0e541241687e"; // Must match guid in VsCommandTable.vsct

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // Request any services while on the background thread
            var commandService = await GetServiceAsync((typeof(IMenuCommandService))) as IMenuCommandService;

            // Switch to the main thread before initializing the MyButton command
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Now initialize the MyButton command and pass it the commandService
            MyButton.Initialize(this, commandService);
        }
    }
}
