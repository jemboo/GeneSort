using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using GeneSort.FileDb.V1;
using GeneSort.Project.V1;
using GeneSort.Project.V1.Parameters;
using GeneSort.Project.V1.Runtime;
using GeneSort.WPF.Settings;
using RunParameters = GeneSort.Project.V1.Parameters.parameterSet<GeneSort.Project.V1.Parameters.runParameterKind>;

namespace GeneSort.WPF.ViewModels;

public sealed class RunParameterItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private int? liveGeneration;
    private string[] columnKeys = [];
    public string Status { get; private set; } = "Waiting";
    public string FailureMessage { get; private set; } = "";
    public string FailureDetails => $"Run: {Parameters.GetRunName()?.Value}\nReplica: {Parameters.GetRepl()?.Value}\nID: {Parameters.GetId()?.Value}\n\n{FailureMessage}";
    internal void ShowStatus(runRowStatus status, string? error = null)
    {
        // Cancellation stops this attempt; the unfinished row is available to run again.
        Status = status == runRowStatus.Cancelled ? "Waiting" : status.ToString();
        FailureMessage = status == runRowStatus.Failed
            ? string.IsNullOrWhiteSpace(error) ? ProcessingFailureStore.MissingMessage : error
            : "";
        PropertyChanged?.Invoke(this, new(nameof(FailureMessage)));
        PropertyChanged?.Invoke(this, new(nameof(FailureDetails)));
        PropertyChanged?.Invoke(this, new(nameof(Status)));
        SetColumns(columnKeys);
    }
    public int Number { get; }
    private runPhaseOutput state;
    private string? executionError;
    public RunParameters Parameters => RunPhaseOutput.toParameters(state);
    public IReadOnlyDictionary<string, string> Values { get; private set; } = new Dictionary<string, string>();
    public string?[] Cells { get; private set; } = [];
    public Tuple<evoPath, evoProgress> Evolution => state is runPhaseOutput.Evolution evolution
        ? Tuple.Create(evolution.Item1, evolution.Item2)
        : throw new InvalidOperationException($"Row {Number}: {executionError ?? "Run Setup before processing."}");

    internal RunParameterItem(int number, RunParameters parameters, run? configured)
    {
        Number = number;
        Status = parameters.GetRunFinished()?.Value == true ? "Completed" : "Waiting";
        state = runPhaseOutput.NewParameters(parameters);
        // This is the saved-run opening boundary. Phase updates never reconstruct the path.
        if (configured?.IsEvolution == true)
        {
            try
            {
                var pathAndProgress = EvoPath.fromParameters(configured, parameters);
                state = runPhaseOutput.NewEvolution(pathAndProgress.Item1, pathAndProgress.Item2);
            }
            catch (Exception error) { executionError = error.Message; }
        }
        Values = Parameters.ParamMap.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    internal void Apply(runPhaseOutput updated)
    {
        state = updated;
        executionError = null;
        Values = Parameters.ParamMap.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    internal void SetColumns(string[] keys)
    {
        columnKeys = keys;
        Cells = keys.Select(key => key == "Status" ? Status : key == ParamKeys.generationCurrentKey && liveGeneration is int generation
            ? generation.ToString(CultureInfo.InvariantCulture) : Values.GetValueOrDefault(key)).ToArray();
        PropertyChanged?.Invoke(this, new(nameof(Cells)));
    }

    internal void ShowGeneration(int? generation)
    {
        liveGeneration = generation;
        SetColumns(columnKeys);
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
    private RunParameterItem[] selectedItems = [];
    public RunParameterItem[] SelectedItems => selectedItems.ToArray();
    public RunParameters[] SelectedParameters => selectedItems.Select(row => row.Parameters).ToArray();
    public int SelectedCount => selectedItems.Length;
    public string SelectionSummary => $"{SelectedCount:N0} selected. Use Ctrl+click or Shift+click to select rows.";

    public void Select(IEnumerable<RunParameterItem> rows)
    {
        selectedItems = rows.ToArray();
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

    public void SetParameters(IEnumerable<RunParameters> parameters, run? configured = null)
    {
        ShowItems(parameters.Select((row, index) => new RunParameterItem(index + 1, row, configured)).ToArray());
    }

    public void ApplyUpdates(RunParameterItem[] selected, Tuple<int, runPhaseOutput>[] updates)
    {
        foreach (var update in updates)
            selected[update.Item1].Apply(update.Item2);
        ShowItems(Items.SourceCollection.Cast<RunParameterItem>().ToArray());
    }

    private void ShowItems(RunParameterItem[] data)
    {
        string[] leading = ["Status", ParamKeys.replKey, ParamKeys.mutationModKey, ParamKeys.sourceMutationModKey,
            ParamKeys.partitionCountKey, ParamKeys.generationCurrentKey, ParamKeys.runNameKey];
        var keys = data.SelectMany(row => row.Values.Keys).Concat(data.Length > 0 ? ["Status"] : Array.Empty<string>()).Distinct(StringComparer.Ordinal).ToArray();
        Keys = leading.Where(keys.Contains).Concat(keys.Except(leading).Order(StringComparer.Ordinal)).ToArray();
        foreach (var item in data) item.SetColumns(Keys);
        Items = new(data.ToList());
        Select([]);
        var replicas = data.Select(row => row.Parameters.GetRepl()?.Value).Distinct().Count();
        Summary = data.Length == 0 ? "No saved RunParameter files for this run."
            : $"{data.Length:N0} read-only generated RunParameters across {replicas:N0} replica(s). Click a column heading to sort; click again to reverse.";
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
