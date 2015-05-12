using System;
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
            RegisterCommand(CommandIDs.CheckForUpdates, CheckForUpdates);
        }

        private void CheckForUpdates(object sender, EventArgs e)
        {
            PaketOutputPane.OutputPane.Activate();

            System.Threading.Tasks.Task.Run(() =>
            {
                Paket.Dependencies.Locate(tracker.GetSelectedFileName())
                    .ShowOutdated(false, true);
            });
        }

        private void UpdatePackage(object sender, EventArgs e)
        {
            var node = tracker.SelectedGraphNode;
            if (node == null || !node.HasCategory(PaketGraphSchema.PaketCategory)) 
                return;

            PaketOutputPane.OutputPane.Activate();

            System.Threading.Tasks.Task.Run(() =>
            {             
                Paket.Dependencies.Locate(node.Id.GetFileName())
                    .UpdatePackage(node.GetPackageName(), Microsoft.FSharp.Core.FSharpOption<string>.None, false, false);
            });
        }

        private void RemovePackage(object sender, EventArgs e)
        {
            var node = tracker.SelectedGraphNode;
            if (node == null || !node.HasCategory(PaketGraphSchema.PaketCategory))
                return;

            PaketOutputPane.OutputPane.Activate();

            System.Threading.Tasks.Task.Run(() =>
            {
                Paket.Dependencies.Locate(node.Id.GetFileName())
                    .Remove(node.GetPackageName());
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