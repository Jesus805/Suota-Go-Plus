using System;
using System.Globalization;
using Xamarin.Forms;

namespace suota_pgp
{
    public class ErrorStateToGridLengthConverter : IValueConverter
    {
        public readonly GridLength disabledLength = new GridLength(.5, GridUnitType.Star);

        public readonly GridLength enabledLength = new GridLength(0);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (value is ErrorState state)
            {
                if (state == ErrorState.None)
                    return enabledLength;             
                else
                    return disabledLength;
            }
            else
            {
                throw new ArgumentException("object value must be ErrorState");
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
