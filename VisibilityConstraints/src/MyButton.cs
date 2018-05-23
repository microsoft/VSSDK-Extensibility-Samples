using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisibilityConstraintsSample
{
    internal sealed class MyButton
    {
        // CommandId must match the MyButtonId specified in the .vsct file
        public const int CommandId = 0x0100;

        // Guid must match the guidMyButtonPackageCmdSet specified in the .vsct file
        public static readonly Guid CommandSet = new Guid("497de1d3-ed31-4519-a864-bbcd992fa57d"); 

        private readonly AsyncPackage _package;

        private MyButton(AsyncPackage package, IMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var cmdID = new CommandID(CommandSet, CommandId);
            var command = new OleMenuCommand(Execute, cmdID)
            {
                // This defers the visibility logic back to the VisibilityConstraints in the .vsct file
                Supported = false
            };

            // The MyQueryStatus method makes the exact same check as the ProvideUIContextRule attribute
            // does on the MyPackage class. When that is the case, there is no need to specify
            // a QueryStatus method and we can set command.Supported=false to defer the logic back 
            // to the VisibilityConstraint in the .vsct file.
             //command.BeforeQueryStatus += MyQueryStatus;

            commandService.AddCommand(command);
        }

        public static MyButton Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider
        {
            get { return _package; }
        }

        public static void Initialize(AsyncPackage package, IMenuCommandService commandService)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Instance = new MyButton(package, commandService);
        }

        private void MyQueryStatus(object sender, EventArgs e)
        {
            var button = (MenuCommand)sender;

            // Make the button invisible by default
            button.Visible = false;

            var dte = ServiceProvider.GetService(typeof(DTE)) as DTE2;
            ProjectItem item = dte.SelectedItems.Item(1)?.ProjectItem;

            if (item != null)
            {
                string fileExtension = Path.GetExtension(item.Name).ToLowerInvariant();
                string[] supportedFiles = new[] { ".cs", ".vb" };

                // Show the button only if a supported file is selected
                button.Visible = supportedFiles.Contains(fileExtension);
            }
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", GetType().FullName);
            string title = nameof(MyButton);

            VsShellUtilities.ShowMessageBox(
                _package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
