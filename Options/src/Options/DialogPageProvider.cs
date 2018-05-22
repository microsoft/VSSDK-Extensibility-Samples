namespace OptionsSample.Options
{
    /// <summary>
    /// A provider for custom <see cref="DialogPage" /> implementations.
    /// </summary>
    internal class DialogPageProvider
    {
        public class General : BaseOptionPage<GeneralOptions> { }
        public class Other : BaseOptionPage<OtherOptions> { }
    }
}
