using suota_pgp.Data;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace suota_pgp.Converters
{
    public class ErrorStateToIsVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ErrorState state)
            {
                if (state == ErrorState.None)
                {
                    return true;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
