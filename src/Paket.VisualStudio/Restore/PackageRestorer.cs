using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.Shell;
using Paket.VisualStudio.SolutionExplorer;

namespace Paket.VisualStudio.Restore
{
    public class PackageRestorer
    {
        private readonly IPackageRestorer restorer;
        private readonly DTE dte;
        private readonly BuildEvents buildEvents;

        public PackageRestorer(IPackageRestorer restorer)
        {
            dte = (DTE)Package.GetGlobalService(typeof(DTE));
            buildEvents = dte.Events.BuildEvents;
            buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
            this.restorer = restorer;
        }

        private void BuildEvents_OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            if (action == vsBuildAction.vsBuildActionClean)
                return;

            Restore();
        }

        private void Restore()
        {
            var dir = SolutionExplorerExtensions.GetSolutionDirectory();
            var dependencies = Dependencies.Locate(dir);

            var projects = SolutionExplorerExtensions.GetAllProjects()
                .Select(p => new { ProjectName = p.Name, ReferenceFile = ProjectFile.FindReferencesFile(new FileInfo(p.FullName)) })
                .Where(p => FSharpOption<string>.get_IsSome(p.ReferenceFile))
                .Select(p => new RestoringProject(p.ProjectName, p.ReferenceFile.Value))
                .ToList();

            restorer.Restore(projects);
        }
    }
}