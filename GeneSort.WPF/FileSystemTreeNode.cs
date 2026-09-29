using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace GeneSort.WPF;

public sealed class FileSystemTreeNode : INotifyPropertyChanged
{
    private bool isExpanded;

    private FileSystemTreeNode(string fullPath, bool isFile, bool isPlaceholder = false)
    {
        FullPath = fullPath;
        IsFile = isFile;
        IsPlaceholder = isPlaceholder;
        DisplayName = isPlaceholder ? "Loading…" : Path.GetFileName(fullPath);
        if (string.IsNullOrEmpty(DisplayName) && !isPlaceholder)
            DisplayName = fullPath;
        OutputDataType = isFile ? new MessagePackFile(fullPath).OutputDataType : string.Empty;
    }

    public string FullPath { get; }
    public string DisplayName { get; }
    public string OutputDataType { get; }
    public bool IsFile { get; }
    public bool IsPlaceholder { get; }
    public bool IsLoaded { get; set; }
    public ObservableCollection<FileSystemTreeNode> Children { get; } = [];

    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (isExpanded == value)
                return;
            isExpanded = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public static FileSystemTreeNode CreateDirectory(string path, bool isRoot = false)
    {
        var node = new FileSystemTreeNode(path, isFile: false) { IsExpanded = isRoot };
        node.Children.Add(new FileSystemTreeNode(string.Empty, isFile: false, isPlaceholder: true));
        return node;
    }

    public static FileSystemTreeNode CreateFile(string path) => new(path, isFile: true) { IsLoaded = true };

    public static IReadOnlyList<FileSystemTreeNode> EnumerateChildren(string directoryPath)
    {
        var children = new List<FileSystemTreeNode>();

        foreach (var path in Directory.EnumerateDirectories(directoryPath))
        {
            try
            {
                if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) == 0)
                    children.Add(CreateDirectory(path));
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        foreach (var path in Directory.EnumerateFiles(directoryPath))
        {
            var extension = Path.GetExtension(path);
            if (extension.Equals(".msgpack", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".txt", StringComparison.OrdinalIgnoreCase)
                    && MessagePackFile.GetKind(path) == OutputDataKind.TextReport)
                children.Add(CreateFile(path));
        }

        return children
            .OrderBy(node => node.IsFile)
            .ThenBy(node => node.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
