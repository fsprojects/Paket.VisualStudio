using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.InteropServices;
using Clide;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Paket.VisualStudio
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShellExport
    {
        [Export]
        public IShellPackage Shell
        {
            get { return (IShellPackage)ServiceProvider.GlobalProvider.GetLoadedPackage(new Guid(Guids.PackageGuid)); }
        }
    }

    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource(1000, 5)]
    [Guid(Guids.PackageGuid)]
    [ProvideBindingPath]
    public sealed class PaketPackage : Package, IShellPackage
    {
        public IDevEnv DevEnv { get; private set; }
        public ISelectedGraphNode SelectedNode { get; set; }

        protected override void Initialize()
        {
            base.Initialize();

            Host.Initialize(this);
            DevEnv = Clide.DevEnv.Get(this);
        }
    }
}
