using System.ComponentModel.Composition;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;

namespace Paket.VisualStudio.SolutionExplorer
{
    [Export(typeof(IAttachedCollectionSourceProvider))]
    [Name("PaketItemProvider")]
    [Order(Before = HierarchyItemsProviderNames.Contains)]
    public class PaketItemProvider : AttachedCollectionSourceProvider<IVsHierarchyItem>
    {
        protected override IAttachedCollectionSource CreateCollectionSource(IVsHierarchyItem item, string relationshipName)
        {
            if (item != null && relationshipName == KnownRelationships.Contains)
            {
                if (item.Text.Equals("paket.dependencies") || item.Text.Equals("paket.references"))
                {
                    return new PaketItemSource(item);
                }
            }

            return null;
        }
    }
}
