using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GeneSort.UI.Converters
{
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public InverseBoolToVisibilityConverter()
        {
        }

        public static InverseBoolToVisibilityConverter Instance { get; } = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isTrue)
            {
                return isTrue ? Visibility.Collapsed : Visibility.Visible;
            }

            // Fallback: If not a bool, default to Visible (or throw, depending on needs)
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }

            throw new ArgumentException("Value must be of type Visibility.", nameof(value));
        }
    }




}
