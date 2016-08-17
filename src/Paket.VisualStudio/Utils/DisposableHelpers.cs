using System;
using System.Reactive.Disposables;

namespace Paket.VisualStudio.Utils
{
    public static class DisposableHelpers
    {
        /// <summary>
        /// Helper function to add IDisposables to a composable disposable.
        /// This allows for easy chaining when using Reactive Extensions.
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="compositeDisposable"></param>
        public static void AddTo(this IDisposable disposable, CompositeDisposable compositeDisposable)
        {
            compositeDisposable.Add(disposable);
        }
    }
}
