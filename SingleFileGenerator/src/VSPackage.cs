using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using Task = System.Threading.Tasks.Task;

namespace SingleFileGeneratorSample
{
    [Guid("2e927fa3-8684-47fc-9674-0046499860d3")] // Must match the GUID in the .vsct file
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("Single File Generator Sample", "", "1.0")]       
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideCodeGenerator(typeof(MinifyCodeGenerator), MinifyCodeGenerator.Name, MinifyCodeGenerator.Description, true)]
    [ProvideUIContextRule("69760bd3-80f0-4901-818d-c4656aaa08e9", // Must match the GUID in the .vsct file
        name: "UI Context",
        expression: "js | css | html", // This will make the button only show on .js, .css and .htm(l) files
        termNames: new[] { "js", "css", "html" },
        termValues: new[] { "HierSingleSelectionName:.js$", "HierSingleSelectionName:.css$", "HierSingleSelectionName:.html?$" })]
    public sealed class VSPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await ApplyCustomTool.InitializeAsync(this);
        }
    }}
