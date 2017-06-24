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
            PaketOutputPane.OutputPane.OutputStringThreadSafe("Restoring packages\r\n");

            try
            {
                using (var loggingSub = Paket.Logging.@event.Publish.Subscribe(trace =>
                {
                    PaketOutputPane.OutputPane.OutputStringThreadSafe(
                        "Paket: "+ trace.Text + (trace.NewLine ? "\r\n" : ""));
                }))
                {
                    restorer.Restore(dependencies, projects);
                }
            }
            finally
            {
                PaketOutputPane.OutputPane.OutputStringThreadSafe("Ready\r\n");
            }
        }
    }
}