using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GeneSort.WPF.ViewModels;

public sealed class MessagePackBrowserViewModel : INotifyPropertyChanged
{
    private MessagePackFile? selectedFile;
    private string folderPath = string.Empty;
    private string fileSummary = string.Empty;
    private string statusMessage = "Choose a folder or open a MessagePack file.";
    private string selectionTitle = "No selection";
    private string selectionType = string.Empty;
    private string selectionPath = string.Empty;
    private string selectionSize = string.Empty;

    public ObservableCollection<FileSystemTreeNode> RootNodes { get; } = [];
    public MessagePackFile? SelectedFile { get => selectedFile; set => Set(ref selectedFile, value); }
    public string FolderPath { get => folderPath; set => Set(ref folderPath, value); }
    public string FileSummary { get => fileSummary; set => Set(ref fileSummary, value); }
    public string StatusMessage { get => statusMessage; set => Set(ref statusMessage, value); }
    public string SelectionTitle { get => selectionTitle; set => Set(ref selectionTitle, value); }
    public string SelectionType { get => selectionType; set => Set(ref selectionType, value); }
    public string SelectionPath { get => selectionPath; set => Set(ref selectionPath, value); }
    public string SelectionSize { get => selectionSize; set => Set(ref selectionSize, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
