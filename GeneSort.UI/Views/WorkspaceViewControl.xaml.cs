using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GeneSort.UI.Views
{
    public partial class WorkspaceViewControl : UserControl
    {
        public WorkspaceViewControl()
        {
            InitializeComponent();
        }
    }

    // Converter for null to visibility
    public class NullToVisibilityConverter : IValueConverter
    {
        public static NullToVisibilityConverter Visible { get; } = new NullToVisibilityConverter { NullValue = Visibility.Visible, NotNullValue = Visibility.Collapsed };
        public static NullToVisibilityConverter Collapsed { get; } = new NullToVisibilityConverter { NullValue = Visibility.Collapsed, NotNullValue = Visibility.Visible };

        public Visibility NullValue { get; set; } = Visibility.Collapsed;
        public Visibility NotNullValue { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? NullValue : NotNullValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}