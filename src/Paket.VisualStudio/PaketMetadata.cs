namespace Paket.VisualStudio
{
    public class PaketMetadata
    {
        public PaketMetadata(string packageName, string version)
        {
            Id = packageName;
            VersionString = version;
        }

        public string Id { get; private set; }
        public string VersionString { get; private set; }
    }
}