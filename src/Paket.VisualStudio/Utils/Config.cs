using System;
using System.IO;

namespace Paket.VisualStudio.Utils
{
    public static class Config
    {
        public static string GetPaketConfigPath()
        {
            const string PAKET_FOLDER = "paket";
            const string PAKET_CONFIG_FILE = "paket.config";
            var roamingFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var paketConfigFolder = Path.Combine(roamingFolder, PAKET_FOLDER);
            var paketConfigPath = Path.Combine(paketConfigFolder, PAKET_CONFIG_FILE);
            return paketConfigPath;
        }
    }
}