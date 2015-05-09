﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Paket.VisualStudio.IntelliSense.Classifier
{
    internal class PaketClassifier : IClassifier
    {
        private readonly IClassificationType keyword, comment;

        private static readonly HashSet<string> validKeywords = new HashSet<string>
        {
            "source", "nuget", "github", "gist", "http",
            "content", "reference", "redirects"
        };

        public static IEnumerable<string> ValidKeywords
        {
            get { return validKeywords; }
        }

        internal PaketClassifier(IClassificationTypeRegistryService registry)
        {
            keyword = registry.GetClassificationType(PredefinedClassificationTypeNames.Keyword);
            comment = registry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            IList<ClassificationSpan> spans = new List<ClassificationSpan>();

            string text = span.GetText();
            int index = text.IndexOf("#", StringComparison.Ordinal);

            if (index > -1)
            {
                var result = new SnapshotSpan(span.Snapshot, span.Start + index, text.Length - index);
                spans.Add(new ClassificationSpan(result, comment));
            }

            if (index == -1 || index > 0)
            {
                string[] args = text.Split(' ');

                if (args.Length >= 2 && ValidKeywords.Contains(args[0].Trim().ToLowerInvariant()))
                {
                    var result = new SnapshotSpan(span.Snapshot, span.Start, args[0].Length);
                    spans.Add(new ClassificationSpan(result, keyword));
                }
            }

            return spans;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged = delegate { };

        public void OnClassificationChanged(SnapshotSpan span)
        {
            ClassificationChanged(this, new ClassificationChangedEventArgs(span));
        }
    }
}
