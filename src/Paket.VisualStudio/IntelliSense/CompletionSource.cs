using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Paket.VisualStudio.IntelliSense.Classifier;

namespace Paket.VisualStudio.IntelliSense
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("text")]
    [Name("Paket IntelliSense Provider")]
    internal class CompletionSourceProvider : ICompletionSourceProvider
    {
        private readonly IGlyphService glyphService;

        [ImportingConstructor]
        public CompletionSourceProvider(IGlyphService glyphService)
        {
            this.glyphService = glyphService;
        }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new PaketCompletionSource(glyphService, textBuffer);
        }
    }

    internal class PaketCompletionSource : ICompletionSource
    {
        private readonly ITextBuffer buffer;
        private readonly ImageSource glyph;
        private bool disposed;

        public PaketCompletionSource(IGlyphService glyphService, ITextBuffer buffer)
        {
            this.buffer = buffer;
            glyph = glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic);
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            if (disposed)
                return;

            List<Completion> completions = PaketClassifier.Keywords.Select(item => new Completion(item, item, null, glyph, item)).ToList();

            ITextSnapshot snapshot = buffer.CurrentSnapshot;
            var triggerPoint = session.GetTriggerPoint(snapshot);

            if (triggerPoint == null)
                return;

            var line = triggerPoint.Value.GetContainingLine();
            string text = line.GetText();
            int index = text.IndexOf(' ');
            int hash = text.IndexOf('#');
            SnapshotPoint start = triggerPoint.Value;

            if (hash > -1 && hash < triggerPoint.Value.Position || (index > -1 && (start - line.Start.Position) > index))
                return;

            while (start > line.Start && !char.IsWhiteSpace((start - 1).GetChar()))
            {
                start -= 1;
            }

            var applicableTo = snapshot.CreateTrackingSpan(new SnapshotSpan(start, triggerPoint.Value), SpanTrackingMode.EdgeInclusive);

            completionSets.Add(new CompletionSet("All", "All", applicableTo, completions, Enumerable.Empty<Completion>()));
        }

        public void Dispose()
        {
            disposed = true;
        }
    }
}