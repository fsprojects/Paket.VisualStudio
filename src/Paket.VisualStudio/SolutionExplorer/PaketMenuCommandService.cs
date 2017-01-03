using System;
using System.ComponentModel.Design;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Paket.VisualStudio.Commands;
using System.Threading.Tasks;
using System.IO;
using Microsoft.FSharp.Core;
using System.Threading;

namespace Paket.VisualStudio.SolutionExplorer
{
    class PackageInfo
    {
        public string DependenciesFileName { get; set; }
        public string ReferencesFileName { get; set; }
        public string GroupName { get; set; }
        public string PackageName { get; set; }
    }

    class SolutionInfo
    {
        public string Directory { get; set; }
        public string FileName { get; set; }
    }



    internal class PaketMenuCommandService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly OleMenuCommandService menuCommandService;
        private readonly ActiveGraphNodeTracker tracker;

        public PaketMenuCommandService(IServiceProvider serviceProvider, OleMenuCommandService menuCommandService, ActiveGraphNodeTracker tracker)
        {
            this.serviceProvider = serviceProvider;
            this.menuCommandService = menuCommandService;
            this.tracker = tracker;
        }


        private void RegisterCommands()
        {
            RegisterCommand(CommandIDs.UpdatePackage, UpdatePackage, null);
            RegisterCommand(CommandIDs.UpdateGroup, UpdateGroup, null);
            RegisterCommand(CommandIDs.RemovePackage, RemovePackage, OnlyBelowDependenciesFileNodes);
            RegisterCommand(CommandIDs.RemovePackageFromProject, RemovePackageFromProject, OnlyBelowReferencesFileNodes);
            RegisterCommand(CommandIDs.CheckForUpdates, CheckForUpdates, OnlyDependenciesFileNodes);
            RegisterCommand(CommandIDs.Update, Update, OnlyDependenciesFileNodes);
            RegisterCommand(CommandIDs.Install, Install, OnlyDependenciesFileNodes);
            RegisterCommand(CommandIDs.Restore, Restore, OnlyDependenciesFileNodes);
            RegisterCommand(CommandIDs.Simplify, Simplify, OnlyDependenciesFileNodes);
            RegisterCommand(CommandIDs.AddPackage, AddPackage, OnlyDependenciesFileNodes);
            RegisterCommand(CommandIDs.Init, Init, null);
            RegisterCommand(CommandIDs.ConvertFromNuget, ConvertFromNuGet, null);
            RegisterCommand(CommandIDs.UpdateSolution, Update, null);
            RegisterCommand(CommandIDs.InstallSolution, Install, null);
            RegisterCommand(CommandIDs.RestoreSolution, Restore, null);
            RegisterCommand(CommandIDs.AddPackageToProject, AddPackageToProject, OnlyReferencesFileNodes);
            RegisterCommand(CommandIDs.AddPackageToProjectOnReferences, AddPackageToProject, null);
        }

        private void OnlyDependenciesFileNodes(object sender, EventArgs e)
        {
            var menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                menuCommand.Visible = false;
                menuCommand.Enabled = false;

                var fileName = tracker.GetSelectedFileName();
                if (String.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(Constants.DependenciesFileName))
                    return;

                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        private void OnlyReferencesFileNodes(object sender, EventArgs e)
        {
            var menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                menuCommand.Visible = false;
                menuCommand.Enabled = false;

                var fileName = tracker.GetSelectedFileName();
                if (String.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(Constants.ReferencesFile))
                    return;

                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        private void OnlyBelowDependenciesFileNodes(object sender, EventArgs e)
        {
            var menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                menuCommand.Visible = false;
                menuCommand.Enabled = false;

                var node = tracker.SelectedGraphNode;
                if (node == null || !node.HasCategory(PaketGraphSchema.PaketCategory))
                    return;

                var fileName = node.Id.GetFileName();
                if (String.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(Constants.DependenciesFileName))
                    return;

                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        private void OnlyBelowReferencesFileNodes(object sender, EventArgs e)
        {
            var menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                menuCommand.Visible = false;
                menuCommand.Enabled = false;

                var node = tracker.SelectedGraphNode;
                if (node == null || !node.HasCategory(PaketGraphSchema.PaketCategory))
                    return;

                var fileName = node.Id.GetFileName();
                if (String.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(Constants.ReferencesFile))
                    return;

                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        private void RunCommand(string helpTopic,
            Action<SolutionInfo> command,
            TaskScheduler taskScheduler = null)
        {
            PaketOutputPane.OutputPane.Activate();
            taskScheduler = taskScheduler ?? TaskScheduler.Default;
            PaketErrorPane.Clear();
            StatusBarService.UpdateText("Paket command started.");
            var info = new SolutionInfo();
            info.Directory = SolutionExplorerExtensions.GetSolutionDirectory();
            info.FileName = SolutionExplorerExtensions.GetSolutionDirectory();
            System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    try
                    {
                        command(info);
                        PaketOutputPane.OutputPane.OutputStringThreadSafe("Ready\r\n");
                        StatusBarService.UpdateText("Ready");
                    }
                    catch (Exception ex)
                    {
                        PaketErrorPane.ShowError(ex.Message, tracker.GetSelectedFileName(), helpTopic);
                        PaketOutputPane.OutputPane.OutputStringThreadSafe(ex.Message + "\r\n");
                    }
                }, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
            System.Threading.Tasks.Task.Run(() =>
            {

            });
        }

        private void RunCommandOnPackageAndReloadAllDependendProjects(string helpTopic, Action<PackageInfo> command)
        {
            var node = tracker.SelectedGraphNode;
            if (node == null || !node.HasCategory(PaketGraphSchema.PaketCategory))
                return;

            PaketOutputPane.OutputPane.Activate();
            PaketErrorPane.Clear();
            StatusBarService.UpdateText("Paket command started.");

            var info = new PackageInfo();
            info.DependenciesFileName = node.Id.GetFileName();
            info.PackageName = node.GetPackageName();
            info.GroupName = node.GetGroupName();

            var projectGuids =
                    Dependencies.Locate(info.DependenciesFileName)
                        .FindProjectsFor(info.GroupName,info.PackageName)
                        .Select(project => project.GetProjectGuid())
                        .ToArray();

            SolutionExplorerExtensions.SaveSolution();
            foreach (var projectGuid in projectGuids)
                SolutionExplorerExtensions.UnloadProject(projectGuid);

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    command(info);
                    PaketOutputPane.OutputPane.OutputStringThreadSafe("Ready\r\n");
                    StatusBarService.UpdateText("Ready");
                }
                catch (Exception ex)
                {
                    PaketErrorPane.ShowError(ex.Message, info.DependenciesFileName, helpTopic);
                    PaketOutputPane.OutputPane.OutputStringThreadSafe(ex.Message + "\r\n");
                }
            }).ContinueWith(_ =>
            {
                foreach (var projectGuid in projectGuids)
                    SolutionExplorerExtensions.ReloadProject(projectGuid);
            });
        }


