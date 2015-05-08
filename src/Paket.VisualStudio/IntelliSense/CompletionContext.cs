using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Paket.VisualStudio.IntelliSense
{
    public class CompletionContext
    {
        public int SpanStart { get; private set; }
        public int SpanLength { get; private set; }
        public ITextSnapshot Snapshot { get; set; }
        public IIntellisenseSession Session { get; set; }
        public CompletionContextType ContextType { get; set; }

        public CompletionContext(Span span)
        {
            SpanStart = span.Start;
            SpanLength = span.Length;
        }

        public string GetSearchTerm()
        {
            return Snapshot.GetText(SpanStart, SpanLength);
        }
    }
}