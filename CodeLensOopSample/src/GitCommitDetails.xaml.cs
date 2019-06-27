using CodeLensOopProviderShared;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            }
        }
    }
}
