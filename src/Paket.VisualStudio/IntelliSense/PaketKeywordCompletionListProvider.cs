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

        public IEnumerable<Completion> GetCompletionEntries(CompletionContext context)
        {
            return PaketClassifier.Keywords.Select(item => new Completion2(item, item, null, glyph, item));
        }
    }
}