using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using GeneSort.Core;
using GeneSort.FileDb.V1;
using GeneSort.WPF.Settings;

namespace GeneSort.WPF.ViewModels;

public sealed class SummaryReportItem
{
    public int Number { get; }
    public string SourcePath { get; }
    public string?[] Cells { get; }

    internal SummaryReportItem(int number, string sourcePath, string?[] cells)
    {
        Number = number;
        SourcePath = sourcePath;
        Cells = cells;
    }
}

/// Combines selected reports by column name, preserving every saved report row.
public sealed class SummaryReportListViewModel : INotifyPropertyChanged
{
    internal TableLayoutSettings DisplaySettings { get; }
    public SummaryReportListViewModel() : this(App.TableLayouts?.SummaryReports ?? new()) { }
    internal SummaryReportListViewModel(TableLayoutSettings settings) => DisplaySettings = settings;

    public string[] Keys { get; private set; } = [];
    public ListCollectionView Items { get; private set; } = new(new List<SummaryReportItem>());
    public string Summary { get; private set; } = "Select rows in Run parameters to view their saved Summary Reports.";
    public string[] Errors { get; private set; } = [];
    public string? SortKey => DisplaySettings.SortKey;
    public bool Descending => DisplaySettings.Descending;
    public bool IsLoading { get; private set; }
    public bool IsSaving { get; private set; }
    public bool CanSaveCombined => !IsLoading && !IsSaving && Count > 0;
    public string SaveStatus { get; private set; } = "";
    public int Count => Items.Count;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Changed(string property) => PropertyChanged?.Invoke(this, new(property));
    private void NotifyRows()
    {
        foreach (var property in new[] { nameof(Keys), nameof(Items), nameof(Count), nameof(Summary), nameof(Errors), nameof(IsLoading), nameof(SortKey), nameof(Descending), nameof(CanSaveCombined), nameof(SaveStatus) })
            Changed(property);
    }

    public void Clear(string message = "Select rows in Run parameters to view their saved Summary Reports.", bool loading = false)
    {
        Keys = [];
        Items = new(new List<SummaryReportItem>());
        Errors = [];
        Summary = message;
        IsLoading = loading;
        if (!IsSaving) SaveStatus = "";
        NotifyRows();
    }

    public void SetReports(summaryReportFiles result, int selectedCount)
    {
        var reports = result.Reports;
        var keys = reports.SelectMany(report => report.Headers).Distinct(StringComparer.Ordinal).ToArray();
        string[] leading = ["RunName", "repl", "MutationMod", "SourceMutationMod", "PartitionCount", "GenerationNumber"];
        Keys = leading.Where(keys.Contains).Concat(keys.Except(leading)).ToArray();
        var items = new List<SummaryReportItem>();
        foreach (var report in reports)
        {
            var indices = report.Headers.Select((key, index) => (key, index)).ToDictionary(pair => pair.key, pair => pair.index);
            foreach (var row in report.Rows)
                items.Add(new(items.Count + 1, report.Path, Keys.Select(key => indices.TryGetValue(key, out var index) ? row[index] : null).ToArray()));
        }
        Items = new(items);
        Errors = result.Errors;
        IsLoading = false;
        Summary = $"{items.Count:N0} summary rows from {reports.Length:N0} reports · {selectedCount:N0} RunParameter rows selected. " +
            "Latest saved report per selection. Click a column heading to sort; click again to reverse.";
        if (Errors.Length > 0) Summary += $" {Errors.Length:N0} reports unavailable; see details below.";
        NotifyRows();
        // Keep the preferred sort when a selection temporarily omits its column.
        var sortKey = SortKey ?? Keys.FirstOrDefault();
        if (sortKey is not null && Keys.Contains(sortKey)) ApplySort(sortKey, Descending);
    }

    public void Sort(string key)
    {
        if (Keys.Contains(key)) ApplySort(key, SortKey == key && !Descending);
    }

    /// Called on the UI thread: snapshot the collection view before handing file IO to FileDb.
    /// A null column key denotes the displayed row-number column.
    public async Task<bool> SaveCombinedAsync(string path, string?[] columnOrder, bool overwrite)
    {
        if (!CanSaveCombined) return false;
        IsSaving = true;
        Changed(nameof(IsSaving)); Changed(nameof(CanSaveCombined));
        try
        {
            var dataColumns = columnOrder.Where(key => key is not null).ToArray();
            if (columnOrder.Length != Keys.Length + 1 || dataColumns.Length != Keys.Length ||
                dataColumns.Distinct(StringComparer.Ordinal).Count() != Keys.Length || dataColumns.Any(key => !Keys.Contains(key)))
                throw new ArgumentException("The displayed columns have changed. Try saving again.");
            var indices = columnOrder.Select(key => key is null ? -1 : Array.IndexOf(Keys, key)).ToArray();
            var report = DataTableReport.create("Combined Summary Reports", columnOrder.Select(key => key ?? "#").ToArray());
            // Enumerating Items respects sorting/filtering and includes rows outside the virtualized viewport.
            foreach (var row in Items.Cast<SummaryReportItem>())
                report.AppendDataRow(indices.Select(index => index < 0 ? row.Number.ToString(CultureInfo.InvariantCulture) : row.Cells[index] ?? "").ToArray());
            var count = report.DataRows.Count;
            SaveStatus = $"Saving {count:N0} summary rows…";
            Changed(nameof(SaveStatus));
            var savedPath = await RunSummaryFiles.saveCombinedAsync(path, report, overwrite);
            SaveStatus = $"Saved {count:N0} summary rows to {savedPath}";
            return true;
        }
        catch (Exception error)
        {
            SaveStatus = $"Could not save combined report: {error.GetBaseException().Message}";
            return false;
        }
        finally
        {
            IsSaving = false;
            Changed(nameof(IsSaving)); Changed(nameof(CanSaveCombined)); Changed(nameof(SaveStatus));
        }
    }

    private void ApplySort(string key, bool descending)
    {
        var index = Array.IndexOf(Keys, key);
        Items.CustomSort = TableColumnSort.Create(Items.SourceCollection.Cast<SummaryReportItem>(), row => row.Cells[index], row => row.Number, descending);
        DisplaySettings.SortKey = key;
        DisplaySettings.Descending = descending;
        Changed(nameof(SortKey)); Changed(nameof(Descending));
    }
}
