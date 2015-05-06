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
using Paket.VisualStudio.Utils;

namespace Paket.VisualStudio.IntelliSense
{
    internal class NuGetNameCompletionListProvider : ICompletionListProvider
    {
        private static IEnumerable<string> searchResults = null;
        
        private static readonly Regex r = new Regex("nuget (?<query>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public CompletionContextType ContextType
        {
            get { return CompletionContextType.NuGet; }
        }

        public IEnumerable<CompletionEntry> GetCompletionEntries(CompletionContext context)
        {
            ImageSource imageSource = GetImageSource();

            if (searchResults != null)
            {
                foreach (var value in searchResults)
                {
                    yield return new CompletionEntry(value, value.AddQuotes(), value, imageSource);
                }

                searchResults = null;
            }
            else
            {

                Action<CompletionEntry> action = entry => {
                    string text = context.Snapshot.GetText(context.SpanStart, context.SpanLength);
                    string searchTerm = r.Match(text).Groups["query"].Value.TrimQuotes();

                    entry.UpdateDisplayText(searchTerm);

                    ExecuteSearch(searchTerm);
                };

                yield return new CompletionEntry("Search remote NuGet packages...", null, null, null, commitAction: action);
            }
        }

        private void ExecuteSearch(string searchTerm)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Task<string[]> task = FSharpAsync.StartAsTask(
                    NuGetV3.FindPackages(FSharpOption<Paket.Utils.Auth>.None, "http://nuget.org/api/v2", searchTerm),
                    new FSharpOption<TaskCreationOptions>(TaskCreationOptions.None),
                    new FSharpOption<CancellationToken>(CancellationToken.None));

                searchResults = task.Result;

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