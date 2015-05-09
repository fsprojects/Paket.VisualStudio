using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Paket.VisualStudio.IntelliSense.Classifier;
using Intel = Microsoft.VisualStudio.Language.Intellisense;

namespace Paket.VisualStudio.IntelliSense.CompletionProviders
{
    internal class PaketKeywordCompletionListProvider : ICompletionListProvider
    {
        private readonly ImageSource glyph;

        public CompletionContextType ContextType
        {
            get { return CompletionContextType.Keyword; }
        }

        public PaketKeywordCompletionListProvider(Intel.IGlyphService glyphService)
        {
            glyph = glyphService.GetGlyph(Intel.StandardGlyphGroup.GlyphGroupVariable, Intel.StandardGlyphItem.GlyphItemPublic);
        }

        public IEnumerable<Intel.Completion> GetCompletionEntries(CompletionContext context)
        {
            return PaketClassifier.ValidKeywords.OrderBy(x => x).Select(item => new Intel.Completion2(item, item, null, glyph, "iconAutomationText"));
        }
    }
}