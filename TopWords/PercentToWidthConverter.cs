using System;
using System.Globalization;
using System.Windows.Data;

namespace TopWordsTestApp
{
    /// <summary>
    /// Converts percentage to width
    /// </summary>
    internal class PercentToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            var count = (int) value[0];
            var max = (int) value[1];
            var parentWidth = (double) value[2];

            return max != 0 ? parentWidth*count/max : 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}