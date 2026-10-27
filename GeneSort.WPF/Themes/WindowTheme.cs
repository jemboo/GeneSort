using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace GeneSort.WPF.Themes;

internal static class WindowTheme
{
    // Call after WPF has created the native window handle.
    internal static void Apply(Window window)
    {
        const int useImmersiveDarkMode = 20;
        const int captionColor = 35;
        const int textColor = 36;
        var handle = new WindowInteropHelper(window).Handle;

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
            var color = ((SolidColorBrush)window.FindResource(resourceKey)).Color;
            return color.R | (color.G << 8) | (color.B << 16);
        }
    }

    [DllImport("dwmapi.dll", ExactSpelling = true)]
    private static extern int DwmSetWindowAttribute(nint window, int attribute, ref int value, int size);
}
