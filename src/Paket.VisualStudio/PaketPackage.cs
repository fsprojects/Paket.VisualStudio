using System;
using System.Runtime.InteropServices;
using Clide;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Paket.VisualStudio
{
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
