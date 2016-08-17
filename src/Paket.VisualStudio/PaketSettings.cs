using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace Paket.VisualStudio
{
    public class PaketSettings
    {
        private const string StoreCollection = "Paket";
        private const string AutoRestoreKey = "AutoRestore";
        private readonly WritableSettingsStore settingsStore;

        public PaketSettings(ShellSettingsManager settingsManager)
        {
            settingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            if (!settingsStore.CollectionExists(StoreCollection))
                settingsStore.CreateCollection(StoreCollection);
        }

        public bool AutoRestore
        {
            get { return settingsStore.GetBoolean(StoreCollection, AutoRestoreKey, true); }
            set { settingsStore.SetBoolean(StoreCollection, AutoRestoreKey, value); }
        }
    }
}
