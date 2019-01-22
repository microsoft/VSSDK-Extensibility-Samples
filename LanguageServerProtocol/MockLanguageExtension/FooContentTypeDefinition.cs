using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace MockLanguageExtension
{
    public class FooContentDefinition
    {
        [Export]
        [Name("codestream")]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static ContentTypeDefinition FooContentTypeDefinition;


        [Export]
        [FileExtension(".foo")]
        [ContentType("codestream")]
        internal static FileExtensionToContentTypeDefinition FooFileExtensionDefinition;

        [Export]
        [FileExtension(".bar")]
        [ContentType("codestream")]
        internal static FileExtensionToContentTypeDefinition BarFileExtensionDefinition;

        [Export]
        [FileExtension(".xml")]
        [ContentType("codestream")]
        internal static FileExtensionToContentTypeDefinition XmlFileExtensionDefinition;

        [Export]
        [FileExtension(".md")]
        [ContentType("codestream")]
        internal static FileExtensionToContentTypeDefinition MdFileExtensionDefinition;
    }
}
