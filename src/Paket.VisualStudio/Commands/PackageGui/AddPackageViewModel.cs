using System.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Splat;

namespace Paket.VisualStudio.Commands.PackageGui
{
    /// <summary>
    /// This helps with keeping design time view models in sync with real view models
    /// </summary>
    public interface IAddPackageViewModel
    {
        string SearchText { get; set; }
        NugetResult SelectedPackage { get; set; }
        IEnumerable<NugetResult> NugetResults { get; }

        ReactiveCommand<IEnumerable<NugetResult>> SearchNuget { get; }
        ReactiveCommand<System.Reactive.Unit> AddPackage { get; }
        IObservable<string> PaketTrace { get; }
        LoadingState AddPackageState { get; }
    }

    public class NugetResult
    {
        public string PackageName { get; set; }
    }

    public enum LoadingState
    {
        Loading,
        Success,
        Failure
    }

    public class AddPackageViewModel : ReactiveObject, IAddPackageViewModel
    {
        private readonly Func<string, CancellationToken, Task<string[]>> _findPackageCallback;
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

        private ObservableAsPropertyHelper<LoadingState> _addPackageState;

        public LoadingState AddPackageState
        {
            get { return _addPackageState.Value; }
        }

        private ObservableAsPropertyHelper<IEnumerable<NugetResult>> _results;

        public IEnumerable<NugetResult> NugetResults
        {
            get { return _results.Value; }
        }


        public ReactiveCommand<IEnumerable<NugetResult>> SearchNuget { get; private set; }


        public ReactiveCommand<System.Reactive.Unit> AddPackage { get; private set; }

        public AddPackageViewModel(
            Func<string, CancellationToken, Task<string[]>> findPackageCallback,
            Action<NugetResult> addPackageCallback,
            IObservable<string> paketTraceFunObservable)
        {
            var logger = new DebugLogger() {Level = LogLevel.Debug};
            Splat.Locator.CurrentMutable.RegisterConstant(logger, typeof(ILogger));

            _findPackageCallback = findPackageCallback;
            _paketTraceFunObservable = paketTraceFunObservable;
            SearchNuget = ReactiveCommand.CreateAsyncTask(
                this.ObservableForProperty(x => x.SearchText)
                .Select(x => !string.IsNullOrEmpty(SearchText)), 
                (_, cancellationToken) => SearchPackagesByName(SearchText, cancellationToken));

            //TODO: Localization
            var errorMessage = "NuGet packages couldn't be loaded.";
            var errorResolution = "You may not have internet or NuGet may be down.";
            
            SearchNuget.ThrownExceptions
                .Log(this, "Search NuGet exception:", e => e.ToString())
                .Select(ex => new UserError(errorMessage, errorResolution))
                .SelectMany(UserError.Throw)
                .Subscribe();
            this.WhenAnyObservable(x => x._results.ThrownExceptions)
                .Log(this, "Results observable property exception:", e => e.ToString())
                .Subscribe();

            _results = SearchNuget.ToProperty(this, x => x.NugetResults, out _results);

            AddPackage = ReactiveCommand.CreateAsyncTask(
                this.WhenAnyValue(x => x.SelectedPackage).Select(x => x != null),
                _ => Task.Run(() => addPackageCallback(SelectedPackage)));

            _addPackageState = AddPackage
                .ThrownExceptions
                .Log(this, "Add Package exception:", e => e.ToString())
                .Select(_ => LoadingState.Failure)
                .Merge(
                   AddPackage
                    .IsExecuting
                    .Where(isExecuting => isExecuting)
                    .Select(_ => LoadingState.Loading))
                .Merge(
                     AddPackage
                        .Select(_ => LoadingState.Success))
                .ToProperty(this, v => v.AddPackageState, out _addPackageState);

            _addPackageState
                .ThrownExceptions
                .Log(this, "Add package state exception:", e => e.ToString())
                .Select(ex => new UserError("", errorResolution))
                .SelectMany(UserError.Throw)
                .Subscribe();
            
            this.ObservableForProperty(x => x.SearchText)
                .Where(x => !string.IsNullOrEmpty(SearchText))
                .Throttle(TimeSpan.FromMilliseconds(250))
                .InvokeCommand(SearchNuget);
        }

        private async Task<IEnumerable<NugetResult>>  SearchPackagesByName(string name, CancellationToken cancellationToken)
        {
            var results = await _findPackageCallback(name, cancellationToken);

            return results.Select(r => new NugetResult { PackageName = r });
        }
    }
}
