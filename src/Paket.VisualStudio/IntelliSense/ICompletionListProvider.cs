using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Paket.VisualStudio.IntelliSense
{
    public interface ICompletionListProvider
    {
        CompletionContextType ContextType { get; }
        IEnumerable<CompletionEntry> GetCompletionEntries(CompletionContext context);
    }
}