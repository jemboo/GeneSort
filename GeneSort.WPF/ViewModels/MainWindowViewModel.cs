namespace GeneSort.WPF.ViewModels;

public sealed class MainWindowViewModel
{
    public MessagePackBrowserViewModel MessagePackBrowser { get; } = new();
    public SandboxViewModel Sandbox { get; } = new();
}
