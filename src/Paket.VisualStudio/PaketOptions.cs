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

        protected override IWin32Window Window
        {
            get { return General; }
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);
            General.Font = VsShellUtilities.GetEnvironmentFont(ServiceProvider);
            General.OnActivated();
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);
            General.OnApply();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            General.OnClosed();
        }

        public GeneralOptionControl General
        {
            get {
                if (window == null)
                {
                    window = 
                        new GeneralOptionControl(
                            new PaketSettings(
                                new ShellSettingsManager(ServiceProvider)));
                    window.Location = new Point(0, 0);
                }

                return window;
            }
        }

        private IServiceProvider ServiceProvider
        {
            get { return (IServiceProvider)GetService(typeof(Package)); }
        }
    }
}
