using System;
using System.Globalization;
using MvvmCross.Platform.Converters;
using Xamarin.Forms;

namespace BLE.Client.Converters
{
    public class InverseBooleanValueConverter : MvxValueConverter<bool, bool>, IValueConverter
    {
        protected override bool Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return !value;
        }

        protected override bool ConvertBack(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return !value;
        }
    }
}