using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.Shell;
using Paket.VisualStudio.SolutionExplorer;

namespace Paket.VisualStudio
{
    public class PackageRestorer
    {
        private readonly DTE dte;
        private readonly BuildEvents buildEvents;

        public PackageRestorer()
        {
            dte = (DTE)Package.GetGlobalService(typeof (DTE));
            buildEvents = dte.Events.BuildEvents;
            buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
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
            var dir = SolutionExplorerExtensions.GetSolutionDirectory();
            var dependencies = Paket.Dependencies.Locate(dir);

            var referenceFiles = SolutionExplorerExtensions.GetAllProjectFiles()
                .Select(p => ProjectFile.FindReferencesFile(new FileInfo(p)))
                .Where(FSharpOption<string>.get_IsSome)
                .Select(p => p.Value);

            dependencies.Restore(FSharpOption<string>.None, ListModule.OfSeq(referenceFiles));
        }
    }
}
