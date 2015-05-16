using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Paket.VisualStudio.Commands
{
    public class AddPackageProcess
    {
        public static void ShowAddPackageDialog(string selectedFileName, bool addToProject)
        {
            var dependenciesFile = Paket.Dependencies.Locate(selectedFileName);
            var frm = new Form();
            frm.Height = 500;
            frm.Width = 600;
            var text = new TextBox();
            text.Dock = DockStyle.Top;
            var listBox = new ListBox();
            listBox.Dock = DockStyle.Fill;
            listBox.SelectionMode = SelectionMode.One;
            text.TextChanged += (s, ev) =>
            {
                var searchResults =
                        FSharpAsync.RunSynchronously(
                            NuGetV3.FindPackages(FSharpOption<Paket.Utils.Auth>.None, Constants.DefaultNugetStream, text.Text, 1000),
                            FSharpOption<int>.None,
                            FSharpOption<CancellationToken>.None);
                listBox.Items.Clear();
                foreach (var r in searchResults)
                    listBox.Items.Add(r);
            };
            frm.Controls.Add(text);
            frm.Controls.Add(listBox);
            listBox.DoubleClick += (s, ev) =>
            {
                if (listBox.SelectedItem == null)
                    return;
                var packageName = listBox.SelectedItem.ToString();
                frm.Close();
                Application.DoEvents();

                if (addToProject)
                    dependenciesFile.AddToProject(packageName, "", false, false, selectedFileName, true);
                else
                    dependenciesFile.Add(packageName, "", false, false, false, true);
            };
            frm.ShowDialog();
        }
    }
}
