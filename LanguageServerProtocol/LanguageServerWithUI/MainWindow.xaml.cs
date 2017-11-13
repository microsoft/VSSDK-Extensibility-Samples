using System.Windows;

namespace LanguageServerWithUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel viewModel = new MainWindowViewModel();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void OnAddButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.Tags.Add(new DiagnosticTag());
        }

        private void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.Tags.Clear();
        }

        private void OnSendDiagnosticsButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.SendDiagnostics();
        }

        private void OnShowMessageButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.SendMessage();
        }

        private void OnShowMessageRequestButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.SendMessageRequest();
        }

        private void OnLogMessageButtonClick(object sender, RoutedEventArgs e)
        {
            viewModel.SendLogMessage();
        }
    }
}
