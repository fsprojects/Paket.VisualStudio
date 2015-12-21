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

    internal static class DependenciesFileCompletionEngine
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
            new SourceCompletionListProvider(),
            new SimpleOptionCompletionListProvider(CompletionContextType.Strategy, "min", "max")
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
            TextExtent endPosition = navigator.GetExtentOfWord(position - 1);
            TextExtent startPosition = endPosition;
            
            // try to extend the span over .
            while (!String.IsNullOrWhiteSpace(paketDocument.GetCharAt(startPosition.Span.Start.Position - 1)))
            {
                startPosition = navigator.GetExtentOfWord(startPosition.Span.Start - 2);
            }

            var startPos = startPosition.Span.Start.Position ;
            var length = endPosition.Span.End.Position - startPos;
            var span = new Span(startPos,length);
            var snapShotSpan = new SnapshotSpan(position.Snapshot, span);

            var context = new CompletionContext(span);

            TextExtent previous = navigator.GetExtentOfWord(startPosition.Span.Start - 1);
            // try to extend the span over blanks
            while (paketDocument.GetCharAt(previous.Span.Start.Position) == " ")
            {
                previous = navigator.GetExtentOfWord(previous.Span.Start - 1);
            }
            var lastWord = previous.Span.GetText();

            switch(lastWord)
            {
                case "nuget": context.ContextType = CompletionContextType.NuGet; break;
                case "source": context.ContextType = CompletionContextType.Source; break;
                case "strategy": context.ContextType = CompletionContextType.Strategy; break;
                default: context.ContextType = CompletionContextType.Keyword; break;
            }

            context.Snapshot = snapShotSpan.Snapshot;
            return context;
        }
    }

    internal static class ReferencesFileCompletionEngine
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
            new InstalledNuGetNameCompletionListProvider(),
            new SourceCompletionListProvider(),
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
            var startPos = position.Position;
            var length = 0;

            if (position.Position > 0)
            {
                TextExtent endPosition = navigator.GetExtentOfWord(position);
                TextExtent startPosition = endPosition;

                // try to extend the span over .
                while (!String.IsNullOrWhiteSpace(paketDocument.GetCharAt(startPosition.Span.Start.Position - 1)))
                {
                    startPosition = navigator.GetExtentOfWord(startPosition.Span.Start - 2);
                }


                startPos = startPosition.Span.Start.Position;
                length = endPosition.Span.End.Position - startPos;
            }

            var span = new Span(startPos, length);
            var snapShotSpan = new SnapshotSpan(position.Snapshot, span);

            var context = new CompletionContext(span);

            context.ContextType = CompletionContextType.InstalledNuGet;
            context.Snapshot = snapShotSpan.Snapshot;
            return context;
        }
    }
}