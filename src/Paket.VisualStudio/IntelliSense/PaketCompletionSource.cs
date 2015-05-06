using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Paket.VisualStudio.Utils;

namespace Paket.VisualStudio.IntelliSense
{
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

            SnapshotPoint? triggerPoint = session.GetTriggerPoint(textBuffer.CurrentSnapshot);
            if (!triggerPoint.HasValue)
                return;

            int position = triggerPoint.Value.Position;
            
            CompletionContext context;
            var completionProviders = CompletionEngine.GetCompletionProviders(session, textBuffer, position, out context).ToList();
            if (completionProviders.Count == 0 || context == null)
                return;

            List<CompletionEntry> list = new List<CompletionEntry>();

            foreach (ICompletionListProvider completionListProvider in completionProviders)
                list.AddRange(completionListProvider.GetCompletionEntries(context));

            list.RemoveDuplicates();
            if (list.Count == 0)
                return;

            ITrackingSpan trackingSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(position <= context.SpanStart || position > context.SpanStart + context.SpanLength ? new Span(position, 0) : new Span(context.SpanStart, context.SpanLength), SpanTrackingMode.EdgeInclusive);
            CompletionSet completionSet = new CompletionSet("PaketCompletion", "Paket", trackingSpan, list, Enumerable.Empty<CompletionEntry>());
            
            completionSets.Add(completionSet);
        }

        public void Dispose()
        {
            disposed = true;
        }
    }
}