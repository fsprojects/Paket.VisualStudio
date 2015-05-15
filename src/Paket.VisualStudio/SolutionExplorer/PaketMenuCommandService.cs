using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Paket.VisualStudio.Commands;
using System.Threading.Tasks;
using EnvDTE;
using MadsKristensen.EditorExtensions;
using System.IO;

namespace Paket.VisualStudio.SolutionExplorer
{
    class PackageInfo
    {
        public string DependenciesFileName { get; set; }
        public string ReferencesFileName { get; set; }
        public string PackageName { get; set;}
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
            RegisterCommand(CommandIDs.RemovePackage, RemovePackage, OnlyBelowDependenciesFileNodes);
            RegisterCommand(CommandIDs.RemovePackageFromProject, RemovePackageFromProject, OnlyBelowReferencesFileNodes);
            RegisterCommand(CommandIDs.CheckForUpdates, CheckForUpdates, OnlyDependenciesFileNodes);
            RegisterCommand(CommandIDs.Update, Update, OnlyDependenciesFileNodes);
            RegisterCommand(CommandIDs.Install, Install, OnlyDependenciesFileNodes);
            RegisterCommand(CommandIDs.Restore, Restore, OnlyDependenciesFileNodes);
            RegisterCommand(CommandIDs.Simplify, Simplify, OnlyDependenciesFileNodes);
            RegisterCommand(CommandIDs.ConvertFromNuget, ConvertFromNuGet, null);
            RegisterCommand(CommandIDs.UpdateSolution, Update, null);
        }

