using System.Collections.Generic;
using Paket.VisualStudio.Utils;
using Paket.VisualStudio.SolutionExplorer;

namespace Paket.VisualStudio.Restore
{
    public class PaketRestorer : IPackageRestorer
    {
        public void Restore(IEnumerable<RestoringProject> project)
        {
            string PaketSubCommand = "restore";
            foreach (RestoringProject p in project)
                PaketSubCommand += $" --references-file {p.ReferenceFile} ";

            try
            {
                PaketLauncher.LaunchPaket(SolutionExplorerExtensions.GetPaketDirectory(), PaketSubCommand,
                                        (send, args) => PaketOutputPane.OutputPane.OutputStringThreadSafe(args.Data + "\n"));
            }
            catch (System.Exception ex)
            {
                /* One of the known reasons for this block to get executed is that if the paket.exe is old then it is likely
                 * that --references-file is not supported and --references-files is supported instead. paket-4.8.4 for instance
                 */
                PaketOutputPane.OutputPane.OutputStringThreadSafe("Seems like you are using an older version of paket.exe. Trying restore with --references-files\n");
                PaketSubCommand = "restore --references-files";
                foreach (RestoringProject p in project)
                    PaketSubCommand += $" {p.ReferenceFile} ";
                PaketLauncher.LaunchPaket(SolutionExplorerExtensions.GetPaketDirectory(), PaketSubCommand,
                                        (send, args) => PaketOutputPane.OutputPane.OutputStringThreadSafe(args.Data + "\n"));
            }
        }
    }
}