using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using GeneSort.WPF.ViewModels;

namespace GeneSort.WPF.Views;

public partial class SummaryReportListView : UserControl
{
    private SummaryReportListViewModel? model;
    private readonly GridViewColumnLayout columnLayout;

    public SummaryReportListView()
    {
        columnLayout = new(nameof(SummaryReportItem.Number), key => model?.Sort(key));
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
        model = DataContext as SummaryReportListViewModel;
        if (model is not null) model.PropertyChanged += ModelChanged;
        BuildColumns();
        if (model is not null) model.DisplaySettings.CaptureLayout = columnLayout.Capture;
    }

    private void ModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SummaryReportListViewModel.Keys)) BuildColumns();
        if (e.PropertyName is nameof(SummaryReportListViewModel.SortKey) or nameof(SummaryReportListViewModel.Descending)) UpdateHeaders();
    }

    private void BuildColumns()
    {
        columnLayout.Apply(ReportColumns.Columns, model?.Keys ?? [], model?.DisplaySettings);
        UpdateHeaders();
    }

    private void UpdateHeaders()
    {
        foreach (var column in ReportColumns.Columns)
            if (column.Header is GridViewColumnHeader { Tag: string key } header)
                header.Content = key + (model?.SortKey == key ? model.Descending ? " ▼" : " ▲" : "");
    }

    public Task<bool> SaveCombinedReportAsync(string path, bool overwrite)
    {
        var current = DataContext as SummaryReportListViewModel;
        if (current is null || !current.CanSaveCombined) return Task.FromResult(false);
        // GridView's collection reflects drag-reordered columns; Tags retain the raw names without sort arrows.
        var order = ReportColumns.Columns.Select(column => (column.Header as GridViewColumnHeader)?.Tag as string).ToArray();
        return current.SaveCombinedAsync(path, order, overwrite);
    }

    private void ChooseColumns_Click(object sender, RoutedEventArgs e)
    {
        if (model is null) return;
        columnLayout.OpenChooser((Button)sender, () =>
        {
            UpdateHeaders();
            model?.ColumnVisibilityChanged();
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
        model.ColumnVisibilityChanged();
        var save = model.DisplaySettings.SaveChanges;
        var error = save?.Invoke();
        LayoutStatus.Text = error is not null ? $"Layout reset, but settings were not saved. {error}"
            : save is null ? "Default layout restored." : "Default layout restored and saved.";
    }

    private async void SaveCombined_Click(object sender, RoutedEventArgs e)
    {
        if (model?.CanSaveCombined != true) return;
        var dialog = new SaveFileDialog
        {
            Title = "Save combined Summary Reports", FileName = "SummaryReports_combined.tsv",
            DefaultExt = ".tsv", Filter = "Tab-separated values (*.tsv)|*.tsv|Tab-separated text (*.txt)|*.txt",
            AddExtension = true, CheckPathExists = true, OverwritePrompt = false
        };
        var owner = Window.GetWindow(this);
        if (dialog.ShowDialog(owner) != true) return;
        var overwrite = File.Exists(dialog.FileName);
        if (overwrite && MessageDialog.Show(owner, $"Replace this combined Summary Report?\n\n{dialog.FileName}",
                "Save combined Summary Reports", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        await SaveCombinedReportAsync(dialog.FileName, overwrite);
    }
}
