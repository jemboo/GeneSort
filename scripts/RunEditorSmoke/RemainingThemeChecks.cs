using System.IO;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GeneSort.WPF.Controls;

internal static partial class Program
{
    private static void CheckRemainingThemeStates(string? previewDirectory)
    {
        var panel = new StackPanel { Margin = new Thickness(20) };
        var surface = new Border { Width = 620, Height = 620, Child = panel,
            Background = (Brush)Application.Current.FindResource("SurfaceBrush") };
        panel.Children.Add(new TextBlock { Text = "Progress and browser table", Foreground = (Brush)Application.Current.FindResource("TextBrush") });
        var progress = new ProgressBar { Value = 40, Height = 12, Margin = new Thickness(0, 12, 0, 12) };
        var busy = new ProgressBar { IsIndeterminate = true, Height = 12, Margin = new Thickness(0, 0, 0, 12) };
        panel.Children.Add(progress);
        panel.Children.Add(busy);
        var text = new TextBox { Text = "Selectable text with edit commands", Margin = new Thickness(0, 0, 0, 12) };
        panel.Children.Add(text);
        var readOnly = new TextBox { Text = "Read-only path or error details", IsReadOnly = true, Margin = new Thickness(0, 0, 0, 12) };
        panel.Children.Add(readOnly);
        var report = new TextReportOutputDataControl("RunName\tGeneration\tSorterCount\n" + string.Join('\n',
            Enumerable.Range(1, 30).Select(i => $"Long example run name {i}\t100\t32")), "Theme states") { Height = 360 };
        panel.Children.Add(report);
        using var host = new OffscreenWindow(surface);
        var grid = Descendants<DataGrid>(report).Single();
        grid.RowHeaderWidth = 24;
        foreach (var column in grid.Columns) column.Width = 230;
        grid.SelectedIndex = 0;
        surface.UpdateLayout(); Pump();
        Save(surface, "dark-control-states.png");

        // Exercise commands through real menu items without replacing the user's clipboard.
        OpenMenu(text);
        Invoke(text.ContextMenu!, ApplicationCommands.SelectAll);
        Check(text.SelectedText == text.Text, "The text menu did not select the field's text.");
        var original = text.Text;
        text.SelectedText = "Changed text";
        OpenMenu(text);
        Invoke(text.ContextMenu!, ApplicationCommands.Undo);
        Check(text.Text == original, "The text menu did not undo the edit.");
        OpenMenu(text);
        Invoke(text.ContextMenu!, ApplicationCommands.Redo);
        Check(text.Text == "Changed text", "The text menu did not redo the edit.");

        readOnly.SelectAll();
        OpenMenu(readOnly);
        var menu = readOnly.ContextMenu!;
        try
        {
            Check(Item(menu, ApplicationCommands.Copy).IsEnabled, "Read-only details cannot be copied.");
            Check(!Item(menu, ApplicationCommands.Cut).IsEnabled && !Item(menu, ApplicationCommands.Paste).IsEnabled,
                "The read-only menu allows destructive editing commands.");
            Save(menu, "dark-text-menu.png");
            readOnly.Select(0, 0);
            Invoke(menu, ApplicationCommands.SelectAll);
            Check(readOnly.SelectedText == readOnly.Text && text.Text == "Changed text",
                "The shared text menu still targets the previously opened field.");
        }
        finally { menu.IsOpen = false; }
        var tip = new ToolTip { Content = "Parameter help and validation details", PlacementTarget = text, IsOpen = true };
        Pump(); Save(tip, "dark-tooltip.png"); tip.IsOpen = false;
        Console.WriteLine("PASS: Dark text-menu command targeting, Select All, Undo/Redo and read-only command availability.");

        static MenuItem Item(ContextMenu menu, ICommand command) => menu.Items.OfType<MenuItem>().Single(item => item.Command == command);

        static void OpenMenu(TextBox target)
        {
            target.Focus();
            var menu = target.ContextMenu;
            Check(menu is not null, "The text field has no themed editing menu.");
            menu!.PlacementTarget = target;
            menu.IsOpen = true;
            CommandManager.InvalidateRequerySuggested(); Pump();
            Check(menu.Items.OfType<MenuItem>().All(item => ReferenceEquals(item.CommandTarget, target)),
                "An editing command targets the wrong field.");
        }

        static void Invoke(ContextMenu menu, ICommand command)
        {
            var item = Item(menu, command);
            Check(item.IsEnabled, $"The editing command {command} is disabled.");
            var peer = new MenuItemAutomationPeer(item);
            ((IInvokeProvider)peer.GetPattern(PatternInterface.Invoke)).Invoke(); Pump();
            menu.IsOpen = false;
        }

        void Save(FrameworkElement element, string name)
        {
            if (previewDirectory is null) return;
            element.UpdateLayout();
            var bounds = new Rect(new Size(element.ActualWidth, element.ActualHeight));
            var visual = new DrawingVisual();
            using (var drawing = visual.RenderOpen())
                drawing.DrawRectangle(new VisualBrush(element) { Viewbox = bounds, ViewboxUnits = BrushMappingMode.Absolute }, null, bounds);
            var bitmap = new RenderTargetBitmap((int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height), 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(Path.Combine(previewDirectory, name));
            encoder.Save(stream);
        }
    }
}
