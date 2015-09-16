using System;
using System.Collections.Generic;

namespace Paket.VisualStudio.Restore
{
    public class OutputPaneRestorer : IPackageRestorer
    {
        private readonly IPackageRestorer restorer;

        public OutputPaneRestorer(IPackageRestorer restorer)
        {
            if (restorer == null)
                throw new ArgumentNullException("restorer");

            this.restorer = restorer;
        }

        public void Restore(Dependencies dependencies, IEnumerable<RestoringProject> projects)
        {
            PaketErrorPane.Clear();
            PaketOutputPane.OutputPane.Activate();
            PaketOutputPane.OutputPane.OutputStringThreadSafe("Restoring packages\r\n");

            try
            {
                restorer.Restore(dependencies, projects);
            }
            finally
            {
                PaketOutputPane.OutputPane.OutputStringThreadSafe("Ready\r\n");
            }
        }
    }
}