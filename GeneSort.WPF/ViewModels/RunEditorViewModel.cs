using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using GeneSort.FileDb.V1;
using GeneSort.Dispatch.V1;
using GeneSort.Project.V1;
using GeneSort.Project.V1.Editing;
using GeneSort.Project.V1.Catalogs.Queries;
using GeneSort.Project.V1.Parameters;
using GeneSort.Project.V1.ParamBundles;
using GeneSort.Project.V1.Runtime;
using GeneSort.Project.V1.Workflows;
using GeneSort.SortingLib.Sorter;
using RunParameters = GeneSort.Project.V1.Parameters.parameterSet<GeneSort.Project.V1.Parameters.runParameterKind>;

namespace GeneSort.WPF.ViewModels;

/// Presentation state for the selected Dispatch run type; validation belongs to Project.
public sealed class RunEditorViewModel : INotifyPropertyChanged
{
    private string projectName = "", databaseName = "", runName = "", description = "";
    private string sortableTestsModule = "", sorterEvalModule = "";
    private string checkpointIntervals = "", saveIntervals = "", summaryIntervals = "", projectsFolder = "";
    private string status = "New definition. Choose a projects folder before saving.";
    private string firstReplica = "0", replicaCount = "1";
    private bool allowOverwrite, isDirty, isBusy, loading;
    private run? validatedRun;
    private run? loadedRun;
    private bool isRunningPhase;
    private int phaseCompleted, phaseTotal = 1;
    private string phaseSummary = "";
    private CancellationTokenSource? summaryLoad;
    private CancellationTokenSource? phaseCancellation;
    public bool IsPhaseRunning => phaseCancellation is not null;
    public string PhaseActivity => phaseCancellation?.IsCancellationRequested == true ? "Cancelling…" : isRunningPhase ? "Running…" : "Ready";
    public bool CanCancelPhase => phaseCancellation is { IsCancellationRequested: false };
    public void CancelPhase()
    {
        if (!CanCancelPhase) return;
        phaseCancellation!.Cancel();
        Changed(nameof(CanCancelPhase));
        Changed(nameof(PhaseActivity));
        Status = "Cancelling... Waiting for active work and checkpoint saves to finish.";
        PhaseSummary = Status;
    }
    private readonly HashSet<ParameterRow> observedRows = [];
    private bool isSelectingRun = true;
    private bool hasChosenRun;
    private string? sourceModule;
    private readonly Settings.WorkspaceSettingsStore? workspaceSettings;
    public RunSelectionViewModel Selection { get; } = new(RunTypeCatalog.all);
    public bool IsSelectingRun { get => isSelectingRun; private set { isSelectingRun = value; Changed(); Changed(nameof(IsEditingRun)); Changed(nameof(CanReturnToEditor)); } }
    public bool IsEditingRun => !IsSelectingRun;
    public bool CanReturnToEditor => hasChosenRun && IsSelectingRun && !Selection.IsLoading;

    public async Task BeginSelectionAsync()
    {
        if (IsBusy) return;
        Selection.Folder = ProjectsFolder;
        IsSelectingRun = true;
        if (Selection.CanContinue)
        {
            await Selection.LoadAsync(false);
            if (sourceModule is not null) Selection.OpenModule(sourceModule);
        }
        else Selection.ChangeFolder();
    }

    public void CancelSelection() => IsSelectingRun = false;

