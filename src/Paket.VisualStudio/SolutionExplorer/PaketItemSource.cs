using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;

namespace Paket.VisualStudio.SolutionExplorer
{
    internal class PaketItemSource : IAttachedCollectionSource
    {
        private readonly IVsHierarchyItem paketFile;
        private BulkObservableCollection<PaketItem> paketItems;

        public PaketItemSource(IVsHierarchyItem paketFile)
        {
            this.paketFile = paketFile;
        }

        public object SourceItem
        {
            get { return paketFile; }
        }

        public bool HasItems
        {
            get
            {
                string paketDependenciesFile = paketFile.CanonicalName;

                return DependenciesFile.ReadFromFile(paketDependenciesFile)
                                       .Packages.Any();
            }
        }

        public IEnumerable Items
        {
            get
            {
                if (paketItems == null)
                {
                    paketItems = new BulkObservableCollection<PaketItem>();
                    paketItems.AddRange(GetPaketItems().ToList());
                }

                return paketItems;
            }
        }

        private IEnumerable<PaketItem> GetPaketItems()
        {
            string paketDependenciesFile = paketFile.CanonicalName;

            return DependenciesFile.ReadFromFile(paketDependenciesFile)
                                   .Packages
                                   .Select(d => new PaketItem(d));
        }
    }
}
