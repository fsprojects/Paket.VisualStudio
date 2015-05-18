using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Chessie.ErrorHandling;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using MahApps.Metro.Controls;

namespace Paket.VisualStudio.Commands.PackageGui
{
    /// <summary>
    /// Interaction logic for AddPackage.xaml
    /// </summary>
    public partial class AddPackage : MetroWindow, IViewFor<IAddPackageViewModel>
    {
        private CompositeDisposable _compositeDisposable;

        public AddPackage()
        {
            _compositeDisposable = new CompositeDisposable();
            InitializeComponent();

            this.Events().Loaded.Subscribe(__ =>
            {
                //Bindings
                this.OneWayBind(ViewModel, vm => vm.NugetResults, v => v.NugetResults.ItemsSource);
                this.Bind(ViewModel, vm => vm.SearchText, v => v.SearchTextBox.Text);
                this.Bind(ViewModel, vm => vm.SelectedPackage, v => v.NugetResults.SelectedItem);
                this.BindCommand(ViewModel, x => x.AddPackage, v => v.AddPackageButton);



                //TODO: These visual states should be handled more elegantly
                //Show progress bar when searching for packages
                ViewModel
                    .SearchNuget
                    .IsExecuting
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(inProgress =>
                    {
                        SearchProgressBar.Visibility = inProgress ? Visibility.Visible : Visibility.Hidden;
                    });
                //Open an output dialog window when adding a new package executes
                ViewModel.AddPackage
                    .IsExecuting
                    .Where(isExecuting => isExecuting)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(async _ =>
                    {
                        PaketAddSuccess.Visibility = Visibility.Collapsed;
                        OutProgressRing.Visibility = Visibility.Visible;
                        OutProgressRing.IsActive = true;
                        OutputDialogBox.Clear();
                        OutputFlyout.IsOpen = true;
                     
                    })
                    .AddTo(_compositeDisposable);


                ViewModel.AddPackage
                     .Where(_ => OutputFlyout.IsOpen)
                     .ObserveOn(RxApp.MainThreadScheduler)
                    //OnNext gets called when the AddPackage command finishes
                     .Subscribe(_ =>
                     {

                         OutProgressRing.IsActive = false;
                         OutProgressRing.Visibility = Visibility.Collapsed;
                         PaketAddSuccess.Visibility = Visibility.Visible;
                         //dialog.ProgressBar.Value = 100;
                         //Close the dialog after 5 seconds after adding a package executes
                         //TODO: Should give use visual cue this will happen
                         Observable.Timer(TimeSpan.FromSeconds(5))
                             .ObserveOn(RxApp.MainThreadScheduler)
                             .Subscribe(___ => { OutputFlyout.IsOpen = false; })
                             .AddTo(_compositeDisposable);
                     },
                         _ =>
                         {
                             //dialog.ProgressBar.IsIndeterminate = false;
                             //dialog.ProgressBar.Value = 100;
                             //dialog.ProgressBar.Foreground = Brushes.Red;
                         })
                .AddTo(_compositeDisposable);







                //Listen to the paket trace and put it in the dialog box
                ViewModel.PaketTrace
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(text =>
                    {


                        OutputDialogBox.Text += string.Format("{0}{1}", text, Environment.NewLine);
                        OutputDialogBox.CaretIndex = OutputDialogBox.Text.Length;
                        OutputDialogBox.ScrollToEnd();
                    })
                    .AddTo(_compositeDisposable);

                var disconnectHandler = UserError.RegisterHandler(async error =>
                {
                    // We don't know what thread a UserError can be thrown from, we usually 
                    // need to move things to the Main thread.
                    RxApp.MainThreadScheduler.Schedule(async () =>
                    {
                        // NOTE: This code is Incorrect, as it throws away 
                        // Recovery Options and just returns Cancel. This is Bad™.

                        //Errors.Text = error.ErrorMessage;
                        //Errors.Visibility = Visibility.Visible;
                        //await Task.Delay(8000);
                        //Errors.Text = string.Empty;
                        //Errors.Visibility = Visibility.Hidden;

                    });

                    return RecoveryOptionResult.CancelOperation;
                });
            });

            this.Events().Closed.Subscribe(__ =>
            {
                _compositeDisposable.Dispose();
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
        public ReactiveCommand<IEnumerable<NugetResult>> SearchNuget { get; private set; }

        public ReactiveCommand<Unit> AddPackage { get; private set; }
        public IObservable<string> PaketTrace { get; private set; }
    }
}
