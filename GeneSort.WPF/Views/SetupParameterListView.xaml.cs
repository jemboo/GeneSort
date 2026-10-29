using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using GeneSort.WPF.ViewModels;

namespace GeneSort.WPF.Views;

public partial class SetupParameterListView : UserControl
{
    private SetupParameterListViewModel? model;
    private readonly GridViewColumnLayout columnLayout;

    public SetupParameterListView()
    {
        columnLayout = new(nameof(SetupParameterItem.Number), key => model?.Sort(key));
        InitializeComponent();
        DataContextChanged += (_, _) => Attach();
        Loaded += (_, _) => Attach();
        Unloaded += (_, _) => Detach();
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
        model = DataContext as SetupParameterListViewModel;
        if (model is not null) model.PropertyChanged += ModelChanged;
        BuildColumns();
        if (model is not null) model.DisplaySettings.CaptureLayout = columnLayout.Capture;
    }

    private void ModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Refreshing shared setups clears the previous selection.
        if (e.PropertyName == nameof(SetupParameterListViewModel.SelectedCount)
            && model?.SelectedCount == 0 && ParameterItems.SelectedItems.Count > 0)
            ParameterItems.SelectedItems.Clear();
        if (e.PropertyName == nameof(SetupParameterListViewModel.Keys)) BuildColumns();
        if (e.PropertyName is nameof(SetupParameterListViewModel.SortKey) or nameof(SetupParameterListViewModel.Descending)) UpdateHeaders();
    }

    private void BuildColumns()
    {
        columnLayout.Apply(ParameterColumns.Columns, model?.Keys ?? [], model?.DisplaySettings);
        UpdateHeaders();
    }

    private void SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        (DataContext as SetupParameterListViewModel)?.Select(ParameterItems.SelectedItems.Cast<SetupParameterItem>());

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

