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

            var group = "Main";

            try
            {
                var text = context.Snapshot.GetText().Substring(0, context.SpanStart + context.SpanLength);
                var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = lines.Length - 1; i >= 0; i--)
                {
                    var line = lines[i].Trim();
                    if (line.StartsWith("group"))
                        group = line.Replace("group", "").Trim();
                }
            }
            catch (Exception)
            {

            }


            var searchResults =
                Dependencies.Locate(context.Snapshot.TextBuffer.GetFileName())
                    .GetInstalledPackages()
                    .Where(x => Paket.Domain.GroupName(x.Item1).Equals(Paket.Domain.GroupName(group)))
                    .Select(x => x.Item2)
                    .ToArray();

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