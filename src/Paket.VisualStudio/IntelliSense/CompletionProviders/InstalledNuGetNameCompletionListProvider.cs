using System;
using System.Collections.Generic;
using System.Linq;
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
    internal class InstalledNuGetNameCompletionListProvider : ICompletionListProvider
    {

        public CompletionContextType ContextType
        {
            get { return CompletionContextType.InstalledNuGet; }
        }

        public IEnumerable<Completion> GetCompletionEntries(CompletionContext context)
        {
            ImageSource imageSource = GetImageSource();

            var searchResults =
                Paket.Dependencies.Locate(context.Snapshot.TextBuffer.GetFileName())
                    .GetInstalledPackages().Select(x => x.Item1);

            foreach (var value in searchResults)
            {
                yield return new Completion2(value, value, null, imageSource, "iconAutomationText");
            }
        }

        private void ExecuteSearch(CompletionContext context, string searchTerm)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                
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