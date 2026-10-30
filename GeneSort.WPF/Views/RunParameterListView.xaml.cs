using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using GeneSort.WPF.ViewModels;

namespace GeneSort.WPF.Views;

public partial class RunParameterListView : UserControl
{
    private RunParameterListViewModel? model;
    private readonly GridViewColumnLayout columnLayout;

    public RunParameterListView()
    {
        columnLayout = new(nameof(RunParameterItem.Number), key => model?.Sort(key),
            key => key == "RunStatus" ? (DataTemplate)FindResource("ProcessingStatusTemplate") : null);
        InitializeComponent();
        DataContextChanged += (_, _) => Attach();
        Loaded += (_, _) => Attach();
        Unloaded += (_, _) => Detach();
    }

    private void FailedStatus_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkContentElement { DataContext: RunParameterItem { Status: "Failed" } row })
        {
            e.Handled = true;
            MessageDialog.Show(Window.GetWindow(this), row.FailureDetails, "Run failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Detach()
    {
        columnLayout.CloseChooser();
        if (model is not null)
        {
            columnLayout.Capture();
            model.DisplaySettings.CaptureLayout = null;
            model.PropertyChanged -= ModelChanged;
        }
        model = null;
    }

    private void Attach()
    {
        Detach();
        model = DataContext as RunParameterListViewModel;
        if (model is not null) model.PropertyChanged += ModelChanged;
        BuildColumns();
        if (model is not null) model.DisplaySettings.CaptureLayout = columnLayout.Capture;
    }

    private void ModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Phase completion keeps live path objects while clearing the completed selection.
        if (e.PropertyName == nameof(RunParameterListViewModel.SelectedCount)
            && model?.SelectedCount == 0 && ParameterItems.SelectedItems.Count > 0)
            ParameterItems.SelectedItems.Clear();
        if (e.PropertyName == nameof(RunParameterListViewModel.Keys)) BuildColumns();
        if (e.PropertyName is nameof(RunParameterListViewModel.SortKey) or nameof(RunParameterListViewModel.Descending)) UpdateHeaders();
    }

    private void BuildColumns()
    {
        columnLayout.Apply(ParameterColumns.Columns, model?.Keys ?? [], model?.DisplaySettings);
        UpdateHeaders();
    }

    private bool updatingSelection;
    public void SelectCompleted()
    {
        var completed = ParameterItems.Items.Cast<RunParameterItem>()
            .Where(row => row.Parameters.GetRunFinished()?.Value == true).ToArray();
        updatingSelection = true;
        try
        {
            ParameterItems.SelectedItems.Clear();
            foreach (var row in completed) ParameterItems.SelectedItems.Add(row);
        }
        finally { updatingSelection = false; }
        model?.Select(completed);
    }

    private void SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!updatingSelection)
            (DataContext as RunParameterListViewModel)?.Select(ParameterItems.SelectedItems.Cast<RunParameterItem>());
    }

    private void ChooseColumns_Click(object sender, RoutedEventArgs e)
    {
        if (model is null) return;
        columnLayout.OpenChooser((Button)sender, () =>
        {
            UpdateHeaders();
            var save = model?.DisplaySettings.SaveChanges;
            var error = save?.Invoke();
            LayoutStatus.Text = error is not null ? $"Column visibility changed, but settings were not saved. {error}"
                : save is null ? "Column visibility updated." : "Column visibility saved.";
        });
    }

    private void ResetLayout_Click(object sender, RoutedEventArgs e)
    {
        if (model is null) return;
        columnLayout.Reset();
        BuildColumns();
        model.ResetSort();
        var save = model.DisplaySettings.SaveChanges;
        var error = save?.Invoke();
        LayoutStatus.Text = error is not null ? $"Layout reset, but settings were not saved. {error}"
            : save is null ? "Default layout restored." : "Default layout restored and saved.";
    }

    private void UpdateHeaders()
    {
        foreach (var column in ParameterColumns.Columns)
            if (column.Header is GridViewColumnHeader { Tag: string key } header)
                header.Content = key + (model?.SortKey == key ? model.Descending ? " ▼" : " ▲" : "");
    }
}
