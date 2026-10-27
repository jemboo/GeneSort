using System.IO;
using System.Windows;
using System.Windows.Controls;
using GeneSort.FileDb.V1;
using GeneSort.Project.V1.Parameters;
using GeneSort.WPF.Settings;
using GeneSort.WPF.ViewModels;
using GeneSort.WPF.Views;
using Microsoft.FSharp.Collections;
using RunParameters = GeneSort.Project.V1.Parameters.parameterSet<GeneSort.Project.V1.Parameters.runParameterKind>;

internal static partial class Program
{
    private static void CheckColumnVisibility(string root, RunParameters[] parameters, summaryReportFiles reports)
    {
        static GridViewColumnCollection Columns(DependencyObject view) => ((GridView)Descendants<ListView>(view).Single().View).Columns;
        static string? Key(GridViewColumn column) => (column.Header as GridViewColumnHeader)?.Tag as string;
        static (string? Key, double Width)[] Snapshot(DependencyObject view) => Columns(view).Select(column => (Key(column), column.Width)).ToArray();
        static ContextMenu Chooser(DependencyObject view)
        {
            var button = Button(view, "Columns…");
            Click(button);
            Check(button.ContextMenu is { IsOpen: true }, $"The column chooser did not open (menu exists: {button.ContextMenu is not null}, button loaded: {button.IsLoaded}).");
            return button.ContextMenu!;
        }
        static void Toggle(MenuItem item, bool show)
        {
            item.IsChecked = show;
            item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Pump();
        }
        static void Show(FrameworkElement view, string? key, bool show)
        {
            var menu = Chooser(view);
            try
            {
                var item = menu.Items.Cast<MenuItem>().Single(item => (string?)item.Tag == key);
                Check(item.IsChecked != show, "The chooser did not reflect the column's visibility.");
                Toggle(item, show);
                Check(menu.IsOpen, "The chooser closed before another column could be selected.");
            }
            finally { menu.IsOpen = false; }
            Layout(view);
        }

        var path = Path.Combine(root, "column-visibility.json");
        var store = new TableLayoutStore(path);
        var parameterModel = new RunParameterListViewModel(store.RunParameters);
        var reportModel = new SummaryReportListViewModel(store.SummaryReports);
        parameterModel.SetParameters(parameters); reportModel.SetReports(reports, 2);
        var parameterView = new RunParameterListView { DataContext = parameterModel, Width = 800, Height = 360 };
        var reportView = new SummaryReportListView { DataContext = reportModel, Width = 800, Height = 360 };
        using var parameterHost = new OffscreenWindow(parameterView);
        using var reportHost = new OffscreenWindow(reportView);
        Layout(parameterView); Layout(reportView);
        var mutation = Columns(parameterView).Single(column => Key(column) == ParamKeys.mutationModKey);
        Columns(parameterView).Move(Columns(parameterView).IndexOf(mutation), 0); mutation.Width = 147;
        var score = Columns(reportView).Single(column => Key(column) == "Score");
        Columns(reportView).Move(Columns(reportView).IndexOf(score), 0); score.Width = 137;
        var parameterLayout = Snapshot(parameterView);
        var reportLayout = Snapshot(reportView);
        parameterModel.Sort(ParamKeys.mutationModKey); parameterModel.Sort(ParamKeys.mutationModKey);
        reportModel.Sort("Score"); reportModel.Sort("Score");
        var list = Descendants<ListView>(parameterView).Single();
        list.SelectedItems.Add(list.Items[0]); list.SelectedItems.Add(list.Items[2]);
        var selected = parameterModel.SelectedParameters.ToHashSet();
        var order = parameterModel.Items.Cast<RunParameterItem>().Select(row => row.Number).ToArray();
        Show(parameterView, ParamKeys.mutationModKey, false); Show(parameterView, null, false);
        Show(reportView, "Score", false); Show(reportView, null, false);
        Check(Snapshot(parameterView).SequenceEqual(parameterLayout.Where(column => column.Key is not null && column.Key != ParamKeys.mutationModKey))
            && Snapshot(reportView).SequenceEqual(reportLayout.Where(column => column.Key is not null && column.Key != "Score")), "The chooser hid the wrong columns.");
        Check(selected.SetEquals(parameterModel.SelectedParameters) && order.SequenceEqual(parameterModel.Items.Cast<RunParameterItem>().Select(row => row.Number))
            && parameterModel.SortKey == ParamKeys.mutationModKey && parameterModel.Descending && reportModel.SortKey == "Score" && reportModel.Descending,
            "Hiding columns changed selected rows or sorting.");
        var persisted = new TableLayoutStore(path);
        Check(persisted.RunParameters.IsHidden(ParamKeys.mutationModKey) && !persisted.SummaryReports.IsHidden("MutationMod")
            && persisted.SummaryReports.IsHidden("Score") && persisted.RunParameters.IsHidden(null) && persisted.SummaryReports.IsHidden(null),
            "Column visibility was not saved independently and immediately.");

        // Hidden columns remain hidden through absent fields, empty selections, and a fresh settings store.
        parameterModel.Clear(); reportModel.Clear();
        parameterModel.SetParameters(parameters.Select(row => RunParameters.create(MapModule.OfSeq(
            row.ParamMap.Where(pair => pair.Key != ParamKeys.mutationModKey).Select(pair => Tuple.Create(pair.Key, pair.Value))))));
        Layout(parameterView); Layout(reportView);
        Check(store.Save() is null, "Could not save visibility while columns were absent.");
        var reopened = new TableLayoutStore(path);
        var nextParameters = new RunParameterListViewModel(reopened.RunParameters);
        var nextReports = new SummaryReportListViewModel(reopened.SummaryReports);
        var nextParameterView = new RunParameterListView { DataContext = nextParameters, Width = 800, Height = 360 };
        var nextReportView = new SummaryReportListView { DataContext = nextReports, Width = 800, Height = 360 };
        using var nextParameterHost = new OffscreenWindow(nextParameterView);
        using var nextReportHost = new OffscreenWindow(nextReportView);
        nextParameters.SetParameters(parameters); nextReports.SetReports(reports, 2);
        Layout(nextParameterView); Layout(nextReportView);
        Check(Snapshot(nextParameterView).SequenceEqual(parameterLayout.Where(column => column.Key is not null && column.Key != ParamKeys.mutationModKey))
            && Snapshot(nextReportView).SequenceEqual(reportLayout.Where(column => column.Key is not null && column.Key != "Score")), "Reopening lost column visibility.");
        foreach (var item in Descendants<ListViewItem>(nextReportView))
        {
            var row = (SummaryReportItem)item.DataContext;
            Check(Descendants<TextBlock>(item).Select(block => block.Text).SequenceEqual(Columns(nextReportView)
                .Select(column => row.Cells[Array.IndexOf(nextReports.Keys, Key(column))] ?? "")), "Hiding columns changed the displayed cell bindings.");
        }
        var exportPath = Path.Combine(root, "visible-summary.tsv");
        var export = nextReportView.SaveCombinedReportAsync(exportPath, overwrite: false); Wait(export);
        var visible = Snapshot(nextReportView).Select(column => column.Key).ToArray();
        var expected = new[] { string.Join('\t', visible) }.Concat(nextReports.Items.Cast<SummaryReportItem>()
            .Select(row => string.Join('\t', visible.Select(key => row.Cells[Array.IndexOf(nextReports.Keys, key)] ?? ""))));
        Check(export.Result && File.ReadAllLines(exportPath).SequenceEqual(expected), "Export included hidden columns or lost row/column order.");
        var duplicate = nextReports.SaveCombinedAsync(Path.Combine(root, "invalid-columns.tsv"), ["RunName", "RunName"], false); Wait(duplicate);
        Check(!duplicate.Result && !File.Exists(Path.Combine(root, "invalid-columns.tsv")), "Export accepted duplicate column keys.");
        Show(nextParameterView, ParamKeys.mutationModKey, true); Show(nextParameterView, null, true);
        Show(nextReportView, "Score", true); Show(nextReportView, null, true);
        Check(Snapshot(nextParameterView).SequenceEqual(parameterLayout) && Snapshot(nextReportView).SequenceEqual(reportLayout),
            "Showing columns again lost their previous positions or widths.");
        Check(((GridViewColumnHeader)Columns(nextReportView)[0].Header).Content.ToString()!.EndsWith(" ▼"), "The restored sort column lost its direction indicator.");

        var before = File.ReadAllBytes(path);
        using (var locked = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)) Show(nextReportView, "Score", false);
        Check(((TextBlock)nextReportView.FindName("LayoutStatus")).Text.Contains("not saved") && before.SequenceEqual(File.ReadAllBytes(path)),
            "A failed visibility save was not reported or damaged the existing settings.");
        Show(nextReportView, "Score", true);

