using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ReactiveUI;

namespace Paket.VisualStudio.Commands.PackageGui
{
    /// <summary>
    /// Interaction logic for AddPackage.xaml
    /// </summary>
    public partial class AddPackage : Window, IViewFor<IAddPackageViewModel>
    {
        public AddPackage()
        {
            InitializeComponent();

            this.Events().Loaded.Subscribe(_ =>
            {
                this.OneWayBind(ViewModel, vm => vm.NugetResults, v => v.NugetResults.ItemsSource);
                this.Bind(ViewModel, vm => vm.SearchText, v => v.SearchTextBox.Text);
                this.Bind(ViewModel, vm => vm.SelectedPackage, v => v.NugetResults.SelectedItem);
                this.BindCommand(ViewModel, x => x.AddPackage, v => v.AddPackageButton);

                var disconnectHandler = UserError.RegisterHandler(async error =>
                {
                    // We don't know what thread a UserError can be thrown from, we usually 
                    // need to move things to the Main thread.
                    RxApp.MainThreadScheduler.Schedule(async () =>
                    {
                        // NOTE: This code is Incorrect, as it throws away 
                        // Recovery Options and just returns Cancel. This is Bad™.

                        Errors.Text = error.ErrorMessage;
                        Errors.Visibility = Visibility.Visible;
                        await Task.Delay(8000);
                        Errors.Text = string.Empty;
                        Errors.Visibility = Visibility.Hidden;
                   
                    });

                    return RecoveryOptionResult.CancelOperation;
                });
            });
        }

        public IAddPackageViewModel ViewModel { get; set; }

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (IAddPackageViewModel)value; }
        }
    }

    public class DesignTimeViewModel : IAddPackageViewModel
    {
        public DesignTimeViewModel()
        {
            SearchText = "Xunit";
            NugetResults = new List<NugetResult> { new NugetResult { PackageName = "Xunit" }, new NugetResult { PackageName = "Xunit.Runners" } };
        }
        public string SearchText { get; set; }
        public NugetResult SelectedPackage { get; set; }
        public IEnumerable<NugetResult> NugetResults { get; private set; }
        public ICommand SearchNuget { get; private set; }
        public ICommand AddPackage { get; private set; }
    }
}
