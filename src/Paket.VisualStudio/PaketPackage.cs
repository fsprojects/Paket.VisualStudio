using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Paket.VisualStudio.Commands;
using Paket.VisualStudio.SolutionExplorer;

namespace Paket.VisualStudio
{
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.PackageGuid)]
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

            packageRestorer = new PackageRestorer();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            commandService.Unregister();
            PaketErrorPane.Unregister();
        }
    }
}
