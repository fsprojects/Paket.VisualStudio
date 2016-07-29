using System.Collections.Generic;

using Microsoft.VisualStudio.Language.Intellisense;

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
            var feeds = Dependencies.Locate().GetDefinedNuGetFeeds();
            foreach (var feed in feeds)
            {
                yield return new Completion2(feed, feed, null, null, "iconAutomationText");
            }
        }
    }
}