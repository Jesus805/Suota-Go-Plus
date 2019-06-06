using System;
using System.Globalization;
using Xamarin.Forms;

namespace suota_pgp
{
    public class AppStateToBoolConverter : IValueConverter
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
                throw new ArgumentException("State expected", "value");
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
