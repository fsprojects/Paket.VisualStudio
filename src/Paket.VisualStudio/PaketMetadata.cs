using System.Collections.Generic;
using NuGet;
using NuGet.VisualStudio;

namespace Paket.VisualStudio
{
    public class PaketMetadata : IVsPackageMetadata
    {
        public PaketMetadata(string id, VersionRequirement versionRequirement)
        {
            Id = id;
            VersionString = versionRequirement.ToString();
        }

        public string Id { get; private set; }
        public SemanticVersion Version { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public IEnumerable<string> Authors { get; private set; }
        public string InstallPath { get; private set; }
        public string VersionString { get; private set; }
    }
}