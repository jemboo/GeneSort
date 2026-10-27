using System.ComponentModel;
using System.Windows;
using GeneSort.WPF.Themes;
using GeneSort.WPF.ViewModels;
using GeneSort.WPF.Views;

namespace GeneSort.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        WindowTheme.Apply(this);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is MainWindowViewModel model)
        {
            if (model.RunEditor.IsBusy)
                e.Cancel = true;
            else if (model.RunEditor.HasUnsavedChanges && MessageDialog.Show(this,
                "Discard the unsaved run definition changes?", "Run editor",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                e.Cancel = true;
        }
        base.OnClosing(e);
        // A themed WPF dialog needs a live dispatcher and owner, before application shutdown.
        if (!e.Cancel && App.TableLayouts?.Save() is string error)
            MessageDialog.Show(this, error, "Table display settings", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
