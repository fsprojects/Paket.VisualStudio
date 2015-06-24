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
        [Export]
        [Name("Paket")]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition PaketContentTypeDefinition;

        [Export]
        [FileExtension(".dependencies")]
        [ContentType("Paket")]
        internal static FileExtensionToContentTypeDefinition PaketFileExtensionDefinition;
    }
}
