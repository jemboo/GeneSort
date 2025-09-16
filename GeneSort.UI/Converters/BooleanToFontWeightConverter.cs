using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GeneSort.UI.Converters
{
    public class BooleanToFontWeightConverter : IValueConverter
    {
        public static BooleanToFontWeightConverter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? FontWeights.Bold : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
