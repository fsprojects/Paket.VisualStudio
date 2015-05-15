using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Paket.VisualStudio
{
    public static class StatusBarService
    {
        private static IServiceProvider _serviceProvider;

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static void UpdateText(string text)
        {
            var statusBar = _serviceProvider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;

            if (statusBar == null)
            {
                throw new InvalidOperationException("Cannot find the StatusBar.");
            }

            int frozen;

            statusBar.IsFrozen(out frozen);

            if (frozen == 0)
            {
                statusBar.SetText(text);
            }
        }
    }
}