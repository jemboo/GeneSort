using System.ComponentModel;
using System.Windows;
using GeneSort.WPF.ViewModels;

namespace GeneSort.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is MainWindowViewModel model)
        {
            if (model.RunEditor.IsBusy)
                e.Cancel = true;
            else if (model.RunEditor.HasUnsavedChanges && MessageBox.Show(this,
                "Discard the unsaved run definition changes?", "Run editor",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                e.Cancel = true;
        }
        base.OnClosing(e);
    }
}
