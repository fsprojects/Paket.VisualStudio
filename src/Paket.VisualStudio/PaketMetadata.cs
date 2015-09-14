namespace Paket.VisualStudio
{
    public class PaketMetadata
    {
        public PaketMetadata(string groupName, string packageName, string version)
        {
            GroupName = groupName;
            Id = packageName;
            VersionString = version;
        }

        public string GroupName { get; private set; }
        public string Id { get; private set; }
        public string VersionString { get; private set; }
    }
}