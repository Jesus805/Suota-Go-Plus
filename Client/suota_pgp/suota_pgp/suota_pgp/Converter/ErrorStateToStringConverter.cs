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
                return null;

            if (value is ErrorState state)
            {
                if (state.HasFlag(ErrorState.LocationUnauthorized) &&
                    state.HasFlag(ErrorState.StorageUnauthorized))
                {
                    return "⚠️ Please enable location and storage permissions.";
                }
                else if (state.HasFlag(ErrorState.StorageUnauthorized))
                {
                    return "⚠️ Please enable storage permissions";
                }
                else if (state.HasFlag(ErrorState.LocationUnauthorized))
                {
                    return "⚠️ Please enable location permissions.";
                }
                else if (state.HasFlag(ErrorState.BluetoothDisabled))
                {
                    return "⚠️ Bluetooth adapter is disabled.";
                }

                return string.Empty;
            }
            else
            {
                throw new ArgumentException("value must be ErrorState", "value");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
