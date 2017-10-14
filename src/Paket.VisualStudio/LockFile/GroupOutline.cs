namespace Paket.VisualStudio
{
    public struct GroupOutline
    {
        public GroupOutline(string name, int startLine, int endLine)
        {
            this.Name = name;
            this.StartLine = startLine;
            this.EndLine = endLine;
        }

        public string Name { get; }

        public int StartLine { get; }

        public int EndLine { get; }
    }
}
