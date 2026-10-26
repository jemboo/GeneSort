using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using GeneSort.FileDb.V1;
using GeneSort.Project.V1;
using GeneSort.Project.V1.Editing;

namespace GeneSort.WPF.ViewModels;

public sealed class ParameterRow : INotifyPropertyChanged
{
    private string key = "";
    private string values = "";
    public string Key { get => key; set { key = value; Changed(); } }
    public string Values { get => values; set { values = value; Changed(); } }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Changed([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
    public parameterDraft Snapshot() => parameterDraft.create(Key, ParseValues(Values));
    public static string FormatValues(IEnumerable<string> values) => string.Join(", ", values.Select(value =>
        value != value.Trim() || value.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? "\"" + value.Replace("\"", "\"\"") + "\"" : value));

    private static string[] ParseValues(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        var values = new List<string>();
        var index = 0;
        while (true)
        {
            while (index < text.Length && char.IsWhiteSpace(text[index])) index++;
            if (index < text.Length && text[index] == '"')
            {
                index++;
                var value = new StringBuilder();
                var closed = false;
                while (index < text.Length)
                {
                    var ch = text[index++];
                    if (ch != '"') { value.Append(ch); continue; }
                    if (index < text.Length && text[index] == '"') { value.Append('"'); index++; continue; }
                    closed = true;
                    break;
                }
                while (index < text.Length && char.IsWhiteSpace(text[index])) index++;
                if (!closed || (index < text.Length && text[index] != ','))
                    throw new FormatException("Close quoted values with a double quote, then separate values with commas.");
                values.Add(value.ToString());
            }
            else
            {
                var start = index;
                while (index < text.Length && text[index] != ',') index++;
                var value = text[start..index].Trim();
                if (value.Contains('"')) throw new FormatException("Wrap values containing quotes in double quotes, and double each inner quote.");
                values.Add(value);
            }
            if (index == text.Length) break;
            index++;
        }
        return values.ToArray();
    }
}

/// Only presentation state lives here. Workflow choices and validation belong to Project.
public sealed class RunEditorViewModel : INotifyPropertyChanged
{
    private string projectName = "", databaseName = "", runName = "", description = "";
    private string saveIntervals = "", summaryIntervals = "", projectsFolder = "";
    private string status = "New definition. Choose a projects folder before saving.";
    private bool allowOverwrite, isDirty, isBusy, loading;
    private readonly HashSet<ParameterRow> observedRows = [];

    public string WorkflowName => RunEditor.workflowName;
    public string WorkflowDescription => RunEditor.workflowDescription;
    public catalogChoice[] Catalogs { get; } = RunEditor.catalogChoices();
    public catalogChoice[] Intervals { get; } = RunEditor.intervals();
    public parameterDescription[] Parameters { get; } = RunEditor.parameters();
    public string[] SetupKeys => Parameters.Select(spec => spec.Key).ToArray();
    public string[] ExecutionKeys => Parameters.Where(spec => spec.ExecutionAllowed).Select(spec => spec.Key).ToArray();
    public ObservableCollection<ParameterRow> SetupParameters { get; } = [];
    public ObservableCollection<ParameterRow> ExecutionParameters { get; } = [];
    public ObservableCollection<string> Issues { get; } = [];

    public string ProjectName { get => projectName; set => Edit(ref projectName, value); }
    public string DatabaseName { get => databaseName; set => Edit(ref databaseName, value); }
    public string RunName { get => runName; set => Edit(ref runName, value); }
    public string Description { get => description; set => Edit(ref description, value); }
    public bool AllowOverwrite { get => allowOverwrite; set => Edit(ref allowOverwrite, value); }
    public string SaveIntervals { get => saveIntervals; set => Edit(ref saveIntervals, value); }
    public string SummaryIntervals { get => summaryIntervals; set => Edit(ref summaryIntervals, value); }
    public string ProjectsFolder { get => projectsFolder; set => Edit(ref projectsFolder, value); }
    public bool IsDirty { get => isDirty; private set { isDirty = value; Changed(); Changed(nameof(DirtyLabel)); } }
    public string DirtyLabel => IsDirty ? "Unsaved changes" : "";
    public bool IsBusy { get => isBusy; set { isBusy = value; Changed(); Changed(nameof(IsReady)); } }
    public bool IsReady => !IsBusy;
    public string Status { get => status; set { status = value; Changed(); } }
    public string Destination
    {
        get
        {
            try
            {
                var built = RunEditor.build(Snapshot());
                if (!built.IsOk || string.IsNullOrWhiteSpace(ProjectsFolder)) return "Choose valid names and a projects folder to see the destination.";
                return RunDefinitionFiles.pathFor(ProjectsFolder, built.ResultValue);
            }
            catch (FormatException) { return "Correct the parameter values before saving."; }
            catch (ArgumentException) { return "Choose an absolute projects folder."; }
        }
    }

    public RunEditorViewModel()
    {
        SetupParameters.CollectionChanged += RowsChanged;
        ExecutionParameters.CollectionChanged += RowsChanged;
        Apply(RunEditor.newDraft());
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Changed([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
    private void Edit<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        Changed(name);
        MarkEdited();
    }
    private void MarkEdited()
    {
        if (loading) return;
        IsDirty = true;
        Issues.Clear();
        Status = "Definition changed. Validate or save when ready.";
        Changed(nameof(Destination));
    }
    private void RowsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Handle Reset as well as Add/Remove, without keeping cleared rows alive.
        foreach (var row in observedRows) row.PropertyChanged -= RowChanged;
        observedRows.Clear();
        foreach (var row in SetupParameters.Concat(ExecutionParameters))
            if (observedRows.Add(row)) row.PropertyChanged += RowChanged;
        MarkEdited();
    }
    private void RowChanged(object? sender, PropertyChangedEventArgs e) => MarkEdited();

    public runDraft Snapshot() => runDraft.create(new runDraftArgs
    {
        ProjectName = ProjectName, DatabaseName = DatabaseName, RunName = RunName,
        Description = Description, AllowOverwrite = AllowOverwrite,
        GenSaveIntervalsName = SaveIntervals, GenSaveSubIntervalsName = SummaryIntervals,
        SetupParameters = SnapshotRows("Setup", SetupParameters),
        ExecutionParameters = SnapshotRows("Execution", ExecutionParameters)
    });

    private static parameterDraft[] SnapshotRows(string phase, IEnumerable<ParameterRow> rows) => rows.Select((row, index) =>
    {
        try { return row.Snapshot(); }
        catch (FormatException error) { throw new FormatException($"{phase}[{index + 1}].{row.Key}: {error.Message}"); }
    }).ToArray();

    private void Apply(runDraft draft)
    {
        loading = true;
        try
        {
            ProjectName = draft.ProjectName; DatabaseName = draft.DatabaseName; RunName = draft.RunName;
            Description = draft.Description; AllowOverwrite = draft.AllowOverwrite;
            SaveIntervals = draft.GenSaveIntervalsName; SummaryIntervals = draft.GenSaveSubIntervalsName;
            SetupParameters.Clear(); ExecutionParameters.Clear();
            foreach (var row in draft.SetupParameters)
                SetupParameters.Add(new() { Key = row.Key, Values = ParameterRow.FormatValues(row.Values) });
            foreach (var row in draft.ExecutionParameters)
                ExecutionParameters.Add(new() { Key = row.Key, Values = ParameterRow.FormatValues(row.Values) });
            Issues.Clear(); IsDirty = false;
        }
        finally { loading = false; }
        Changed(nameof(Destination));
    }

    public void New()
    {
        Apply(RunEditor.newDraft());
        Status = "New definition. Edit its names and parameters, then save.";
    }

    public run? Validate()
    {
        Issues.Clear();
        runDraft draft;
        try { draft = Snapshot(); }
        catch (FormatException error)
        {
            Issues.Add(error.Message);
            Status = "Correct the parameter text before saving.";
            return null;
        }
        var result = RunEditor.build(draft);
        if (result.IsOk)
        {
            Status = "Definition is valid. Saving does not start setup or execution.";
            return result.ResultValue;
        }
        foreach (var issue in result.ErrorValue) Issues.Add($"{issue.Field}: {issue.Message}");
        Status = $"Fix {Issues.Count} validation issue(s) before saving.";
        return null;
    }

    public async Task OpenAsync(string path)
    {
        var configured = await RunDefinitionFiles.loadAsync(path);
        var imported = RunEditor.fromRun(configured);
        if (!imported.IsOk) throw new InvalidOperationException(imported.ErrorValue);
        var root = RunDefinitionFiles.tryProjectsFolder(path, configured);
        Apply(imported.ResultValue);
        loading = true;
        ProjectsFolder = root?.Value ?? "";
        loading = false;
        Changed(nameof(Destination));
        Validate();
        if (Issues.Count == 0) Status = $"Opened {path}";
    }

    public async Task SaveAsync(run configured, bool overwrite)
    {
        var path = await RunDefinitionFiles.saveAsync(ProjectsFolder, configured, overwrite);
        IsDirty = false;
        Status = $"Saved {path}";
    }
}
