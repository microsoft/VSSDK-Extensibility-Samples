using System.Windows;

namespace AdvancedVisualizer.DebuggerSide
{
    /// <summary>
    /// Interaction logic for VisualizerDialog.xaml
    /// </summary>
    public partial class VisualizerDialog : Window
    {
        private AdvancedVisualizerViewModel ViewModel => (AdvancedVisualizerViewModel)this.DataContext;

        public VisualizerDialog()
        {
            InitializeComponent();

            this.Loaded += VisualizerLoaded;
        }

        public void VisualizerLoaded(object sender, RoutedEventArgs e)
        {
            _ = Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    string data = await this.ViewModel.GetDataAsync();

                    this.DataLabel.Visibility = Visibility.Visible;
                    this.DataLabel.Content = data;
                }
                catch
                {
                    this.ErrorLabel.Content = "Error getting data.";
                }
                finally
                {
                    this.progressControl.Visibility = Visibility.Collapsed;
                }
            });
        }
    }
}
