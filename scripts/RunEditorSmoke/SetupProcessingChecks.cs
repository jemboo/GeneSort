using System.Windows.Documents;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GeneSort.Core;
using GeneSort.FileDb.V1;
using GeneSort.Project.V1;
using GeneSort.Project.V1.Parameters;
using GeneSort.WPF.ViewModels;
using GeneSort.WPF.Views;

internal static partial class Program
{
    private static void SelectEditorTab(RunEditorView view, string header)
    {
        var tabs = Descendants<TabControl>(view).Single();
        tabs.SelectedItem = tabs.Items.Cast<TabItem>().Single(tab => Equals(tab.Header, header));
        Layout(view);
    }

    private static void CheckSetupProcessing(string root, RunEditorViewModel model, RunEditorView view, StringBuilder bindingErrors)
    {
        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SorterSgd.p32.Msuf4a.SimpleS.Soss"));
        model.SelectExample(model.Examples.Single(example => example.Name == "SossS"));
        model.ProjectsFolder = root;
        model.Clone();
        SelectEditorTab(view, "Save intervals");
        var checkpoints = Descendants<ComboBox>(view).Single(box => AutomationProperties.GetName(box) == "Checkpoint schedule");
        var snapshots = Descendants<ComboBox>(view).Single(box => AutomationProperties.GetName(box) == "Snapshot schedule");
        Check(!checkpoints.IsEnabled && !snapshots.IsEnabled, "Module-owned schedules should remain read-only.");
        var configured = model.Validate() ?? throw new Exception(string.Join("; ", model.Issues));
        var savedRows = RunParameterGeneration.generate(configured, 0, 2);
        var runs = new RunParameterListViewModel();
        runs.SetParameters(savedRows, configured);
        var setups = new SetupParameterListViewModel();
        setups.SetRuns(configured, runs.Items.Cast<RunParameterItem>());
        Check(setups.Count * 2 == savedRows.Length, "SossS did not group the two activator mutation rates by shared inputs.");
        foreach (var item in setups.Items.Cast<SetupParameterItem>())
        {
            Check(item.Runs.Length == 2 && item.Values["LinkedRuns"] == "2", "A shared setup has an incorrect linked run count.");
            Check(item.Runs.Select(row => row.Values[ParamKeys.activatorMutationRateKey]).Order().SequenceEqual(new[] { "0", "0.01" }),
                "A setup grouped different inputs instead of the two mutation rates.");
            Check(!item.Values.ContainsKey(ParamKeys.activatorMutationRateKey), "An execution mutation rate was displayed as a Setup input.");
        }
        var liveRow = runs.Items.Cast<RunParameterItem>().First();
        var generationColumn = Array.IndexOf(runs.Keys, ParamKeys.generationCurrentKey);
        var savedGeneration = liveRow.Parameters.GetGenerationCurrent();
        var savedCell = liveRow.Cells[generationColumn];
        var cellChanged = false;
        liveRow.PropertyChanged += (_, e) => cellChanged |= e.PropertyName == nameof(RunParameterItem.Cells);
        runs.Select([liveRow]);
        liveRow.ShowGeneration(123);
        Check(cellChanged && liveRow.Cells[generationColumn] == "123", "Live generation did not notify the table.");
        Check(Equals(savedGeneration, liveRow.Parameters.GetGenerationCurrent()) && runs.SelectedItems.Single() == liveRow,
            "Live progress changed saved state or selection.");
        liveRow.ShowGeneration(null);
        Check(liveRow.Cells[generationColumn] == savedCell, "Live generation did not reset to saved state.");
        Check(!model.CanCancelPhase, "Cancel is enabled without an active phase.");
        Check(!runs.Keys.Contains("RunFinished") && !runs.Keys.Contains("SetupCompleted")
            && runs.Keys.Contains("RunStatus") && runs.Keys.Contains("SetupStatus"), "Legacy boolean status columns remain visible.");
        var shared = setups.Items.Cast<SetupParameterItem>().First(item => item.Runs.Contains(liveRow));
        liveRow.ShowSetupStatus(runRowStatus.Running);
        Check(shared.SetupStatus == "Running" && shared.Cells[Array.IndexOf(setups.Keys, "SetupStatus")] == "Running"
            && liveRow.Cells[Array.IndexOf(runs.Keys, "SetupStatus")] == "Running", "Setup status did not update both tables live.");
        liveRow.ShowSetupStatus(runRowStatus.Cancelled);
        Check(shared.SetupStatus == "Pending", "Cancelled setup was not reset to Pending.");
        setups.Select([setups.Items.Cast<SetupParameterItem>().First()]);
        var selected = setups.SelectedRuns.ToHashSet();
        setups.Sort(ParamKeys.replKey);
        setups.Sort(ParamKeys.replKey);
        Check(selected.SetEquals(setups.SelectedRuns), "Sorting changed the selected linked runs.");

        // Exercise real Setup and Execution with a tiny generated evaluation library.
        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SorterEval.Standard.Channels4"));
        model.Clone();
        void Set(string key, string value)
        {
            var row = model.SetupParameters.Single(row => row.Key == key);
            while (row.Values.Count > 1) row.Values.RemoveAt(row.Values.Count - 1);
            row.Values[0].Text = value;
        }
        Set(ParamKeys.sorterCountKey, "2");
        Set(ParamKeys.simpleSorterModelTypeKey, "Msce");
        var evalType = model.ExecutionParameters.Single(row => row.Key == ParamKeys.sorterEvalTypeKey);
        evalType.AddValue();
        evalType.Values[1].Text = "V1";
        model.RunName = "SharedSetup";
        model.ReplicaCount = "1";
        var preparedTask = model.PrepareGenerationAsync(); Wait(preparedTask);
        var prepared = preparedTask.Result ?? throw new Exception(string.Join("; ", model.Issues));
        Wait(model.GenerateParametersAsync(prepared, false));
        Check(model.RunParameterList.Count == 2 && model.SetupParameterList.Count == 1, "Evaluation overrides did not share their setup.");
        Check(model.SetupParameterList.Items.Cast<SetupParameterItem>().Single().SetupStatus == "Pending", "New setup was not marked Not run.");
        var originalRows = model.RunParameterList.Items.Cast<RunParameterItem>().ToArray();
        var ids = originalRows.Select(row => row.Parameters.GetId()).ToArray();
        SelectEditorTab(view, "Setup parameters");
        Check(!Descendants<Button>(view).Any(button => Equals(button.Content, "Run Processing")), "Execution is offered on the Setup tab.");
        var setupList = Descendants<ListView>(view).Single(control => AutomationProperties.GetName(control) == "Shared SetupParameters");
        setupList.SelectedItems.Add(setupList.Items[0]); Pump();
        Check(model.CanRunSetup && !model.CanRunExecution && model.SetupParameterList.SelectedRuns.Length == 2,
            "Setup selection enabled Execution or omitted linked runs.");
        model.ParallelThreads = "0";
        Check(!model.CanRunSetup && model.ParallelThreadsError.Length > 0, "Invalid thread count was accepted.");
        model.ParallelThreads = "2";
        Check(model.CanRunSetup, "Valid thread count did not enable Setup.");
        SelectEditorTab(view, "Processing parameters");
        Check(model.CanRunSetup && model.SetupParameterList.SelectedCount == 1, "Switching tabs discarded the Setup selection.");
        SelectEditorTab(view, "Setup parameters");
        model.IsBusy = true; Layout(view);
        var setupScroll = Descendants<ScrollViewer>(setupList).First();
        Check(setupScroll.IsEnabled && setupList.IsEnabled, "Setup scrolling is disabled while busy.");
        setupScroll.ScrollToRightEnd(); Layout(view); Layout(view);
        Check(setupScroll.ScrollableWidth == 0 || setupScroll.HorizontalOffset > 0, "Setup table could not scroll while busy.");
        Check(!Descendants<Button>(view).Single(button => button.ToolTip?.ToString() == "Back to this run module. Your editor changes are kept.").IsEnabled && !Button(view, "Run Setup").IsEnabled,
            "Run-changing controls became enabled while busy.");
        Check(!Descendants<TextBox>(view).Single(box => AutomationProperties.GetName(box) == "Run name").IsEnabled,
            "Run metadata became editable while busy.");
        SelectEditorTab(view, "Processing parameters");
        var processingList = Descendants<ListView>(view).Single(control => AutomationProperties.GetName(control) == "Saved RunParameters");
        Check(processingList.IsEnabled && Descendants<ScrollViewer>(processingList).First().IsEnabled,
            "Processing scrolling is disabled while busy.");
        Check(!Button(view, "Run Processing").IsEnabled, "Processing could be started while busy.");
        Check(!Descendants<TextBox>(processingList).Any() && !Descendants<TextBox>(setupList).Any(), "Generated parameter cells are editable.");
        Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "artifacts", "processing-check"));
        Render(view, Path.Combine(Environment.CurrentDirectory, "artifacts", "processing-check", "parameters-running.png"));
        SelectEditorTab(view, "Setup parameters");
        model.IsBusy = false; Layout(view);
        var cancelledSetup = model.RunSetupAsync();
        Layout(view);
        Check(Button(view, "Cancel").IsEnabled && model.IsPhaseRunning, "Cancel is disabled while the editor is busy.");
        Check(Descendants<ProgressBar>(view).Any(bar => AutomationProperties.GetName(bar) == "Active phase activity" && bar.IsIndeterminate && bar.Visibility == Visibility.Visible
                && bar.Parent is StackPanel { Parent: DockPanel { Parent: Border { Visibility: Visibility.Visible } } }),
            "The active phase has no visible animated activity indicator.");
        Click(Button(view, "Cancel"));
        Check(!model.CanCancelPhase, "Cancel remained enabled after the request.");
        Wait(cancelledSetup); Layout(view);
        Check(!model.IsBusy && !model.IsPhaseRunning && model.PhaseSummary.Contains("cancelled"), "Cancellation did not finish cleanly.");
        setupList.SelectedItems.Add(setupList.Items[0]); Pump();
        Click(Button(view, "Run Setup")); WaitUntil(() => !model.IsBusy); Layout(view);
        Check(model.PhaseSummary == "Setup finished: 2 succeeded, 0 failed, 0 skipped." && model.SetupParameterList.SelectedCount == 0,
            "Setup failed to prepare both linked runs: " + model.ErrorMessages);
        Check(model.SetupParameterList.Items.Cast<SetupParameterItem>().Single().SetupStatus == "Completed", "Setup completion was not displayed.");
        Check(originalRows.SequenceEqual(model.RunParameterList.Items.Cast<RunParameterItem>())
            && ids.SequenceEqual(originalRows.Select(row => row.Parameters.GetId())), "Setup replaced live run items or changed their IDs.");
        SelectEditorTab(view, "Processing parameters");
        Check(!Descendants<Button>(view).Any(button => Equals(button.Content, "Run Setup")), "Setup is offered on the Run tab.");
        var runList = Descendants<ListView>(view).Single(control => AutomationProperties.GetName(control) == "Saved RunParameters");
        Check(model.RunParameterList.Keys.Contains("RunStatus"), "Processing status column is missing.");
        Check(originalRows.All(row => row.Status == "Pending"), "Setup changed Processing statuses.");
        var statusRow = (RunParameterItem)runList.Items[0];
        var stripeColors = new HashSet<string>();
        foreach (var status in Enum.GetValues<runRowStatus>())
        {
            statusRow.ShowStatus(status); Layout(view);
            var container = (ListViewItem)runList.ItemContainerGenerator.ContainerFromItem(statusRow);
            Check(statusRow.Cells[Array.IndexOf(model.RunParameterList.Keys, "RunStatus")] == (status is runRowStatus.Cancelled or runRowStatus.Waiting ? "Pending" : status.ToString()), "Status cell did not update.");
            stripeColors.Add(container.BorderBrush.ToString());
        }
        Check(stripeColors.Count == 4, "Processing states do not have distinct row colors.");
        statusRow.ShowStatus(runRowStatus.Waiting);
        runList.SelectedItems.Add(runList.Items[0]); Pump();
        Check(model.CanRunExecution && !model.CanRunSetup, "Run selection changed Setup selection.");
        Click(Button(view, "Run Processing")); WaitUntil(() => !model.IsBusy); Layout(view);
        Check(model.PhaseSummary == "Processing finished: 1 succeeded, 0 failed, 0 skipped.", "Execution failed after shared Setup: " + model.ErrorMessages);
        Check(statusRow.Status == "Completed" && originalRows.Count(row => row.Status == "Pending") == 1,
            "Processing completion updated the wrong row status.");
        var savedTask = RunParameterFiles.loadAllAsync(root, model.Build().ResultValue); Wait(savedTask);
        Check(savedTask.Result.All(row => row.GetSetupCompleted()?.Value == true), "Setup completion was not persisted.");
        Check(savedTask.Result.Count(row => row.GetRunFinished()?.Value == true) == 1, "Execution processed unselected linked runs.");

        var parameterFolder = RunParameterFiles.folderFor(root, model.Build().ResultValue);
        var pendingId = savedTask.Result.Single(row => row.GetRunFinished()?.Value != true).GetId()!.Value;
        File.WriteAllText(parameterFolder + ".processing-failures.json", System.Text.Json.JsonSerializer.Serialize(new[] { pendingId }));
        var legacyFailure = GeneSort.WPF.Settings.ProcessingFailureStore.LoadAsync(parameterFolder); Wait(legacyFailure);
        Check(legacyFailure.Result[pendingId] == GeneSort.WPF.Settings.ProcessingFailureStore.MissingMessage, "Legacy failed status was not migrated.");
        Wait(GeneSort.WPF.Settings.ProcessingFailureStore.SaveAsync(parameterFolder, new Dictionary<Guid, string> { [pendingId] = "Could not load checkpoint.\nMissing source file." }));
        Wait(model.LoadRunParametersAsync());
        Check(model.RunParameterList.Items.Cast<RunParameterItem>().Single(row => row.Parameters.GetId()!.Value == pendingId).Status == "Failed",
            "Failed status was not restored from disk.");
        var retryRow = model.RunParameterList.Items.Cast<RunParameterItem>().Single(row => row.Parameters.GetId()!.Value == pendingId);
        Check(retryRow.FailureMessage == "Could not load checkpoint.\nMissing source file." && retryRow.FailureDetails.Contains(pendingId.ToString()),
            "Per-run failure details were not restored with their row identity.");
        Layout(view);
        var failureLink = Descendants<TextBlock>(view).SelectMany(text => text.Inlines.OfType<Hyperlink>())
            .Single(link => ReferenceEquals(link.DataContext, retryRow));
        Check(failureLink.IsEnabled && ((TextBlock)failureLink.Parent).Visibility == Visibility.Visible,
            "Failed status does not offer a visible, enabled error link.");
        model.RunParameterList.Select([retryRow]);
        var cancelledRetry = model.RunExecutionAsync();
        model.CancelPhase();
        Wait(cancelledRetry);
        Check(retryRow.Status == "Pending", "Cancelled retry retained a cancelled or failed status.");
        Wait(model.LoadRunParametersAsync());
        Check(model.RunParameterList.Items.Cast<RunParameterItem>().Single(row => row.Parameters.GetId()!.Value == pendingId).Status == "Pending",
            "Cancelled retry preserved its old failure on disk.");

        SelectEditorTab(view, "Setup parameters");
        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SortableTests.Prefix.Channels24")); Layout(view);
        var tabs = Descendants<TabControl>(view).Single();
        Check(tabs.Items.Cast<TabItem>().Single(tab => Equals(tab.Header, "Setup parameters")).Visibility == Visibility.Collapsed
            && Equals(((TabItem)tabs.SelectedItem).Header, "Processing parameters"), "A direct run retained a selected Setup tab.");
        Check(model.SetupParameterList.Count == 0 && model.RunParameterList.Count == 0 && !model.CanRunSetup, "Changing run types retained shared setups.");
        Check(bindingErrors.Length == 0, "Processing tab binding errors: " + bindingErrors);
    }
}
