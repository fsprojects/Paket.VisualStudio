using System.Reactive;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Paket.VisualStudio.Commands.PackageGui;
using Paket.VisualStudio.SolutionExplorer;

namespace Paket.VisualStudio.Commands
{
    public class AddPackageProcess
    {
        public static Task<string[]> SearchPackagesByName(string name)
        {
           return FSharpAsync.StartAsTask(NuGetV3.FindPackages(FSharpOption<Paket.Utils.Auth>.None, Constants.DefaultNugetStream,
                    name, 1000),
                FSharpOption<TaskCreationOptions>.None,
                FSharpOption<CancellationToken>.None);

        }
        public static void ShowAddPackageDialog(string selectedFileName, string projectGuid = null)
        {
            var dependenciesFile = Paket.Dependencies.Locate(selectedFileName);

            var secondWindow = new AddPackage();
            //TODO: Use interfaces?

            Func<string, CancellationToken, Task<string[]>> findPackages = (search, ct) =>
            {
                //TODO: this should probably return a success/failure type to indicate whether the search was successful.  (Like lack of internet, nuget down)
                return FSharpAsync.StartAsTask(NuGetV3.FindPackages(FSharpOption<Paket.Utils.Auth>.None, Constants.DefaultNugetStream,
                       search, 1000),
                   FSharpOption<TaskCreationOptions>.None,
                   FSharpOption<CancellationToken>.Some(ct));
            };

            Action<NugetResult> addPackageToDependencies = result =>
            {
                var packageName = result.PackageName;
                secondWindow.Close();
                Application.DoEvents();

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

            secondWindow.ViewModel = new AddPackageViewModel(findPackages,addPackageToDependencies); 
            secondWindow.ShowDialog();
        }
    }
}
