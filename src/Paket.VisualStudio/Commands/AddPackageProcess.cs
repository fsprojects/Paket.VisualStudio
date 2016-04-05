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
                var dir = new System.IO.FileInfo(SolutionExplorerExtensions.GetSolutionFileName()).Directory.FullName;
                Dependencies.Init(dir);
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
                    dependenciesFile.AddToProject(FSharpOption<string>.None, result.PackageName, "", false, false, false, false, selectedFileName, true, SemVerUpdateMode.NoRestriction, false);
                    SolutionExplorerExtensions.ReloadProject(guid);
                }
                else
                    dependenciesFile.Add(FSharpOption<string>.None, result.PackageName, "", false, false, false, false, false, true, SemVerUpdateMode.NoRestriction, false);
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
