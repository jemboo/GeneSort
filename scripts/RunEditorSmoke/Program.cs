using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GeneSort.Core;
using GeneSort.FileDb.V1;
using GeneSort.Project.V1;
using GeneSort.Project.V1.Editing;
using GeneSort.Project.V1.Parameters;
using GeneSort.Project.V1.Runtime;
using GeneSort.Project.V1.Storage;
using GeneSort.WPF.ViewModels;
using GeneSort.WPF.Views;
using GeneSort.WPF.Settings;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using RunParameters = GeneSort.Project.V1.Parameters.parameterSet<GeneSort.Project.V1.Parameters.runParameterKind>;

// Offscreen WPF checks in an isolated folder. Phase failures are exercised without prepared inputs;
// user project data is never touched.
internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        var root = Path.Combine(Path.GetTempPath(), "GeneSort-EditorSmoke-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
            var errors = new StringBuilder();
            PresentationTraceSources.DataBindingSource.Listeners.Add(new TextWriterTraceListener(new StringWriter(errors)));
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
            var model = new RunEditorViewModel();
            var view = new RunEditorView
            {
                DataContext = model, Width = 1060, Height = 900,
                Background = new SolidColorBrush(Color.FromRgb(243, 245, 248))
            };
            Layout(view);
            var saveButton = Button(view, "Save");
            Check(!saveButton.IsEnabled, "Save is enabled before selecting a folder and validating.");
            Click(Button(view, "Validate"));
            Check(!saveButton.IsEnabled, "Validation enabled Save without a projects folder.");
            model.ProjectsFolder = "relative-folder"; Pump();
            Check(!saveButton.IsEnabled, "Save accepts a relative projects folder.");
            model.ProjectsFolder = Path.Combine(root, "missing"); Pump();
            Check(!saveButton.IsEnabled, "Save accepts a nonexistent projects folder.");
            model.ProjectsFolder = root; Pump();
            Check(saveButton.IsEnabled, "Selecting a valid folder did not enable an already validated definition.");
            model.Description += " edited"; Pump();
            Check(!saveButton.IsEnabled, "Editing the definition did not invalidate Save.");
            Click(Button(view, "Validate"));
            Check(saveButton.IsEnabled, "Successful validation did not enable Save.");
            model.IsBusy = true; Pump();
            Check(!saveButton.IsEnabled, "Save is enabled during an operation.");
            model.IsBusy = false; Pump();
            var runName = Descendants<TextBox>(view).Single(box => AutomationProperties.GetName(box) == "Run name");
            runName.Text = "EditedRun";
            Pump();
            Check(model.RunName == "EditedRun" && model.IsDirty, "Run name binding did not update the draft.");
            var size = model.SetupParameters.Single(row => row.Key == ParamKeys.sorterCountPerPoolSetKey);
            size.Values[0].Text = "33";
            Check(model.Validate() is null && model.Issues.Any(issue => issue.Contains("divisible")), "Invalid pool size was accepted.");
            size.Values[0].Text = "32";
            Check(!model.CanSave, "Correcting a value enabled Save before revalidation.");
            Check(typeof(ParameterRow).GetProperty(nameof(ParameterRow.Key))!.SetMethod is null, "Parameter identity can still be changed.");
            Check(!model.SetupSection.HasAvailableParameters, "Existing setup keys can still be added twice.");
            Check(model.ExecutionSection.AvailableParameters.All(spec => spec.ExecutionAllowed), "Setup-only keys are offered as overrides.");

            // Remove and recreate parameters through the actual WPF controls.
            var setup = Section(view, model.SetupSection);
            var mutation = model.SetupParameters.Single(row => row.Key == ParamKeys.mutationModKey);
            var code = model.SetupParameters.Single(row => row.Key == ParamKeys.codeModKey);
            Click(Descendants<Button>(setup).Single(button => Equals(button.Content, "Remove") && button.DataContext == mutation));
            Click(Descendants<Button>(setup).Single(button => Equals(button.Content, "Remove") && button.DataContext == code));
            Check(model.Validate() is null && model.Issues.Any(issue => issue.Contains("MutationMod")), "Removing a required parameter was accepted as a valid run.");
            Click(Button(setup, "Add parameter"));
            Layout(view);
            var picker = Descendants<ComboBox>(setup).Single(box => AutomationProperties.GetName(box) == "Parameter to add");
            picker.SelectedItem = model.SetupSection.AvailableParameters.Single(spec => spec.Key == ParamKeys.codeModKey);
            Layout(view);
            Check(model.SetupSection.PendingRow!.Values[0].Kind == parameterValueKind.Text, "Text editor was not selected.");
            EditValue(setup, model.SetupSection.PendingRow.Values[0], "temporary label");
            picker.SelectedItem = model.SetupSection.AvailableParameters.Single(spec => spec.Key == ParamKeys.mutationModKey);
            Layout(view);
            var pending = model.SetupSection.PendingRow!;
            Check(pending.Values.All(value => value.Kind == parameterValueKind.Integer)
                && pending.Snapshot().Values.SequenceEqual(new[] { "0", "1", "2", "3" }), "Choosing another parameter carried incompatible values over.");
            EditValue(setup, pending.Values[0], "1.5");
            Check(pending.HasErrors && !Button(setup, "Add").IsEnabled, "Fractional integers can be added.");
            EditValue(setup, pending.Values[0], "01");
            Check(pending.Error.Contains("duplicated") && !model.SetupSection.CanAdd, "Equivalent duplicate integers can be added.");
            EditValue(setup, pending.Values[0], "0");
            Check(model.HasPendingParameter && model.Validate() is null, "A pending parameter can be omitted silently when saving.");
            if (args.Length > 0)
            {
                Directory.CreateDirectory(args[0]);
                Layout(view);
                Render(view, Path.Combine(args[0], "run-editor-add.png"));
            }
            Click(Button(setup, "Add"));
            Check(!model.SetupSection.IsAdding && model.SetupParameters.Contains(pending)
                && model.SetupSection.AvailableParameters.All(spec => spec.Key != pending.Key), "Adding did not lock in or exclude the chosen parameter.");
            Check(!model.SetupSection.Add(), "A second Add duplicated the row.");

            var labels = new[] { "NoMods", "label, with comma", "quoted \"label\"", " padded label " };
            Click(Button(setup, "Add parameter"));
            picker.SelectedItem = model.SetupSection.AvailableParameters.Single(spec => spec.Key == ParamKeys.codeModKey);
            Layout(view);
            code = model.SetupSection.PendingRow!;
            for (var index = 0; index < labels.Length; index++)
            {
                if (index > 0) { code.AddValue(); Layout(view); }
                EditValue(setup, code.Values[index], labels[index]);
            }
            Click(Button(setup, "Add"));
            Check(code.Snapshot().Values.SequenceEqual(labels), "Text values changed during editing.");

            // Choice editors must preserve invalid saved values until the user corrects them.
            var variant = model.SetupParameters.Single(row => row.Key == ParamKeys.mutatorVariantKey);
            variant.Values[0].Text = "V9";
            Layout(view);
            var choice = Descendants<ComboBox>(setup).Single(box => box.DataContext == variant.Values[0]);
            Check(choice.Text == "V9" && variant.HasErrors, "An unknown saved choice was silently replaced.");
            choice.SelectedItem = "V2";
            Pump();
            Check(variant.Values[0].Text == "V2" && !variant.HasErrors, "Choice selection did not update the draft.");
            variant.AddValue(); variant.AddValue();
            Check(variant.Snapshot().Values.Distinct().Count() == 3 && !variant.CanAddValue, "Choice sweeps allow duplicate defaults.");
            variant.Values.RemoveAt(2); variant.Values.RemoveAt(1);

            var execution = Section(view, model.ExecutionSection);
            Click(Button(execution, "Add override"));
            Layout(view);
            var overridePicker = Descendants<ComboBox>(execution).Single(box => AutomationProperties.GetName(box) == "Parameter to add");
            overridePicker.SelectedItem = model.ExecutionSection.AvailableParameters.Single(spec => spec.Key == ParamKeys.modificationRateKey);
            Layout(view);
            var rate = model.ExecutionSection.PendingRow!;
            Check(rate.Values[0].Text == "0.035" && rate.InheritedValues.Contains("0.035"), "Override did not start from its inherited value.");
            EditValue(execution, rate.Values[0], "1.01");
            Check(rate.HasErrors && !Button(execution, "Add").IsEnabled, "An out-of-range probability can be added.");
            EditValue(execution, rate.Values[0], "0.05");
            rate.AddValue(); Layout(view);
            EditValue(execution, rate.Values[1], "0.1");
            Click(Button(execution, "Add"));
            Layout(view);
            var removeOverride = Descendants<Button>(execution).Single(button => Equals(button.Content, "Remove"));
            var removeBounds = removeOverride.TransformToAncestor(execution).TransformBounds(new Rect(removeOverride.RenderSize));
            Check(removeBounds.Width >= 50 && removeBounds.Right <= execution.ActualWidth,
                "The first override's Remove button is squeezed out of the table.");
            model.SetupParameters.Single(row => row.Key == rate.Key).Values[0].Text = "0.025";
            Check(rate.InheritedValues.Contains("0.025") && rate.Snapshot().Values.SequenceEqual(new[] { "0.05", "0.1" }),
                "Setup edits did not refresh the inherited display or changed the override.");
            if (args.Length > 0)
            {
                Layout(view);
                ((ScrollViewer)view.FindName("EditorScroll")).ScrollToBottom();
                Layout(view);
                Render(view, Path.Combine(args[0], "run-editor-override.png"));
                ((ScrollViewer)view.FindName("EditorScroll")).ScrollToTop();
            }
            var configured = model.Validate() ?? throw new Exception(string.Join("\n", model.Issues));
            Wait(model.SaveAsync(configured, false));
            Check(!model.IsDirty, "Save did not clear the dirty flag.");
            var path = RunDefinitionFiles.pathFor(root, configured);
            model.Description = "Unsaved description";
            var corrupt = Path.Combine(root, "invalid.msgpack");
            File.WriteAllText(corrupt, "not a run definition");
            var rejected = false;
            try { Wait(model.OpenAsync(corrupt)); }
            catch (Exception) { rejected = true; }
            Check(rejected && model.Description == "Unsaved description" && model.IsDirty, "Failed open changed the current draft.");
            Wait(model.OpenAsync(path));
            Check(!model.IsDirty && model.RunName == "EditedRun" && model.ProjectsFolder == root, "Opening changed the identity or folder.");
            Check(model.ExecutionParameters.Single().Snapshot().Values.SequenceEqual(new[] { "0.05", "0.1" }), "Execution overrides were lost.");
            Check(model.SetupParameters.Single(row => row.Key == ParamKeys.codeModKey).Snapshot().Values.SequenceEqual(labels), "Saving changed quoted parameter values.");
            model.ExecutionSection.BeginAdd();
            model.ExecutionSection.SelectedParameter = model.ExecutionSection.AvailableParameters.First();
            Check(model.HasUnsavedChanges && !model.IsDirty, "An uncommitted parameter is not protected on close.");
            Check(model.Validate() is null, "Pending edits do not block saving.");
            model.ExecutionSection.CancelAdd();
            Check(!model.HasUnsavedChanges && model.ExecutionParameters.Count == 1 && model.Issues.Count == 0,
                "Cancelling added a row, dirtied the saved definition or left a stale pending warning.");
            var orphan = model.SetupParameters[0];
            model.New();
            orphan.Values[0].Text = "99";
            Check(!model.IsDirty, "Cleared rows still affect the editor.");

            // Unsupported opened parameters remain visible and require deliberate removal.
            var unknown = new ParameterRow("UnsupportedKey", new[] { "original, literal value" });
            model.SetupParameters.Add(unknown);
            Layout(view);
            Check(unknown.Snapshot().Values[0] == "original, literal value" && model.Validate() is null,
                "An unknown parameter was changed or accepted.");
            model.SetupParameters.Remove(unknown);
            var detachedValue = variant.Values[0];
            var changes = 0;
            variant.Values.Clear();
            variant.PropertyChanged += (_, _) => changes++;
            detachedValue.Text = "V3";
            Check(changes == 0, "Removed values still notify their old row.");
            model.New();

            var tabs = Descendants<TabControl>(view).Single();
            for (var index = 0; index < tabs.Items.Count; index++)
            {
                tabs.SelectedIndex = index;
                Layout(view);
                if (index == 1)
                {
                    var interval = Descendants<ComboBox>(view).Single(box => AutomationProperties.GetName(box) == "Generation save schedule");
                    interval.SelectedValue = GenIntervalNames.uniformInterval2;
                    Pump();
                    Check(model.SaveIntervals == GenIntervalNames.uniformInterval2, "Interval selection did not update the draft.");
                }
                if (index == 3)
                {
                    var first = Descendants<TextBox>(view).Single(box => AutomationProperties.GetName(box) == "First replica");
                    var count = Descendants<TextBox>(view).Single(box => AutomationProperties.GetName(box) == "Replica count");
                    var generate = Button(view, "Generate RunParameters");
                    first.Text = "-1"; count.Text = "3"; Pump();
                    Check(!generate.IsEnabled, "Negative first replica was accepted.");
                    first.Text = "2"; count.Text = "0"; Pump();
                    Check(!generate.IsEnabled, "Zero replicas were accepted.");
                    count.Text = "3"; Pump();
                    Check(generate.IsEnabled && model.GenerationPreview.Contains("12 RunParameter files")
                        && model.GenerationPreview.Contains("2–4"), "The generated-file preview does not match the replica range.");
                    Check(!Directory.Exists(RunParameterFiles.folderFor(root, RunEditor.build(model.Snapshot()).ResultValue)),
                        "The live preview wrote parameter files.");
                    Click(generate);
                    WaitUntil(() => !model.IsBusy);
                    var generated = RunEditor.build(model.Snapshot()).ResultValue;
                    var folder = RunParameterFiles.folderFor(root, generated);
                    Check(Directory.GetFiles(folder, "*.msgpack", SearchOption.AllDirectories).Length == 12
                        && !model.IsDirty && model.Status.Contains("12 RunParameter files"), "The Generate button did not publish the previewed set.");
                    Check(model.RunParameterList.Count == 12 && model.CanSave, "Generation did not load its saved rows or retain validation.");
                    count.Text = "1"; Pump();
                    var previewTask = model.PrepareGenerationAsync();
                    Wait(previewTask);
                    var prepared = previewTask.Result ?? throw new Exception("No generation plan was created.");
                    Check(prepared.RequiresReplacement && prepared.ExistingFileCount == 12 && prepared.FileCount == 4,
                        "Regeneration did not identify the existing files.");
                    var refused = false;
                    try { Wait(model.GenerateParametersAsync(prepared, false)); }
                    catch (InvalidOperationException) { refused = true; }
                    Check(refused && Directory.GetFiles(folder, "*.msgpack", SearchOption.AllDirectories).Length == 12,
                        "Regeneration overwrote files without permission.");
                    Wait(model.GenerateParametersAsync(prepared, true));
                    Check(Directory.GetFiles(folder, "*.msgpack", SearchOption.AllDirectories).Length == 4
                        && Directory.GetDirectories(folder).Select(Path.GetFileName).SequenceEqual(new[] { "2" }),
                        "Regeneration left obsolete replicas behind.");
                    model.ExecutionSection.BeginAdd(); Pump();
                    Check(!generate.IsEnabled, "Generation is enabled while a parameter is pending.");
                    Check(!saveButton.IsEnabled, "Save is enabled while a parameter is pending.");
                    model.ExecutionSection.CancelAdd();
                    Layout(view);
                }
                if (index == 4)
                {
                    var list = Descendants<ListView>(view).Single(list => AutomationProperties.GetName(list) == "Saved RunParameters");
                    Check(list.Items.Count == 4, "The RunParameters items control does not show the saved set.");
                    var columns = (GridView)list.View;
                    Check(columns.Columns.Count == model.RunParameterList.Keys.Length + 1, "Not every saved property has a column.");
                    var mutationHeader = Descendants<GridViewColumnHeader>(list).Single(header => Equals(header.Tag, ParamKeys.mutationModKey));
                    mutationHeader.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); Pump();
                    Check(model.RunParameterList.Items.Cast<RunParameterItem>().Select(row => row.Values[ParamKeys.mutationModKey])
                        .SequenceEqual(new[] { "0", "1", "2", "3" }), "Column click did not sort ascending.");
                    mutationHeader.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); Pump();
                    Check(model.RunParameterList.Descending && mutationHeader.Content.ToString()!.Contains("▼"), "Second column click did not reverse or indicate sorting.");
                    var mutationColumn = columns.Columns.Single(column => ReferenceEquals(column.Header, mutationHeader));
                    var numberColumn = columns.Columns[0];
                    columns.Columns.Move(columns.Columns.IndexOf(mutationColumn), 0);
                    columns.Columns.Move(columns.Columns.IndexOf(numberColumn), columns.Columns.Count - 1);
                    mutationColumn.Width = 143;
                    numberColumn.Width = 61;
                    var parameterLayout = columns.Columns.Select(column => ((column.Header as GridViewColumnHeader)?.Tag, column.Width)).ToArray();
                    void CheckParameterLayout() => Check(columns.Columns.Select(column => ((column.Header as GridViewColumnHeader)?.Tag, column.Width)).SequenceEqual(parameterLayout),
                        "Refreshing saved RunParameters lost the column order or widths.");
                    model.FirstReplica = "99";
                    Click(Button(view, "Refresh saved files"));
                    WaitUntil(() => !model.IsBusy); Layout(view);
                    CheckParameterLayout();
                    Check(model.RunParameterList.Count == 4 && model.RunParameterList.Descending
                        && model.RunParameterList.SortKey == ParamKeys.mutationModKey
                        && mutationHeader.Content.ToString()!.EndsWith(" ▼")
                        && model.RunParameterList.Items.Cast<RunParameterItem>().Select(row => row.Values[ParamKeys.mutationModKey]).SequenceEqual(new[] { "3", "2", "1", "0" })
                        && model.RunParameterList.Items.Cast<RunParameterItem>().All(row => row.Values[ParamKeys.replKey] == "2"),
                        "Refresh used the generation range or lost the selected sort.");
                    mutationHeader.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); Pump();
                    Click(Button(view, "Refresh saved files"));
                    WaitUntil(() => !model.IsBusy); Layout(view);
                    CheckParameterLayout();
                    Check(!model.RunParameterList.Descending && mutationHeader.Content.ToString()!.EndsWith(" ▲")
                        && model.RunParameterList.Items.Cast<RunParameterItem>().Select(row => row.Values[ParamKeys.mutationModKey]).SequenceEqual(new[] { "0", "1", "2", "3" }),
                        "Refreshing RunParameters did not retain ascending sorting.");
                    var savedDefinition = RunDefinitionFiles.pathFor(root, RunEditor.build(model.Snapshot()).ResultValue);
                    Wait(model.OpenAsync(savedDefinition));
                    Layout(view);
                    Check(model.RunParameterList.Count == 4 && model.CanSave, "Opening a definition did not load and validate its saved parameters.");
                    var runSetup = Button(view, "Run Setup");
                    var runExecution = Button(view, "Run Execution");
                    var summaryReports = Button(view, "Generate Summary Reports");
                    Check(!runSetup.IsEnabled && !runExecution.IsEnabled && !summaryReports.IsEnabled, "A run action is enabled without a selection.");
                    list.SelectedItems.Add(list.Items[1]);
                    list.SelectedItems.Add(list.Items[3]);
                    Pump();
                    var selected = model.RunParameterList.SelectedParameters.ToHashSet();
                    Check(runSetup.IsEnabled && runExecution.IsEnabled && summaryReports.IsEnabled && model.RunParameterList.SelectedCount == 2,
                        "Selected rows did not enable the phase and report actions.");
                    model.RunParameterList.Sort(ParamKeys.mutationModKey); Pump();
                    Check(selected.SetEquals(model.RunParameterList.SelectedParameters), "Sorting changed the selected parameter identities.");
                    var sawBusy = false;
                    var sawProgress = false;
                    model.PropertyChanged += (_, change) =>
                    {
                        if (change.PropertyName == nameof(model.IsBusy) && model.IsBusy)
                            sawBusy = !model.CanRunSetup && !model.CanRunExecution && !model.CanGenerateSummaryReports && !model.CanLoadRunParameters;
                        if (change.PropertyName == nameof(model.PhaseCompleted) && model.PhaseCompleted > 0)
                            sawProgress = true;
                    };
                    Click(runSetup);
                    WaitUntil(() => !model.IsBusy);
                    Layout(view);
                    Check(sawBusy && sawProgress && model.PhaseTotal == 2 && model.PhaseCompleted == 2,
                        "Setup did not disable other operations or report progress for the selected rows.");
                    Check(model.PhaseSummary.Contains("2 failed") && model.Issues.Count == 2
                        && model.Issues.All(issue => issue.Contains("source library")),
                        "Missing sources were not reported for each selected row.");
                    Check(model.RunParameterList.Count == 4 && model.RunParameterList.SelectedCount == 0 && !runSetup.IsEnabled,
                        "Setup did not refresh saved rows and clear its completed selection.");
                    CheckParameterLayout();
                    Check(!runExecution.IsEnabled, "Execution is enabled after the completed selection was cleared.");
                    list.SelectedItems.Add(list.Items[0]);
                    list.SelectedItems.Add(list.Items[2]);
                    Pump();
                    selected = model.RunParameterList.SelectedParameters.ToHashSet();
                    model.RunParameterList.Sort(ParamKeys.mutationModKey); Pump();
                    Check(selected.SetEquals(model.RunParameterList.SelectedParameters), "Sorting changed the selection for Execution.");
                    sawBusy = false; sawProgress = false;
                    Click(runExecution);
                    WaitUntil(() => !model.IsBusy);
                    Layout(view);
                    Check(sawBusy && sawProgress && model.PhaseTotal == 2 && model.PhaseCompleted == 2,
                        "Execution did not disable other operations or report selected-row progress.");
                    Check(model.PhaseSummary == "Execution finished: 0 succeeded, 2 failed, 0 skipped."
                        && model.Issues.Count == 2 && model.Issues.All(issue => issue.Contains("Setup") && !issue.Contains("source library")),
                        "Execution did not report missing prepared inputs independently of Setup.");
                    Check(model.RunParameterList.Count == 4 && model.RunParameterList.SelectedCount == 0
                        && !runSetup.IsEnabled && !runExecution.IsEnabled, "Execution did not refresh rows and reset phase actions.");
                    CheckParameterLayout();
                    Check(!Directory.Exists(Path.Combine(root, model.ProjectName, model.DatabaseName, "Data", "Setup")),
                        "Execution unexpectedly prepared missing setup files.");
                    Check(!summaryReports.IsEnabled, "Summary Reports is enabled after the completed selection was cleared.");
                    list.SelectedItems.Add(list.Items[1]);
                    list.SelectedItems.Add(list.Items[2]);
                    Pump();
                    selected = model.RunParameterList.SelectedParameters.ToHashSet();
                    model.RunParameterList.Sort(ParamKeys.mutationModKey); Pump();
                    Check(selected.SetEquals(model.RunParameterList.SelectedParameters), "Sorting changed the selection for Summary Reports.");
                    sawBusy = false; sawProgress = false;
                    Click(summaryReports);
                    WaitUntil(() => !model.IsBusy);
                    Layout(view);
                    Check(sawBusy && sawProgress && model.PhaseTotal == 2 && model.PhaseCompleted == 2,
                        "Summary Reports did not disable other operations or report selected-row progress.");
                    Check(model.PhaseSummary == "Summary Reports finished: 0 succeeded, 2 failed, 0 skipped."
                        && model.Issues.Count == 2 && model.Issues.All(issue => issue.Contains("No valid slice files")),
                        "Summary Reports did not report missing saved results for each selected row.");
                    Check(model.RunParameterList.Count == 4 && model.RunParameterList.SelectedCount == 0
                        && !runSetup.IsEnabled && !runExecution.IsEnabled && !summaryReports.IsEnabled,
                        "Summary Reports did not refresh rows and reset actions.");
                    CheckParameterLayout();
                    Check(!Directory.Exists(Path.Combine(root, model.ProjectName, model.DatabaseName, "Data", "Setup")),
                        "Reporting unexpectedly prepared setup files.");
                }
                if (index == 5)
                {
                    CheckSummaryViewer(root, view, model, tabs);
                }
                if (args.Length > 0)
                {
                    Directory.CreateDirectory(args[0]);
                    Render(view, Path.Combine(args[0], $"run-editor-{index}.png"));
                }
            }
            tabs.SelectedIndex = 0;
            view.Width = 700; view.Height = 400;
            Layout(view);
            var save = Descendants<Button>(view).Single(button => Equals(button.Content, "Save"));
            var bounds = save.TransformToAncestor(view).TransformBounds(new Rect(save.RenderSize));
            Check(save.Visibility == Visibility.Visible && bounds.Width > 0 && bounds.Left >= 0 && bounds.Right <= view.Width
                && bounds.Top >= 0 && bounds.Bottom <= view.Height, "Save is outside the minimum layout.");
            ((ScrollViewer)view.FindName("EditorScroll")).ScrollToVerticalOffset(300);
            Layout(view);
            var firstValues = Descendants<ParameterValuesView>(view).First(editor => editor.DataContext == model.SetupParameters[0]);
            var valueBounds = firstValues.TransformToAncestor(view).TransformBounds(new Rect(firstValues.RenderSize));
            Check(valueBounds.Left >= 0 && valueBounds.Right <= view.Width && valueBounds.Width >= 200,
                "The parameter value editor is clipped horizontally at the minimum width.");
            if (args.Length > 0) Render(view, Path.Combine(args[0], "run-editor-small.png"));
            CheckSortingAndVirtualization();
            var current = RunEditor.build(model.Snapshot()).ResultValue;
            var parameterFolder = RunParameterFiles.folderFor(root, current);
            var parameterPath = Directory.GetFiles(parameterFolder, "*.msgpack", SearchOption.AllDirectories)[0];
            var parameterBytes = File.ReadAllBytes(parameterPath);
            if (model.RunParameterList.SortKey != ParamKeys.mutationModKey) model.RunParameterList.Sort(ParamKeys.mutationModKey);
            if (!model.RunParameterList.Descending) model.RunParameterList.Sort(ParamKeys.mutationModKey);
            tabs.SelectedIndex = 4; Layout(view);
            var parameterColumns = ((GridView)Descendants<ListView>(view).Single(list => AutomationProperties.GetName(list) == "Saved RunParameters").View).Columns;
            var layoutBeforeFailure = parameterColumns.Select(column => ((column.Header as GridViewColumnHeader)?.Tag, column.Width)).ToArray();
            File.WriteAllText(parameterPath, "invalid");
            Wait(model.LoadRunParametersAsync());
            Check(model.RunParameterList.Count == 0 && model.SummaryReportList.Count == 0 && model.Status.Contains("Could not load"), "A failed refresh left stale parameters or reports visible.");
            Check(model.RunParameterList.SortKey == ParamKeys.mutationModKey && model.RunParameterList.Descending, "A failed refresh discarded the RunParameters sort preference.");
            File.WriteAllBytes(parameterPath, parameterBytes);
            Wait(model.LoadRunParametersAsync()); Layout(view);
            Check(parameterColumns.Select(column => ((column.Header as GridViewColumnHeader)?.Tag, column.Width)).SequenceEqual(layoutBeforeFailure)
                && model.RunParameterList.SortKey == ParamKeys.mutationModKey && model.RunParameterList.Descending
                && model.RunParameterList.Items.Cast<RunParameterItem>().Select(row => row.Values[ParamKeys.mutationModKey]).SequenceEqual(new[] { "3", "2", "1", "0" }),
                "Recovery from a failed refresh did not restore the RunParameters layout and sorting.");
            model.RunName = "AnotherRun";
            Check(!model.CanSave && model.RunParameterList.Count == 0 && model.SummaryReportList.Count == 0, "Changing runs retained validation or rows from the previous run.");
            if (errors.Length > 0) throw new Exception("WPF binding errors:\n" + errors);
            Console.WriteLine("PASS: Selected-row Setup, Execution and Summary Reports, report viewer loading/refresh/layout preservation/sorting/virtualization/selection changes/export, progress/failure reporting, busy gating, Save validation/folder gating, RunParameters loading/refresh/layout preservation/sorting/virtualization, generation, parameter controls and WPF layout/bindings.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally { Directory.Delete(root, true); }
    }

    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void CheckSummaryViewer(string root, RunEditorView view, RunEditorViewModel model, TabControl tabs)
    {
        Check(!Button(view, "Refresh reports").IsEnabled && model.SummaryReportList.Count == 0,
            "Report viewing is enabled without a selection.");
        Check(!Button(view, "Save combined report…").IsEnabled, "Combined export is enabled without displayed rows.");
        var configured = RunEditor.build(model.Snapshot()).ResultValue;
        var dataRoot = Path.Combine(root, configured.ProjectName, configured.DatabaseName, "Data");
        IGeneSortDb database = new GeneSortDbMp(dataRoot, FSharpOption<string>.Some(configured.RunQueryCatalogName));
        var generation = GenIntervalConfig.getSampleAtIndex(RunIntervals.forRun(configured).Value, 0).Value;
        var parameters = model.RunParameterList.Items.Cast<RunParameterItem>().OrderBy(row => row.Values[ParamKeys.mutationModKey]).ToArray();
        string WriteReport(int index, string contents)
        {
            var query = database.makeQueryParamsFromRunParams(parameters[index].Parameters.WithGenerationCurrent(FSharpOption<int>.Some(generation)),
                geneSortDataType.NewTextReport("SummaryReport")).Value;
            var path = OutputDataFile.getFullOutputDataFilePath(dataRoot, geneSortDataMode.Results, query);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, contents);
            return path;
        }
        var path0 = WriteReport(0, $"RunName\trepl\tMutationMod\tScore\tSeed\tLabel\n{configured.RunName}\t2\t0\t10\t18446744073709551615\tzeta\n{configured.RunName}\t2\t0\tNaN\t1\tAlpha\n");
        var path1 = WriteReport(1, $"Score\tRunName\tMutationMod\tSeed\tExtra[0]\tLabel\n2\t{configured.RunName}\t1\t18446744073709551614\tyes\tbeta\nInfinity\t{configured.RunName}\t1\t2\t\tgamma\n");
        var before0 = File.ReadAllBytes(path0);
        var before1 = File.ReadAllBytes(path1);
        tabs.SelectedIndex = 4; Layout(view);
        var parameterList = Descendants<ListView>(view).Single(list => AutomationProperties.GetName(list) == "Saved RunParameters");
        parameterList.SelectedItems.Add(parameters[0]);
        parameterList.SelectedItems.Add(parameters[1]);
        parameterList.SelectedItems.Add(parameters[2]); // Deliberately missing.
        Pump();
        WaitUntil(() => !model.SummaryReportList.IsLoading);
        tabs.SelectedIndex = 5; Layout(view);
        var reports = model.SummaryReportList;
        var reportList = Descendants<ListView>(view).Single(list => AutomationProperties.GetName(list) == "Saved Summary Reports");
        Check(reports.Count == 4 && reports.Errors.Length == 1 && reports.Errors[0].Contains("No saved Summary Report"),
            "The viewer did not combine available reports and identify a missing report.");
        Check(Descendants<ListViewItem>(reportList).Count() == 4, "Loaded summary rows are not visible in the control.");
        Check(((GridView)reportList.View).Columns.Count == reports.Keys.Length + 1 && reports.Keys.Contains("Extra[0]"),
            "The viewer did not build the union of report columns.");
        string?[] Values(string key) => reports.Items.Cast<SummaryReportItem>().Select(row => row.Cells[Array.IndexOf(reports.Keys, key)]).ToArray();
        var scoreHeader = Descendants<GridViewColumnHeader>(reportList).Single(header => Equals(header.Tag, "Score"));
        scoreHeader.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); Pump();
        Check(Values("Score").SequenceEqual(new[] { "2", "10", "Infinity", "NaN" }), "Summary scores were not sorted numerically with unavailable values last.");
        scoreHeader.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); Pump();
        Check(Values("Score").SequenceEqual(new[] { "Infinity", "10", "2", "NaN" }) && scoreHeader.Content.ToString()!.Contains("▼"),
            "Report sorting did not reverse or indicate its direction.");
        reports.Sort("Seed");
        Check(Values("Seed").SequenceEqual(new[] { "1", "2", "18446744073709551614", "18446744073709551615" }), "Report seeds lost integer precision while sorting.");
        reports.Sort("Label");
        Check(Values("Label").SequenceEqual(new[] { "Alpha", "beta", "gamma", "zeta" }), "Report labels did not sort alphabetically.");
        reports.Sort("Extra[0]");
        Check(Values("Extra[0]")[0] == "yes" && Values("Extra[0]").Skip(1).All(string.IsNullOrEmpty), "Absent report columns did not stay at the end.");
        CheckSummaryExport(root, view, reports);
        var savedReports = RunSummaryFiles.loadAsync(root, configured,
            new[] { parameters[0].Parameters, parameters[1].Parameters }, CancellationToken.None);
        Wait(savedReports);
        CheckPersistentLayouts(root, parameters.Select(row => row.Parameters).ToArray(), savedReports.Result);
        WriteReport(3, $"RunName\tScore\tLaterMetric\n{configured.RunName}\t7\tnew\n");
        CheckSummaryLayout(root, view, model, tabs, parameters);
        Click(Button(view, "Refresh reports")); WaitUntil(() => !reports.IsLoading); Layout(view);
        Check(reports.SortKey == "Extra[0]" && reports.Count == 4, "Refreshing reports lost the sort or report rows.");
        Check(before0.SequenceEqual(File.ReadAllBytes(path0)) && before1.SequenceEqual(File.ReadAllBytes(path1)), "Viewing or sorting reports changed the saved files.");

        tabs.SelectedIndex = 4; Layout(view);
        parameterList = Descendants<ListView>(view).Single(list => AutomationProperties.GetName(list) == "Saved RunParameters");
        // Rapid selection changes exercise cancellation and prevent an older load from winning the race.
        parameterList.SelectedItems.Clear();
        parameterList.SelectedItems.Add(parameters[0]);
        parameterList.SelectedItems.Clear();
        parameterList.SelectedItems.Add(parameters[1]);
        Pump(); WaitUntil(() => !reports.IsLoading);
        Check(reports.Count == 2 && Values("MutationMod").All(value => value == "1"), "An earlier selection replaced the current report rows.");
        parameterList.SelectedItems.Clear(); Pump();
        Check(reports.Count == 0 && reports.Errors.Length == 0 && !reports.CanSaveCombined, "Clearing the selection left stale reports available for export.");

        // The same ItemsControl must virtualize large summary tables.
        WriteReport(0, "RunName\tScore\n" + string.Join("\n", Enumerable.Range(0, 2000).Select(value => $"{configured.RunName}\t{value}")));
        parameterList.SelectedItems.Add(parameters[0]); Pump(); WaitUntil(() => !reports.IsLoading);
        tabs.SelectedIndex = 5; Layout(view);
        reportList = Descendants<ListView>(view).Single(list => AutomationProperties.GetName(list) == "Saved Summary Reports");
        Check(reports.Count == 2000 && Descendants<ListViewItem>(reportList).Count() is > 0 and < 100, "The Summary Reports view does not virtualize its rows.");
        var largeExport = Path.Combine(root, "all-summary-rows.tsv");
        var reportControl = Descendants<SummaryReportListView>(view).Single();
        var savingAll = reportControl.SaveCombinedReportAsync(largeExport, overwrite: false);
        Wait(savingAll);
        Check(savingAll.Result && File.ReadAllLines(largeExport).Length == 2001, "Combined export omitted rows outside the visible viewport.");
        // Leave a small, readable combined table for the rendered preview.
        File.WriteAllBytes(path0, before0);
        tabs.SelectedIndex = 4; Layout(view);
        parameterList = Descendants<ListView>(view).Single(list => AutomationProperties.GetName(list) == "Saved RunParameters");
        parameterList.SelectedItems.Add(parameters[1]); Pump(); WaitUntil(() => !reports.IsLoading);
        tabs.SelectedIndex = 5; Layout(view);
        reports.Sort("Score");
        Layout(view);
        reportList = Descendants<ListView>(view).Single(list => AutomationProperties.GetName(list) == "Saved Summary Reports");
        Check(Descendants<ListViewItem>(reportList).Count() == 4, "Refreshed summary rows are not visible in the control.");
    }

    private static void CheckPersistentLayouts(string root, RunParameters[] parameters, summaryReportFiles reports)
    {
        var path = Path.Combine(root, "display-settings", "table-layouts.json");
        var store = new TableLayoutStore(path);
        var parameterModel = new RunParameterListViewModel(store.RunParameters);
        var reportModel = new SummaryReportListViewModel(store.SummaryReports);
        parameterModel.SetParameters(parameters);
        reportModel.SetReports(reports, 2);
        var parameterView = new RunParameterListView { DataContext = parameterModel, Width = 800, Height = 360 };
        var reportView = new SummaryReportListView { DataContext = reportModel, Width = 800, Height = 360 };
        Layout(parameterView); Layout(reportView);
        static GridViewColumnCollection Columns(DependencyObject view) => ((GridView)Descendants<ListView>(view).Single().View).Columns;
        static string? Key(GridViewColumn column) => (column.Header as GridViewColumnHeader)?.Tag as string;
        static (string? Key, double Width)[] Snapshot(DependencyObject view) => Columns(view).Select(column => (Key(column), column.Width)).ToArray();
        static void MoveFirst(DependencyObject view, string key, double width)
        {
            var columns = Columns(view);
            var column = columns.Single(column => Key(column) == key);
            column.Width = width;
            columns.Move(columns.IndexOf(column), 0);
        }
        MoveFirst(parameterView, ParamKeys.mutationModKey, 147);
        var number = Columns(parameterView).Single(column => Key(column) is null);
        number.Width = 59;
        Columns(parameterView).Move(Columns(parameterView).IndexOf(number), Columns(parameterView).Count - 1);
        Columns(parameterView).Single(column => Key(column) == ParamKeys.runNameKey).Width = double.NaN;
        parameterModel.Sort(ParamKeys.mutationModKey); parameterModel.Sort(ParamKeys.mutationModKey);
        MoveFirst(reportView, "Score", 123);
        Columns(reportView).Single(column => Key(column) is null).Width = 72;
        Columns(reportView).Single(column => Key(column) == "MutationMod").Width = 185;
        reportModel.Sort("Score");
        var parameterLayout = Snapshot(parameterView);
        var reportLayout = Snapshot(reportView);

        // One control is hidden, the other still visible at shutdown with its last resize/reorder unsnapshotted.
        reportView.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        reportModel.Clear();
        Check(store.Save() is null && File.Exists(path), "Closing did not save table display settings.");
        var saved = File.ReadAllBytes(path);
        using (var locked = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            Check(store.Save() is not null, "An unsuccessful settings save was not reported.");
        Check(saved.SequenceEqual(File.ReadAllBytes(path)) && !Directory.GetFiles(Path.GetDirectoryName(path)!, "*.tmp").Any(),
            "An unsuccessful settings save changed the previous file or left a temporary file.");

        // Reopen with fresh objects and no rows, then close again before all saved columns have appeared.
        var emptySession = new TableLayoutStore(path);
        var emptyParameters = new RunParameterListViewModel(emptySession.RunParameters);
        var emptyReports = new SummaryReportListViewModel(emptySession.SummaryReports);
        var emptyParameterView = new RunParameterListView { DataContext = emptyParameters, Width = 800, Height = 360 };
        var emptyReportView = new SummaryReportListView { DataContext = emptyReports, Width = 800, Height = 360 };
        emptyParameters.Clear(); emptyReports.Clear();
        emptyParameters.SetParameters(parameters.Select(row => RunParameters.create(MapModule.OfSeq(
            row.ParamMap.Where(pair => pair.Key != ParamKeys.mutationModKey).Select(pair => Tuple.Create(pair.Key, pair.Value))))));
        Layout(emptyParameterView); Layout(emptyReportView);
        Check(Snapshot(emptyParameterView).SequenceEqual(parameterLayout.Where(column => column.Key != ParamKeys.mutationModKey))
            && Snapshot(emptyReportView).SequenceEqual(reportLayout.Where(column => column.Key is null)),
            "Reopening with absent fields lost the saved column layout.");
        Check(emptyParameters.SortKey == ParamKeys.mutationModKey && emptyParameters.Descending
            && emptyReports.SortKey == "Score" && !emptyReports.Descending, "Reopening without the sort field reset the saved sort.");
        Check(emptySession.Save() is null, "An empty session could not preserve display settings.");

        var reopened = new TableLayoutStore(path);
        var restoredParameters = new RunParameterListViewModel(reopened.RunParameters);
        var restoredReports = new SummaryReportListViewModel(reopened.SummaryReports);
        restoredParameters.Clear(); restoredReports.Clear();
        restoredParameters.SetParameters(parameters);
        restoredReports.SetReports(reports, 2);
        var restoredParameterView = new RunParameterListView { DataContext = restoredParameters, Width = 800, Height = 360 };
        var restoredReportView = new SummaryReportListView { DataContext = restoredReports, Width = 800, Height = 360 };
        Layout(restoredParameterView); Layout(restoredReportView);
        Check(Snapshot(restoredParameterView).SequenceEqual(parameterLayout) && Snapshot(restoredReportView).SequenceEqual(reportLayout),
            "Reopening did not restore both tables' independent column orders and widths.");
        Check(restoredParameters.Items.Cast<RunParameterItem>().Select(row => row.Values[ParamKeys.mutationModKey]).SequenceEqual(new[] { "3", "2", "1", "0" })
            && restoredReports.Items.Cast<SummaryReportItem>().Select(row => row.Cells[Array.IndexOf(restoredReports.Keys, "Score")]).SequenceEqual(new[] { "2", "10", "Infinity", "NaN" }),
            "Reopening did not restore both tables' independent sorting.");
        Check(((GridViewColumnHeader)Columns(restoredParameterView)[0].Header).Content.ToString()!.EndsWith(" ▼")
            && ((GridViewColumnHeader)Columns(restoredReportView)[0].Header).Content.ToString()!.EndsWith(" ▲"),
            "Restored sort headings have the wrong direction indicators.");
        var exportPath = Path.Combine(root, "reopened-summary.tsv");
        var export = restoredReportView.SaveCombinedReportAsync(exportPath, overwrite: false);
        Wait(export);
        var exported = File.ReadAllLines(exportPath);
        Check(export.Result && exported[0] == string.Join('\t', reportLayout.Select(column => column.Key ?? "#"))
            && exported.Skip(1).Select(line => line.Split('\t')[0]).SequenceEqual(new[] { "2", "10", "Infinity", "NaN" }),
            "Combined export ignored the layout and sorting restored from disk.");

        var invalidPath = Path.Combine(root, "invalid-display-settings.json");
        File.WriteAllText(invalidPath, "{invalid");
        var invalid = new TableLayoutStore(invalidPath);
        Check(invalid.RunParameters.Columns.Count == 0 && invalid.SummaryReports.SortKey is null, "Malformed display settings did not fall back to defaults.");
        File.WriteAllText(invalidPath, """
            {"RunParameters":{"Columns":[null,{"Key":"Score","Width":-1},{"Key":"Score","Width":99},{"Key":null,"Width":59}],"SortKey":""},"SummaryReports":null}
            """);
        invalid = new TableLayoutStore(invalidPath);
        Check(invalid.RunParameters.Columns.Count == 2 && invalid.RunParameters.Columns[0].Width is null
            && invalid.RunParameters.SortKey is null && invalid.SummaryReports.Columns.Count == 0,
            "Invalid or duplicated display settings were not safely normalized.");
    }

    private static void CheckSummaryLayout(string root, RunEditorView view, RunEditorViewModel model, TabControl tabs, RunParameterItem[] parameters)
    {
        var reports = model.SummaryReportList;
        GridViewColumnCollection Columns() => ((GridView)Descendants<ListView>(view)
            .Single(list => AutomationProperties.GetName(list) == "Saved Summary Reports").View).Columns;
        static string? Key(GridViewColumn column) => (column.Header as GridViewColumnHeader)?.Tag as string;
        GridViewColumn Column(string? key) => Columns().Single(column => Key(column) == key);
        void Select(params int[] indices)
        {
            tabs.SelectedIndex = 4; Layout(view);
            var list = Descendants<ListView>(view).Single(list => AutomationProperties.GetName(list) == "Saved RunParameters");
            list.SelectedItems.Clear();
            foreach (var index in indices) list.SelectedItems.Add(parameters[index]);
            Pump(); WaitUntil(() => !reports.IsLoading);
            tabs.SelectedIndex = 5; Layout(view);
        }
        void CheckLayout(IEnumerable<(string? Key, double Width)> expected)
        {
            var columns = Columns();
            Check(columns.Select(column => (Key(column), column.Width)).SequenceEqual(expected),
                "Summary column order or widths changed after reloading reports.");
            // Check rendered cells too: changing schemas changes the backing cell indices.
            var list = Descendants<ListView>(view).Single(list => AutomationProperties.GetName(list) == "Saved Summary Reports");
            foreach (var item in Descendants<ListViewItem>(list))
            {
                var row = (SummaryReportItem)item.DataContext;
                var cells = columns.Select(column => Key(column) is string key
                    ? row.Cells[Array.IndexOf(reports.Keys, key)] ?? "" : row.Number.ToString());
                Check(Descendants<TextBlock>(item).Select(block => block.Text).SequenceEqual(cells),
                    "Restored summary columns display values from the wrong fields.");
            }
        }
        void CheckSort(string key, bool descending)
        {
            Check(reports.SortKey == key && reports.Descending == descending, "A reload lost the preferred summary sort.");
            if (reports.Keys.Contains(key))
                Check(((GridViewColumnHeader)Column(key).Header).Content.ToString()!.EndsWith(descending ? " ▼" : " ▲"),
                    "The restored summary sort has the wrong header indicator.");
        }

        var columns = Columns();
        columns.Move(columns.IndexOf(Column("Score")), 0);
        columns.Move(columns.IndexOf(Column(null)), columns.Count - 1);
        Column("Score").Width = 137;
        Column(null).Width = 63;
        Column("Extra[0]").Width = 174;
        Column("Seed").Width = double.NaN; // Preserve automatic sizing as well as explicit widths.
        var expected = columns.Select(column => (Key: Key(column), Width: column.Width)).ToList();
        reports.Sort("Score"); reports.Sort("Score");
        Click(Button(view, "Refresh reports")); WaitUntil(() => !reports.IsLoading); Layout(view);
        CheckLayout(expected); CheckSort("Score", true);
        Check(reports.Items.Cast<SummaryReportItem>().Select(row => row.Cells[Array.IndexOf(reports.Keys, "Score")])
            .SequenceEqual(new[] { "Infinity", "10", "2", "NaN" }), "Refresh did not reapply the summary sort.");

        Select(1); // No repl column, and the source file uses a different header order.
        CheckLayout(expected.Where(column => column.Key != "repl")); CheckSort("Score", true);
        reports.Sort("Extra[0]"); reports.Sort("Extra[0]");
        Select(0); // The preferred sort column disappears.
        CheckLayout(expected.Where(column => column.Key != "Extra[0]")); CheckSort("Extra[0]", true);
        columns = Columns();
        columns.Move(columns.IndexOf(Column("Label")), 0);
        Column("Label").Width = 119;
        expected.RemoveAll(column => column.Key == "Label");
        expected.Insert(0, ("Label", 119));

        Select();
        Check(reports.Count == 0, "An empty selection retained summary rows.");
        CheckLayout(expected.Where(column => column.Key is null)); CheckSort("Extra[0]", true);
        Select(2); // An unavailable report must not reset layout or sorting either.
        Check(reports.Count == 0 && reports.Errors.Length == 1, "The missing report fixture unexpectedly loaded.");
        CheckLayout(expected.Where(column => column.Key is null)); CheckSort("Extra[0]", true);
        Select(0, 1);
        CheckLayout(expected); CheckSort("Extra[0]", true);
        Check(reports.Items.Cast<SummaryReportItem>().Select(row => row.Cells[Array.IndexOf(reports.Keys, "Label")])
            .SequenceEqual(new[] { "beta", "zeta", "Alpha", "gamma" }), "The returning sort column did not restore row order.");

        Select(0, 1, 3); // Newly encountered columns belong after the remembered layout.
        CheckLayout(expected.Append(("LaterMetric", 110d))); CheckSort("Extra[0]", true);
        Select(0, 1, 2);
        CheckLayout(expected); CheckSort("Extra[0]", true);
        var path = Path.Combine(root, "restored-summary-layout.tsv");
        var saving = Descendants<SummaryReportListView>(view).Single().SaveCombinedReportAsync(path, overwrite: false);
        Wait(saving);
        var lines = File.ReadAllLines(path);
        Check(saving.Result && lines[0] == string.Join('\t', expected.Select(column => column.Key ?? "#"))
            && lines.Skip(1).Select(line => line.Split('\t')[0]).SequenceEqual(new[] { "beta", "zeta", "Alpha", "gamma" }),
            "Combined export did not use the restored summary layout and sorting.");
    }

    private static void CheckSummaryExport(string root, RunEditorView view, SummaryReportListViewModel reports)
    {
        var control = Descendants<SummaryReportListView>(view).Single();
        var list = Descendants<ListView>(control).Single();
        var columns = ((GridView)list.View).Columns;
        var originalColumns = columns.ToArray();
        Check(Button(control, "Save combined report…").IsEnabled, "Combined export is disabled for available report rows.");
        var score = columns.Single(column => column.Header is GridViewColumnHeader { Tag: "Score" });
        columns.Move(columns.IndexOf(score), 0);
        columns.Move(columns.IndexOf(originalColumns[0]), columns.Count - 1); // Move the displayed # too.
        reports.Sort("Score"); // Ascending, then descending on the next export.
        var sawSaving = false;
        System.ComponentModel.PropertyChangedEventHandler savingChanged = (_, change) =>
        {
            if (change.PropertyName == nameof(reports.IsSaving) && reports.IsSaving)
                sawSaving = !reports.CanSaveCombined;
        };
        reports.PropertyChanged += savingChanged;
        var path = Path.Combine(root, "displayed-summary.tsv");
        try
        {
            foreach (var descending in new[] { false, true })
            {
                if (descending) reports.Sort("Score");
                var order = columns.Select(column => (column.Header as GridViewColumnHeader)?.Tag as string).ToArray();
                var expected = new[] { string.Join('\t', order.Select(key => key ?? "#")) }
                    .Concat(list.Items.Cast<SummaryReportItem>().Select(row => string.Join('\t', order.Select(key =>
                        key is null ? row.Number.ToString() : row.Cells[Array.IndexOf(reports.Keys, key)] ?? "")))).ToArray();
                var saving = control.SaveCombinedReportAsync(path, overwrite: descending);
                Wait(saving);
                Check(saving.Result && File.ReadAllLines(path).SequenceEqual(expected), "Combined export did not preserve displayed row and column order.");
                var scores = File.ReadAllLines(path).Skip(1).Select(line => line.Split('\t')[0]);
                Check(scores.SequenceEqual(descending ? new[] { "Infinity", "10", "2", "NaN" } : new[] { "2", "10", "Infinity", "NaN" }),
                    "Combined export lost numeric sorting or missing-value placement.");
            }
            Check(sawSaving && reports.CanSaveCombined && reports.SaveStatus.Contains("Saved 4 summary rows"), "Combined export did not report success or restore its button.");
            var before = File.ReadAllBytes(path);
            var refused = control.SaveCombinedReportAsync(path, overwrite: false);
            Wait(refused);
            Check(!refused.Result && before.SequenceEqual(File.ReadAllBytes(path)) && reports.SaveStatus.Contains("Could not save"),
                "An unsuccessful export overwrote the destination or did not show its error.");

            // A later sort/column change must not alter a save already in progress.
            var frozenOrder = columns.Select(column => (column.Header as GridViewColumnHeader)?.Tag as string).ToArray();
            var frozenScores = reports.Items.Cast<SummaryReportItem>().Select(row => row.Cells[Array.IndexOf(reports.Keys, "Score")]).ToArray();
            var snapshotPath = Path.Combine(root, "summary-snapshot.tsv");
            var snapshot = control.SaveCombinedReportAsync(snapshotPath, overwrite: false);
            reports.Sort("Label");
            columns.Move(0, columns.Count - 1);
            Wait(snapshot);
            var lines = File.ReadAllLines(snapshotPath);
            Check(snapshot.Result && lines[0] == string.Join('\t', frozenOrder.Select(key => key ?? "#"))
                && lines.Skip(1).Select(line => line.Split('\t')[0]).SequenceEqual(frozenScores), "An in-progress export changed when the display changed.");
            Check(!Directory.GetFiles(root, ".summary-*.tmp").Any(), "An export left temporary files behind.");
        }
        finally
        {
            reports.PropertyChanged -= savingChanged;
            for (var index = 0; index < originalColumns.Length; index++) columns.Move(columns.IndexOf(originalColumns[index]), index);
            reports.Sort("Extra[0]");
            Layout(view);
        }
    }
    private static void CheckSortingAndVirtualization()
    {
        static RunParameters Row(params (string Key, string Value)[] values) =>
            RunParameters.create(MapModule.OfSeq(values.Select(pair => Tuple.Create(pair.Key, pair.Value))));
        var rows = new[] {
            Row(("Repl", "10"), ("MutationMod", "2"), ("Rate", "0.1"), ("Seed", "18446744073709551615"), ("State", "zeta"), ("A[0].Label", "literal")),
            Row(("Repl", "2"), ("MutationMod", "10"), ("Rate", "0.02"), ("Seed", "18446744073709551614"), ("State", "Alpha")),
            Row(("Repl", "-1"), ("MutationMod", "2"), ("Rate", "-0.5"), ("State", "alpha")),
            Row(("Repl", "2"), ("Rate", "1e-4"), ("Seed", "1"))
        };
        var model = new RunParameterListViewModel();
        model.SetParameters(rows);
        void Order(params int[] expected) => Check(model.Items.Cast<RunParameterItem>().Select(row => row.Number).SequenceEqual(expected), $"Incorrect order for {model.SortKey} (descending={model.Descending}).");
        Order(3, 2, 4, 1);
        model.Sort("MutationMod"); Order(1, 3, 2, 4);
        model.Sort("MutationMod"); Order(2, 1, 3, 4);
        model.Sort("Rate"); Order(3, 4, 2, 1);
        model.Sort("Seed"); Order(4, 2, 1, 3);
        model.Sort("State"); Order(2, 3, 1, 4);
        model.Sort("State"); Order(1, 2, 3, 4);
        Check(rows[0].ParamMap["Repl"] == "10", "Sorting mutated the source parameters.");
        var view = new RunParameterListView { DataContext = model, Width = 700, Height = 360 };
        Layout(view);
        Check(model.Keys.Contains("A[0].Label"), "Unknown property names were dropped.");
        var list = Descendants<ListView>(view).Single();
        var columns = ((GridView)list.View).Columns;
        static string? Key(GridViewColumn column) => (column.Header as GridViewColumnHeader)?.Tag as string;
        var stateColumn = columns.Single(column => Key(column) == "State");
        var numberColumn = columns.Single(column => Key(column) is null);
        columns.Move(columns.IndexOf(stateColumn), 0);
        columns.Move(columns.IndexOf(numberColumn), columns.Count - 1);
        stateColumn.Width = 133;
        numberColumn.Width = 51;
        columns.Single(column => Key(column) == "A[0].Label").Width = 96;
        var expected = columns.Select(column => (Key: Key(column), Width: column.Width)).ToArray();
        void CheckLayout(IEnumerable<(string? Key, double Width)> layout)
        {
            Layout(view);
            Check(columns.Select(column => (Key(column), column.Width)).SequenceEqual(layout), "RunParameters lost its column layout when the available fields changed.");
            foreach (var item in Descendants<ListViewItem>(list))
            {
                var row = (RunParameterItem)item.DataContext;
                var cells = columns.Select(column => Key(column) is string key ? row.Values.GetValueOrDefault(key) ?? "" : row.Number.ToString());
                Check(Descendants<TextBlock>(item).Select(block => block.Text).SequenceEqual(cells), "Restored RunParameters columns display the wrong parameter values.");
            }
        }
        model.Clear();
        model.SetParameters(rows);
        CheckLayout(expected); Order(1, 2, 3, 4);
        Check(model.SortKey == "State" && model.Descending, "Refreshing RunParameters lost the descending sort preference.");
        // Empty results and a temporarily missing sort field must not replace the user's preference.
        model.SetParameters([]);
        CheckLayout(expected.Where(column => column.Key is null));
        model.SetParameters(rows.Select(row => Row(row.ParamMap.Where(pair => pair.Key != "State")
            .Select(pair => (pair.Key, pair.Value)).Append(("Added", "new")).ToArray())));
        CheckLayout(expected.Where(column => column.Key != "State").Append(("Added", 110d)));
        Check(model.SortKey == "State" && model.Descending, "An absent sort field reset the RunParameters sort preference.");
        model.SetParameters(rows);
        CheckLayout(expected); Order(1, 2, 3, 4);
        var header = (GridViewColumnHeader)stateColumn.Header;
        Check(header.Content.ToString()!.EndsWith(" ▼"), "The returning RunParameters sort field lost its direction indicator.");
        header.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); Pump();
        Order(2, 3, 1, 4);
        model.Clear();
        model.SetParameters(rows);
        CheckLayout(expected); Order(2, 3, 1, 4);
        Check(model.SortKey == "State" && !model.Descending && header.Content.ToString()!.EndsWith(" ▲"),
            "Refreshing RunParameters lost the ascending sort or attached duplicate header handlers.");
        model.SetParameters(Enumerable.Range(0, 2000).Select(index => Row(("Repl", index.ToString()), ("Value", "example"))));
        Layout(view);
        Check(list.Items.Count == 2000 && Descendants<ListViewItem>(view).Count() < 100,
            "The list renders all rows instead of virtualizing them.");
        model.Clear(); Layout(view);
        Check(list.Items.Count == 0 && ((GridView)list.View).Columns.Count == 1, "Clearing leaves stale rows or columns.");
    }
    private static ParameterSectionView Section(DependencyObject view, ParameterSectionViewModel model) =>
        Descendants<ParameterSectionView>(view).Single(section => section.DataContext == model);
    private static Button Button(DependencyObject view, string label) =>
        Descendants<Button>(view).Single(button => Equals(button.Content, label));
    private static void Click(Button button)
    {
        Check(button.IsEnabled, $"Button '{button.Content}' is disabled.");
        button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
        Pump();
    }
    private static void EditValue(DependencyObject view, ParameterValue value, string text)
    {
        Descendants<TextBox>(view).Single(box => box.DataContext == value).Text = text;
        Pump();
        Check(value.Text == text, "Value binding did not update the draft.");
    }
    private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
    private static void WaitUntil(Func<bool> complete)
    {
        var watch = Stopwatch.StartNew();
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(25) };
        timer.Tick += (_, _) => { if (complete() || watch.Elapsed > TimeSpan.FromSeconds(30)) frame.Continue = false; };
        timer.Start();
        Dispatcher.PushFrame(frame);
        timer.Stop();
        Check(complete(), "The UI operation did not finish within 30 seconds.");
    }
    private static void Wait(Task task)
    {
        // Allow awaited view-model operations to resume on the STA dispatcher.
        var frame = new DispatcherFrame();
        var dispatcher = Dispatcher.CurrentDispatcher;
        task.ContinueWith(_ => dispatcher.BeginInvoke(() => frame.Continue = false), TaskScheduler.Default);
        Dispatcher.PushFrame(frame);
        task.GetAwaiter().GetResult();
    }
    private static void Layout(FrameworkElement view)
    {
        view.Measure(new Size(view.Width, view.Height));
        view.Arrange(new Rect(0, 0, view.Width, view.Height));
        view.UpdateLayout();
        Pump();
    }
    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) yield return match;
            foreach (var descendant in Descendants<T>(child)) yield return descendant;
        }
    }
    private static void Render(FrameworkElement view, string path)
    {
        var bitmap = new RenderTargetBitmap((int)view.Width, (int)view.Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(view);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}