    public async Task ChooseRunAsync(RunSelectionTile tile)
    {
        if (IsBusy || tile.Definition is null) return;
        definitionReadOnly = !tile.CreateNew;
        modulePropertiesLocked = true;
        if (tile.SavedPath is not null && !tile.CreateNew)
        {
            await OpenAsync(tile.SavedPath);
            SelectedRunType = RunTypes.FirstOrDefault(type => type.Name == tile.ModulePath);
            SelectedExample = SelectedRunType?.Examples.FirstOrDefault(example => example.Run.RunName == tile.Definition.RunName);
            RefreshTypeSelection();
        }
        else
        {
            SelectedRunType = RunTypes.FirstOrDefault(type => type.Name == tile.ModulePath);
            SelectedExample = SelectedRunType?.Examples.FirstOrDefault(example => example.Name == tile.Name);
            editorTemplate = tile.Definition;
            Apply(tile.CreateNew ? RunTypeEditor.newDraft(editorTemplate) : RunTypeEditor.fromRun(editorTemplate));
            ProjectsFolder = Selection.Folder;
            if (tile.CreateNew)
            {
                var suffix = 1;
                while (Build() is { IsOk: true } built && File.Exists(RunDefinitionFiles.pathFor(ProjectsFolder, built.ResultValue)))
                    RunName = "NewRun" + suffix++;
            }
            IsDirty = tile.CreateNew;
            Validate();
            Status = tile.CreateNew ? "New leaf. Edit its name and parameters, then save." :
                "Code definition selected. Save and generate its parameters before running.";
        }
        sourceModule = tile.ModulePath;
        hasChosenRun = true;
        IsSelectingRun = false;
    }

    private bool definitionReadOnly;
    public bool DefinitionReadOnly => definitionReadOnly;
    public bool CanEditDefinition => !definitionReadOnly && !IsBusy;
    public string DefinitionEditHelp => definitionReadOnly
        ? "This run definition is read-only. Add replicas, or Clone to edit a new run."
        : "Edit the run name and declared parameter values. Module settings are read-only.";
    private bool modulePropertiesLocked;
    public bool ModulePropertiesLocked => modulePropertiesLocked;
    public bool CanEditModuleProperties => !modulePropertiesLocked && CanEditDefinition;
    public bool CanClone => !IsBusy && !HasPendingParameter && editorTemplate is not null && Build().IsOk;
    private bool AllowedInPhase(string key, bool setup) => !RunTypeEditor.isModuleOwnedParameter(editorTemplate, key) && (!modulePropertiesLocked ||
        (setup ? editorTemplate.SetupParameterSpans : editorTemplate.RunParameterSpans).Any(span => span.Item1 == key));
    private run editorTemplate = null!;
    public runType[] RunTypes { get; } = RunTypeCatalog.all;
    public runType? SelectedRunType { get; private set; }
    public runExample[] Examples => SelectedRunType?.Examples ?? [];
    public runExample? SelectedExample { get; private set; }
    public string WorkflowName => SelectedRunType?.Name ?? "Saved run configuration";
    public string WorkflowDescription => SelectedRunType?.Description ?? editorTemplate.Description;
    public bool IsEvolution => editorTemplate.IsEvolution;
    public bool HasSgdSourceSelection => editorTemplate.IsEvolutionRun;
    public bool HasSetup => editorTemplate.HasSetup;
    public string PhaseHelp => HasSetup
        ? "Save the definition and generate its parameter files. Run Setup, then Processing."
        : "Save the definition and generate its parameter files, then run Processing.";
    public string SourceDescription => editorTemplate.ParameterSource is null ? "" :
        $"Continuation source: {editorTemplate.ParameterSource.Value.Item1.ProjectName}/{editorTemplate.ParameterSource.Value.Item1.DatabaseName}/{editorTemplate.ParameterSource.Value.Item1.RunName}. Source selection is retained from the example.";
    public catalogChoice[] Catalogs => RunTypeEditor.catalogChoices(editorTemplate);
    public catalogChoice[] Intervals { get; } = RunEditor.intervals();
    public int ModuleChannelCount => SelectedRunType?.ChannelCount
        ?? (RunTypeEditor.channelCounts(editorTemplate) is [var count] ? count : 0);
    public string ModuleChannelCountLabel => ModuleChannelCount > 0 ? $"Channel count: {ModuleChannelCount} (module property)" : "Channel count: no matching module";
    public parameterDescription[] Parameters => RunTypeEditor.parametersForChannelCount(editorTemplate, ModuleChannelCount);
    public ParameterSectionViewModel SetupSection { get; }
    public ParameterSectionViewModel ExecutionSection { get; }
    public ObservableCollection<ParameterRow> SetupParameters => SetupSection.Rows;
    public ObservableCollection<ParameterRow> ExecutionParameters => ExecutionSection.Rows;
    public ObservableCollection<string> Issues { get; } = [];
    public bool HasErrors => Issues.Count > 0;
    public string ErrorMessages => string.Join(Environment.NewLine + Environment.NewLine, Issues);
    public RunParameterListViewModel RunParameterList { get; } = new();
    public SetupParameterListViewModel SetupParameterList { get; } = new();
    public SummaryReportListViewModel SummaryReportList { get; } = new();

