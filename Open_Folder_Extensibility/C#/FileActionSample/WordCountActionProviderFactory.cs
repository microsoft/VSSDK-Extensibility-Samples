using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Workspace;
using Microsoft.VisualStudio.Workspace.Extensions.VS;
using Task = System.Threading.Tasks.Task;
using OpenFolderExtensibility.VSPackage;
using OpenFolderExtensibility.SettingsSample;

namespace OpenFolderExtensibility.FileActionSample
{
    [ExportFileContextActionProvider(ProviderType, PackageIds.TxtFileContextType)]
    public class WordCountActionProviderFactory : IWorkspaceProviderFactory<IFileContextActionProvider>
    {
        // Unique Guid for WordCountActionProvider.
        private const string ProviderType = "0DD39C9C-3DE4-4B9C-BE19-7D011341A65B";

        private static readonly Guid ProviderCommandGroup = PackageIds.GuidVsPackageCmdSet;

        public IFileContextActionProvider CreateProvider(IWorkspace workspaceContext)
        {
            return new WordCountActionProvider(workspaceContext);
        }

        internal class WordCountActionProvider : IFileContextActionProvider
        {
            private static readonly Guid ActionOutputWindowPane = new Guid("{35F304B6-2329-4A0C-B9BE-92AFAB7AF858}");
            private IWorkspace workspaceContext;

            internal WordCountActionProvider(IWorkspace workspaceContext)
            {
                this.workspaceContext = workspaceContext;
            }

            public Task<IReadOnlyList<IFileContextAction>> GetActionsAsync(string filePath, FileContext fileContext, CancellationToken cancellationToken)
            {
                return Task.FromResult<IReadOnlyList<IFileContextAction>>(new IFileContextAction[]
                {
                    // Word count command:
                    new MyContextAction(
                        fileContext,
                        new Tuple<Guid, uint>(ProviderCommandGroup, PackageIds.WordCountCmdId),
                        "My Action" + fileContext.DisplayName,
                        async (fCtxt, progress, ct) =>
                        {
                            WordCountSettings settings = WordCountSettings.GetSettings(workspaceContext);
                            string action =
                                settings.CountType == WordCountSettings.WordCountType.WordCount ?
                                    "Word Count" : "Line Count";
                            await OutputWindowPaneAsync(action + " " + fCtxt.Context.ToString());
                        }),

                    // Toggle word count type command:
                    new MyContextAction(
                        fileContext,
                        new Tuple<Guid, uint>(ProviderCommandGroup, PackageIds.ToggleWordCountCmdId),
                        "My Action" + fileContext.DisplayName,
                        async (fCtxt, progress, ct) =>
                        {
                            WordCountSettings settings = WordCountSettings.GetSettings(workspaceContext);
                            settings.CountType =
                                settings.CountType == WordCountSettings.WordCountType.LineCount ?
                                WordCountSettings.WordCountType.WordCount : 
                                WordCountSettings.WordCountType.LineCount;
                            WordCountSettings.StoreSettings(workspaceContext, settings);
                                                            
                            await OutputWindowPaneAsync(
                                settings.CountType == WordCountSettings.WordCountType.WordCount ?
                                    "Counting Words\n" : "Counting Lines\n");
                        }),
                });
            }

            internal static async Task OutputWindowPaneAsync(string message)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                IVsOutputWindowPane outputPane = null;
                var outputWindow = ServiceProvider.GlobalProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                if (outputWindow != null && ErrorHandler.Failed(outputWindow.GetPane(ActionOutputWindowPane, out outputPane)))
                {
                    IVsWindowFrame windowFrame;
                    var vsUiShell = ServiceProvider.GlobalProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
                    uint flags = (uint)__VSFINDTOOLWIN.FTW_fForceCreate;
                    vsUiShell.FindToolWindow(flags, VSConstants.StandardToolWindows.Output, out windowFrame);
                    windowFrame.Show();

                    outputWindow.CreatePane(ActionOutputWindowPane, "Actions", 1, 1);
                    outputWindow.GetPane(ActionOutputWindowPane, out outputPane);
                    outputPane.Activate();
                }

                outputPane?.OutputString(message);
            }

            internal class MyContextAction : IFileContextAction, IVsCommandItem
            {
                private Func<FileContext, IProgress<IFileContextActionProgressUpdate>, CancellationToken, Task> executeAction;

                internal MyContextAction(
                    FileContext fileContext,
                    Tuple<Guid, uint> command,
                    string displayName,
                    Func<FileContext, IProgress<IFileContextActionProgressUpdate>, CancellationToken, Task> executeAction)
                {
                    this.executeAction = executeAction;
                    this.Source = fileContext;
                    this.CommandGroup = command.Item1;
                    this.CommandId = command.Item2;
                    this.DisplayName = displayName;
                }

                public Guid CommandGroup { get; }
                public uint CommandId { get; }
                public string DisplayName { get; }
                public FileContext Source { get; }

                public async Task<IFileContextActionResult> ExecuteAsync(IProgress<IFileContextActionProgressUpdate> progress, CancellationToken cancellationToken)
                {
                    await this.executeAction(this.Source, progress, cancellationToken);
                    return new FileContextActionResult(true);
                }
            }
        }
    }
}
