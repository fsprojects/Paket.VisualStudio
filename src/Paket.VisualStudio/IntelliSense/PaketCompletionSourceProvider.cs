using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using Paket.VisualStudio.IntelliSense.Classifier;
using Paket.VisualStudio.Utils;

namespace Paket.VisualStudio.IntelliSense
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType(PaketFileContentType.ContentType)]
    [Name("Paket IntelliSense Provider")]
    internal class PaketCompletionSourceProvider : ICompletionSourceProvider
    {
        private readonly IGlyphService glyphService;

        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService;

        [ImportingConstructor]
        public PaketCompletionSourceProvider(IGlyphService glyphService)
        {
            this.glyphService = glyphService;
        }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            string filename = System.IO.Path.GetFileName(textBuffer.GetFileName());

            if (PaketClassifierProvider.IsPaketDependenciesFile(filename))
            {
                return new PaketDependenciesFileCompletionSource(glyphService, textBuffer, NavigatorService.GetTextStructureNavigator(textBuffer));
            }

            return null;
        }
    }

    internal class PaketDependenciesFileCompletionSource : ICompletionSource
    {
        private readonly ITextBuffer textBuffer;
        private readonly ITextStructureNavigator navigator;
        private readonly ImageSource glyph;
        private bool disposed;

        public PaketDependenciesFileCompletionSource(IGlyphService glyphService, ITextBuffer textBuffer, ITextStructureNavigator navigator)
        {
            this.textBuffer = textBuffer;
            this.navigator = navigator;
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
            var completionProviders = CompletionEngine.GetCompletionProviders(session, textBuffer, triggerPoint.Value, navigator, out context).ToList();
            if (completionProviders.Count == 0 || context == null)
                return;

            var completions = new List<Completion>();

            foreach (ICompletionListProvider completionListProvider in completionProviders)
                completions.AddRange(completionListProvider.GetCompletionEntries(context));

            if (completions.Count == 0)
                return;

            ITrackingSpan trackingSpan =
                textBuffer.CurrentSnapshot.CreateTrackingSpan(
                    position <= context.SpanStart || position > context.SpanStart + context.SpanLength
                        ? new Span(position, 0)
                        : new Span(context.SpanStart, context.SpanLength), SpanTrackingMode.EdgeInclusive);

            CompletionSet completionSet = new CompletionSet("PaketDependenciesFileCompletion", "Paket", trackingSpan, completions, Enumerable.Empty<Completion>());

            completionSets.Add(completionSet);
        }

        public void Dispose()
        {
            disposed = true;
        }
    }
}