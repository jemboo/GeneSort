using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GeneSort.WPF.Themes;

namespace GeneSort.WPF.Views;

/// App-owned messages share the theme, while file and folder selection stays with Windows.
public partial class MessageDialog : Window
{
    internal MessageBoxResult Result { get; private set; }

    internal MessageDialog(string message, string title, MessageBoxButton buttons, MessageBoxImage image)
    {
        InitializeComponent();
        Title = title;
        Message.Text = message;
        Width = Math.Min(Width, SystemParameters.WorkArea.Width - 32);
        Message.MaxHeight = Math.Max(80, SystemParameters.WorkArea.Height - 220);

        (string Label, MessageBoxResult Result)[] choices = buttons switch
        {
            MessageBoxButton.OK => [("_OK", MessageBoxResult.OK)],
            MessageBoxButton.OKCancel => [("_OK", MessageBoxResult.OK), ("_Cancel", MessageBoxResult.Cancel)],
            MessageBoxButton.YesNo => [("_Yes", MessageBoxResult.Yes), ("_No", MessageBoxResult.No)],
            MessageBoxButton.YesNoCancel => [("_Yes", MessageBoxResult.Yes), ("_No", MessageBoxResult.No), ("_Cancel", MessageBoxResult.Cancel)],
            _ => throw new ArgumentOutOfRangeException(nameof(buttons))
        };
        // Escape and the title-bar close button never accept a confirmation.
        Result = choices[^1].Result;
        foreach (var (label, choice) in choices)
        {
            var button = new Button
            {
                Content = label, MinWidth = 84, Margin = new Thickness(8, 0, 0, 0),
                IsDefault = choice == choices[0].Result, IsCancel = choice == choices[^1].Result
            };
            if (button.IsDefault) button.SetResourceReference(StyleProperty, "PrimaryButton");
            button.Click += (_, _) => { Result = choice; DialogResult = choice is MessageBoxResult.OK or MessageBoxResult.Yes; };
            Buttons.Children.Add(button);
        }

        var (symbol, brush, labelText) = image switch
        {
            MessageBoxImage.Error => ("×", "ErrorBrush", "Error"),
            MessageBoxImage.Warning => ("!", "WarningBrush", "Warning"),
            MessageBoxImage.Question => ("?", "AccentBrush", "Confirmation"),
            _ => ("i", "AccentBrush", "Information")
        };
        SeverityIcon.Text = symbol;
        SeverityIcon.SetResourceReference(TextBlock.ForegroundProperty, brush);
        AutomationProperties.SetName(SeverityIcon, labelText);
        if (image == MessageBoxImage.None) IconBadge.Visibility = Visibility.Collapsed;
        Loaded += (_, _) => ((Button)Buttons.Children[0]).Focus();
    }

    public static MessageBoxResult Show(Window? owner, string message, string title,
        MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Information)
    {
        var dialog = new MessageDialog(message, title, buttons, image);
        if (owner?.IsVisible == true) dialog.Owner = owner;
        else dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        dialog.ShowDialog();
        return dialog.Result;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        WindowTheme.Apply(this);
    }
}
