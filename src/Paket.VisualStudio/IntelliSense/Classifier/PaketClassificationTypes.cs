using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Paket.VisualStudio.IntelliSense.Classifier
{
    internal static class PaketClassificationTypes
    {
        public const string Keyword = "paket_keyword";

        [Export, Name(Keyword)]
        internal static ClassificationTypeDefinition PaketClassificationBold = null;
    }
}
