using System.Reactive.Disposables;
using Microsoft.FSharp.Core;
using System;
using System.Reactive.Linq;
using System.Threading;
using Paket.VisualStudio.Commands.PackageGui;
using Paket.VisualStudio.SolutionExplorer;
using Paket.VisualStudio.Utils;

namespace Paket.VisualStudio.Commands
{
    public class AddPackageProcess
    {
        public static void ShowAddPackageDialog(string selectedFileName, string projectGuid = null)
        {
            Paket.Dependencies dependenciesFile = null;

            try
            {
                dependenciesFile = Dependencies.Locate(selectedFileName);
            }
            catch (Exception)
            {
                PaketLauncher.LaunchPaket(SolutionExplorerExtensions.GetPaketDirectory(), "init",
                     (send, args) => PaketOutputPane.OutputPane.OutputStringThreadSafe(args.Data + "\n"));
                dependenciesFile = Dependencies.Locate(selectedFileName);
            }

            var secondWindow = new AddPackage();

            //Create observable paket trace
            var paketTraceObs = Observable.Create<Logging.Trace>(observer =>
            {
                Logging.@event.Publish.Subscribe(x => observer.OnNext(x));
                return Disposable.Create(() =>
                {
                   
                });
               
            });

            Action<NugetResult> addPackageToDependencies = result =>
            {
                if (projectGuid != null)
                {
                    var guid = Guid.Parse(projectGuid);
                    DteHelper.ExecuteCommand("File.SaveAll");
                    SolutionExplorerExtensions.UnloadProject(guid);
                    PaketLauncher.LaunchPaket(SolutionExplorerExtensions.GetPaketDirectory(), "add " + result.PackageName + " --project " + selectedFileName,
                        (send, args) => PaketOutputPane.OutputPane.OutputStringThreadSafe(args.Data + "\n"));
                    SolutionExplorerExtensions.ReloadProject(guid);
                }
                else
                    PaketLauncher.LaunchPaket(SolutionExplorerExtensions.GetPaketDirectory(), "add " + result.PackageName,
                        (send, args) => PaketOutputPane.OutputPane.OutputStringThreadSafe(args.Data + "\n"));
            };

            Func<string, IObservable<string>> searchNuGet = 
                searchText => Observable.Create<string>(obs =>
                {
                    var disposable = new CancellationDisposable();

                    dependenciesFile
                        .SearchPackagesByName(
                            searchText,
                            FSharpOption<CancellationToken>.Some(disposable.Token),
                            FSharpOption<int>.None)
                        .Subscribe(obs);

                    return disposable;
                });
          
            //TODO: Use interfaces?
            secondWindow.ViewModel = new AddPackageViewModel(searchNuGet, addPackageToDependencies, paketTraceObs);
            secondWindow.ShowDialog();
        }
    }
}