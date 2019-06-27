using CodeLensOopProviderShared;
using Microsoft.VisualStudio.Shell;
using System.Windows.Controls;
using System.Windows.Input;

namespace CodeLensOopProviderVsix
{
    /// <summary>
    /// Interaction logic for GitCommitDetails.xaml
    /// </summary>
    public partial class GitCommitDetails : UserControl
    {
        public GitCommitDetails()
        {
            InitializeComponent();
        }

        private void CommitDescription_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (sender is TextBlock textBlock && textBlock.DataContext is GitCommitCustomDetailsData commitData)
            {
                CodeLensOopProviderPackage.NavigateToCommit(commitData.CommitSha, ServiceProvider.GlobalProvider);
                e.Handled = true;
            }
        }
    }
}
