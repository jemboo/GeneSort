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
using GeneSort.Project.V1.Editing;
using GeneSort.Project.V1.Parameters;
using GeneSort.WPF.ViewModels;
using GeneSort.WPF.Views;
using Microsoft.FSharp.Collections;
using RunParameters = GeneSort.Project.V1.Parameters.parameterSet<GeneSort.Project.V1.Parameters.runParameterKind>;

// Offscreen WPF binding and editing checks. No setup or evolution is executed.
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
                    model.FirstReplica = "99";
                    Click(Button(view, "Refresh saved files"));
                    WaitUntil(() => !model.IsBusy);
                    Check(model.RunParameterList.Count == 4 && model.RunParameterList.Descending
                        && model.RunParameterList.Items.Cast<RunParameterItem>().All(row => row.Values[ParamKeys.replKey] == "2"),
                        "Refresh used the generation range or lost the selected sort.");
                    var savedDefinition = RunDefinitionFiles.pathFor(root, RunEditor.build(model.Snapshot()).ResultValue);
                    Wait(model.OpenAsync(savedDefinition));
                    Layout(view);
                    Check(model.RunParameterList.Count == 4 && model.CanSave, "Opening a definition did not load and validate its saved parameters.");
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
            File.WriteAllText(Directory.GetFiles(parameterFolder, "*.msgpack", SearchOption.AllDirectories)[0], "invalid");
            Wait(model.LoadRunParametersAsync());
            Check(model.RunParameterList.Count == 0 && model.Status.Contains("Could not load"), "A failed refresh left a stale or incomplete list visible.");
            model.RunName = "AnotherRun";
            Check(!model.CanSave && model.RunParameterList.Count == 0, "Changing runs retained validation or rows from the previous run.");
            if (errors.Length > 0) throw new Exception("WPF binding errors:\n" + errors);
            Console.WriteLine("PASS: Save validation/folder gating, RunParameters loading/refresh, numeric/text/missing-value sorting, row virtualization, generation, parameter controls and WPF layout/bindings.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally { Directory.Delete(root, true); }
    }

    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
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
        model.SetParameters(Enumerable.Range(0, 2000).Select(index => Row(("Repl", index.ToString()), ("Value", "example"))));
        Layout(view);
        var list = Descendants<ListView>(view).Single();
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
