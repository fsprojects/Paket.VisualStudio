using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Paket.VisualStudio.IntelliSense.CompletionProviders;

namespace Paket.VisualStudio.IntelliSense
{
    internal static class Globals
    {
        public static T GetGlobalService<T>(Type type = null) where T : class
        {
            return Package.GetGlobalService(type ?? typeof(T)) as T;
        }
    }

    internal static class CompletionEngine
    {


        private static ExportProvider ExportProvider
        {
            get
            {
                IComponentModel globalService = Globals.GetGlobalService<IComponentModel>(typeof(SComponentModel));
                return globalService.DefaultExportProvider;
            }
        }

        private static readonly HashSet<ICompletionListProvider> completionProviders = new HashSet<ICompletionListProvider>
        {
            new PaketKeywordCompletionListProvider(ExportProvider.GetExport<IGlyphService>().Value),
            new NuGetNameCompletionListProvider(),
        };

        public static IEnumerable<ICompletionListProvider> GetCompletionProviders(IIntellisenseSession session, ITextBuffer textBuffer, SnapshotPoint position, ITextStructureNavigator navigator, out CompletionContext context)
        {
            IEnumerable<ICompletionListProvider> providers = GetCompletionProviders(PaketDocument.FromTextBuffer(textBuffer), navigator, position, out context);
            if (context == null)
            {
                return providers;
            }

            if (context.Snapshot == null)
                context.Snapshot = textBuffer.CurrentSnapshot;
            if (context.Session != null)
                return providers;

            context.Session = session;
            return providers;
        }

        private static IEnumerable<ICompletionListProvider> GetCompletionProviders(PaketDocument paketDocument, ITextStructureNavigator navigator, SnapshotPoint position, out CompletionContext context)
        {
            context = GetCompletionContext(paketDocument, navigator, position);
            return GetCompletionProviders(context.ContextType);
        }

        private static IEnumerable<ICompletionListProvider> GetCompletionProviders(CompletionContextType contextType)
        {
            return completionProviders.Where(provider => provider.ContextType == contextType);
        }

        private static CompletionContext GetCompletionContext(PaketDocument paketDocument, ITextStructureNavigator navigator, SnapshotPoint position)
        {
            TextExtent wordUnderCaret = navigator.GetExtentOfWord(position - 1);
            TextExtent wordUnderCaret2 = navigator.GetExtentOfWord(position - 1);
            while (paketDocument.GetCharAt(wordUnderCaret2.Span.Start.Position - 1) == ".")
            {
                wordUnderCaret2 = navigator.GetExtentOfWord(wordUnderCaret2.Span.Start - 2);
            }
            var startPos = wordUnderCaret2.Span.Start.Position ;
            var length = wordUnderCaret.Span.End.Position - startPos;
            var span = new Span(startPos,length);
            var snapShotSpan = new SnapshotSpan(position.Snapshot, span);

            var context = new CompletionContext(span);

            if (paketDocument.GetLineAt(position).GetText().StartsWith("nuget "))
                context.ContextType = CompletionContextType.NuGet;
            else
                context.ContextType = CompletionContextType.Keyword;

            context.Snapshot = snapShotSpan.Snapshot;
            return context;
        }
    }
}