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
            new SimpleOptionCompletionListProvider(CompletionContextType.Strategy, "min", "max"),
            new SimpleOptionCompletionListProvider(CompletionContextType.Framework,
                "auto-detect",
                "net35",
                "net40",
                "net45",
                "net451",
                "net452",
                "net46",
                "net461",
                "net462",
                "net47",
                "netstandard1.0",
                "netstandard1.1",
                "netstandard1.2",
                "netstandard1.3",
                "netstandard1.4",
                "netstandard1.5",
                "netstandard1.6",
                "netstandard2.0",
                "netcoreapp1.0",
                "netcoreapp1.1",
                "netcoreapp2.0",
                "uap",
                "wp7",
                "wp75",
                "wp8",
                "wp81",
                "wpa81"),
            new SimpleOptionCompletionListProvider(CompletionContextType.Version, "<any_paket_version>", "--prefer-nuget"),
            new SimpleOptionCompletionListProvider(CompletionContextType.Storage, "none", "packages", "symlink"),
            new SimpleOptionCompletionListProvider(CompletionContextType.Content, "none", "once"),
            new SimpleOptionCompletionListProvider(CompletionContextType.CopyToOutputDirectory, "always", "never", "preserve_newest"),
            new SimpleOptionCompletionListProvider(CompletionContextType.CopyLocal, "copy_local", "true", "false"),
            new SimpleOptionCompletionListProvider(CompletionContextType.ImportTargets, "import_targets", "true", "false"),
            new SimpleOptionCompletionListProvider(CompletionContextType.DownloadLicense, "download_license", "true", "false"),
            new SimpleOptionCompletionListProvider(CompletionContextType.LowestMatching, "lowest_matching", "true", "false"),
            new SimpleOptionCompletionListProvider(CompletionContextType.GenerateLoadScripts, "generate_load_scripts", "true", "false"),
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

            var startPos = startPosition.Span.Start.Position;
            var length = endPosition.Span.End.Position - startPos;
            var span = new Span(startPos,length);
            var snapShotSpan = new SnapshotSpan(position.Snapshot, span);

            var context = new CompletionContext(span);

            var pos = startPosition.Span.Start;
            if (startPosition.Span.Start.Position > 0)
                pos = startPosition.Span.Start - 1;

            TextExtent previous = navigator.GetExtentOfWord(pos);

            // try to extend the span over blanks
            while (paketDocument.GetCharAt(previous.Span.Start.Position) == " ")
            {
                var pos2 = previous.Span.Start;
                if (previous.Span.Start.Position > 0)
                    pos2 = previous.Span.Start - 1;

                previous = navigator.GetExtentOfWord(pos2);
            }
            var lastWord = previous.Span.GetText();

            switch(lastWord)
            {
                case "nuget": context.ContextType = CompletionContextType.NuGet; break;
                case "source": context.ContextType = CompletionContextType.Source; break;
                case "strategy": context.ContextType = CompletionContextType.Strategy; break;
                case "framework": context.ContextType = CompletionContextType.Framework; break;
                case "version": context.ContextType = CompletionContextType.Version; break;
                case "storage": context.ContextType = CompletionContextType.Storage; break;
                case "content": context.ContextType = CompletionContextType.Content; break;
                case "copy_content_to_output_dir": context.ContextType = CompletionContextType.CopyToOutputDirectory; break;
                case "copy_local": context.ContextType = CompletionContextType.CopyLocal; break;
                case "import_targets": context.ContextType = CompletionContextType.ImportTargets; break;
                case "download_license": context.ContextType = CompletionContextType.DownloadLicense; break;
                case "redirects": context.ContextType = CompletionContextType.Redirects; break;
                case "lowest_matching": context.ContextType = CompletionContextType.LowestMatching; break;
                case "generate_load_scripts": context.ContextType = CompletionContextType.GenerateLoadScripts; break;
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