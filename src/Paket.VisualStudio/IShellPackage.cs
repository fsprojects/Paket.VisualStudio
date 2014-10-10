using System;
using Clide;
using Microsoft.VisualStudio.Shell;

namespace Paket.VisualStudio
{
    public interface IShellPackage : IServiceProvider
    {
        IDevEnv DevEnv { get; }
        ISelectedGraphNode SelectedNode { get; set; }
    }
}