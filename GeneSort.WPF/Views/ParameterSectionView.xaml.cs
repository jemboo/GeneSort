using System.Windows;
using System.Windows.Controls;
using GeneSort.WPF.ViewModels;

namespace GeneSort.WPF.Views;

public partial class ParameterSectionView : UserControl
{
    private ParameterSectionViewModel Model => (ParameterSectionViewModel)DataContext;
    public ParameterSectionView() => InitializeComponent();
    private void BeginAdd_Click(object sender, RoutedEventArgs e) => Model.BeginAdd();
    private void CancelAdd_Click(object sender, RoutedEventArgs e) => Model.CancelAdd();
    private void ConfirmAdd_Click(object sender, RoutedEventArgs e) => Model.Add();
    private void RemoveRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ParameterRow row }) Model.Rows.Remove(row);
    }
}
