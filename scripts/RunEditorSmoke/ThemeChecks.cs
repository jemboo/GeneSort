using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GeneSort.WPF;
using GeneSort.WPF.Controls;
using GeneSort.WPF.ViewModels;
using GeneSort.WPF.Views;

internal static partial class Program
{
    private static void CheckThemedControls(RunEditorView editor, string? previewDirectory)
    {
        // Popups need a real window; keep the original unhosted view available for size checks.
        var view = new RunEditorView { DataContext = editor.DataContext, Width = editor.Width,
            Height = editor.Height, Background = editor.Background };
        using (var host = new OffscreenWindow(view))
        {
            var tabs = Descendants<TabControl>(view).Single();
            tabs.SelectedIndex = 5; Layout(view);
            var list = Descendants<ListView>(view).Single();
            list.SelectedIndex = 0;
            Layout(view);
            SavePreview(view, "dark-selected-report.png");

            // Custom header templates must retain WPF's actual drag-to-resize behavior.
            var header = Descendants<GridViewColumnHeader>(list).First(item => item.Column is not null);
            var width = header.Column.Width;
            var actualWidth = header.Column.ActualWidth;
            var grip = (Thumb)header.Template.FindName("PART_HeaderGripper", header);
            grip.RaiseEvent(new DragStartedEventArgs(0, 0) { RoutedEvent = Thumb.DragStartedEvent });
            grip.RaiseEvent(new DragDeltaEventArgs(24, 0) { RoutedEvent = Thumb.DragDeltaEvent });
            grip.RaiseEvent(new DragCompletedEventArgs(24, 0, false) { RoutedEvent = Thumb.DragCompletedEvent });
            Check(Math.Abs(header.Column.Width - actualWidth - 24) < 1, "The themed header no longer resizes its column.");
            header.Column.Width = width;

            var chooserButton = Button(view, "Columns…");
            Click(chooserButton); Pump();
            var chooser = chooserButton.ContextMenu!;
            Check(chooser.IsOpen && Descendants<MenuItem>(chooser).Any(), "The themed column chooser has no visible items.");
            SavePreview(chooser, "dark-column-chooser.png");
            chooser.IsOpen = false;
            list.SelectedIndex = -1;

            tabs.SelectedIndex = 1; Layout(view);
            var combo = Descendants<ComboBox>(view).First();
            combo.IsDropDownOpen = true; Pump();
            var popup = (Popup)combo.Template.FindName("PART_Popup", combo);
            Check(popup.IsOpen && Descendants<ComboBoxItem>(popup.Child).Any(), "The themed dropdown has no visible choices.");
            SavePreview((FrameworkElement)popup.Child, "dark-dropdown.png");
            combo.IsDropDownOpen = false;
            tabs.SelectedIndex = 5; Layout(view);
        }

        CheckRemainingThemeStates(previewDirectory);

        if (previewDirectory is null) return;
        var window = new MainWindow { Left = -20000, Top = -20000, ShowActivated = false, ShowInTaskbar = false, Width = 1280, Height = 900 };
        try
        {
            window.Show(); Pump();
            var tabs = Descendants<TabControl>(window).First();
            tabs.SelectedIndex = 1; window.UpdateLayout(); Pump();
            SavePreview((FrameworkElement)window.Content, "dark-workspace.png");
            tabs.SelectedIndex = 0; window.UpdateLayout(); Pump();
            var browser = Descendants<MessagePackBrowserView>(window).Single();
            using var json = JsonDocument.Parse("""{"RunName":"TestStandard","MutationMod":2,"Generation":100,"Pools":[{"SorterCount":32,"BestSwitchCount":185}]}""");
            browser.CurrentViewer = new RunParametersOutputDataControl([MessagePackNode.FromJson("Run parameters", json.RootElement)]);
            window.UpdateLayout(); Pump();
            var node = Descendants<TreeViewItem>(browser.CurrentViewer).First();
            node.IsExpanded = true;
            window.UpdateLayout(); Pump();
            SavePreview((FrameworkElement)window.Content, "dark-browser.png");
            browser.CurrentViewer = new TextReportOutputDataControl("RunName\tGeneration\tSorterCount\nTestStandard\t100\t32\nTestSimpleR\t100\t32", "Preview");
            window.UpdateLayout(); Pump();
            Descendants<DataGrid>(browser.CurrentViewer).Single().SelectedIndex = 0;
            window.UpdateLayout(); Pump();
            SavePreview((FrameworkElement)window.Content, "dark-browser-table.png");
        }
        finally { window.DataContext = null; window.Close(); }

        void SavePreview(FrameworkElement element, string name)
        {
            if (previewDirectory is null) return;
            element.UpdateLayout();
            var bitmap = new RenderTargetBitmap((int)Math.Ceiling(element.ActualWidth), (int)Math.Ceiling(element.ActualHeight), 96, 96, PixelFormats.Pbgra32);
            // Render local bounds so a hosted view's window offset does not crop the preview.
            var bounds = new Rect(new Size(element.ActualWidth, element.ActualHeight));
            var visual = new DrawingVisual();
            using (var drawing = visual.RenderOpen())
                drawing.DrawRectangle(new VisualBrush(element) { Viewbox = bounds, ViewboxUnits = BrushMappingMode.Absolute }, null, bounds);
            bitmap.Render(visual);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(Path.Combine(previewDirectory, name));
            encoder.Save(stream);
        }
    }
}
