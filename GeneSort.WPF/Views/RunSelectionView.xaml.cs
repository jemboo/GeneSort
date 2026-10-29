using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GeneSort.WPF.ViewModels;

namespace GeneSort.WPF.Views;

public partial class RunSelectionView : UserControl
{
    public RunSelectionView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (DataContext is RunSelectionViewModel model) await model.InitializeAsync();
        };
    }
    private RunSelectionViewModel Model => (RunSelectionViewModel)DataContext;
    public event Action<RunSelectionTile>? RunChosen;
    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Choose the folder containing your Sortopia projects" };
        if (dialog.ShowDialog(Window.GetWindow(this)) == true) Model.Folder = dialog.FolderName;
    }
    private async void Continue_Click(object sender, RoutedEventArgs e) => await Model.LoadAsync();
    private async void Refresh_Click(object sender, RoutedEventArgs e) => await Model.LoadAsync(false);
    private void Back_Click(object sender, RoutedEventArgs e) => Model.Back();
    private void Home_Click(object sender, RoutedEventArgs e) => Model.Home();
    private void Folder_Click(object sender, RoutedEventArgs e) => Model.ChangeFolder();
    private void Breadcrumb_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: RunBreadcrumb breadcrumb }) Model.JumpTo(breadcrumb);
    }
    private void Tile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: RunSelectionTile tile } && !Model.Navigate(tile) && tile.Definition is not null)
            RunChosen?.Invoke(tile);
    }
}
