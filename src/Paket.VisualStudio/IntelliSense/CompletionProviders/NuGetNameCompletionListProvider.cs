using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Language.Intellisense;
using Paket.VisualStudio.Utils;

namespace Paket.VisualStudio.IntelliSense.CompletionProviders
{
    internal class NuGetNameCompletionListProvider : ICompletionListProvider
    {
        private static IEnumerable<string> searchResults;

        public CompletionContextType ContextType
        {
            get { return CompletionContextType.NuGet; }
        }

        public IEnumerable<Completion> GetCompletionEntries(CompletionContext context)
        {
            ImageSource imageSource = GetImageSource();

            if (searchResults != null)
            {
                foreach (var value in searchResults)
                {
                    yield return new Completion2(value, value, null, imageSource, "iconAutomationText");
                }

                searchResults = null;
            }
            else
            {
                Action<CompletionEntry> action = entry =>
                {
                    string searchTerm = context.Snapshot.GetText(context.SpanStart, context.SpanLength);

                    entry.UpdateDisplayText(searchTerm);

                    ExecuteSearch(searchTerm);
                };

                yield return new CompletionEntry("Search remote NuGet packages...", null, null, imageSource, commitAction: action);
            }
        }

        private void ExecuteSearch(string searchTerm)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                searchResults =
                    FSharpAsync.RunSynchronously(
                        NuGetV3.FindPackages(FSharpOption<Paket.Utils.Auth>.None, Constants.DefaultNugetStream, searchTerm, 20),
                        FSharpOption<int>.None,
                        FSharpOption<CancellationToken>.None);

                DteHelper.ExecuteCommand("Edit.CompleteWord");
            });
        }

        private static ImageSource GetImageSource()
        {
            BitmapSource source = ImageHelper.BitmapSourceFromBitmap(new Bitmap(Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Paket.VisualStudio.Resources.NuGet.ico"))));
            Int32Rect sourceRect = new Int32Rect(0, 0, 16, 16);
            ImageSource imageSource = new CroppedBitmap(source, sourceRect);
            imageSource.Freeze();
            return imageSource;
        }
    }
}