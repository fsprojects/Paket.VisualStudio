using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace Paket.VisualStudio.SolutionExplorer
{
    internal sealed partial class PaketItem
    {
        private class BrowseObject : LocalizableProperties
        {
            private readonly PaketItem paketItem;

            public BrowseObject(PaketItem paketItem)
            {
                this.paketItem = paketItem;
            }
            public override string GetClassName()
            {
                return "Package Properties";
            }

            [DisplayName("ID")]
            public string Id
            {
                get { return paketItem.req.Name.Id; }
            }

            [DisplayName("Title")]
            public string Title
            {
                get { return paketItem.req.Name.ToString(); }
            }
        }
    }
}
