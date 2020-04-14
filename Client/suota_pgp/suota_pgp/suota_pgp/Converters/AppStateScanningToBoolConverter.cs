using suota_pgp.Data;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace suota_pgp.Converters
{
    public class AppStateScanningToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result;
            if (value is AppState state)
            {
                result = (state == AppState.Scanning);
            }
            else
            {
                throw new ArgumentException("AppState expected", nameof(value));
            }

            if (parameter is string inverse)
            {
                if (inverse == "inverse")
                {
                    result = !result;
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not needed
            throw new NotImplementedException();
        }
    }
}
