namespace Paket.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Tagging;

    internal sealed class LockFileTagger : ITagger<IOutliningRegionTag>
    {
        private readonly ITextBuffer buffer;
        private readonly List<GroupOutline> groups = new List<GroupOutline>();

        public LockFileTagger(ITextBuffer buffer)
        {
            this.buffer = buffer;
            buffer.Changed += this.OnBufferChanged;
            this.Update();
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var group in this.groups)
            {
                var startLine = this.buffer.CurrentSnapshot.GetLineFromLineNumber(group.StartLine);
                var endLine = this.buffer.CurrentSnapshot.GetLineFromLineNumber(group.EndLine);
                yield return new TagSpan<IOutliningRegionTag>(
                    new SnapshotSpan(startLine.Start, endLine.End),
                    new OutliningRegionTag(false, true, group.Name, null));
            }
        }

        private void OnBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            this.Update();
            this.TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(this.buffer.CurrentSnapshot, 0, this.buffer.CurrentSnapshot.Length)));
        }

        private void Update()
        {
            var ln = 0;
            this.groups.Clear();
            var start = 0;
            string name = null;
            foreach (var line in this.buffer.CurrentSnapshot.Lines)
            {
                var text = line.GetText();
                if (text.StartsWith("GROUP "))
                {
                    if (ln > start)
                    {
                        this.groups.Add(new GroupOutline(name ?? "Main", start, ln - 1));
                    }

                    start = ln;
                    name = text;
                }

                ln++;
            }

            if (start > 0)
            {
                this.groups.Add(new GroupOutline(name, start, ln - 1));
            }
        }
    }
}
