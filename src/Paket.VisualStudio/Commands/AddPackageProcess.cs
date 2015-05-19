using System.Reactive.Disposables;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Paket.VisualStudio.Commands.PackageGui;
using Paket.VisualStudio.SolutionExplorer;

namespace Paket.VisualStudio.Commands
{
    public class AddPackageProcess
    {
        public static Task<string[]> SearchPackagesByName(string search, CancellationToken ct)
        {
            //TODO: this should probably return a success/failure type to indicate whether the search was successful.  (Like lack of internet, nuget down)
            return FSharpAsync.StartAsTask(
                NuGetV3.FindPackages(FSharpOption<Paket.Utils.Auth>.None, Constants.DefaultNugetStream, search, 1000),
                FSharpOption<TaskCreationOptions>.None,
                FSharpOption<CancellationToken>.Some(ct));
        }

        public static void ShowAddPackageDialog(string selectedFileName, string projectGuid = null)
        {
            var dependenciesFile = Paket.Dependencies.Locate(selectedFileName);

            var secondWindow = new AddPackage();

            //Create observable paket trace
            var paketTraceObs = Observable.Create<string>(observer =>
            {
                Paket.Logging.RegisterTraceFunction(observer.OnNext);
                return Disposable.Create(() =>
                {
                    Paket.Logging.RemoveTraceFunction(observer.OnNext);
                });
            });

            Action<NugetResult> addPackageToDependencies = result =>
            {
                var packageName = result.PackageName;

                if (projectGuid != null)
                {
                    var guid = Guid.Parse(projectGuid);
                    SolutionExplorerExtensions.UnloadProject(guid);
                    dependenciesFile.AddToProject(packageName, "", false, false, selectedFileName, true);
                    SolutionExplorerExtensions.ReloadProject(guid);
                }
                else
                    dependenciesFile.Add(packageName, "", false, false, false, true);
            };
            //TODO: Use interfaces?
            secondWindow.ViewModel = new AddPackageViewModel(SearchPackagesByName, addPackageToDependencies, paketTraceObs);
            secondWindow.ShowDialog();
        }
    }
}
