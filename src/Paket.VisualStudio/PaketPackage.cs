using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Paket.VisualStudio.Commands;

namespace Paket.VisualStudio
{
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.PackageGuid)]
    public sealed class PaketPackage : Package
    {
        protected override void Initialize()
        {
            base.Initialize();

            var menuCommandService = (OleMenuCommandService)GetService(typeof(IMenuCommandService));
            RegisterMenuCommands(menuCommandService);

        }

        private void RegisterMenuCommands(IMenuCommandService menuCommandService)
        {
            menuCommandService.AddCommand(new OleMenuCommand(id: CommandIDs.UpdatePackage, invokeHandler:
                (sender, args) => { UpdatePackageCommand.Execute(); }));
        }
    }
}
