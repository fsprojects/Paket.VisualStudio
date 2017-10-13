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

            PaketLauncher.LaunchPaket(SolutionExplorerExtensions.GetSolutionDirectory(), PaketSubCommand,
                                    (send, args) => PaketOutputPane.OutputPane.OutputStringThreadSafe(args.Data + "\n"));
        }
    }
}