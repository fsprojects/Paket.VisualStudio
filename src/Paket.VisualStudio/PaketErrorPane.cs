using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

namespace Paket.VisualStudio
{
    internal class PaketErrorPane
    {
        private static ErrorListProvider vsbaseWarningProvider;
        private static IServiceProvider _serviceProvider;

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            vsbaseWarningProvider = new ErrorListProvider(serviceProvider);
        }

        public static void Unregister()
        {
            if (vsbaseWarningProvider != null)
                vsbaseWarningProvider.Dispose();
        }

        internal static void ShowError(string message, string docsUrl)
        {
            ErrorTask task = new ErrorTask()
            {
                Category = TaskCategory.Misc,
                ErrorCategory = TaskErrorCategory.Error,
                Text = message
            };

            task.Navigate += (s, e) =>
            {
                DTE dte = (DTE)(_serviceProvider.GetService(typeof(DTE)));
                IServiceProvider serviceProvider = new ServiceProvider(dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);

                IVsWebBrowsingService webBrowsingService = serviceProvider.GetService(typeof(SVsWebBrowsingService)) as IVsWebBrowsingService;
                if (webBrowsingService != null)
                {
                    IVsWindowFrame windowFrame;
                    webBrowsingService.Navigate(docsUrl, 0, out windowFrame);
                    return;
                }

                IVsUIShellOpenDocument openDocument = serviceProvider.GetService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
                if (openDocument != null)
                {
                    openDocument.OpenStandardPreviewer(0, docsUrl, VSPREVIEWRESOLUTION.PR_Default, 0);
                    return;
                }
            };
            vsbaseWarningProvider.Tasks.Add(task);
            vsbaseWarningProvider.Show();
        }
    }
}