using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace Paket.VisualStudio.IntelliSense
{
    internal class PaketDocument
    {
        private readonly ITextSnapshot textSnapshot;

        private PaketDocument(ITextSnapshot textSnapshot)
        {
            this.textSnapshot = textSnapshot;
        }

        public static PaketDocument FromTextBuffer(ITextBuffer textBuffer)
        {
            return new PaketDocument(textBuffer.CurrentSnapshot);
        }

        public ITextSnapshotLine GetLineAt(int position)
        {
            return textSnapshot.GetLineFromPosition(position);
        }
    }
}