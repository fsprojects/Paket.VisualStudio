using Clide;
using Microsoft.VisualStudio.Shell;

namespace Paket.VisualStudio
{
    public interface IShellPackage
    {
        IDevEnv DevEnv { get; }
        ISelectedGraphNode SelectedNode { get; set; }
    }
}