        private void OnlyDependenciesFileNodes(object sender, EventArgs e)
        {
            var menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                menuCommand.Visible = false;
                menuCommand.Enabled = false;

                var fileName = tracker.GetSelectedFileName();
                if (String.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(Paket.Constants.DependenciesFileName))
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
                if (String.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(Paket.Constants.DependenciesFileName))
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
                if (String.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(Paket.Constants.ReferencesFile))
                    return;

                menuCommand.Visible = true;
                menuCommand.Enabled = true;
            }
        }

        private void RunCommand(object sender, EventArgs e, string helpTopic, Action command)
        {
            PaketOutputPane.OutputPane.Activate();
            PaketErrorPane.Clear();
            StatusBarService.UpdateText("Paket command started.");

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    command();
                    PaketOutputPane.OutputPane.OutputStringThreadSafe("Ready\r\n");
                    StatusBarService.UpdateText("Ready");
                }
                catch (Exception ex)
                {
                    PaketErrorPane.ShowError(ex.Message, tracker.GetSelectedFileName(), helpTopic);
                    PaketOutputPane.OutputPane.OutputStringThreadSafe(ex.Message +"\r\n");
                }
            });
        }

        private void RunCommandOnPackage(object sender, EventArgs e, string helpTopic, Action<PackageInfo> command)
        {
            var node = tracker.SelectedGraphNode;
            if (node == null || !node.HasCategory(PaketGraphSchema.PaketCategory))
                return;

            PaketOutputPane.OutputPane.Activate();
            PaketErrorPane.Clear();
            StatusBarService.UpdateText("Paket command started.");

            System.Threading.Tasks.Task.Run(() =>
            {
                var info = new PackageInfo();
                info.DependenciesFileName = node.Id.GetFileName();
                info.PackageName = node.GetPackageName();
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
            });
        }

        private void RunCommandOnPackageAndReloadAllDependendProjects(object sender, EventArgs e, string helpTopic, Action<PackageInfo> command)
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

            var projectGuids = 
                    Paket.Dependencies.Locate(info.DependenciesFileName)
                        .FindProjectsFor(info.PackageName)
                        .Select(project => project.GetProjectGuid())
                        .ToArray();

            SolutionExplorerExtensions.SaveSolution();
            foreach(var projectGuid in projectGuids)
                SolutionExplorerExtensions.UnloadProject(projectGuid);

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

            foreach (var projectGuid in projectGuids)
                SolutionExplorerExtensions.ReloadProject(projectGuid);
        }


        private void RunCommandOnPackageAndReloadAllProjects(object sender, EventArgs e, string helpTopic, Action<SolutionInfo> command)
        {
            PaketOutputPane.OutputPane.Activate();
            PaketErrorPane.Clear();
            StatusBarService.UpdateText("Paket command started.");

            var info = new SolutionInfo();
            info.Directory = SolutionExplorerExtensions.GetSolutionDirectory();
            info.FileName = SolutionExplorerExtensions.GetSolutionDirectory();
            var projectGuids = SolutionExplorerExtensions.GetAllProjectGuids();

            SolutionExplorerExtensions.SaveSolution();
            foreach (var projectGuid in projectGuids)
                SolutionExplorerExtensions.UnloadProject(projectGuid);

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

            foreach (var projectGuid in projectGuids)
                SolutionExplorerExtensions.ReloadProject(projectGuid);
        }

        private void RunCommandOnPackageInProject(object sender, EventArgs e, string helpTopic, Action<PackageInfo> command)
        {
            var node = tracker.SelectedGraphNode;
            if (node == null || !node.HasCategory(PaketGraphSchema.PaketCategory))
                return;

            PaketOutputPane.OutputPane.Activate();
            PaketErrorPane.Clear();
            StatusBarService.UpdateText("Paket command started.");

            System.Threading.Tasks.Task.Run(() =>
            {
                var info = new PackageInfo();
                info.DependenciesFileName = node.Id.GetFileName();
                info.ReferencesFileName = node.Id.GetFileName();
                info.PackageName = node.GetPackageName();
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
            });
        }

        private void RunCommandOnPackageInUnloadedProject(object sender, EventArgs e, string helpTopic, Action<PackageInfo> command)
        {
            var node = tracker.SelectedGraphNode;
            if (node == null || !node.HasCategory(PaketGraphSchema.PaketCategory))
                return;

            PaketOutputPane.OutputPane.Activate();
            PaketErrorPane.Clear();
            StatusBarService.UpdateText("Paket command started.");

            var projectGuid = tracker.GetSelectedProject();
            SolutionExplorerExtensions.SaveSolution();
            SolutionExplorerExtensions.UnloadProject(projectGuid);

            var info = new PackageInfo();
            info.DependenciesFileName = node.Id.GetFileName();
            info.ReferencesFileName = node.Id.GetFileName();
            info.PackageName = node.GetPackageName();
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

            SolutionExplorerExtensions.ReloadProject(projectGuid);
        }

        private void CheckForUpdates(object sender, EventArgs e)
        {
            RunCommand(sender, e, "paket-outdated.html", () =>
            {
                Paket.Dependencies.Locate(tracker.GetSelectedFileName())
                    .ShowOutdated(false, true);
            });
        }

        private void Update(object sender, EventArgs e)
        {
            RunCommandOnPackageAndReloadAllProjects(sender, e, "paket-update.html", _ =>
            {
                Paket.Dependencies.Locate(tracker.GetSelectedFileName())
                    .Update(false, false);
            });
        }

        private void Install(object sender, EventArgs e)
        {
            RunCommandOnPackageAndReloadAllProjects(sender, e, "paket-install.html", _ =>
            {
                Paket.Dependencies.Locate(tracker.GetSelectedFileName())
                    .Install(false, false);
            });
        }

        private void Restore(object sender, EventArgs e)
        {
            RunCommand(sender, e, "paket-restore.html", () => // Do we need to unload?
            {
                Paket.Dependencies.Locate(tracker.GetSelectedFileName())
                    .Restore();
            });
        }

        private void Simplify(object sender, EventArgs e)
        {
            RunCommand(sender, e, "paket-simplify.html", () => // Should work without unload
            {
                Paket.Dependencies.Locate(tracker.GetSelectedFileName())
                    .Simplify(false);
            });
        }

        private void UpdatePackage(object sender, EventArgs e)
        {
            RunCommandOnPackageAndReloadAllDependendProjects(sender, e, "paket-update.html#Updating-a-single-package", info =>
            {             
                Paket.Dependencies.Locate(info.DependenciesFileName)
                    .UpdatePackage(info.PackageName, Microsoft.FSharp.Core.FSharpOption<string>.None, false, false);
            });
        }

        private void RemovePackage(object sender, EventArgs e)
        {
            RunCommandOnPackageAndReloadAllDependendProjects(sender, e, "paket-remove.html", info =>
            {
                Paket.Dependencies.Locate(info.DependenciesFileName)
                    .Remove(info.PackageName);
            });
        }

        private void RemovePackageFromProject(object sender, EventArgs e)
        {
            RunCommandOnPackageInUnloadedProject(sender, e, "paket-remove.html#Removing-from-a-single-project", info =>
            {
                Paket.Dependencies.Locate(info.DependenciesFileName)
                    .RemoveFromProject(info.PackageName, false, false, info.ReferencesFileName, true);
            });
        }

        private void ConvertFromNuGet(object sender, EventArgs e)
        {
            RunCommandOnPackageAndReloadAllProjects(sender, e, "paket-convert-from-nuget.html", info =>
            {
                var dir = Microsoft.FSharp.Core.FSharpOption<DirectoryInfo>.Some(new DirectoryInfo(info.Directory));
                Paket.Dependencies
                    .ConvertFromNuget(true, true, true, Microsoft.FSharp.Core.FSharpOption<string>.None, dir);
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
