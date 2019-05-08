using System.ComponentModel;

namespace OptionsSample.Options
{
    internal class OtherOptions : BaseOptionModel<OtherOptions>
    {
        [Category("A category")]
        [DisplayName("Show message")]
        [Description("The description of the property")]
        [DefaultValue(true)]
        public bool ShowMessage { get; set; } = true;

        [Category("Another category")]
        [DisplayName("Favorite clothing")]
        [Description("The description of the property")]
        [DefaultValue(Clothing.Pants)]
        [TypeConverter(typeof(EnumConverter))] // This will make use of enums more resilient
        public Clothing ClothingChoice { get; set; } = Clothing.Pants;

        [Category("My category")]
        [DisplayName("This is a boolean")]
        [Description("The description of the property")]
        [DefaultValue(true)]
        [Browsable(false)] // This will hide it from the Tools -> Options page, but still work like normal
        public bool HiddenProperty { get; set; } = true;
    }

    public enum Clothing
    {
        Pants,
        Sweater,
        Shoes
    }
}
