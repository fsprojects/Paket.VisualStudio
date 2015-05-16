using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows.Input;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Threading.Tasks;

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

        ICommand SearchNuget { get; }
        ICommand AddPackage { get; }
    }

    public class NugetResult
    {
        public string PackageName { get; set; }
    }

    public class AddPackageViewModel : ReactiveObject, IAddPackageViewModel
    {
        private readonly Func<string, CancellationToken, Task<string[]>> _findPackageCallback;
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

        private ObservableAsPropertyHelper<IEnumerable<NugetResult>> _results;

        public IEnumerable<NugetResult> NugetResults
        {
            get { return _results.Value; }
        }

        private ReactiveCommand<IEnumerable<NugetResult>> _searchNuget;
        public ICommand SearchNuget { get { return _searchNuget; } }

        private ReactiveCommand<object> _addPackage;
        public ICommand AddPackage { get { return _addPackage; } }

        public AddPackageViewModel(
            Func<string, CancellationToken, Task<string[]>> findPackageCallback,
            Action<NugetResult> addPackageCallback)
        {
            _findPackageCallback = findPackageCallback;
            _searchNuget = ReactiveCommand.CreateAsyncTask((_, cancellationToken) => SearchPackagesByName(SearchText, cancellationToken));

            //TODO: Localization
            var errorMessage = "Nuget packages couldn't be loaded";
            var errorResolution = "You may not have internet or nuget may be down.";
            _searchNuget.ThrownExceptions
                .Select(ex => new UserError(errorMessage, errorResolution))
                .SelectMany(UserError.Throw)
                .Subscribe();
            _searchNuget.ToProperty(this, x => x.NugetResults, out _results);

            _addPackage = ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedPackage).Select(x => x != null));
            _addPackage.Subscribe(_ => addPackageCallback(SelectedPackage));
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
