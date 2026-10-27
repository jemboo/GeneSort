using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using GeneSort.FileDb.V1;
using GeneSort.Project.V1;
using GeneSort.Project.V1.Editing;
using GeneSort.Project.V1.Runtime;
using GeneSort.Project.V1.Workflows;
using RunParameters = GeneSort.Project.V1.Parameters.parameterSet<GeneSort.Project.V1.Parameters.runParameterKind>;

namespace GeneSort.WPF.ViewModels;

/// Only presentation state lives here. Workflow choices and validation belong to Project.
public sealed class RunEditorViewModel : INotifyPropertyChanged
{
    private string projectName = "", databaseName = "", runName = "", description = "";
    private string saveIntervals = "", summaryIntervals = "", projectsFolder = "";
    private string status = "New definition. Choose a projects folder before saving.";
    private string firstReplica = "0", replicaCount = "1";
    private bool allowOverwrite, isDirty, isBusy, loading;
    private run? validatedRun;
    private run? loadedRun;
    private bool isRunningPhase;
    private int phaseCompleted, phaseTotal = 1;
    private string phaseSummary = "";
    private CancellationTokenSource? summaryLoad;
    private readonly HashSet<ParameterRow> observedRows = [];

    public string WorkflowName => RunEditor.workflowName;
    public string WorkflowDescription => RunEditor.workflowDescription;
    public catalogChoice[] Catalogs { get; } = RunEditor.catalogChoices();
    public catalogChoice[] Intervals { get; } = RunEditor.intervals();
    public parameterDescription[] Parameters { get; } = RunEditor.parameters();
    public ParameterSectionViewModel SetupSection { get; }
    public ParameterSectionViewModel ExecutionSection { get; }
    public ObservableCollection<ParameterRow> SetupParameters => SetupSection.Rows;
    public ObservableCollection<ParameterRow> ExecutionParameters => ExecutionSection.Rows;
    public ObservableCollection<string> Issues { get; } = [];
    public RunParameterListViewModel RunParameterList { get; } = new();
    public SummaryReportListViewModel SummaryReportList { get; } = new();

