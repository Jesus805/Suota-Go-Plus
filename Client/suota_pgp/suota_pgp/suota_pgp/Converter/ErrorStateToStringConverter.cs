using System;
using System.Globalization;
using Xamarin.Forms;

namespace suota_pgp
{
    public class ErrorStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is ErrorState state)
            {
                if (state.HasFlag(ErrorState.LocationUnauthorized) &&
                    state.HasFlag(ErrorState.StorageUnauthorized))
                {
                    return Resources.EnableLocationAndStorageString;
                }
                else if (state.HasFlag(ErrorState.StorageUnauthorized))
                {
                    return Resources.EnableStorageString;
                }
                else if (state.HasFlag(ErrorState.LocationUnauthorized))
                {
                    return Resources.EnableLocationString;
                }
                else if (state.HasFlag(ErrorState.BluetoothDisabled))
                {
                    return Resources.BluetoothDisabledString;
                }

                return string.Empty;
            }
            else
            {
                throw new ArgumentException("value must be ErrorState", nameof(value));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
