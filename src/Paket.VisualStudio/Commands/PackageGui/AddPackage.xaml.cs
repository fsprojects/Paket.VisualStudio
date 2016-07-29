using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

using Paket.VisualStudio.Commands.PackageGui.Converters;
using Paket.VisualStudio.Utils;
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
                this.OneWayBind(ViewModel, vm => vm.AddPackageState, v => v.OutProgressRing.Visibility,
                    () => LoadingState.Loading, LoadingState.Loading, new LoadingSuccessFailureVisibilityConverter());
                this.OneWayBind(ViewModel, vm => vm.AddPackageState, v => v.PaketAddSuccess.Visibility,
                    () => LoadingState.Loading, LoadingState.Success, new LoadingSuccessFailureVisibilityConverter());
                this.OneWayBind(ViewModel, vm => vm.AddPackageState, v => v.PaketAddFailure.Visibility,
                    () => LoadingState.Loading, LoadingState.Failure, new LoadingSuccessFailureVisibilityConverter());
                

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
                         //Close the dialog after 5 seconds after adding a package executes
                         //TODO: Should give use visual cue this will happen
                         Observable.Timer(TimeSpan.FromSeconds(5))
                             .ObserveOn(RxApp.MainThreadScheduler)
                             .Subscribe(___ => { OutputFlyout.IsOpen = false; })
                             .AddTo(_compositeDisposable);
                     })
                .AddTo(_compositeDisposable);



                ViewModel
                    .AddPackage
                    .ThrownExceptions
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Select(ex => ex.ToString())
                    .Subscribe(AppendMessageToOutputbox);



                //Listen to the paket trace and put it in the dialog box
                ViewModel.PaketTrace
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Select(x => x.Text)
                    .Subscribe(AppendMessageToOutputbox)
                    .AddTo(_compositeDisposable);

                var disconnectHandler = UserError.RegisterHandler(async error =>
                {
                    // We don't know what thread a UserError can be thrown from, we usually 
                    // need to move things to the Main thread.
                    RxApp.MainThreadScheduler.Schedule(async () =>
                    {
                        //Ugly
                        MessageBox.Show(error.ErrorMessage, error.ErrorCauseOrResolution);
                        //awaiting https://github.com/MahApps/MahApps.Metro/issues/1931
                        //await this.ShowMessageAsync(error.ErrorMessage, error.ErrorCauseOrResolution);


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

        private void AppendMessageToOutputbox(string message)
        {
            OutputDialogBox.Text += string.Format("{0}{1}", message, Environment.NewLine);
            OutputDialogBox.CaretIndex = OutputDialogBox.Text.Length;
            OutputDialogBox.ScrollToEnd();
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
            NugetResults = new ReactiveList<NugetResult> { new NugetResult { PackageName = "Xunit" }, new NugetResult { PackageName = "Xunit.Runners" } };
        }
        public string SearchText { get; set; }
        public NugetResult SelectedPackage { get; set; }
        public ReactiveList<NugetResult> NugetResults { get; private set; }
        public ReactiveCommand<NugetResult> SearchNuget { get; private set; }


        public ReactiveCommand<Unit> AddPackage { get; private set; }
        public IObservable<Logging.Trace> PaketTrace { get; private set; }
        public LoadingState AddPackageState { get; private set; }
    }
}
