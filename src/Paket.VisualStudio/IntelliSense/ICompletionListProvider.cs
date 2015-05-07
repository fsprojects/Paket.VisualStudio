using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Paket.VisualStudio.IntelliSense
{
    public interface ICompletionListProvider
    {
        CompletionContextType ContextType { get; }
        IEnumerable<Completion> GetCompletionEntries(CompletionContext context);
    }
}