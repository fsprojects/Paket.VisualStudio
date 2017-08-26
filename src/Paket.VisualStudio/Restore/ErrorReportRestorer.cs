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

        public void Restore(Dependencies dependencies, IEnumerable<RestoringProject> projects)
        {
            if (projects == null)
            {
                try
                {
                    restorer.Restore(dependencies, null);
                }
                catch (Exception ex)
                {
                    PaketErrorPane.ShowError(ex.Message, dependencies.GetLockFile().FileName, "paket-restore.html");
                    PaketOutputPane.OutputPane.OutputStringThreadSafe(ex.Message + "\r\n");
                }
            }
            else
            {
                foreach (var project in projects)
                {
                    try
                    {
                        restorer.Restore(dependencies, new[] { project });
                    }
                    catch (Exception ex)
                    {
                        PaketErrorPane.ShowError(ex.Message, project.ReferenceFile, "paket-restore.html");
                        PaketOutputPane.OutputPane.OutputStringThreadSafe(ex.Message + "\r\n");
                    }
                }
            }
        }
    }
}