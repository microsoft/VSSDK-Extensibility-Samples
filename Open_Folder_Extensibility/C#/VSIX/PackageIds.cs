using System;

namespace OpenFolderExtensibility.VSPackage
{
    public static class PackageIds
    {
        // Guids from VSCT file.
        public static readonly Guid GuidVsPackageCmdSet = new Guid("8F79B0C8-352F-4594-BDF1-793320C049CA");
        public const int WordCountCmdId = 0x0101; // ToDo, look into why Command ID 0x100 creates an extra entry in the context menu based on the display name of the command.
        public const int ToggleWordCountCmdId = 0x0102;

        // Guid to associate file action factories.
        public const string TxtFileContextType = "993E0240-FE75-4909-BADC-4B9AC576B7F9";
    }
}