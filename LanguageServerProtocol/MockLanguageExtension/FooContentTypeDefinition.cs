using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace MockLanguageExtension
{
    public class FooContentDefinition
    {
        [Export]
        [Name("foo")]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static ContentTypeDefinition FooContentTypeDefinition;


        [Export]
        [FileExtension(".foo")]
        [ContentType("foo")]
        internal static FileExtensionToContentTypeDefinition FooFileExtensionDefinition;
    }
}
