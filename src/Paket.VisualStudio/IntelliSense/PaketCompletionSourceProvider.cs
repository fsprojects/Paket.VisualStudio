using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Paket.VisualStudio.IntelliSense.Classifier;
using Paket.VisualStudio.Utils;
using Intel = Microsoft.VisualStudio.Language.Intellisense;

namespace Paket.VisualStudio.IntelliSense
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("text")]
    [Name("Paket IntelliSense Provider")]
    internal class PaketCompletionSourceProvider : ICompletionSourceProvider
    {
        private readonly IGlyphService glyphService;

        [ImportingConstructor]
        public PaketCompletionSourceProvider(IGlyphService glyphService)
        {
            this.glyphService = glyphService;
        }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            string filename = System.IO.Path.GetFileName(textBuffer.GetFileName());

            if (PaketClassifierProvider.IsPaketFile(filename))
            {
                return new PaketCompletionSource(glyphService, textBuffer);
            }

            return null;
        }
    }

    internal class PaketCompletionSource : ICompletionSource
    {
        private readonly ITextBuffer textBuffer;
        private readonly ImageSource glyph;
        private bool disposed;

        public PaketCompletionSource(IGlyphService glyphService, ITextBuffer textBuffer)
        {
            this.textBuffer = textBuffer;
            glyph = glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic);
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            if (disposed)
                return;

            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            SnapshotPoint? triggerPoint = session.GetTriggerPoint(snapshot);
            if (triggerPoint == null)
                return;

            int position = triggerPoint.Value.Position;

            CompletionContext context;
            var completionProviders = CompletionEngine.GetCompletionProviders(session, textBuffer, position, out context).ToList();
            if (completionProviders.Count == 0 || context == null)
                return;

            var completions = new List<Intel.Completion>();

            foreach (ICompletionListProvider completionListProvider in completionProviders)
                completions.AddRange(completionListProvider.GetCompletionEntries(context));

            if (completions.Count == 0)
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

            ITrackingSpan applicableTo = snapshot.CreateTrackingSpan(new SnapshotSpan(start, triggerPoint.Value), SpanTrackingMode.EdgeInclusive);
            CompletionSet completionSet = new CompletionSet("PaketCompletion", "Paket", applicableTo, completions, Enumerable.Empty<Intel.Completion>());

            completionSets.Add(completionSet);
        }

        public void Dispose()
        {
            disposed = true;
        }
    }
}