using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace AsyncPackageMigration
{
    public class MyCommand
    {
        private static readonly Guid _commandSet = Guid.Parse("9cc1062b-4c82-46d2-adcb-f5c17d55fb85");
        private const int _commandId = 0x0100;

        // Asynchronous initialization
        public static async Task InitializeAsync(AsyncPackage package, EnvDTE.DTE dte)
        {
            var commandService = (IMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));

            var cmdId = new CommandID(_commandSet, _commandId);
            var cmd = new MenuCommand((s, e) => Execute(package, dte), cmdId);
            commandService.AddCommand(cmd);
        }

        // Synchronous initialization
        public static void Initialize(Package package, EnvDTE.DTE dte)
        {
            var serviceProvider = (IServiceProvider)package;
            var commandService = (IMenuCommandService)serviceProvider.GetService(typeof(IMenuCommandService));

            var cmdId = new CommandID(_commandSet, _commandId);
            var cmd = new MenuCommand((s, e) => Execute(package, dte), cmdId);
            commandService.AddCommand(cmd);
        }

        private static void Execute(Package package, EnvDTE.DTE dte)
        {
            System.Windows.Forms.MessageBox.Show(dte.FullName);
        }
    }
}
