using suota_pgp.Data;
using suota_pgp.Properties;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace suota_pgp.Converters
{
    public class AppStateScanningToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AppState state)
            {
                if (parameter is string inverse && 
                    inverse == "inverse")
                {
                    return state != AppState.Scanning;
                }
                else
                {
                    return state == AppState.Scanning;
                }
            }
            else
            {
                throw new ArgumentException(string.Format(Resources.ConverterValueError, nameof(AppState)), nameof(value));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
