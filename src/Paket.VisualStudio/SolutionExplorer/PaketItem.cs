using System;
using System.ComponentModel;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Imaging;

namespace Paket.VisualStudio.SolutionExplorer
{
    internal partial class PaketItem : BaseItem
    {
        public override event PropertyChangedEventHandler PropertyChanged;

        private readonly Requirements.PackageRequirement req;

        public PaketItem(Requirements.PackageRequirement req)
            : base(req.Name.ToString())
        {
            this.req = req;
        }

        public override object GetBrowseObject()
        {
            return new BrowseObject(this);
        }

        public override ImageMoniker IconMoniker
        {
            get
            {
                return new ImageMoniker
                {
                    Id = KnownImageIds.NuGet,
                    Guid = KnownImageIds.ImageCatalogGuid
                };
            }
        }
    }
}
