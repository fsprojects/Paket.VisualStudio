using System.Reactive.Concurrency;
using System.Threading;
using System.Windows.Input;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;

namespace Paket.VisualStudio.Commands.PackageGui
{
    /// <summary>
    /// This helps with keeping design time view models in sync with real view models
    /// </summary>
    public interface IAddPackageViewModel
    {
        string SearchText { get; set; }
        NugetResult SelectedPackage { get; set; }
        ReactiveList<NugetResult> NugetResults { get; }

        ReactiveCommand<NugetResult> SearchNuget { get; }
        ReactiveCommand<System.Reactive.Unit> AddPackage { get; }
        IObservable<string> PaketTrace { get; }
    }

    public class NugetResult
    {
        public string PackageName { get; set; }
    }

    public class AddPackageViewModel : ReactiveObject, IAddPackageViewModel
    {
        private readonly Paket.Dependencies _dependenciesFile;
        private readonly IObservable<string> _paketTraceFunObservable;

        public IObservable<string> PaketTrace
        {
            get { return _paketTraceFunObservable; }
        }
        string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                this.RaiseAndSetIfChanged(ref _searchText,value);
            }
        }

        NugetResult _selectedPackage;
        public NugetResult SelectedPackage
        {
            get { return _selectedPackage; }
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedPackage, value);
            }
        }

        private ReactiveList<NugetResult> _nugetResults = new ReactiveList<NugetResult>();
        public ReactiveList<NugetResult> NugetResults { get { return _nugetResults; }  } 

        public ReactiveCommand<NugetResult> SearchNuget { get; private set; }


        public ReactiveCommand<System.Reactive.Unit> AddPackage { get; private set; }

        public AddPackageViewModel(
            Paket.Dependencies dependenciesFile,
            Action<NugetResult> addPackageCallback,
            IObservable<string> paketTraceFunObservable)
        {

            _dependenciesFile = dependenciesFile;
            _paketTraceFunObservable = paketTraceFunObservable;
            SearchNuget = 
                ReactiveCommand.CreateAsyncObservable(
                    this.ObservableForProperty(x => x.SearchText)
                      .Select(x => !string.IsNullOrEmpty(SearchText)),
                    _ =>
                    {
                        return dependenciesFile
                            .SearchPackagesByName(
                                SearchText,
                                FSharpOption<CancellationToken>.None, // TODO: Some(cancellationToken),
                                FSharpOption<int>.None)
                            .SelectMany(x => x)
                            .Select(x => new NugetResult {PackageName = x});
                    });

            //TODO: Localization
            var errorMessage = "NuGet packages couldn't be loaded.";
            var errorResolution = "You may not have internet or NuGet may be down.";
            
            SearchNuget.ThrownExceptions
                .Select(ex => new UserError(errorMessage, errorResolution))
                .SelectMany(UserError.Throw)
                .Subscribe();
            SearchNuget.IsExecuting
                .Where(isExecuting => isExecuting)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    NugetResults.Clear();
                });
            SearchNuget.Subscribe(NugetResults.Add);

            AddPackage = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.SelectedPackage).Select(x => x != null),
                _ => Task.Run(() => addPackageCallback(SelectedPackage)));
            
            this.ObservableForProperty(x => x.SearchText)
                .Where(x => !string.IsNullOrEmpty(SearchText))
                .Throttle(TimeSpan.FromMilliseconds(250))
                .InvokeCommand(SearchNuget);
        }
    }
}
