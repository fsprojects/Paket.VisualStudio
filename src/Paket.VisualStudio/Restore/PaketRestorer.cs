using System;
using System.Collections.Generic;
using Paket.VisualStudio.SolutionExplorer;
using Paket.VisualStudio.Utils;

namespace Paket.VisualStudio.Restore
{
    public class PaketRestorer : IPackageRestorer
    {
        private const int CommandLineLenght = 7000;

        public void Restore(IEnumerable<RestoringProject> project)
        {
            var limitSubCommand = LimitSubCommand(project, p => $" --references-file \"{p.ReferenceFile}\" ");
            try
            {
                foreach (var s in limitSubCommand)
                    PaketLauncher.LaunchPaket(SolutionExplorerExtensions.GetPaketDirectory(), "restore" + s,
                        (send, args) => PaketOutputPane.OutputPane.OutputStringThreadSafe(args.Data + "\n"));
            }
            catch (PaketRuntimeException ex)
            {
                /* One of the known reasons for this block to get executed is that if the paket.exe is old then it is likely
                 * that --references-file is not supported and --references-files is supported instead. paket-4.8.4 for instance
                 */
                PaketOutputPane.OutputPane.OutputStringThreadSafe(
                    "Seems like you are using an older version of paket.exe. Trying restore with --references-files\n");

                var limitedReferenceFiles = LimitSubCommand(project, p => $" {p.ReferenceFile} ");

                foreach (var limitedReferenceFile in limitedReferenceFiles)
                    PaketLauncher.LaunchPaket(SolutionExplorerExtensions.GetPaketDirectory(),
                        "restore --references-files" + limitedReferenceFile,
                        (send, args) => PaketOutputPane.OutputPane.OutputStringThreadSafe(args.Data + "\n"));
            }
        }

        private static List<string> LimitSubCommand(IEnumerable<RestoringProject> project,
            Func<RestoringProject, string> commandResolver)
        {
            var subCommands = new List<string>();
            var subCommand = string.Empty;
            foreach (var p in project)
            {
                var commandToAdd = commandResolver(p);

                if (subCommand.Length + commandToAdd.Length > CommandLineLenght)
                {
                    subCommands.Add(subCommand);
                    subCommand = commandToAdd;
                }
                else
                {
                    subCommand += commandToAdd;
                }
            }

            return subCommands;
        }
    }
}
