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
        private IVsOutputWindow _outputWindow;
        private const string PaneName = "Paket";
        private static readonly Guid PaneGuid = new Guid("A66E0A70-1A2A-4E3C-A806-1E537E608776");
        private IVsOutputWindowPane _outputPane;

        private IVsOutputWindowPane OutputPane
        {
            get
            {
                if (_outputPane == null)
                {
                    Guid generalPaneGuid = PaneGuid;
                    IVsOutputWindowPane pane;

                    OutputWindow.GetPane(ref generalPaneGuid, out pane);

                    if (pane == null)
                    {
                        OutputWindow.CreatePane(ref generalPaneGuid, PaneName, 1, 1);
                        OutputWindow.GetPane(ref generalPaneGuid, out pane);

                        Paket.Logging.RegisterTraceFunction(text => { OutputPane.OutputStringThreadSafe(text + "\r\n"); });
                    }

                    _outputPane = pane;
                }

                return _outputPane;
            }
        }


        public PaketMenuCommandService(IServiceProvider serviceProvider, OleMenuCommandService menuCommandService, ActiveGraphNodeTracker tracker)
        {
            this.serviceProvider = serviceProvider;
            this.menuCommandService = menuCommandService;
            this.tracker = tracker;
        }

        private IVsOutputWindow OutputWindow
        {
            get
            {
                if (_outputWindow == null)
                {
                    DTE dte = (DTE)(this.serviceProvider.GetService(typeof(DTE)));
                    IServiceProvider serviceProvider = new ServiceProvider(dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
                    _outputWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                }

                return _outputWindow;
            }
        }

        private void UpdatePackageAsync(string dependenciesFileName, string packageName)
        {
            OutputPane.Activate();

            System.Threading.Tasks.Task.Run(() =>
            {                
                OutputPane.OutputStringThreadSafe(String.Format("Updating {0}\r\n", packageName));
                Paket.Dependencies.Locate(dependenciesFileName)
                    .UpdatePackage(packageName, Microsoft.FSharp.Core.FSharpOption<string>.None, false, false);
            });
        }

        private void RegisterCommands()
        {
            RegisterCommand(CommandIDs.UpdatePackage, UpdatePackage);
        }

        private void UpdatePackage(object sender, EventArgs e)
        {
            var node = tracker.SelectedGraphNode;
            if (node == null || !node.HasCategory(PaketGraphSchema.PaketCategory)) 
                return;

            UpdatePackageAsync(node.Id.GetFileName(), node.GetPackageName());
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