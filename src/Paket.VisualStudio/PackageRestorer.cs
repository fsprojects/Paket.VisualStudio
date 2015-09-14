using System;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Paket.VisualStudio.SolutionExplorer;

namespace Paket.VisualStudio
{
    public class PackageRestorer
    {
        private readonly DTE dte;
        private readonly BuildEvents buildEvents;
        private readonly IServiceProvider serviceProvider;

        public PackageRestorer(IServiceProvider serviceProvider)
        {
            dte = (DTE)Package.GetGlobalService(typeof (DTE));
            buildEvents = dte.Events.BuildEvents;
            buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
            this.serviceProvider = serviceProvider;
        }

        private void BuildEvents_OnBuildBegin(
            vsBuildScope scope,
            vsBuildAction action)
        {
            if (action == vsBuildAction.vsBuildActionClean)
                return;

            Restore();
        }

        private void Restore()
        {
            var options = new PaketSettings(new ShellSettingsManager(serviceProvider));
            if (!options.AutoRestore)
                return;

            var dir = SolutionExplorerExtensions.GetSolutionDirectory();
            var dependencies = Paket.Dependencies.Locate(dir);

            var referenceFiles = SolutionExplorerExtensions.GetAllProjectFiles()
                .Select(p => ProjectFile.FindReferencesFile(new FileInfo(p)))
                .Where(FSharpOption<string>.get_IsSome)
                .Select(p => p.Value);

            PaketOutputPane.OutputPane.Activate();
            PaketErrorPane.Clear();
            PaketOutputPane.OutputPane.OutputStringThreadSafe("Restoring packages\r\n");
            StatusBarService.UpdateText("Restoring packages");

            foreach (var referenceFile in referenceFiles)
            {
                try
                {
                    dependencies.Restore(FSharpOption<string>.None, ListModule.OfArray(new[] { referenceFile }));
                }
                catch (Exception ex)
                {
                    PaketErrorPane.ShowError(ex.Message, referenceFile, "paket-restore.html");
                    PaketOutputPane.OutputPane.OutputStringThreadSafe(ex.Message + "\r\n");
                }
            }

            StatusBarService.UpdateText("Ready");
            PaketOutputPane.OutputPane.OutputStringThreadSafe("Ready\r\n");
        }
    }
}