        private void RunCommandAndReloadAllProjects(string helpTopic, Action<SolutionInfo> command)
        {
            PaketOutputPane.OutputPane.Activate();
            PaketErrorPane.Clear();
            StatusBarService.UpdateText("Paket command started.");

            var info = new SolutionInfo();
            info.Directory = SolutionExplorerExtensions.GetSolutionDirectory();
            info.FileName = SolutionExplorerExtensions.GetSolutionDirectory();
            var projectGuids = SolutionExplorerExtensions.GetAllProjectGuids();

            SolutionExplorerExtensions.SaveSolution();

            // https://github.com/fsprojects/Paket.VisualStudio/issues/84
            // explicitly save unsaved projects
            foreach (var project in SolutionExplorerExtensions.GetAllProjects().Where(p => false == p.Saved))
            {
                project.Save();
            }

            foreach (var projectGuid in projectGuids)
                SolutionExplorerExtensions.UnloadProject(projectGuid);

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    command(info);
                    PaketOutputPane.OutputPane.OutputStringThreadSafe("Ready\r\n");
                    StatusBarService.UpdateText("Ready");
                }
                catch (Exception ex)
                {
                    PaketErrorPane.ShowError(ex.Message, info.FileName, helpTopic);
                    PaketOutputPane.OutputPane.OutputStringThreadSafe(ex.Message + "\r\n");
                }
            }).ContinueWith(_ =>
            {
                foreach (var projectGuid in projectGuids)
                    SolutionExplorerExtensions.ReloadProject(projectGuid);
            });
        }

        private void RunCommandOnPackageInUnloadedProject(string helpTopic, Action<PackageInfo> command)
        {
            var node = tracker.SelectedGraphNode;
            if (node == null || !node.HasCategory(PaketGraphSchema.PaketCategory))
                return;

            PaketOutputPane.OutputPane.Activate();
            PaketErrorPane.Clear();
            StatusBarService.UpdateText("Paket command started.");

            var projectGuid = tracker.GetSelectedProjectGuid();
            SolutionExplorerExtensions.SaveSolution();
            SolutionExplorerExtensions.UnloadProject(projectGuid);

            var info = new PackageInfo();
            info.DependenciesFileName = node.Id.GetFileName();
            info.ReferencesFileName = node.Id.GetFileName();
            info.PackageName = node.GetPackageName();
            info.GroupName = node.GetGroupName();

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    command(info);
                    PaketOutputPane.OutputPane.OutputStringThreadSafe("Ready\r\n");
                    StatusBarService.UpdateText("Ready");
                }
                catch (Exception ex)
                {
                    PaketErrorPane.ShowError(ex.Message, info.DependenciesFileName, helpTopic);
                    PaketOutputPane.OutputPane.OutputStringThreadSafe(ex.Message + "\r\n");
                }

            }).ContinueWith(_ =>
            {
                SolutionExplorerExtensions.ReloadProject(projectGuid);
            });
        }


        private void CheckForUpdates(object sender, EventArgs e)
        {
            RunCommand("paket-outdated.html", info =>
            {
                Dependencies.Locate(tracker.GetSelectedFileName())
                    .ShowOutdated(true, false, FSharpOption<string>.None);
            });
        }

        private void Update(object sender, EventArgs e)
        {
            RunCommandAndReloadAllProjects("paket-update.html", _ =>
            {
                Dependencies.Locate(tracker.GetSelectedFileName())
                    .Update(false);
            });
        }

        private void Install(object sender, EventArgs e)
        {
            RunCommandAndReloadAllProjects("paket-install.html", _ =>
            {
                Dependencies.Locate(tracker.GetSelectedFileName())
                    .Install(false);
            });
        }

        private void Restore(object sender, EventArgs e)
        {
            RunCommandAndReloadAllProjects("paket-restore.html", info =>
            {
                Dependencies.Locate(tracker.GetSelectedFileName())
                    .Restore();
            });
        }

        private void Simplify(object sender, EventArgs e)
        {
            RunCommand("paket-simplify.html", info => // Should work without unload
            {
                Dependencies.Locate(tracker.GetSelectedFileName())
                    .Simplify(false);
            });
        }

        private void UpdatePackage(object sender, EventArgs e)
        {
            RunCommandOnPackageAndReloadAllDependendProjects("paket-update.html#Updating-a-single-package", info =>
            {
                Dependencies.Locate(info.DependenciesFileName)
                    .UpdatePackage(FSharpOption<string>.Some(info.GroupName), info.PackageName, FSharpOption<string>.None, false, SemVerUpdateMode.NoRestriction, false);
            });
        }
        

        private void UpdateGroup(object sender, EventArgs e)
        {
            RunCommandOnPackageAndReloadAllDependendProjects("paket-update.html#Updating-a-single-group", info =>
            {
                Dependencies.Locate(info.DependenciesFileName)
                    .UpdateGroup(info.GroupName, false, false, false, false, true, SemVerUpdateMode.NoRestriction, false);
            });
        }

        private void RemovePackage(object sender, EventArgs e)
        {
            RunCommandOnPackageAndReloadAllDependendProjects("paket-remove.html", info =>
            {
                Dependencies.Locate(info.DependenciesFileName)
                    .Remove(Microsoft.FSharp.Core.FSharpOption<string>.Some(info.GroupName), info.PackageName);
            });
        }

        private void AddPackage(object sender, EventArgs e)
        {
            RunCommand("paket-add.html", info =>
            {
                AddPackageProcess.ShowAddPackageDialog(tracker.GetSelectedFileName());
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void AddPackageToProject(object sender, EventArgs e)
        {
            var helpTopic = "paket-add.html#Adding-to-a-single-project";

            PaketOutputPane.OutputPane.Activate();
            PaketErrorPane.Clear();

            var projectFileName = tracker.GetSelectedFileName();
            StatusBarService.UpdateText("Add NuGet package to " + projectFileName);

            var projectGuid = tracker.GetSelectedProjectGuid();
            SolutionExplorerExtensions.SaveSolution();

            try
            {
                AddPackageProcess.ShowAddPackageDialog(projectFileName, projectGuid.ToString());

                PaketOutputPane.OutputPane.OutputStringThreadSafe("Ready\r\n");
                StatusBarService.UpdateText("Ready");
            }
            catch (Exception ex)
            {
                SolutionExplorerExtensions.ReloadProject(projectGuid);
                PaketErrorPane.ShowError(ex.Message, projectFileName, helpTopic);
                PaketOutputPane.OutputPane.OutputStringThreadSafe(ex.Message + "\r\n");
            }
        }

        private void RemovePackageFromProject(object sender, EventArgs e)
        {
            RunCommandOnPackageInUnloadedProject("paket-remove.html#Removing-from-a-single-project", info =>
            {
                Dependencies.Locate(info.DependenciesFileName)
                    .RemoveFromProject(FSharpOption<string>.Some(info.GroupName), info.PackageName, false, info.ReferencesFileName, true);
            });
        }

        private void Init(object sender, EventArgs e)
        {
            RunCommandAndReloadAllProjects("paket-init.html", info =>
            {
                Dependencies.Init(info.Directory);
            });
        }

        private void ConvertFromNuGet(object sender, EventArgs e)
        {
            RunCommandAndReloadAllProjects("paket-convert-from-nuget.html", info =>
            {
                var dir = FSharpOption<DirectoryInfo>.Some(new DirectoryInfo(info.Directory));
                Dependencies
                    .ConvertFromNuget(true, true, true, FSharpOption<string>.None, dir);
            });
        }

        private void RegisterCommand(CommandID commandId, EventHandler invokeHandler, EventHandler beforeQueryStatusHandler)
        {
            menuCommandService.AddCommand(new OleMenuCommand(
                id: commandId,
                invokeHandler: invokeHandler,
                changeHandler: null,
                beforeQueryStatus: beforeQueryStatusHandler));
        }

        public void Register()
        {
            tracker.Register();
            RegisterCommands();
        }

        public void Unregister()
        {
            tracker.Unregister();
        }
    }
}