        // All columns can be hidden; the chooser and reset stay available, while empty-column export is disabled.
        var all = Chooser(nextReportView);
        try { foreach (var item in all.Items.Cast<MenuItem>()) Toggle(item, false); }
        finally { all.IsOpen = false; }
        Layout(nextReportView);
        Check(Columns(nextReportView).Count == 0 && nextReports.Count == 4 && !Button(nextReportView, "Save combined report…").IsEnabled,
            "Hiding every column left export enabled or removed report rows.");
        var noColumnsPath = Path.Combine(root, "no-columns.tsv");
        var noColumns = nextReportView.SaveCombinedReportAsync(noColumnsPath, false); Wait(noColumns);
        Check(!noColumns.Result && !File.Exists(noColumnsPath), "Export wrote a report without columns.");
        var choices = Chooser(nextReportView);
        Check(choices.Items.Count == nextReports.Keys.Length + 1 && choices.Items.Cast<MenuItem>().All(item => !item.IsChecked), "Hidden columns cannot be found in the chooser.");
        choices.IsOpen = false;
        Click(Button(nextReportView, "Reset layout")); Layout(nextReportView);
        Check(Columns(nextReportView).Select(Key).SequenceEqual(nextReports.Keys.Cast<string?>().Prepend(null)) && nextReports.CanSaveCombined
            && new TableLayoutStore(path).SummaryReports.Columns.All(column => !column.Hidden), "Reset layout did not show and save all report columns.");
        Show(nextParameterView, ParamKeys.mutationModKey, false);
        nextParameters.Clear(); Layout(nextParameterView);
        Click(Button(nextParameterView, "Reset layout"));
        nextParameters.SetParameters(parameters); Layout(nextParameterView);
        Check(Columns(nextParameterView).Select(Key).SequenceEqual(nextParameters.Keys.Cast<string?>().Prepend(null))
            && new TableLayoutStore(path).RunParameters.Columns.All(column => !column.Hidden), "Reset while empty retained hidden parameter columns.");
    }

    // Popup menus require a loaded presentation source, unlike the other offscreen layout checks.
    private sealed class OffscreenWindow : IDisposable
    {
        private readonly Window window;
        public OffscreenWindow(FrameworkElement view)
        {
            window = new Window { Content = view, Width = view.Width, Height = view.Height,
                Left = -20000, Top = -20000, ShowActivated = false, ShowInTaskbar = false, WindowStyle = WindowStyle.None };
            window.Show();
            Layout(view);
        }
        public void Dispose()
        {
            window.Content = null;
            window.Close();
        }
    }
}
