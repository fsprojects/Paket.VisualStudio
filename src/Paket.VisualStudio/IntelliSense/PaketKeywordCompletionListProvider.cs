using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;
using Paket.VisualStudio.IntelliSense.Classifier;

namespace Paket.VisualStudio.IntelliSense
{
    internal class PaketKeywordCompletionListProvider : ICompletionListProvider
    {
        private readonly ImageSource glyph;

        public CompletionContextType ContextType
        {
            get { return CompletionContextType.Keyword; }
        }

        public PaketKeywordCompletionListProvider(IGlyphService glyphService)
        {
            glyph = glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic);
        }

        public IEnumerable<CompletionEntry> GetCompletionEntries(CompletionContext context)
        {
            return PaketClassifier.ValidKeywords.Select(item => new CompletionEntry(item, item, null, glyph, "iconAutomationText"));
        }
    }
}