    public string ProjectName { get => projectName; set => Edit(ref projectName, value); }
    public string DatabaseName { get => databaseName; set => Edit(ref databaseName, value); }
    public string SortableTestsModule { get => sortableTestsModule; set => Edit(ref sortableTestsModule, value); }
    public string SorterEvalModule { get => sorterEvalModule; set => Edit(ref sorterEvalModule, value); }
    public string RunName { get => runName; set => Edit(ref runName, value); }
    public string Description { get => description; set => Edit(ref description, value); }
    public bool AllowOverwrite { get => allowOverwrite; set => Edit(ref allowOverwrite, value); }
    public string CheckpointIntervals { get => checkpointIntervals; set => Edit(ref checkpointIntervals, value); }
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
                SetupParameterList.Clear();
                loadedRun = null;
                PhaseSummary = "";
                Status = "Projects folder changed. Validate the definition if needed, then save.";
            }
            Changed(nameof(Destination));
            SaveStateChanged(); GenerationChanged();
            RememberProjectsFolder(value);
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
    public bool CanSave => CanEditDefinition && !HasPendingParameter && validatedRun is not null && HasProjectsFolder;
    public string SaveHint => definitionReadOnly ? "This definition is read-only. Clone it to make changes." : !HasProjectsFolder ? "Select an existing projects folder first."
        : HasPendingParameter ? "Add or cancel the pending parameter first."
        : validatedRun is null ? "Validate the current definition before saving." : "Save the validated run definition.";
    public bool CanLoadRunParameters => !IsBusy && !HasPendingParameter && HasProjectsFolder && Build().IsOk;
    private string parallelThreads = "1";
    public string ParallelThreads
    {
        get => parallelThreads;
        set { parallelThreads = value; Changed(nameof(ParallelThreads)); Changed(nameof(ParallelThreadsError)); PhaseActionsChanged(); }
    }
    public string ParallelThreadsError => int.TryParse(ParallelThreads, out var count) && count > 0 ? "" : "Enter a positive whole number.";
    private int ParallelThreadCount => int.Parse(ParallelThreads, CultureInfo.InvariantCulture);
    private bool CanProcessSavedRows => ParallelThreadsError.Length == 0 && !IsBusy && !HasUnsavedChanges && loadedRun is not null && HasProjectsFolder;
    private bool CanRunSelectedRows => CanProcessSavedRows && RunParameterList.SelectedCount > 0;
    public bool CanRunSetup => HasSetup && CanProcessSavedRows && SetupParameterList.SelectedCount > 0;
    public bool CanRunExecution => CanRunSelectedRows;
    public bool CanGenerateSummaryReports => IsEvolution && CanRunSelectedRows
        && RunParameterList.SelectedItems.All(row => row.Parameters.GetRunFinished()?.Value == true);
    public bool CanRefreshSummaryReports => IsEvolution && CanRunSelectedRows && !SummaryReportList.IsLoading;
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
        Changed(nameof(DefinitionReadOnly)); Changed(nameof(CanEditDefinition)); Changed(nameof(DefinitionEditHelp));

        Changed(nameof(CanClone)); Changed(nameof(ModulePropertiesLocked)); Changed(nameof(CanEditModuleProperties));
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
        var result = Build();
        if (!result.IsOk) return (false, "Correct the run definition to preview its parameter files.");
        try
        {
            var args = PreparationArgs(result.ResultValue);
            if (args.Run.ParameterSource is not null)
                return (true, "Continuation parameters will be expanded from the example's source selection when generated.");
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
                var built = Build();
                if (!built.IsOk || string.IsNullOrWhiteSpace(ProjectsFolder)) return "Choose valid names and a projects folder to see the destination.";
                return RunDefinitionFiles.pathFor(ProjectsFolder, built.ResultValue);
            }
            catch (ArgumentException) { return "Choose an absolute projects folder."; }
        }
    }

    private void RememberProjectsFolder(string folder)
    {
        if (workspaceSettings?.RememberFolder(folder) is string error) Status = error;
    }

    public RunEditorViewModel() : this(App.WorkspaceSettings) { }

    internal RunEditorViewModel(Settings.WorkspaceSettingsStore? settings)
    {
        workspaceSettings = settings;
        // Restore backing state directly: choosing a folder is not a draft edit at startup.
        projectsFolder = settings?.ProjectsFolder ?? "";
        Selection.Folder = projectsFolder;
        Selection.FolderSelected += RememberProjectsFolder;
        Selection.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(RunSelectionViewModel.IsLoading)) Changed(nameof(CanReturnToEditor));
        };
        SelectedRunType = RunTypes.First(type => type.Name == "SorterSgd.cc32.p4a.Msuf4.Simple.Standard.Pool_32");
        SelectedExample = SelectedRunType.Examples[0];
        editorTemplate = SelectedExample.Run;
        SetupSection = new("Setup parameters", Parameters.Where(spec => AllowedInPhase(spec.Key, true) && RunTypeEditor.canEditSetup(editorTemplate, spec.Key)).ToArray());
        ExecutionSection = new("Processing overrides", Parameters.Where(spec => AllowedInPhase(spec.Key, false) && spec.ExecutionAllowed).ToArray(),
            key => SetupParameters.FirstOrDefault(row => row.Key == key)?.Snapshot().Values);
        SetupSection.PropertyChanged += SectionChanged;
        ExecutionSection.PropertyChanged += SectionChanged;
        SetupParameters.CollectionChanged += RowsChanged;
        ExecutionParameters.CollectionChanged += RowsChanged;
        Issues.CollectionChanged += (_, _) => { Changed(nameof(HasErrors)); Changed(nameof(ErrorMessages)); };
        RunParameterList.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(RunParameterListViewModel.SelectedCount))
            {
                PhaseActionsChanged();
                _ = LoadSummaryReportsAsync();
            }
        };
        SetupParameterList.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SetupParameterListViewModel.SelectedCount)) PhaseActionsChanged();
        };
        Apply(RunTypeEditor.newDraft(editorTemplate));
    }

    public void SelectRunType(runType type)
    {
        if (IsBusy || ReferenceEquals(type, SelectedRunType)) return;
        if (!RunTypes.Contains(type)) throw new ArgumentException("Choose a registered run type.", nameof(type));
        SelectedRunType = type;
        sourceModule = type.Name;
        SelectExample(type.Examples[0]);
    }

    public void SelectExample(runExample example)
    {
        if (IsBusy) return;
        if (!Examples.Contains(example)) throw new ArgumentException("Choose an example from this run type.", nameof(example));
        SelectedExample = example;
        sourceModule = SelectedRunType?.Name;
        definitionReadOnly = true;
        modulePropertiesLocked = true;
        editorTemplate = example.Run;
        Apply(RunTypeEditor.fromRun(editorTemplate));
        IsDirty = false;
        Status = "Code definition selected. Add replicas, or Clone to edit a new run.";
    }

    public Microsoft.FSharp.Core.FSharpResult<run, validationIssue[]> Build()
    {
        if (definitionReadOnly && !Equals(Snapshot(), RunTypeEditor.fromRun(editorTemplate)))
            return Microsoft.FSharp.Core.FSharpResult<run, validationIssue[]>.NewError(
                [validationIssue.create("Run", "This definition is read-only. Clone it to edit its name or parameters.")]);
        var built = modulePropertiesLocked ? RunTypeEditor.buildModuleRun(editorTemplate, Snapshot())
            : RunTypeEditor.build(editorTemplate, Snapshot());
        var channelIssues = RunTypeEditor.validateChannelCount(ModuleChannelCount, editorTemplate, Snapshot());
        if (channelIssues.Length > 0)
            return Microsoft.FSharp.Core.FSharpResult<run, validationIssue[]>.NewError(
                built.IsError ? [.. built.ErrorValue, .. channelIssues] : channelIssues);
        return built;
    }

    public void RefreshTypeSelection()
    {
        foreach (var name in new[] { nameof(SelectedRunType), nameof(Examples), nameof(SelectedExample), nameof(WorkflowName),
            nameof(WorkflowDescription), nameof(ModuleChannelCount), nameof(ModuleChannelCountLabel), nameof(Catalogs), nameof(Parameters), nameof(IsEvolution), nameof(SourceDescription),
            nameof(HasSetup), nameof(HasSgdSourceSelection), nameof(PhaseHelp) }) Changed(name);
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
        SetupParameterList.Clear();
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
        if (!loading) RefreshFixedValues();
        MarkEdited();
    }
    private void RowChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ParameterRow.Values)) return;
        ExecutionSection.RefreshInheritance();
        if (!loading) RefreshFixedValues();
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
        SortableTestsModule = SortableTestsModule, SorterEvalModule = SorterEvalModule,
        Description = Description, AllowOverwrite = AllowOverwrite,
        CheckpointIntervalsName = CheckpointIntervals, GenSaveIntervalsName = SaveIntervals, GenSaveSubIntervalsName = SummaryIntervals,
        SetupParameters = SetupParameters.Select(row => row.Snapshot()).ToArray(),
        ExecutionParameters = ExecutionParameters.Select(row => row.Snapshot()).ToArray()
    });

    private void Apply(runDraft draft)
    {
        loading = true;
        try
        {
            SetupSection.CancelAdd(); ExecutionSection.CancelAdd();
            var defaults = draft.SetupParameters.Concat(draft.ExecutionParameters).ToArray();
            SetupSection.Configure(Parameters.Where(spec => AllowedInPhase(spec.Key, true) && RunTypeEditor.canEditSetup(editorTemplate, spec.Key)).ToArray(), defaults);
            ExecutionSection.Configure(Parameters.Where(spec => AllowedInPhase(spec.Key, false) && spec.ExecutionAllowed).ToArray(), defaults);
            FirstReplica = "0"; ReplicaCount = "1";
            ProjectName = draft.ProjectName; DatabaseName = draft.DatabaseName; RunName = draft.RunName;
            SortableTestsModule = draft.SortableTestsModule; SorterEvalModule = draft.SorterEvalModule;
            Description = draft.Description; AllowOverwrite = draft.AllowOverwrite;
            CheckpointIntervals = draft.CheckpointIntervalsName; SaveIntervals = draft.GenSaveIntervalsName; SummaryIntervals = draft.GenSaveSubIntervalsName;
            SetupParameters.Clear(); ExecutionParameters.Clear();
            foreach (var row in draft.SetupParameters)
                SetupParameters.Add(new(row.Key, row.Values, Parameters.FirstOrDefault(spec => spec.Key == row.Key), RunTypeEditor.isModuleOwnedParameter(editorTemplate, row.Key)));
            foreach (var row in draft.ExecutionParameters)
                ExecutionParameters.Add(new(row.Key, row.Values, Parameters.FirstOrDefault(spec => spec.Key == row.Key),
                    RunTypeEditor.isModuleOwnedParameter(editorTemplate, row.Key) || GeneSort.Project.V1.Parameters.RunParameterRoles.isFixed(Run.parameterRole(editorTemplate, row.Key))));
            RefreshFixedValues();
            Issues.Clear(); IsDirty = false;
            validatedRun = null;
            RunParameterList.Clear();
            SetupParameterList.Clear();
            loadedRun = null;
            PhaseSummary = "";
            ClearSummaryReports();
        }
        finally { loading = false; }
        RefreshTypeSelection();
        Changed(nameof(Destination));
        SaveStateChanged();
        GenerationChanged();
    }

    public void Clone()
    {
        if (!CanClone) return;
        var source = Build().ResultValue;
        var draft = RunTypeEditor.cloneDraft(source);
        editorTemplate = RunTypeEditor.buildModuleRun(source, draft).ResultValue;
        definitionReadOnly = false;
        modulePropertiesLocked = true;
        Apply(draft);
        IsDirty = true;
        hasChosenRun = true;
        IsSelectingRun = false;
        Validate();
        Status = "Cloned definition. Edit the run name and declared parameter values, then save. Module settings are read-only.";
    }

    public void New()
    {
        definitionReadOnly = false;
        modulePropertiesLocked = true;
        editorTemplate = SelectedExample?.Run ?? editorTemplate;
        Apply(RunTypeEditor.newDraft(editorTemplate));
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
        var result = Build();
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
        string? recordedModule = null;
        try { recordedModule = RunSelectionViewModel.ReadModule(path); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or System.Text.Json.JsonException) { /* The definition remains usable without its optional label. */ }
        var match = RunTypes.SelectMany(type => type.Examples.Select(example => (Type: type, Example: example)))
            .Where(item => RunTypeEditor.matches(item.Example.Run, configured)
                && RunTypeEditor.channelCounts(configured).SequenceEqual(new[] { item.Type.ChannelCount }))
            .OrderByDescending(item => item.Type.Name == recordedModule)
            .ThenByDescending(item => item.Example.Run.ProjectName == configured.ProjectName && item.Example.Run.DatabaseName == configured.DatabaseName)
            .ThenByDescending(item => item.Example.Run.RunName == configured.RunName)
            .FirstOrDefault();
        SelectedRunType = match.Type;
        sourceModule = match.Type?.Name;
        SelectedExample = match.Example;
        definitionReadOnly = true;
        modulePropertiesLocked = true;
        editorTemplate = configured;
        var root = RunDefinitionFiles.tryProjectsFolder(path, configured);
        Apply(RunTypeEditor.fromRun(configured));
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
        var path = await RunDefinitionFiles.saveAsync(ProjectsFolder, configured, false);
        await RunSelectionViewModel.SaveModuleAsync(path, sourceModule ?? SelectedRunType?.Name);
        editorTemplate = configured;
        definitionReadOnly = true;
        IsDirty = false;
        SaveStateChanged();
        Status = $"Saved {path}. The definition is now read-only; add replicas or Clone to make changes.";
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
        await RunParameterFiles.appendAsync(prepared, progress);
        await RunSelectionViewModel.SaveModuleAsync(RunDefinitionFiles.pathFor(ProjectsFolder, prepared.Run), sourceModule ?? SelectedRunType?.Name);
        editorTemplate = prepared.Run;
        definitionReadOnly = true;
        IsDirty = false;
        SaveStateChanged();
        Status = $"Saved the run definition and {prepared.FileCount:N0} RunParameter files to {prepared.ParameterFolder}";
        await LoadRunParametersAsync(false);
        GenerationChanged();
    }

    public async Task LoadRunParametersAsync(bool updateStatus = true)
    {
        if (updateStatus) PhaseSummary = "";
        loadedRun = null;
        RunParameterList.Clear("Loading saved RunParameters…");
        SetupParameterList.Clear("Loading saved RunParameters…");
        try
        {
            if (!HasProjectsFolder) throw new InvalidOperationException("Select an existing projects folder first.");
            var configured = Build();
            if (!configured.IsOk) throw new InvalidOperationException("Correct the run definition before loading its saved parameters.");
            var rows = await RunParameterFiles.loadAllAsync(ProjectsFolder, configured.ResultValue);
            RunParameterList.SetParameters(rows, configured.ResultValue);
            try
            {
                var failures = await Settings.ProcessingFailureStore.LoadAsync(RunParameterFiles.folderFor(ProjectsFolder, configured.ResultValue));
                foreach (var row in RunParameterList.Items.SourceCollection.Cast<RunParameterItem>())
                    if (row.Status != "Completed" && row.Parameters.GetId() is { } id && failures.TryGetValue(id.Value, out var message))
                        row.ShowStatus(runRowStatus.Failed, message);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
            { Issues.Add($"Could not restore failed-run statuses: {error.Message}"); }
            SetupParameterList.SetRuns(configured.ResultValue, RunParameterList.Items.SourceCollection.Cast<RunParameterItem>());
            loadedRun = configured.ResultValue;
            PhaseActionsChanged();
            if (updateStatus) Status = $"Loaded {rows.Length:N0} saved RunParameters for {RunName}.";
        }
        catch (Exception error)
        {
            var message = $"Could not load saved RunParameters: {error.GetBaseException().Message}";
            RunParameterList.Clear(message);
            SetupParameterList.Clear(message);
            Status = updateStatus ? message : $"{Status}\n{message}";
        }
    }

    private void RefreshFixedValues()
    {
        ExecutionSection.InheritedRows.Clear();
        foreach (var row in RunTypeEditor.inheritedParameters(editorTemplate, Snapshot()))
            if (!ExecutionParameters.Any(existing => existing.Key == row.Key && existing.IsReadOnly))
                ExecutionSection.InheritedRows.Add(new(row.Key, row.Values,
                    parameterDescription.forKey(row.Key, false, false), isReadOnly: true));
    }

    public Task RunSetupAsync()
    {
        if (!HasSetup) throw new InvalidOperationException("This run executes directly and has no Setup phase.");
        if (!CanRunSetup) throw new InvalidOperationException("Save the current definition and select shared setups before running Setup.");
        return RunSelectedAsync("Setup", (folder, configured, rows, progress, token) =>
            RunSetupFiles.runCancellableAsync(token, ParallelThreadCount, folder, configured, rows.Select(row => row.Parameters).ToArray(), progress), SetupParameterList.SelectedRuns);
    }
    public Task RunExecutionAsync() => RunSelectedAsync("Processing", (folder, configured, rows, progress, token) =>
        configured.IsEvolution
            ? RunExecutionFiles.runPathsCancellableAsync(token, ParallelThreadCount, folder, configured, rows.Select(row => row.Evolution).ToArray(), progress)
            : RunExecutionFiles.runSavedCancellableAsync(token, ParallelThreadCount, folder, configured, rows.Select(row => row.Parameters).ToArray(), progress));
    public Task GenerateSummaryReportsAsync()
    {
        if (!CanGenerateSummaryReports)
            throw new InvalidOperationException("Select only completed runs before generating summary reports.");
        return RunSelectedAsync("Summary Reports", (folder, configured, rows, progress, token) =>
            RunSummaryFiles.runCancellableAsync(token, folder, configured, rows.Select(row => row.Parameters).ToArray(), progress));
    }

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
        if (!IsEvolution || !CanRunSelectedRows) return;
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

    private async Task SaveProcessingFailuresAsync(run configured)
    {
        try
        {
            var failures = RunParameterList.Items.SourceCollection.Cast<RunParameterItem>()
                .Where(row => row.Status == "Failed" && row.Parameters.GetId() is not null)
                .ToDictionary(row => row.Parameters.GetId()!.Value, row => row.FailureMessage);
            await Settings.ProcessingFailureStore.SaveAsync(RunParameterFiles.folderFor(ProjectsFolder, configured), failures);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        { Issues.Add($"Could not save failed-run statuses: {error.Message}"); }
    }

    private async Task RunSelectedAsync(string phase,
        Func<string, run, RunParameterItem[], IProgress<runPhaseProgress>, CancellationToken, Task<runPhaseResult>> runPhase,
        RunParameterItem[]? setupRuns = null)
    {
        // Own the busy state so all callers, including keyboard/programmatic actions, get the same guard.
        if (!CanProcessSavedRows || (setupRuns?.Length ?? RunParameterList.SelectedCount) == 0)
            throw new InvalidOperationException($"Save the current definition and select saved parameter rows before running {phase}.");
        var configured = loadedRun!;
        var selected = setupRuns ?? RunParameterList.SelectedItems;
        var processing = phase == "Processing";
        if (processing)
            foreach (var row in selected)
                row.ShowStatus(row.Parameters.GetRunFinished()?.Value == true ? runRowStatus.Completed : runRowStatus.Waiting);
        var rowLabel = setupRuns is null ? "selected rows" : "linked run rows";
        ClearSummaryReports();
        IsBusy = true;
        isRunningPhase = true;
        using var cancellation = new CancellationTokenSource();
        phaseCancellation = cancellation;
        Changed(nameof(IsPhaseRunning));
        Changed(nameof(CanCancelPhase));
        Changed(nameof(PhaseActivity));
        Changed(nameof(IsProgressIndeterminate));
        PhaseCompleted = 0;
        PhaseTotal = selected.Length;
        PhaseSummary = $"{phase}: 0 of {PhaseTotal:N0} {rowLabel} processed.";
        Status = $"Starting {phase}…";
        Issues.Clear();
        var acceptingProgress = true;
        var progress = new Progress<runPhaseProgress>(update =>
        {
            if (!acceptingProgress) return;
            if (update.RowIndex is { } rowIndex && update.Generation is { } generation
                && rowIndex.Value >= 0 && rowIndex.Value < selected.Length)
                selected[rowIndex.Value].ShowGeneration(generation.Value);
            if (update.RowIndex is { } statusIndex && update.Status is { } rowStatus
                && statusIndex.Value >= 0 && statusIndex.Value < selected.Length)
            {
                if (processing) selected[statusIndex.Value].ShowStatus(rowStatus.Value, rowStatus.Value == runRowStatus.Failed ? update.Message : null);
                else if (phase == "Setup") selected[statusIndex.Value].ShowSetupStatus(rowStatus.Value);
            }
            PhaseCompleted = update.Completed;
            if (cancellation.IsCancellationRequested) return;
            PhaseSummary = $"{phase}: {update.Completed:N0} of {update.Total:N0} {rowLabel} processed.";
            Status = update.Message;
        });
        try
        {
            var result = await runPhase(ProjectsFolder, configured, selected, progress, cancellation.Token);
            acceptingProgress = false;
            foreach (var row in selected) row.ShowGeneration(null);
            if (processing)
            {
                var statuses = result.RowStatuses;
                var messages = result.RowErrors;
                for (var index = 0; index < selected.Length; index++) selected[index].ShowStatus(statuses[index], messages[index]);
            }
            else
                for (var index = 0; index < selected.Length; index++) selected[index].ShowSetupStatus(result.RowStatuses[index]);
            RunParameterList.ApplyUpdates(selected, result.Updates);
            if (processing) await SaveProcessingFailuresAsync(configured);
            SetupParameterList.SetRuns(configured, RunParameterList.Items.SourceCollection.Cast<RunParameterItem>());
            PhaseCompleted = PhaseTotal;
            PhaseSummary = $"{phase} finished: {result.Succeeded:N0} succeeded, {result.Failed:N0} failed, {result.Skipped:N0} skipped.";
            if (result.Cancelled > 0)
                PhaseSummary = $"{phase} cancelled: {result.Succeeded:N0} succeeded, {result.Failed:N0} failed, {result.Skipped:N0} skipped, {result.Cancelled:N0} cancelled.";
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
            if (processing)
            {
                var selectedIds = selected.Select(row => row.Parameters.GetId()?.Value).ToHashSet();
                foreach (var row in RunParameterList.Items.SourceCollection.Cast<RunParameterItem>())
                    if (selectedIds.Contains(row.Parameters.GetId()?.Value) && row.Status != "Completed")
                        row.ShowStatus(runRowStatus.Failed, error.GetBaseException().Message);
                await SaveProcessingFailuresAsync(configured);
            }
        }
        finally
        {
            acceptingProgress = false;
            foreach (var row in selected) row.ShowGeneration(null);
            phaseCancellation = null;
            Changed(nameof(IsPhaseRunning));
            Changed(nameof(CanCancelPhase));
        Changed(nameof(PhaseActivity));
            isRunningPhase = false;
            IsBusy = false;
        }
    }
}
