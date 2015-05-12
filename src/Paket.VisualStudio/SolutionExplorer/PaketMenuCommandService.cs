﻿using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Paket.VisualStudio.Commands;
using System.Threading.Tasks;
using EnvDTE;

namespace Paket.VisualStudio.SolutionExplorer
{
    class PackageInfo
    {
        public string DependenciesFileName { get; set;}
        public string PackageName { get; set;}
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
            RegisterCommand(CommandIDs.UpdatePackage, UpdatePackage);
            RegisterCommand(CommandIDs.RemovePackage, RemovePackage);
            RegisterCommand(CommandIDs.CheckForUpdates, CheckForUpdates, OnlyDependenciesFileNodes);
            RegisterCommand(CommandIDs.Update, Update, OnlyDependenciesFileNodes);
            RegisterCommand(CommandIDs.Install, Install, OnlyDependenciesFileNodes);
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

        private void RunCommand(object sender, EventArgs e, Action command)
        {
            PaketOutputPane.OutputPane.Activate();

            System.Threading.Tasks.Task.Run(() =>
            {
                command();
                PaketOutputPane.OutputPane.OutputStringThreadSafe("Done.");
            });
        }

        private void RunCommandOnPackage(object sender, EventArgs e, Action<PackageInfo> command)
        {
            var node = tracker.SelectedGraphNode;
            if (node == null || !node.HasCategory(PaketGraphSchema.PaketCategory))
                return;

            PaketOutputPane.OutputPane.Activate();

            System.Threading.Tasks.Task.Run(() =>
            {
                var info = new PackageInfo();
                info.DependenciesFileName = node.Id.GetFileName();
                info.PackageName = node.GetPackageName();
                command(info);
                PaketOutputPane.OutputPane.OutputStringThreadSafe("Done.");
            });
        }

        private void CheckForUpdates(object sender, EventArgs e)
        {
            RunCommand(sender, e, () =>
            {
                Paket.Dependencies.Locate(tracker.GetSelectedFileName())
                    .ShowOutdated(false, true);
            });
        }

        private void Update(object sender, EventArgs e)
        {
            RunCommand(sender, e, () =>
            {
                Paket.Dependencies.Locate(tracker.GetSelectedFileName())
                    .Update(false, false);
            });
        }

        private void Install(object sender, EventArgs e)
        {
            RunCommand(sender, e, () =>
            {
                Paket.Dependencies.Locate(tracker.GetSelectedFileName())
                    .Install(false, false);
            });
        }

        private void UpdatePackage(object sender, EventArgs e)
        {
            RunCommandOnPackage(sender, e, info =>
            {             
                Paket.Dependencies.Locate(info.DependenciesFileName)
                    .UpdatePackage(info.PackageName, Microsoft.FSharp.Core.FSharpOption<string>.None, false, false);
            });
        }

        private void RemovePackage(object sender, EventArgs e)
        {
            RunCommandOnPackage(sender, e, info =>
            {
                Paket.Dependencies.Locate(info.DependenciesFileName)
                    .Remove(info.PackageName);
            });
        }

        private void RegisterCommand(CommandID commandId, EventHandler invokeHandler, EventHandler beforeQueryStatusHandler = null)
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