using System;
using System.Collections.Generic;

namespace Paket.VisualStudio.Restore
{
    public class ErrorReportRestorer : IPackageRestorer
    {
        private readonly IPackageRestorer restorer;

        public ErrorReportRestorer(IPackageRestorer restorer)
        {
            if (restorer == null)
                throw new ArgumentNullException("restorer");

            this.restorer = restorer;
        }

        public void Restore(IEnumerable<RestoringProject> projects)
        {
            try
            {
                restorer.Restore(projects);
            }
            catch (Exception ex)
            {
                PaketErrorPane.ShowError(ex.Message, "Paket restore error !", "paket-restore.html");
                PaketOutputPane.OutputPane.OutputStringThreadSafe(ex.Message + "\r\n");
            }
        }
    }
}