    public string ProjectName { get => projectName; set => Edit(ref projectName, value); }
    public string DatabaseName { get => databaseName; set => Edit(ref databaseName, value); }
    public string RunName { get => runName; set => Edit(ref runName, value); }
    public string Description { get => description; set => Edit(ref description, value); }
    public bool AllowOverwrite { get => allowOverwrite; set => Edit(ref allowOverwrite, value); }
    public string SaveIntervals { get => saveIntervals; set => Edit(ref saveIntervals, value); }
    public string SummaryIntervals { get => summaryIntervals; set => Edit(ref summaryIntervals, value); }
    public string ProjectsFolder
    {
        get => projectsFolder;
        set
        {
            if (projectsFolder == value) return;
            projectsFolder = value;
            Changed();
            if (!loading)
            {
                IsDirty = true;
                RunParameterList.Clear();
                loadedRun = null;
                PhaseSummary = "";
                Status = "Projects folder changed. Validate the definition if needed, then save.";
            }
            Changed(nameof(Destination));
            SaveStateChanged(); GenerationChanged();
        }
    }
    public bool IsDirty { get => isDirty; private set { isDirty = value; Changed(); Changed(nameof(DirtyLabel)); Changed(nameof(HasUnsavedChanges)); PhaseActionsChanged(); } }
    public bool HasPendingParameter => SetupSection.IsAdding || ExecutionSection.IsAdding;
    public bool HasUnsavedChanges => IsDirty || HasPendingParameter;
    public string DirtyLabel => HasPendingParameter ? "Finish adding the parameter, or cancel" : IsDirty ? "Unsaved changes" : "";
    public bool IsBusy { get => isBusy; set { isBusy = value; Changed(); Changed(nameof(IsReady)); Changed(nameof(IsProgressIndeterminate)); SaveStateChanged(); GenerationChanged(); } }
    public bool IsReady => !IsBusy;
    public string Status { get => status; set { status = value; Changed(); } }
    public string FirstReplica { get => firstReplica; set => EditGeneration(ref firstReplica, value); }
    public string ReplicaCount { get => replicaCount; set => EditGeneration(ref replicaCount, value); }
    public string GenerationPreview => GenerationDetails().Text;
    public bool CanGenerateParameters => !IsBusy && GenerationDetails().Valid;
    private bool HasProjectsFolder => !string.IsNullOrWhiteSpace(ProjectsFolder)
        && Path.IsPathFullyQualified(ProjectsFolder) && Directory.Exists(ProjectsFolder);
    public bool CanSave => !IsBusy && !HasPendingParameter && validatedRun is not null && HasProjectsFolder;
    public string SaveHint => !HasProjectsFolder ? "Select an existing projects folder first."
        : HasPendingParameter ? "Add or cancel the pending parameter first."
        : validatedRun is null ? "Validate the current definition before saving." : "Save the validated run definition.";
    public bool CanLoadRunParameters => !IsBusy && !HasPendingParameter && HasProjectsFolder && RunEditor.build(Snapshot()).IsOk;
    private bool CanRunSelectedRows => !IsBusy && !HasUnsavedChanges && loadedRun is not null && HasProjectsFolder && RunParameterList.SelectedCount > 0;
    public bool CanRunSetup => CanRunSelectedRows;
    public bool CanRunExecution => CanRunSelectedRows;
    public bool CanGenerateSummaryReports => CanRunSelectedRows;
    public bool CanRefreshSummaryReports => CanRunSelectedRows && !SummaryReportList.IsLoading;
    public int PhaseCompleted { get => phaseCompleted; private set { phaseCompleted = value; Changed(); } }
    public int PhaseTotal { get => phaseTotal; private set { phaseTotal = value; Changed(); } }
    public string PhaseSummary { get => phaseSummary; private set { phaseSummary = value; Changed(); } }
    public bool IsProgressIndeterminate => IsBusy && !isRunningPhase;

    private void PhaseActionsChanged()
    {
        Changed(nameof(CanRunSetup)); Changed(nameof(CanRunExecution)); Changed(nameof(CanGenerateSummaryReports));
        Changed(nameof(CanRefreshSummaryReports));
    }

    private void SaveStateChanged()
    {
        Changed(nameof(CanSave)); Changed(nameof(SaveHint));
    }

