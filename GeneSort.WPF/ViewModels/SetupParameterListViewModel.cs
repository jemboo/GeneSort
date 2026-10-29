using System.ComponentModel;
using System.Windows.Data;
using GeneSort.Project.V1;
using GeneSort.Project.V1.Parameters;
using GeneSort.WPF.Settings;

namespace GeneSort.WPF.ViewModels;

/// One set of shared input files, with the saved execution rows that use it.
public sealed class SetupParameterItem
{
    public int Number { get; }
    public RunParameterItem[] Runs { get; }
    public IReadOnlyDictionary<string, string> Values { get; }
    public string?[] Cells { get; internal set; } = [];

    internal SetupParameterItem(int number, run configured, RunParameterItem[] runs)
    {
        Number = number;
        Runs = runs;
        var values = new Dictionary<string, string>();
        if (configured.ParameterSource is null)
        {
            // Show the inputs used by the explicit Setup constructors, not execution overrides.
            foreach (var property in SetupOutputs.generatedFiles(configured, runs[0].Parameters)
                .SelectMany(file => file.Properties).Where(pair => pair.Key != "SetupRecipe").GroupBy(pair => pair.Key))
                values[property.Key] = string.Join("; ", property.Select(pair => pair.Value).Distinct());
        }
        else
        {
            foreach (var pair in runs[0].Values.Where(pair => pair.Key.StartsWith("Source", StringComparison.Ordinal)
                || Run.parameterRole(configured, pair.Key).IsSetupOnly
                || Run.parameterRole(configured, pair.Key).IsInheritedReadOnly))
                values[pair.Key] = pair.Value;
        }
        foreach (var file in SetupOutputs.files(configured, runs[0].Parameters))
            values["Setup" + GeneSortDataType.toFolderName(file.OutputDataType) + "Id"] = file.Id.ToString();
        values[ParamKeys.replKey] = string.Join(", ", runs.Select(row => row.Parameters.GetRepl()?.Value).Distinct().Order());
        values["LinkedRuns"] = runs.Length.ToString();
        Values = values;
    }
}

public sealed class SetupParameterListViewModel : INotifyPropertyChanged
{
    internal TableLayoutSettings DisplaySettings { get; } = App.TableLayouts?.SetupParameters ?? new();
    public string[] Keys { get; private set; } = [];
    public ListCollectionView Items { get; private set; } = new(new List<SetupParameterItem>());
    public int Count => Items.Count;
    private SetupParameterItem[] selected = [];
    public int SelectedCount => selected.Length;
    public RunParameterItem[] SelectedRuns => selected.SelectMany(item => item.Runs).Distinct().ToArray();
    public string SelectionSummary => $"{SelectedCount:N0} shared setups selected, affecting {SelectedRuns.Length:N0} run rows. Use Ctrl+click or Shift+click to select.";
    public string Summary { get; private set; } = "Generate parameters or refresh saved files to view shared setups.";
    public string? SortKey => DisplaySettings.SortKey;
    public bool Descending => DisplaySettings.Descending;
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Changed(string name) => PropertyChanged?.Invoke(this, new(name));

    public void Select(IEnumerable<SetupParameterItem> items)
    {
        selected = items.ToArray();
        Changed(nameof(SelectedCount)); Changed(nameof(SelectionSummary));
    }

    public void Clear(string message = "Generate parameters or refresh saved files to view shared setups.")
    {
        Items = new(new List<SetupParameterItem>());
        Keys = [];
        Summary = message;
        Select([]);
        NotifyRows();
    }

    public void SetRuns(run configured, IEnumerable<RunParameterItem> runs)
    {
        if (!configured.HasSetup) { Clear("This run executes directly and has no Setup phase."); return; }
        var items = runs.GroupBy(row => SharedInputKey(configured, row))
            .Select((group, index) => new SetupParameterItem(index + 1, configured, group.ToArray())).ToArray();
        string[] leading = ["LinkedRuns", ParamKeys.replKey];
        var keys = items.SelectMany(item => item.Values.Keys).Distinct().ToArray();
        Keys = leading.Where(keys.Contains).Concat(keys.Except(leading).Order(StringComparer.Ordinal)).ToArray();
        foreach (var item in items) item.Cells = Keys.Select(key => item.Values.GetValueOrDefault(key)).ToArray();
        Items = new(items.ToList());
        Summary = $"{items.Length:N0} shared setups for {items.Sum(item => item.Runs.Length):N0} saved run rows. Setup prepares the inputs and updates every linked run.";
        Select([]);
        NotifyRows();
        var key = SortKey ?? Keys.FirstOrDefault();
        if (key is not null) ApplySort(key, Descending);
    }

    private static string SharedInputKey(run configured, RunParameterItem row)
    {
        // Before an import, the actual checkpoint input IDs are not yet recorded.
        // Keep those requests separate rather than guessing which files they will share.
        var files = SetupOutputs.files(configured, row.Parameters).Select(SetupInputReferences.fileIdentity).ToArray();
        return files.Length == 0 ? "Run:" + row.Parameters.GetId()?.Value
            : string.Join("|", files.OrderBy(file => file.ToString()).Select(file => file.ToString()));
    }

    private void NotifyRows()
    {
        foreach (var name in new[] { nameof(Keys), nameof(Items), nameof(Count), nameof(Summary) }) Changed(name);
    }
    public void Sort(string key) => ApplySort(key, SortKey == key && !Descending);
    public void ResetSort()
    {
        DisplaySettings.SortKey = null;
        DisplaySettings.Descending = false;
        if (Keys.FirstOrDefault() is string key) ApplySort(key, false);
    }
    private void ApplySort(string key, bool descending)
    {
        var index = Array.IndexOf(Keys, key);
        if (index < 0) return;
        Items.CustomSort = TableColumnSort.Create(Items.SourceCollection.Cast<SetupParameterItem>(), row => row.Cells[index], row => row.Number, descending);
        DisplaySettings.SortKey = key;
        DisplaySettings.Descending = descending;
        Changed(nameof(SortKey)); Changed(nameof(Descending));
    }
}
