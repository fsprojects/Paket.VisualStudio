using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Paket.VisualStudio.SolutionExplorer
{
    internal class SolutionExplorerExtensions
    {
        private static IServiceProvider _serviceProvider;

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static void UnloadProject(Guid projectGuid)
        {
            if (projectGuid == Guid.Empty)
                return;

            IVsSolution4 solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution4;
            int hr;

            hr = solution.UnloadProject(ref projectGuid, (uint)_VSProjectUnloadStatus.UNLOADSTATUS_UnloadedByUser);
            ErrorHandler.ThrowOnFailure(hr);
        }

        public static void ReloadProject(Guid projectGuid)
        {
            if (projectGuid == Guid.Empty)
                return;

            IVsSolution4 solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution4;
            int hr;

            hr = solution.ReloadProject(ref projectGuid);
            ErrorHandler.ThrowOnFailure(hr);
        }
    }
}