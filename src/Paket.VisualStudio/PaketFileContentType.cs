using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Paket.VisualStudio
{
    internal static class PaketDependenciesFileContentType
    {
        public const string ContentType = "PaketDependencies";

        [Export]
        [Name(ContentType)]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition PaketDependenciesContentTypeDefinition;

        [Export]
        [FileExtension(".dependencies")]
        [ContentType(ContentType)]
        internal static FileExtensionToContentTypeDefinition PaketDependenciesFileExtensionDefinition;
    }

    internal static class PaketReferencesFileContentType
    {
        public const string ContentType = "PaketReferences";

        [Export]
        [Name(ContentType)]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition PaketReferencesContentTypeDefinition;

        [Export]
        [FileExtension(".references")]
        [ContentType(ContentType)]
        internal static FileExtensionToContentTypeDefinition PaketReferencesFileExtensionDefinition;
    }
}
