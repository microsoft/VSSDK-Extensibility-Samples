using System.ComponentModel;

namespace OptionsSample.Options
{
    internal class GeneralOptions : BaseOptionModel<GeneralOptions>
    {
        [Category("My category")]
        [DisplayName("Message box text")]
        [Description("Specifies the text to show in the message box")]
        [DefaultValue("My message")]
        public string Message { get; set; } = "My message";
    }
}
