using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GeneSort.UI.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public static StringToVisibilityConverter Instance { get; } = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var str = value?.ToString();
            var hiddenValue = parameter?.ToString() ?? "Projects";

            return str == hiddenValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
