using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

namespace Paket.VisualStudio
{
    internal class PaketOutputPane
    {
        private static IVsOutputWindow _outputWindow;
        private const string PaneName = "Paket";
        private static readonly Guid PaneGuid = new Guid("A66E0A70-1A2A-4E3C-A806-1E537E608776");
        private static IVsOutputWindowPane _outputPane;
        static IServiceProvider _serviceProvider;

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        internal static IVsOutputWindowPane OutputPane
        {
            get
            {
                if (_outputPane == null)
                {
                    Guid generalPaneGuid = PaneGuid;
                    IVsOutputWindowPane pane;

                    OutputWindow.GetPane(ref generalPaneGuid, out pane);

                    if (pane == null)
                    {
                        OutputWindow.CreatePane(ref generalPaneGuid, PaneName, 1, 1);
                        OutputWindow.GetPane(ref generalPaneGuid, out pane);

                        Paket.Logging.@event.Publish.Subscribe(text => OutputPane.OutputStringThreadSafe(text + "\r\n"));
                    }

                    _outputPane = pane;
                }

                return _outputPane;
            }
        }

        private static IVsOutputWindow OutputWindow
        {
            get
            {
                if (_outputWindow == null)
                {
                    DTE dte = (DTE)(_serviceProvider.GetService(typeof(DTE)));
                    IServiceProvider serviceProvider = new ServiceProvider(dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
                    _outputWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
                }

                return _outputWindow;
            }
        }
    }
}