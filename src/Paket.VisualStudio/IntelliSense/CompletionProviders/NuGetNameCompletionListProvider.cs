using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Paket.VisualStudio.IntelliSense.CompletionProviders
{
    internal class NuGetNameCompletionListProvider : ICompletionListProvider
    {
        public CompletionContextType ContextType
        {
            get { return CompletionContextType.NuGet; }
        }

        public IEnumerable<Completion> GetCompletionEntries(CompletionContext context)
        {
            ImageSource imageSource = GetImageSource();

            string searchTerm = context.Snapshot.GetText(context.SpanStart, context.SpanLength);

            var searchResults =
                FSharpAsync.RunSynchronously(
                    NuGetV3.FindPackages(FSharpOption<Paket.PackageSources.NugetSourceAuthentication>.None, Constants.DefaultNuGetStream, searchTerm, 20),
                    FSharpOption<int>.None,
                    FSharpOption<CancellationToken>.None);
            if (searchResults.IsOk)
            {
                foreach (var value in searchResults.ResultValue)
                {
                    yield return new Completion2(value, value, null, imageSource, "iconAutomationText");
                }
            }
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