using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Paket.VisualStudio.IntelliSense.Classifier
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = PaketClassificationTypes.Keyword)]
    [Name(PaketClassificationTypes.Keyword)]
    [Order(After = Priority.Default)]
    [UserVisible(true)]
    internal sealed class PaketClassifierFormatDefinition : ClassificationFormatDefinition
    {
        public PaketClassifierFormatDefinition()
        {
            IsBold = true;
            ForegroundColor = Colors.Orange;
            DisplayName = "Paket";
        }
    }
}