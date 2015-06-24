using System.ComponentModel.Composition;
using System.Xml.Schema;
using MadsKristensen.EditorExtensions;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Paket.VisualStudio.IntelliSense.Classifier
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Export(typeof(IClassifierProvider))]
    [ContentType("Paket")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class PaketClassifierProvider : IClassifierProvider, IVsTextViewCreationListener
    {
        [Import]
        public IClassificationTypeRegistryService Registry { get; set; }

        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new PaketClassifier(Registry));
        }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextDocument document;

            var view = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);
            if (TextDocumentFactoryService.TryGetTextDocument(view.TextDataModel.DocumentBuffer, out document))
            {
                string filePath = document.FilePath;
                if (!IsPaketFile(filePath)) return;

                PaketClassifier classifier;
                view.TextDataModel.DocumentBuffer.Properties.TryGetProperty(typeof(PaketClassifier), out classifier);
                view.Properties.GetOrCreateSingletonProperty(() => new CommentCommandTarget(textViewAdapter, view, "#"));

                if (classifier != null)
                {
                    ITextSnapshot snapshot = view.TextBuffer.CurrentSnapshot;
                    classifier.OnClassificationChanged(new SnapshotSpan(snapshot, 0, snapshot.Length));
                }
            }
        }

        public static bool IsPaketFile(string filePath)
        {
            return System.IO.Path.GetFileName(filePath).ToLowerInvariant() == "paket.dependencies";
        }
    }
}