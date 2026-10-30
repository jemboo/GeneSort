using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GeneSort.WPF.Views;

internal static partial class Program
{
    private static void CheckDialogs(string? previewDirectory)
    {
        const string message = "Replace this run definition?\n\nProjects / SorterSgd.cc32.p4a.Msuf4.Simple.Standard / Test / NewRun";
        Check(Show(MessageBoxButton.YesNo, MessageBoxImage.Question, message, dialog =>
        {
            Check(Button(dialog, "_Yes").IsDefault && Button(dialog, "_No").IsCancel, "Confirmation keyboard actions changed.");
            SavePreview(dialog, "dark-confirmation.png");
            Click(Button(dialog, "_Yes"));
        }) == MessageBoxResult.Yes, "Yes did not accept confirmation.");
        Check(Show(MessageBoxButton.YesNo, MessageBoxImage.Question, message, dialog => Click(Button(dialog, "_No"))) == MessageBoxResult.No,
            "No accepted a confirmation.");
        Check(Show(MessageBoxButton.YesNo, MessageBoxImage.Question, message, dialog => dialog.Close()) == MessageBoxResult.No,
            "Closing the confirmation accepted it.");
        Check(Show(MessageBoxButton.YesNo, MessageBoxImage.Question, message, dialog => AccessKeyManager.ProcessKey(PresentationSource.FromVisual(dialog), "\u001b", false)) == MessageBoxResult.No,
            "Escape accepted a confirmation.");
        Check(Show(MessageBoxButton.YesNo, MessageBoxImage.Question, message, dialog => AccessKeyManager.ProcessKey(PresentationSource.FromVisual(dialog), "\r", false)) == MessageBoxResult.Yes,
            "Enter did not invoke the default confirmation action.");
        Check(Show(MessageBoxButton.OKCancel, MessageBoxImage.Warning, "The settings file could not be saved.", dialog => Click(Button(dialog, "_Cancel"))) == MessageBoxResult.Cancel,
            "Cancel did not dismiss the warning.");
        Check(Show(MessageBoxButton.OK, MessageBoxImage.Error, "The run could not be opened.\n\nThe selected file does not contain a supported run definition.", dialog =>
        {
            Check(Descendants<Button>(dialog).Count() == 1, "An error dialog offered unexpected actions.");
            SavePreview(dialog, "dark-error.png");
            AccessKeyManager.ProcessKey(PresentationSource.FromVisual(dialog), "\r", false);
        }) == MessageBoxResult.OK, "Enter did not dismiss the error dialog.");

        var longMessage = string.Join('\n', Enumerable.Repeat("The settings could not be saved: " + new string('x', 180), 80));
        Show(MessageBoxButton.OK, MessageBoxImage.Warning, longMessage, dialog =>
        {
            var body = (TextBox)dialog.FindName("Message");
            Check(body.IsReadOnly && body.Text == longMessage, "The dialog lost message text or allowed editing.");
            body.SelectAll();
            Check(body.SelectedText == longMessage, "Error details cannot be selected for copying.");
            var scroller = Descendants<ScrollViewer>(body).First();
            Check(scroller.ScrollableHeight > 0 && dialog.ActualHeight <= SystemParameters.WorkArea.Height,
                "Long errors push the dialog off screen instead of scrolling.");
            Click(Button(dialog, "_OK"));
        });

        Console.WriteLine("PASS: Dark dialog Yes/No/Cancel/close results, Enter/Escape, selectable messages and long-message scrolling.");

        static MessageBoxResult Show(MessageBoxButton buttons, MessageBoxImage image, string text, Action<MessageDialog> interact)
        {
            var dialog = new MessageDialog(text, image == MessageBoxImage.Question ? "Save run" : "Run editor", buttons, image)
            {
                WindowStartupLocation = WindowStartupLocation.Manual, Left = -20000, Top = -20000, ShowActivated = false
            };
            Exception? failure = null;
            // A watchdog ensures a broken keyboard action cannot leave the smoke runner blocked in ShowDialog.
            var timeout = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            timeout.Tick += (_, _) => { failure = new Exception("The message dialog did not close."); dialog.Close(); };
            dialog.ContentRendered += (_, _) =>
            {
                try { interact(dialog); }
                catch (Exception error) { failure = error; dialog.Close(); }
            };
            timeout.Start();
            try { dialog.ShowDialog(); }
            finally { timeout.Stop(); }
            if (failure is not null) throw failure;
            return dialog.Result;
        }

        void SavePreview(MessageDialog dialog, string name)
        {
            if (previewDirectory is null) return;
            var content = (FrameworkElement)dialog.Content;
            var bounds = new Rect(new Size(content.ActualWidth, content.ActualHeight));
            var visual = new DrawingVisual();
            using (var drawing = visual.RenderOpen())
            {
                drawing.DrawRectangle(dialog.Background, null, bounds);
                drawing.DrawRectangle(new VisualBrush(content), null, bounds);
            }
            var bitmap = new RenderTargetBitmap((int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height), 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(Path.Combine(previewDirectory, name));
            encoder.Save(stream);
        }
    }
}
