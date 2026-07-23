using System;
using Microsoft.UI.Xaml.Data;

namespace CoolapkUWP.Helpers.Converters
{
    public partial class ZoomToolTipValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => ConverterTools.Convert($"{value}%", targetType);

        public object ConvertBack(object value, Type targetType, object parameter, string language) => ConverterTools.Convert(value.ToString().Replace("%", string.Empty), targetType);
    }
}
