using System;
using System.Windows;
using ReactiveUI;

namespace Paket.VisualStudio.Commands.PackageGui.Converters
{
    public class LoadingSuccessFailureVisiblityConverter : IBindingTypeConverter
    {
        public int GetAffinityForObjects(Type fromType, Type toType)
        {
            if (fromType == typeof (LoadingState) && toType == typeof (Visibility)) {return 10;}

            return 0;
        }

        public bool TryConvert(object @from, Type toType, object conversionHint, out object result)
        {
            var hint = conversionHint is LoadingState
                ? (LoadingState) conversionHint
                : LoadingState.Loading;
            var fromLoadingState = (LoadingState) @from;

            result = fromLoadingState == hint ? Visibility.Visible : Visibility.Collapsed;

            return true;
        }
    }
}
