using System.Windows.Forms;

namespace Paket.VisualStudio
{
    public partial class GeneralOptionControl : UserControl
    {
        private readonly PaketSettings settings;
        private bool _initialized;

        public GeneralOptionControl(PaketSettings settings)
        {
            this.settings = settings;
            InitializeComponent();
        }

        internal void OnActivated()
        {
            if (!_initialized)
                autoRestoreCheckBox.Checked = settings.AutoRestore;

            _initialized = true;
        }

        internal void OnApply()
        {
            settings.AutoRestore = autoRestoreCheckBox.Checked;
        }

        internal void OnClosed()
        {
            _initialized = false;
        }
    }
}
