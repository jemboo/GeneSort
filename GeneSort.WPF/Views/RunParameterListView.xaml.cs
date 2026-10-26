using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GeneSort.WPF.ViewModels;

namespace GeneSort.WPF.Views;

public partial class RunParameterListView : UserControl
{
    private RunParameterListViewModel? model;

    public RunParameterListView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Attach();
        Loaded += (_, _) => Attach();
        Unloaded += (_, _) => Detach();
    }

    private void Detach()
    {
        if (model is not null) model.PropertyChanged -= ModelChanged;
        model = null;
    }

    private void Attach()
    {
        Detach();
        model = DataContext as RunParameterListViewModel;
        if (model is not null) model.PropertyChanged += ModelChanged;
        BuildColumns();
    }

    private void ModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RunParameterListViewModel.Keys)) BuildColumns();
        if (e.PropertyName is nameof(RunParameterListViewModel.SortKey) or nameof(RunParameterListViewModel.Descending)) UpdateHeaders();
    }

    private void BuildColumns()
    {
        ParameterColumns.Columns.Clear();
        ParameterColumns.Columns.Add(new() { Header = "#", Width = 45, DisplayMemberBinding = new Binding(nameof(RunParameterItem.Number)) });
        if (model is null) return;
        for (var index = 0; index < model.Keys.Length; index++)
        {
            var key = model.Keys[index];
            var header = new GridViewColumnHeader { Content = key, Tag = key, ToolTip = $"Sort by {key}" };
            header.Click += Sort_Click;
            ParameterColumns.Columns.Add(new()
            {
                Header = header, Width = Math.Clamp(key.Length * 7 + 32, 110, 260),
                DisplayMemberBinding = new Binding($"Cells[{index}]")
            });
        }
        UpdateHeaders();
    }

    private void Sort_Click(object sender, RoutedEventArgs e)
    {
        if (sender is GridViewColumnHeader { Tag: string key }) model?.Sort(key);
    }

    private void UpdateHeaders()
    {
        foreach (var column in ParameterColumns.Columns)
            if (column.Header is GridViewColumnHeader { Tag: string key } header)
                header.Content = key + (model?.SortKey == key ? model.Descending ? " ▼" : " ▲" : "");
    }
}
