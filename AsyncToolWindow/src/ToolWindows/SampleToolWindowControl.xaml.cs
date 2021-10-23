using System.Windows;
using System.Windows.Controls;

namespace AsyncToolWindowSample.ToolWindows
{
    public partial class SampleToolWindowControl : UserControl
    {
        private SampleToolWindowState _state;

        public SampleToolWindowControl(SampleToolWindowState state)
        {
            _state = state;

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string version = _state.DTE.FullName;

            MessageBox.Show($"Visual Studio is located here: '{version}'");
        }
    }
}
