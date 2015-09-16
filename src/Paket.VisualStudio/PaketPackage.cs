using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Paket.VisualStudio.Commands;
using Paket.VisualStudio.Restore;
using Paket.VisualStudio.SolutionExplorer;

namespace Paket.VisualStudio
{
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.PackageGuid)]
    [ProvideOptionPage(typeof(PaketOptions), "Paket", "General", 0, 0, true)]
    public sealed class PaketPackage : Package
    {
        private PaketMenuCommandService commandService;
        private PackageRestorer packageRestorer;

        protected override void Initialize()
        {
            base.Initialize();

            PaketOutputPane.SetServiceProvider(this);
            var tracker = new ActiveGraphNodeTracker(this);
            var menuCommandService = (OleMenuCommandService)GetService(typeof(IMenuCommandService));
            commandService = new PaketMenuCommandService(this, menuCommandService, tracker);
            commandService.Register();

            PaketErrorPane.SetServiceProvider(this);
            SolutionExplorerExtensions.SetServiceProvider(this);
            StatusBarService.SetServiceProvider(this);

            packageRestorer = new PackageRestorer(
                new AutoRestorer(
                    new OutputPaneRestorer(
                        new WaitDialogRestorer(
                            new ErrorReportRestorer(
                                new PaketRestorer()
                            ),
                            (IVsThreadedWaitDialogFactory)
                            GetService(typeof(SVsThreadedWaitDialogFactory))))
                    , new PaketSettings(new ShellSettingsManager(this))
                ));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            commandService.Unregister();
            PaketErrorPane.Unregister();
        }
    }
}