    private runPreparationArgs PreparationArgs(run configured)
    {
        if (!int.TryParse(FirstReplica, NumberStyles.Integer, CultureInfo.InvariantCulture, out var first))
            throw new ArgumentException("Enter a whole number for First replica.");
        if (!int.TryParse(ReplicaCount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count))
            throw new ArgumentException("Enter a whole number for Replica count.");
        return new runPreparationArgs { Run = configured, FirstReplica = first, ReplicaCount = count };
    }
    private (bool Valid, string Text) GenerationDetails()
    {
        if (HasPendingParameter) return (false, "Add or cancel the pending parameter first.");
        if (!HasProjectsFolder) return (false, "Select an existing projects folder first.");
        var result = RunEditor.build(Snapshot());
        if (!result.IsOk) return (false, "Correct the run definition to preview its parameter files.");
        try
        {
            var args = PreparationArgs(result.ResultValue);
            var count = RunPreparation.count(args);
            if (count <= 0) return (false, "No RunParameter files would be generated.");
            if (count > int.MaxValue) return (false, "This sweep produces too many files. Reduce the parameter choices or replica count.");
            var folder = RunParameterFiles.folderFor(ProjectsFolder, args.Run);
            return (true, $"{count:N0} RunParameter files · replicas {args.FirstReplica}–{(long)args.FirstReplica + args.ReplicaCount - 1}\n{folder}");
        }
        catch (ArgumentException error) { return (false, error.Message); }
    }
    private void EditGeneration(ref string field, string value, [CallerMemberName] string? name = null)
    {
        if (field == value) return;
        field = value;
        Changed(name);
        GenerationChanged();
    }
    private void GenerationChanged()
    {
        Changed(nameof(GenerationPreview)); Changed(nameof(CanGenerateParameters));
        Changed(nameof(CanLoadRunParameters));
        PhaseActionsChanged();
    }
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
            catch (ArgumentException) { return "Choose an absolute projects folder."; }
        }
    }

    public RunEditorViewModel()
    {
        SetupSection = new("Setup parameters", Parameters);
        ExecutionSection = new("Execution overrides", Parameters.Where(spec => spec.ExecutionAllowed).ToArray(),
            key => SetupParameters.FirstOrDefault(row => row.Key == key)?.Snapshot().Values);
        SetupSection.PropertyChanged += SectionChanged;
        ExecutionSection.PropertyChanged += SectionChanged;
        SetupParameters.CollectionChanged += RowsChanged;
        ExecutionParameters.CollectionChanged += RowsChanged;
        RunParameterList.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(RunParameterListViewModel.SelectedCount))
            {
                PhaseActionsChanged();
                _ = LoadSummaryReportsAsync();
            }
        };
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
        validatedRun = null;
        RunParameterList.Clear();
        loadedRun = null;
        PhaseSummary = "";
        Issues.Clear();
        Status = "Definition changed. Validate before saving.";
        SaveStateChanged();
        Changed(nameof(Destination));
        GenerationChanged();
    }
    private void RowsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Handle Reset as well as Add/Remove, without keeping cleared rows alive.
        foreach (var row in observedRows) row.PropertyChanged -= RowChanged;
        observedRows.Clear();
        foreach (var row in SetupParameters.Concat(ExecutionParameters))
            if (observedRows.Add(row)) row.PropertyChanged += RowChanged;
        ExecutionSection.RefreshInheritance();
        MarkEdited();
    }
    private void RowChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ParameterRow.Values)) return;
        ExecutionSection.RefreshInheritance();
        MarkEdited();
    }
    private void SectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ParameterSectionViewModel.IsAdding)) return;
        Changed(nameof(HasPendingParameter)); Changed(nameof(HasUnsavedChanges)); Changed(nameof(DirtyLabel));
        SaveStateChanged();
        GenerationChanged();
        if (!loading)
        {
            Issues.Clear();
            Status = HasPendingParameter ? "Choose a parameter and its values, then Add or Cancel."
                : "Parameter panel closed. Validate or save when ready.";
        }
    }

    public runDraft Snapshot() => runDraft.create(new runDraftArgs
    {
        ProjectName = ProjectName, DatabaseName = DatabaseName, RunName = RunName,
        Description = Description, AllowOverwrite = AllowOverwrite,
        GenSaveIntervalsName = SaveIntervals, GenSaveSubIntervalsName = SummaryIntervals,
        SetupParameters = SetupParameters.Select(row => row.Snapshot()).ToArray(),
        ExecutionParameters = ExecutionParameters.Select(row => row.Snapshot()).ToArray()
    });

    private void Apply(runDraft draft)
    {
        loading = true;
        try
        {
            SetupSection.CancelAdd(); ExecutionSection.CancelAdd();
            FirstReplica = "0"; ReplicaCount = "1";
            ProjectName = draft.ProjectName; DatabaseName = draft.DatabaseName; RunName = draft.RunName;
            Description = draft.Description; AllowOverwrite = draft.AllowOverwrite;
            SaveIntervals = draft.GenSaveIntervalsName; SummaryIntervals = draft.GenSaveSubIntervalsName;
            SetupParameters.Clear(); ExecutionParameters.Clear();
            foreach (var row in draft.SetupParameters)
                SetupParameters.Add(new(row.Key, row.Values));
            foreach (var row in draft.ExecutionParameters)
                ExecutionParameters.Add(new(row.Key, row.Values));
            Issues.Clear(); IsDirty = false;
            validatedRun = null;
            RunParameterList.Clear();
            loadedRun = null;
            PhaseSummary = "";
        }
        finally { loading = false; }
        Changed(nameof(Destination));
        SaveStateChanged();
        GenerationChanged();
    }

    public void New()
    {
        Apply(RunEditor.newDraft());
        Status = "New definition. Select a projects folder and validate before saving.";
    }

    public run? Validate()
    {
        validatedRun = null;
        SaveStateChanged();
        Issues.Clear();
        if (HasPendingParameter)
        {
            Issues.Add("Add or cancel the pending parameter before validating or saving.");
            Status = "Finish the Add parameter panel first.";
            return null;
        }
        var result = RunEditor.build(Snapshot());
        if (result.IsOk)
        {
            validatedRun = result.ResultValue;
            SaveStateChanged();
            Status = HasProjectsFolder ? "Definition is valid. Ready to save."
                : "Definition is valid. Select an existing projects folder to enable Save.";
            return validatedRun;
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
        GenerationChanged();
        Validate();
        if (Issues.Count == 0) Status = $"Opened {path}";
        if (HasProjectsFolder && Issues.Count == 0) await LoadRunParametersAsync(false);
    }

    public async Task SaveAsync(run configured, bool overwrite)
    {
        if (HasPendingParameter || validatedRun is null || !Equals(configured, validatedRun) || !HasProjectsFolder)
            throw new InvalidOperationException("Select an existing projects folder and validate the current definition before saving.");
        var path = await RunDefinitionFiles.saveAsync(ProjectsFolder, configured, overwrite);
        IsDirty = false;
        Status = $"Saved {path}";
    }

    public async Task<preparedRunFiles?> PrepareGenerationAsync()
    {
        var configured = Validate();
        if (configured is null) return null;
        var args = PreparationArgs(configured);
        Status = "Preparing RunParameter files…";
        return await RunParameterFiles.prepareAsync(ProjectsFolder, args);
    }

    public async Task GenerateParametersAsync(preparedRunFiles prepared, bool replace)
    {
        if (HasPendingParameter) throw new InvalidOperationException("Add or cancel the pending parameter before generating.");
        var progress = new Progress<string>(message => Status = message);
        await RunParameterFiles.generateAsync(prepared, replace, progress);
        IsDirty = false;
        Status = $"Saved the run definition and {prepared.FileCount:N0} RunParameter files to {prepared.ParameterFolder}";
        await LoadRunParametersAsync(false);
        GenerationChanged();
    }

    public async Task LoadRunParametersAsync(bool updateStatus = true)
    {
        if (updateStatus) PhaseSummary = "";
        loadedRun = null;
        RunParameterList.Clear("Loading saved RunParameters…");
        try
        {
            if (!HasProjectsFolder) throw new InvalidOperationException("Select an existing projects folder first.");
            var configured = RunEditor.build(Snapshot());
            if (!configured.IsOk) throw new InvalidOperationException("Correct the run definition before loading its saved parameters.");
            var rows = await RunParameterFiles.loadAllAsync(ProjectsFolder, configured.ResultValue);
            RunParameterList.SetParameters(rows, configured.ResultValue);
            loadedRun = configured.ResultValue;
            PhaseActionsChanged();
            if (updateStatus) Status = $"Loaded {rows.Length:N0} saved RunParameters for {RunName}.";
        }
        catch (Exception error)
        {
            var message = $"Could not load saved RunParameters: {error.GetBaseException().Message}";
            RunParameterList.Clear(message);
            Status = updateStatus ? message : $"{Status}\n{message}";
        }
    }

    public Task RunSetupAsync() => RunSelectedAsync("Setup", (folder, configured, rows, progress) =>
        RunSetupFiles.runAsync(folder, configured, rows.Select(row => row.Parameters).ToArray(), progress));
    public Task RunExecutionAsync() => RunSelectedAsync("Execution", (folder, configured, rows, progress) =>
        RunExecutionFiles.runPathsAsync(folder, configured, rows.Select(row => row.Evolution).ToArray(), progress));
    public Task GenerateSummaryReportsAsync() => RunSelectedAsync("Summary Reports", (folder, configured, rows, progress) =>
        RunSummaryFiles.runAsync(folder, configured, rows.Select(row => row.Parameters).ToArray(), progress));

    private void ClearSummaryReports()
    {
        summaryLoad?.Cancel();
        summaryLoad = null;
        SummaryReportList.Clear();
        Changed(nameof(CanRefreshSummaryReports));
    }

    public async Task LoadSummaryReportsAsync()
    {
        ClearSummaryReports();
        if (!CanRunSelectedRows) return;
        var configured = loadedRun!;
        var selected = RunParameterList.SelectedParameters;
        using var request = new CancellationTokenSource();
        summaryLoad = request;
        SummaryReportList.Clear($"Loading Summary Reports for {selected.Length:N0} selected rows…", loading: true);
        Changed(nameof(CanRefreshSummaryReports));
        try
        {
            var reports = await RunSummaryFiles.loadAsync(ProjectsFolder, configured, selected, request.Token);
            // A slower read from a previous selection must never replace the current selection's reports.
            if (ReferenceEquals(summaryLoad, request)) SummaryReportList.SetReports(reports, selected.Length);
        }
        catch (OperationCanceledException) when (request.IsCancellationRequested) { }
        catch (Exception error)
        {
            if (ReferenceEquals(summaryLoad, request))
                SummaryReportList.Clear($"Could not load Summary Reports: {error.GetBaseException().Message}");
        }
        finally
        {
            if (ReferenceEquals(summaryLoad, request)) summaryLoad = null;
            Changed(nameof(CanRefreshSummaryReports));
        }
    }

    private async Task RunSelectedAsync(string phase,
        Func<string, run, RunParameterItem[], IProgress<runPhaseProgress>, Task<runPhaseResult>> runPhase)
    {
        // Own the busy state so all callers, including keyboard/programmatic actions, get the same guard.
        if (!CanRunSelectedRows) throw new InvalidOperationException($"Save the current definition and select saved parameter rows before running {phase}.");
        var configured = loadedRun!;
        var selected = RunParameterList.SelectedItems;
        ClearSummaryReports();
        IsBusy = true;
        isRunningPhase = true;
        Changed(nameof(IsProgressIndeterminate));
        PhaseCompleted = 0;
        PhaseTotal = selected.Length;
        PhaseSummary = $"{phase}: 0 of {PhaseTotal:N0} selected rows processed.";
        Status = $"Starting {phase}…";
        Issues.Clear();
        var acceptingProgress = true;
        var progress = new Progress<runPhaseProgress>(update =>
        {
            if (!acceptingProgress) return;
            PhaseCompleted = update.Completed;
            PhaseSummary = $"{phase}: {update.Completed:N0} of {update.Total:N0} selected rows processed.";
            Status = update.Message;
        });
        try
        {
            var result = await runPhase(ProjectsFolder, configured, selected, progress);
            RunParameterList.ApplyUpdates(selected, result.Updates);
            acceptingProgress = false;
            PhaseCompleted = PhaseTotal;
            PhaseSummary = $"{phase} finished: {result.Succeeded:N0} succeeded, {result.Failed:N0} failed, {result.Skipped:N0} skipped.";
            foreach (var error in result.Errors) Issues.Add(error);
            Status = PhaseSummary;
            PhaseActionsChanged();
        }
        catch (Exception error)
        {
            acceptingProgress = false;
            PhaseSummary = $"{phase} stopped after {PhaseCompleted:N0} of {PhaseTotal:N0} rows.";
            Status = $"Could not run {phase}: {error.GetBaseException().Message}";
            // Re-read on stale selections too; never leave outdated parameters available for another attempt.
            await LoadRunParametersAsync(false);
        }
        finally
        {
            acceptingProgress = false;
            isRunningPhase = false;
            IsBusy = false;
        }
    }
}
