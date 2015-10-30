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
    internal class SourceCompletionListProvider : ICompletionListProvider
    {

        public CompletionContextType ContextType
        {
            get { return CompletionContextType.Source; }
        }

        public IEnumerable<Completion> GetCompletionEntries(CompletionContext context)
        {
            var feeds = Paket.Dependencies.Locate().GetDefinedNuGetFeeds();
            foreach (var feed in feeds)
            {
                yield return new Completion(feed, feed, null, null, "iconAutomationText");
            }
        }
    }
}