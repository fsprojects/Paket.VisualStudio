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
    internal static class PaketFileContentType
    {
        public const string ContentType = "Paket";

        [Export]
        [Name(ContentType)]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition PaketContentTypeDefinition;

        [Export]
        [FileExtension(".dependencies")]
        [ContentType(ContentType)]
        internal static FileExtensionToContentTypeDefinition PaketFileExtensionDefinition;
    }
}
