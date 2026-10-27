using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using GeneSort.Project.V1.Parameters;
using GeneSort.WPF.Settings;
using RunParameters = GeneSort.Project.V1.Parameters.parameterSet<GeneSort.Project.V1.Parameters.runParameterKind>;

namespace GeneSort.WPF.ViewModels;

public sealed class RunParameterItem
{
    public int Number { get; }
    public RunParameters Parameters { get; }
    public IReadOnlyDictionary<string, string> Values { get; }
    public string?[] Cells { get; }

    internal RunParameterItem(int number, RunParameters parameters, string[] keys)
    {
        Number = number;
        Parameters = parameters;
        Values = parameters.ParamMap.ToDictionary(pair => pair.Key, pair => pair.Value);
        Cells = keys.Select(key => Values.GetValueOrDefault(key)).ToArray();
    }
}

/// A reusable, read-only collection view; sorting never changes the saved parameters.
public sealed class RunParameterListViewModel : INotifyPropertyChanged
{
    internal TableLayoutSettings DisplaySettings { get; }
    public RunParameterListViewModel() : this(App.TableLayouts?.RunParameters ?? new()) { }
    internal RunParameterListViewModel(TableLayoutSettings settings) => DisplaySettings = settings;

    public string[] Keys { get; private set; } = [];
    public ListCollectionView Items { get; private set; } = new(new List<RunParameterItem>());
    public string Summary { get; private set; } = "Generate parameters or refresh saved files to view this run.";
    public string? SortKey => DisplaySettings.SortKey;
    public bool Descending => DisplaySettings.Descending;
    public int Count => Items.Count;
    private RunParameters[] selectedParameters = [];
    public RunParameters[] SelectedParameters => selectedParameters.ToArray();
    public int SelectedCount => selectedParameters.Length;
    public string SelectionSummary => $"{SelectedCount:N0} selected. Use Ctrl+click or Shift+click to select rows.";

    public void Select(IEnumerable<RunParameterItem> rows)
    {
        selectedParameters = rows.Select(row => row.Parameters).ToArray();
        Changed(nameof(SelectedCount)); Changed(nameof(SelectionSummary));
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Changed(string property) => PropertyChanged?.Invoke(this, new(property));

    public void Clear(string message = "Refresh saved files to view this run.")
    {
        Keys = [];
        Items = new(new List<RunParameterItem>());
        Select([]);
        Summary = message;
        NotifyRows();
    }

    public void SetParameters(IEnumerable<RunParameters> parameters)
    {
        var data = parameters.ToArray();
        string[] leading = [ParamKeys.replKey, ParamKeys.mutationModKey, ParamKeys.sourceMutationModKey,
            ParamKeys.partitionCountKey, ParamKeys.generationCurrentKey, ParamKeys.runNameKey];
        var keys = data.SelectMany(row => row.ParamMap.Keys).Distinct(StringComparer.Ordinal).ToArray();
        Keys = leading.Where(keys.Contains).Concat(keys.Except(leading).Order(StringComparer.Ordinal)).ToArray();
        Items = new(data.Select((row, index) => new RunParameterItem(index + 1, row, Keys)).ToList());
        Select([]);
        var replicas = data.Select(row => row.GetRepl()?.Value).Distinct().Count();
        Summary = data.Length == 0 ? "No saved RunParameter files for this run."
            : $"{data.Length:N0} saved RunParameters across {replicas:N0} replica(s). Click a column heading to sort; click again to reverse.";
        NotifyRows();
        // Keep the preferred sort through empty results or temporarily absent fields.
        var sortKey = SortKey ?? Keys.FirstOrDefault();
        if (sortKey is not null && Keys.Contains(sortKey)) ApplySort(sortKey, Descending);
    }

    private void NotifyRows()
    {
        foreach (var property in new[] { nameof(Keys), nameof(Items), nameof(Count), nameof(Summary), nameof(SortKey), nameof(Descending) })
            Changed(property);
    }

    public void Sort(string key)
    {
        if (Keys.Contains(key)) ApplySort(key, SortKey == key && !Descending);
    }

    public void ResetSort()
    {
        DisplaySettings.SortKey = null;
        DisplaySettings.Descending = false;
        if (Keys.FirstOrDefault() is string key) ApplySort(key, false);
        else
        {
            Items.CustomSort = null;
            Changed(nameof(SortKey)); Changed(nameof(Descending));
        }
    }

    private void ApplySort(string key, bool descending)
    {
        var index = Array.IndexOf(Keys, key);
        Items.CustomSort = TableColumnSort.Create(Items.SourceCollection.Cast<RunParameterItem>(), row => row.Cells[index], row => row.Number, descending);
        DisplaySettings.SortKey = key;
        DisplaySettings.Descending = descending;
        Changed(nameof(SortKey)); Changed(nameof(Descending));
    }

}
