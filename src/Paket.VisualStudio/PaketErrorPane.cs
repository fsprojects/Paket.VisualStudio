using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;

using VsShell = Microsoft.VisualStudio.Shell.VsShellUtilities;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Runtime.InteropServices;

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

        public static int ThrowOnFailure(int hr)
        {
            return ThrowOnFailure(hr, null);
        }

        public static bool Failed(int hr)
        {
            return (hr < 0);
        }

        public static int ThrowOnFailure(int hr, params int[] expectedHRFailure)
        {
            if (Failed(hr))
            {
                if ((null == expectedHRFailure) || (Array.IndexOf(expectedHRFailure, hr) < 0))
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
            return hr;
        }

        internal static void ShowError(string message, string document, string helpSubPage = "", int lineNo = 0, int column = 0)
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
                    try
                    {
                        IVsUIHierarchy hierarchy;
                        uint itemID;
                        IVsWindowFrame docFrame;
                        IVsTextView textView;
                        VsShell.OpenDocument(serviceProvider, document, Guids.LOGVIEWID_Code, out hierarchy, out itemID, out docFrame, out textView);
                        ThrowOnFailure(docFrame.Show());
                        if (textView != null)
                        {
                            ThrowOnFailure(textView.SetCaretPos(lineNo, column));
                        }
                    }catch(Exception)
                    {
                        // don't trow crazy exceptions when trying to navigate to errors
                    }
                };
            }

            task.Help += (s, e) =>
            {
                var mainPage = "http://fsprojects.github.io/Paket/";
                var errorUrl = mainPage;
                if (!String.IsNullOrWhiteSpace(helpSubPage))
                    errorUrl += helpSubPage;

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
            _paketErrorProvider.BringToFront();
        }
    }
}