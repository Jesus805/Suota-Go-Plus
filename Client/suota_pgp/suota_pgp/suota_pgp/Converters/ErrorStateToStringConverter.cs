using suota_pgp.Data;
using suota_pgp.Properties;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace suota_pgp.Converters
{
    public class ErrorStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ErrorState errorState)
            {
                if (errorState.HasFlag(ErrorState.LocationUnauthorized) &&
                    errorState.HasFlag(ErrorState.StorageUnauthorized))
                {
                    return Resources.EnableLocationAndStorageString;
                }
                else if (errorState.HasFlag(ErrorState.StorageUnauthorized))
                {
                    return Resources.EnableStorageString;
                }
                else if (errorState.HasFlag(ErrorState.LocationUnauthorized))
                {
                    return Resources.EnableLocationString;
                }
                else if (errorState.HasFlag(ErrorState.BluetoothDisabled))
                {
                    return Resources.BluetoothDisabledString;
                }

                return string.Empty;
            }
            else
            {
                throw new ArgumentException(string.Format(Resources.ConverterValueError, nameof(ErrorState)), nameof(value));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
