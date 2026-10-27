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
        columnLayout = new(nameof(RunParameterItem.Number), key => model?.Sort(key));
        InitializeComponent();
        DataContextChanged += (_, _) => Attach();
        Loaded += (_, _) => Attach();
        Unloaded += (_, _) => Detach();
    }

    private void Detach()
    {
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
        if (e.PropertyName == nameof(RunParameterListViewModel.Keys)) BuildColumns();
        if (e.PropertyName is nameof(RunParameterListViewModel.SortKey) or nameof(RunParameterListViewModel.Descending)) UpdateHeaders();
    }

    private void BuildColumns()
    {
        columnLayout.Apply(ParameterColumns.Columns, model?.Keys ?? [], model?.DisplaySettings);
        UpdateHeaders();
    }

    private void SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        (DataContext as RunParameterListViewModel)?.Select(ParameterItems.SelectedItems.Cast<RunParameterItem>());

    private void UpdateHeaders()
    {
        foreach (var column in ParameterColumns.Columns)
            if (column.Header is GridViewColumnHeader { Tag: string key } header)
                header.Content = key + (model?.SortKey == key ? model.Descending ? " ▼" : " ▲" : "");
    }
}
