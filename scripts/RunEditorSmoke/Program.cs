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
            var model = new RunEditorViewModel { ProjectsFolder = root };
            var view = new RunEditorView
            {
                DataContext = model, Width = 1060, Height = 640,
                Background = new SolidColorBrush(Color.FromRgb(243, 245, 248))
            };
            Layout(view);
            var runName = Descendants<TextBox>(view).Single(box => AutomationProperties.GetName(box) == "Run name");
            runName.Text = "EditedRun";
            Pump();
            Check(model.RunName == "EditedRun" && model.IsDirty, "Run name binding did not update the draft.");
            var size = model.SetupParameters.Single(row => row.Key == ParamKeys.sorterCountPerPoolSetKey);
            size.Values = "33";
            Check(model.Validate() is null && model.Issues.Any(issue => issue.Contains("divisible")), "Invalid pool size was accepted.");
            size.Values = "32";
            var code = model.SetupParameters.Single(row => row.Key == ParamKeys.codeModKey);
            code.Values = "\"unclosed";
            Check(model.Validate() is null && model.Issues.Any(issue => issue.Contains("CodeMod")), "Malformed quoted values were accepted.");
            var labels = new[] { "NoMods", "label, with comma", "quoted \"label\"", " padded label " };
            code.Values = ParameterRow.FormatValues(labels);
            Check(code.Snapshot().Values.SequenceEqual(labels), "Quoted parameter values changed during editing.");
            model.ExecutionParameters.Add(new ParameterRow { Key = ParamKeys.modificationRateKey, Values = "0.05,0.1" });
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
            Check(model.ExecutionParameters.Single().Values == "0.05, 0.1", "Execution overrides were lost.");
            Check(model.SetupParameters.Single(row => row.Key == ParamKeys.codeModKey).Snapshot().Values.SequenceEqual(labels), "Saving changed quoted parameter values.");
            var orphan = model.SetupParameters[0];
            model.New();
            orphan.Values = "99";
            Check(!model.IsDirty, "Cleared rows still affect the editor.");

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
            if (args.Length > 0) Render(view, Path.Combine(args[0], "run-editor-small.png"));
            if (errors.Length > 0) throw new Exception("WPF binding errors:\n" + errors);
            Console.WriteLine("PASS: WPF bindings, validation, save/open, failed-open preservation, dirty tracking, all tabs and minimum layout.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally { Directory.Delete(root, true); }
    }

    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
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
