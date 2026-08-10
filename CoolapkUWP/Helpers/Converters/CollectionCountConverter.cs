using System;
using System.Collections;
using Microsoft.UI.Xaml.Data;

namespace CoolapkUWP.Helpers.Converters
{
    public partial class CollectionCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int result = value is ICollection collection ? collection.Count : 0;
            return ConverterTools.Convert(result, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => ConverterTools.Convert(value, targetType);
    }
}
