using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using GeneSort.WPF.ViewModels;

namespace GeneSort.WPF;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        const int useImmersiveDarkMode = 20;
        const int captionColor = 35;
        const int textColor = 36;
        var handle = new WindowInteropHelper(this).Handle;

        SetAttribute(useImmersiveDarkMode, 1);
        SetAttribute(captionColor, ColorRef("CanvasBrush"));
        SetAttribute(textColor, ColorRef("TextBrush"));

        void SetAttribute(int attribute, int value)
        {
            // Older Windows versions ignore unsupported attributes and retain native chrome.
            _ = DwmSetWindowAttribute(handle, attribute, ref value, sizeof(int));
        }

        int ColorRef(string resourceKey)
        {
            var color = ((SolidColorBrush)FindResource(resourceKey)).Color;
            return color.R | (color.G << 8) | (color.B << 16);
        }
    }

    [DllImport("dwmapi.dll", ExactSpelling = true)]
    private static extern int DwmSetWindowAttribute(nint window, int attribute, ref int value, int size);

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is MainWindowViewModel model)
        {
            if (model.RunEditor.IsBusy)
                e.Cancel = true;
            else if (model.RunEditor.HasUnsavedChanges && MessageBox.Show(this,
                "Discard the unsaved run definition changes?", "Run editor",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                e.Cancel = true;
        }
        base.OnClosing(e);
    }
}
