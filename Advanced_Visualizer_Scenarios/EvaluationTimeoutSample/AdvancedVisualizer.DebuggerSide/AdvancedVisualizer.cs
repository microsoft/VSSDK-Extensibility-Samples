using System.Diagnostics;
using System.Windows;
using CustomObjects;
using Microsoft.VisualStudio.DebuggerVisualizers;

[assembly: DebuggerVisualizer(typeof(AdvancedVisualizer.DebuggerSide.AdvancedVisualizer), typeof(AdvancedVisualizer.DebuggeeSide.CustomVisualizerObjectSource), Target = typeof(VerySlowObject), Description = "Very Slow Object Visualizer")]
namespace AdvancedVisualizer.DebuggerSide
{
    public class AdvancedVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            IAsyncVisualizerObjectProvider asyncObjectProvider = (IAsyncVisualizerObjectProvider)objectProvider;

            if (asyncObjectProvider != null)
            {
                AdvancedVisualizerViewModel viewModel = new AdvancedVisualizerViewModel(asyncObjectProvider);
                Window advancedVisualizerWindow = new VisualizerDialog() { DataContext = viewModel };

                advancedVisualizerWindow.ShowDialog();
            }
        }
    }
}
