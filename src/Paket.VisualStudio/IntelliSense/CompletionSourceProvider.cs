using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Paket.VisualStudio.IntelliSense.Classifier;

namespace Paket.VisualStudio.IntelliSense
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("text")]
    [Name("Paket IntelliSense Provider")]
    internal class CompletionSourceProvider : ICompletionSourceProvider
    {
        private readonly IGlyphService glyphService;

        [ImportingConstructor]
        public CompletionSourceProvider(IGlyphService glyphService)
        {
            this.glyphService = glyphService;
        }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new PaketCompletionSource(glyphService, textBuffer);
        }
    }
}