using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace Paket.VisualStudio
{
    [Guid(Guids.OptionsGuid)]
    public class PaketOptions : DialogPage
    {
        private GeneralOptionControl window;
        private IServiceProvider ServiceProvider => (IServiceProvider)GetService(typeof(Package));

        protected override IWin32Window Window => General;

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);
            General.Font = VsShellUtilities.GetEnvironmentFont(ServiceProvider);
            General.OnActivated();
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            window.APIKeysControl.EndEdit(DataGridViewDataErrorContexts.Commit);
            General.OnApply();
            base.OnApply(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            General.OnClosed();
            base.OnClosed(e);
        }

        public GeneralOptionControl General
        {
            get {
                if (window != null)
                    return window;

                window =
                    new GeneralOptionControl(
                        new PaketSettings(
                            new ShellSettingsManager(ServiceProvider))) {Location = new Point(0, 0)};

                return window;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (window != null)
                {
                    window.Dispose();
                    window = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
