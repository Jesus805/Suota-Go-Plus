using suota_pgp.Data;
using suota_pgp.Properties;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace suota_pgp.Converters
{
    public class ErrorStateToIsVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ErrorState errorState)
            {
                return errorState == ErrorState.None;
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
