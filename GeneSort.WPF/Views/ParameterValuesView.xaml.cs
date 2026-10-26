using System.Windows;
using System.Windows.Controls;
using GeneSort.WPF.ViewModels;

namespace GeneSort.WPF.Views;

public partial class ParameterValuesView : UserControl
{
    public ParameterValuesView() => InitializeComponent();
    private void AddValue_Click(object sender, RoutedEventArgs e) => ((ParameterRow)DataContext).AddValue();
    private void RemoveValue_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ParameterValue value })
            ((ParameterRow)DataContext).Values.Remove(value);
    }
}
