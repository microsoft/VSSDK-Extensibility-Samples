using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using OptionsSample.Options;

namespace OptionsSample
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class TextviewCreationListener : IWpfTextViewCreationListener
    {
        // This method is executed when you open any file in the editor window
        public void TextViewCreated(IWpfTextView textView)
        {
            // Call the Instance singleton from the UI thread is easy
            bool showMessage = OtherOptions.Instance.ShowMessage;

            if (showMessage)
            {
                System.Threading.Tasks.Task.Run(async () =>
                {
                    // Make the call to GetLiveInstanceAsync from a background thread to avoid blocking the UI thread
                    GeneralOptions options = await GeneralOptions.GetLiveInstanceAsync();
                    string message = options.Message;
                    // Do something with message
                });
            }
        }
    }
}
