using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;

namespace Paket.VisualStudio
{
    internal class PaketErrorPane
    {
        private static ErrorListProvider _paketErrorProvider;
        private static IServiceProvider _serviceProvider;

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _paketErrorProvider = new ErrorListProvider(serviceProvider);
        }

        public static void Clear()
        {
            _paketErrorProvider.Tasks.Clear();
        }

        public static void Unregister()
        {
            if (_paketErrorProvider != null)
                _paketErrorProvider.Dispose();
        }

        internal static void ShowError(string message, string document = null, string errorUrl = "http://fsprojects.github.io/Paket/")
        {
            ErrorTask task = new ErrorTask()
            {
                Category = TaskCategory.Misc,
                ErrorCategory = TaskErrorCategory.Error,
                Text = message                
            };

            DTE dte = (DTE)(_serviceProvider.GetService(typeof(DTE)));
            IServiceProvider serviceProvider = new ServiceProvider(dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);

            if (document != null)
            {
                task.Document = document;

                task.Navigate += (s, e) =>
                {
                    dte.ItemOperations.OpenFile(document);
                };
            }

            task.Help += (s, e) =>
            {
                IVsWebBrowsingService webBrowsingService = serviceProvider.GetService(typeof(SVsWebBrowsingService)) as IVsWebBrowsingService;
                if (webBrowsingService != null)
                {
                    IVsWindowFrame windowFrame;
                    webBrowsingService.Navigate(errorUrl, 0, out windowFrame);
                    return;
                }

                IVsUIShellOpenDocument openDocument = serviceProvider.GetService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
                if (openDocument != null)
                {
                    openDocument.OpenStandardPreviewer(0, errorUrl, VSPREVIEWRESOLUTION.PR_Default, 0);
                    return;
                }
            };
            _paketErrorProvider.Tasks.Add(task);
            _paketErrorProvider.Show();
        }
    }
}