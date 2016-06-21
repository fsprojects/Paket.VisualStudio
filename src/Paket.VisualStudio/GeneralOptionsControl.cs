using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Paket.VisualStudio
{
    public partial class GeneralOptionControl : UserControl
    {
        private readonly PaketSettings settings;
        private bool _initialized;

        public DataGridView APIKeysControl => dgvAPIKeys;

        public GeneralOptionControl(PaketSettings settings)
        {
            this.settings = settings;
            InitializeComponent();
        }

        internal void OnActivated()
        {
            if (!_initialized)
            {
                autoRestoreCheckBox.Checked = settings.AutoRestore;
            }

            dgvAPIKeys.Rows.Clear();
            var apiTokensConfig = GetAPITokensConfig();
            foreach (var token in apiTokensConfig.APITokenCredentials.Tokens.Where(token => token.Source != ""))
            {
                AddPackageSource(token.Source, token.Value);
            }
            
            _initialized = true;
        }

        private void AddPackageSource(string source, string key)
        {
            var row = (DataGridViewRow)dgvAPIKeys.RowTemplate.Clone();
            row.CreateCells(dgvAPIKeys, source, key);
            dgvAPIKeys.Rows.Add(row);
        }

        private static APIToken GetAPITokensConfig()
        {
            APIToken apiTokensConfig;
            var paketConfigPath = Utils.Config.GetPaketConfigPath();
            var paketConfigDirectory = Path.GetDirectoryName(paketConfigPath);
            Directory.CreateDirectory(paketConfigDirectory);
            File.AppendText(paketConfigPath).Close();
            CreateRequiredNodes(paketConfigPath);
            using (var reader = new StreamReader(paketConfigPath))
            {
                apiTokensConfig = (APIToken)new XmlSerializer(typeof(APIToken)).Deserialize(reader);
            }
            return apiTokensConfig;
        }

        internal void OnApply()
        {
            // Save Auto-Restore Option
            settings.AutoRestore = autoRestoreCheckBox.Checked;

            // Remove current tokens
            var paketConfigPath = Utils.Config.GetPaketConfigPath();
            File.AppendText(paketConfigPath).Close();

            var doc = CreateRequiredNodes(paketConfigPath);

            var tokenNodes = doc.DocumentElement?.SelectNodes("/configuration/credentials/token");
            if (tokenNodes != null)
            {
                foreach (XmlNode node in tokenNodes)
                {
                    node.ParentNode?.RemoveChild(node);
                }
            }

            // Add the new tokens
            foreach (var row in dgvAPIKeys.Rows.Cast<DataGridViewRow>().Where(
                row => row.Cells["colSrcURL"].Value?.ToString().Trim() != ""))
            {
                var token = doc.CreateElement("token");
                token.SetAttribute("source", row.Cells["colSrcURL"].Value?.ToString());
                token.SetAttribute("value", row.Cells["colKey"].Value?.ToString());
                doc.DocumentElement?.SelectSingleNode("/configuration/credentials")?.AppendChild(token);
            }

            doc.Save(paketConfigPath);
        }

        private static XmlDocument CreateRequiredNodes(string paketConfigPath)
        {
            var changedFile = false;
            var doc = new XmlDocument();
            XmlNode configurationNode;

            try
            {
                doc.Load(paketConfigPath);
            }
            catch (Exception)
            {
                configurationNode = doc.CreateElement("configuration");
                doc.AppendChild(configurationNode);
                changedFile = true;
            }

            var rootNode = doc.DocumentElement;

            configurationNode = rootNode?.SelectSingleNode("/configuration");
            var credentialNode = rootNode?.SelectSingleNode("/configuration/credentials");
            if (credentialNode == null)
            {
                credentialNode = doc.CreateElement("credentials");
                configurationNode.AppendChild(credentialNode);
                changedFile = true;
            }

            if (changedFile)
                doc.Save(paketConfigPath);

            return doc;
        }

        internal void OnClosed()
        {
            _initialized = false;
        }
    }
}
