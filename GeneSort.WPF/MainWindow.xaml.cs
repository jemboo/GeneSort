using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using MessagePack;

namespace GeneSort.WPF;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private MessagePackFile? selectedFile;
    private string fileSummary = string.Empty;
    private string statusMessage = "Choose a folder or open a MessagePack file.";
    private string selectionTitle = "No selection";
    private string selectionType = string.Empty;
    private string selectionPath = string.Empty;
    private string selectionSize = string.Empty;
    private UserControl? currentViewer;
    private UserControl? detailsViewer;
    private long folderLoadVersion;

    public ObservableCollection<FileSystemTreeNode> RootNodes { get; } = [];

    public MessagePackFile? SelectedFile
    {
        get => selectedFile;
        set { selectedFile = value; OnPropertyChanged(); }
    }

    public string FileSummary
    {
        get => fileSummary;
        set { fileSummary = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => statusMessage;
        set { statusMessage = value; OnPropertyChanged(); }
    }

    public string SelectionTitle
    {
        get => selectionTitle;
        set { selectionTitle = value; OnPropertyChanged(); }
    }

    public string SelectionType
    {
        get => selectionType;
        set { selectionType = value; OnPropertyChanged(); }
    }

    public string SelectionPath
    {
        get => selectionPath;
        set { selectionPath = value; OnPropertyChanged(); }
    }

    public string SelectionSize
    {
        get => selectionSize;
        set { selectionSize = value; OnPropertyChanged(); }
    }

    public UserControl? CurrentViewer
    {
        get => currentViewer;
        set { currentViewer = value; OnPropertyChanged(); }
    }

    public UserControl? DetailsViewer
    {
        get => detailsViewer;
        set { detailsViewer = value; OnPropertyChanged(); }
    }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        DetailsViewer = new FileSelectionDetailsControl();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "Choose the root folder to browse for GeneSort output files",
            UseDescriptionForTitle = true,
            SelectedPath = Directory.Exists(FolderPathBox.Text) ? FolderPathBox.Text : string.Empty
        };
        if (dialog.ShowDialog(this) == true)
            LoadFolder(dialog.SelectedPath);
    }

    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Open a GeneSort output file",
            Filter = "GeneSort output files (*.msgpack;*.txt)|*.msgpack;*.txt|MessagePack files (*.msgpack)|*.msgpack|Text reports (*.txt)|*.txt|All files (*.*)|*.*",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) == true)
        {
            var file = new MessagePackFile(dialog.FileName);
            LoadFolder(file.DirectoryName);
            SelectedFile = file;
            _ = DisplayFileAsync(file);
        }
    }

    private void LoadFolder(string folder)
    {
        Interlocked.Increment(ref folderLoadVersion);
        var fullPath = Path.GetFullPath(folder);
        FolderPathBox.Text = folder;
        RootNodes.Clear();
        CurrentViewer = null;
        FileSummary = string.Empty;
        SelectedFile = null;
        SetFolderDetails(fullPath);
        UpButton.IsEnabled = Directory.GetParent(fullPath) is not null;
        RootNodes.Add(FileSystemTreeNode.CreateDirectory(fullPath, isRoot: true));
        StatusMessage = "Folder root set. Expand folders to browse output files.";
    }

    private void MoveRootUp_Click(object sender, RoutedEventArgs e)
    {
        var parent = Directory.GetParent(FolderPathBox.Text);
        if (parent is not null)
            LoadFolder(parent.FullName);
    }

    private async void FileTreeItem_Expanded(object sender, RoutedEventArgs e)
    {
        if (sender is not TreeViewItem { DataContext: FileSystemTreeNode node } || node.IsFile || node.IsLoaded)
            return;

        node.IsLoaded = true;
        try
        {
            var children = await Task.Run(() => FileSystemTreeNode.EnumerateChildren(node.FullPath));
            node.Children.Clear();
            foreach (var child in children)
                node.Children.Add(child);
        }
        catch (Exception ex)
        {
            node.Children.Clear();
            StatusMessage = $"Unable to read folder {node.FullPath}: {ex.Message}";
        }
    }

    private void FileTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is FileSystemTreeNode { IsFile: true } node)
            _ = DisplayFileAsync(new MessagePackFile(node.FullPath));
        else if (e.NewValue is FileSystemTreeNode { IsFile: false } folder)
        {
            SetFolderDetails(folder.FullPath);
            _ = DisplayRunParametersFolderAsync(folder.FullPath);
        }
    }

    private void SetFolderDetails(string path)
    {
        SelectionTitle = Path.GetFileName(path);
        if (string.IsNullOrEmpty(SelectionTitle))
            SelectionTitle = path;
        SelectionType = "Folder";
        SelectionPath = path;
        SelectionSize = string.Empty;
        DetailsViewer = new FileSelectionDetailsControl();
    }

    private void CenterOutputNode_Selected(MessagePackNode node)
    {
        DetailsViewer = SelectedFile?.Kind == OutputDataKind.SorterPoolSet && node.Children.Count > 0
            ? new MessagePackNodeChildrenGridControl(node)
            : new FileSelectionDetailsControl();
    }

    private async Task DisplayRunParametersFolderAsync(string folderPath)
    {
        var version = Interlocked.Increment(ref folderLoadVersion);
        SelectedFile = null;
        FileSummary = folderPath;
        CurrentViewer = null;
        DetailsViewer = new FileSelectionDetailsControl();
        StatusMessage = $"Reading files in {folderPath}…";

        try
        {
            var result = await Task.Run(() => BuildFolderContents(folderPath));
            if (version != folderLoadVersion)
                return;

            CurrentViewer = new OutputFolderFilesGridControl(Path.GetFileName(folderPath), result.FilesTable);
            FileSummary = $"{folderPath}  •  {result.FileCount:N0} output file(s)";
            if (result.RunParameters.FilesLoaded > 0)
            {
                DetailsViewer = new RunParametersTableControl(Path.GetFileName(folderPath), result.RunParameters.Table);
                StatusMessage = result.RunParameters.FailedFiles == 0
                    ? $"Showing {result.FileCount:N0} folder file(s) and parameters from {result.RunParameters.FilesLoaded:N0} RunParameters file(s)."
                    : $"Showing folder files and parameters from {result.RunParameters.FilesLoaded:N0} of {result.RunParameters.FilesFound:N0} RunParameters files.";
            }
            else if (result.RunParameters.FilesFound > 0)
                StatusMessage = $"Showing folder files; found {result.RunParameters.FilesFound:N0} RunParameters file(s), but none could be read.";
            else
                StatusMessage = $"Showing {result.FileCount:N0} output file(s) in this folder. No RunParameters files found beneath it.";
        }
        catch (Exception ex)
        {
            if (version == folderLoadVersion)
                StatusMessage = $"Unable to search this folder: {ex.Message}";
        }
    }

    private static FolderOutputContentsResult BuildFolderContents(string folderPath)
    {
        var table = new DataTable();
        table.Columns.Add("File", typeof(string));
        table.Columns.Add("Output data type", typeof(string));
        table.Columns.Add("Size (bytes)", typeof(long));
        table.Columns.Add("Last modified", typeof(DateTime));
        table.Columns.Add("Path", typeof(string));

        var outputFiles = Directory.EnumerateFiles(folderPath)
            .Where(path => Path.GetExtension(path).Equals(".msgpack", StringComparison.OrdinalIgnoreCase)
                || Path.GetExtension(path).Equals(".txt", StringComparison.OrdinalIgnoreCase)
                    && MessagePackFile.GetKind(path) == OutputDataKind.TextReport)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var path in outputFiles)
        {
            var info = new FileInfo(path);
            var row = table.NewRow();
            row["File"] = info.Name;
            row["Output data type"] = MessagePackFile.OutputDataTypeForPath(path);
            row["Size (bytes)"] = info.Length;
            row["Last modified"] = info.LastWriteTime;
            row["Path"] = path;
            table.Rows.Add(row);
        }

        return new FolderOutputContentsResult(table, outputFiles.Length, BuildRunParametersTable(folderPath));
    }

    private static RunParametersTableResult BuildRunParametersTable(string folderPath)
    {
        var files = EnumerateRunParametersFiles(folderPath).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        var rows = new List<(string Path, Dictionary<string, string> Values)>();
        var failedFiles = 0;
        foreach (var path in files)
        {
            try
            {
                var bytes = File.ReadAllBytes(path);
                var json = MessagePackSerializer.ConvertToJson(new ReadOnlyMemory<byte>(bytes));
                using var document = JsonDocument.Parse(json);
                rows.Add((Path.GetRelativePath(folderPath, path), ParseRunParameterMap(document.RootElement)));
            }
            catch
            {
                failedFiles++;
            }
        }

        var table = new DataTable { CaseSensitive = true };
        var fileColumnName = "File path";
        while (rows.Any(row => row.Values.ContainsKey(fileColumnName)))
            fileColumnName += " (source)";
        table.Columns.Add(fileColumnName, typeof(string));

        var keys = rows.SelectMany(row => row.Values.Keys)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
            .ThenBy(key => key, StringComparer.Ordinal)
            .ToArray();
        foreach (var key in keys)
            table.Columns.Add(key, typeof(string));

        foreach (var (relativePath, values) in rows)
        {
            var row = table.NewRow();
            row[fileColumnName] = relativePath;
            foreach (var (key, value) in values)
                row[key] = value;
            table.Rows.Add(row);
        }

        return new RunParametersTableResult(table, files.Length, rows.Count, failedFiles);
    }

    private static IEnumerable<string> EnumerateRunParametersFiles(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            var directory = pending.Pop();
            IEnumerable<string> files;
            try { files = Directory.EnumerateFiles(directory, "*.msgpack").ToArray(); }
            catch (UnauthorizedAccessException) { files = []; }
            catch (IOException) { files = []; }
            foreach (var path in files)
                if (MessagePackFile.GetKind(path) == OutputDataKind.RunParameters)
                    yield return path;

            IEnumerable<string> directories;
            try { directories = Directory.EnumerateDirectories(directory).ToArray(); }
            catch (UnauthorizedAccessException) { directories = []; }
            catch (IOException) { directories = []; }
            foreach (var child in directories)
            {
                try
                {
                    if ((File.GetAttributes(child) & FileAttributes.ReparsePoint) == 0)
                        pending.Push(child);
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }
        }
    }

    private static Dictionary<string, string> ParseRunParameterMap(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in root.EnumerateObject())
                if (property.Name.Equals("paramMap", StringComparison.OrdinalIgnoreCase))
                    return ParseStringMap(property.Value);
        }

        if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() == 1)
            return ParseStringMap(root[0]);
        return ParseStringMap(root);
    }

    private static Dictionary<string, string> ParseStringMap(JsonElement element)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
                result[property.Name] = JsonScalarToString(property.Value);
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var pair in element.EnumerateArray())
            {
                if (pair.ValueKind == JsonValueKind.Array && pair.GetArrayLength() >= 2)
                {
                    var key = JsonScalarToString(pair[0]);
                    result[key] = JsonScalarToString(pair[1]);
                }
                else if (pair.ValueKind == JsonValueKind.Object)
                {
                    var key = pair.EnumerateObject().FirstOrDefault(property => property.Name.Equals("Key", StringComparison.OrdinalIgnoreCase));
                    var value = pair.EnumerateObject().FirstOrDefault(property => property.Name.Equals("Value", StringComparison.OrdinalIgnoreCase));
                    if (key.Name is not null)
                        result[JsonScalarToString(key.Value)] = value.Name is null ? string.Empty : JsonScalarToString(value.Value);
                }
            }
        }
        return result;
    }

    private static string JsonScalarToString(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
        _ => value.GetRawText()
    };

    private async Task DisplayFileAsync(MessagePackFile file)
    {
        var version = Interlocked.Increment(ref folderLoadVersion);
        StatusMessage = $"Reading {file.Name}…";
        CurrentViewer = null;
        SelectedFile = file;
        SelectionTitle = file.Name;
        SelectionType = file.OutputDataType;
        SelectionPath = file.FullPath;
        SelectionSize = "Reading…";
        DetailsViewer = new FileSelectionDetailsControl();
        try
        {
            if (file.Kind == OutputDataKind.TextReport)
            {
                var text = await File.ReadAllTextAsync(file.FullPath);
                if (version != folderLoadVersion)
                    return;
                CurrentViewer = new TextReportOutputDataControl(text, file.OutputDataType);
                var info = new FileInfo(file.FullPath);
                FileSummary = $"{file.FullPath}  •  {info.Length:N0} bytes";
                SelectionSize = $"{info.Length:N0} bytes";
            }
            else
            {
                var result = await Task.Run(() => ReadMessagePack(file.FullPath));
                if (version != folderLoadVersion)
                    return;
                var viewer = OutputDataViewerFactory.Create(file.Kind, result.Nodes);
                viewer.NodeSelected += CenterOutputNode_Selected;
                CurrentViewer = viewer;
                FileSummary = $"{file.FullPath}  •  {result.Size:N0} bytes  •  {result.NodeCount:N0} values";
                SelectionSize = $"{result.Size:N0} bytes";
            }
            StatusMessage = "File opened successfully.";
        }
        catch (Exception ex)
        {
            if (version != folderLoadVersion)
                return;
            FileSummary = file.FullPath;
            SelectionSize = "Unable to read";
            StatusMessage = $"Unable to read this output file: {ex.Message}";
        }
    }

    private static (List<MessagePackNode> Nodes, long Size, int NodeCount) ReadMessagePack(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var json = MessagePackSerializer.ConvertToJson(new ReadOnlyMemory<byte>(bytes));
        using var document = JsonDocument.Parse(json);
        var nodes = new List<MessagePackNode> { MessagePackNode.FromJson("root", document.RootElement) };
        return (nodes, bytes.LongLength, nodes[0].Count());
    }
}

internal sealed record RunParametersTableResult(DataTable Table, int FilesFound, int FilesLoaded, int FailedFiles);
internal sealed record FolderOutputContentsResult(DataTable FilesTable, int FileCount, RunParametersTableResult RunParameters);

public sealed class MessagePackFile(string fullPath)
{
    public string FullPath { get; } = Path.GetFullPath(fullPath);
    public string Name => Path.GetFileName(FullPath);
    public string DirectoryName => Path.GetDirectoryName(FullPath) ?? string.Empty;
    public string OutputDataType { get; } = OutputDataTypeFromPath(Path.GetFullPath(fullPath));
    public OutputDataKind Kind { get; } = GetKind(Path.GetFullPath(fullPath));

    public static OutputDataKind GetKind(string path)
    {
        var display = OutputDataTypeFromPath(path);
        var kinds = new (string Name, OutputDataKind Kind)[]
        {
            ("SorterPoolSetSummarySet", OutputDataKind.SorterPoolSetSummarySet),
            ("SorterPoolSetHistory", OutputDataKind.SorterPoolSetHistory),
            ("SorterPoolBinsSetSeries", OutputDataKind.SorterPoolBinsSetSeries),
            ("RunParameters", OutputDataKind.RunParameters),
            ("SorterPoolSet", OutputDataKind.SorterPoolSet),
            ("SortableTest", OutputDataKind.SortableTest),
            ("SorterSetEval", OutputDataKind.SorterSetEval),
            ("SorterSet", OutputDataKind.SorterSet),
            ("TextReport", OutputDataKind.TextReport)
        };

        if (display == "Run" || display.StartsWith("Run (", StringComparison.OrdinalIgnoreCase))
            return OutputDataKind.Run;
        foreach (var (name, kind) in kinds)
            if (display.Equals(name, StringComparison.OrdinalIgnoreCase)
                || display.StartsWith(name + " (", StringComparison.OrdinalIgnoreCase))
                return kind;
        return OutputDataKind.Unknown;
    }

    public static string OutputDataTypeForPath(string path) => OutputDataTypeFromPath(path);

    private static string OutputDataTypeFromPath(string path)
    {
        var fileDirectory = Path.GetDirectoryName(path);
        var fileFolderName = Path.GetFileName(fileDirectory) ?? string.Empty;
        var typeFolderName = Path.GetFileName(Path.GetDirectoryName(fileDirectory)) ?? string.Empty;

        // Run files are stored under Run/<runName>/Run_<runName>.msgpack.
        if (string.Equals(typeFolderName, "Run", StringComparison.OrdinalIgnoreCase))
            return $"Run ({fileFolderName})";

        // Run parameters add a Run/ parent above the outputDataType folder.
        if (string.Equals(Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(fileDirectory))), "Run", StringComparison.OrdinalIgnoreCase)
            && TryFormatType(typeFolderName, "RunParameters", "RunParameters", out var runParametersType))
            return runParametersType;

        // Text reports use the nested folder returned by toFolderName: Report\\TextReport_<name>.
        if (string.Equals(Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(fileDirectory))), "Report", StringComparison.OrdinalIgnoreCase)
            && TryFormatType(typeFolderName, "TextReport", "TextReport", out var reportType))
            return reportType;

        return TryFormatKnownType(typeFolderName, out var outputDataType)
            ? outputDataType
            : "Unknown (folder does not match a known outputDataType)";
    }

    private static bool TryFormatKnownType(string folderName, out string outputDataType)
    {
        var types = new (string FolderPrefix, string CaseName)[]
        {
            ("RunParameters", "RunParameters"),
            ("SorterPoolSetSummarySet", "SorterPoolSetSummarySet"),
            ("SorterPoolSetHistoryCollection", "SorterPoolSetHistory"),
            ("SorterPoolBinsSetSeries", "SorterPoolBinsSetSeries"),
            ("SorterPoolSet", "SorterPoolSet"),
            ("SortableTest", "SortableTest"),
            ("SorterSetEval", "SorterSetEval"),
            ("SorterSet", "SorterSet")
        };

        foreach (var (prefix, caseName) in types)
            if (TryFormatType(folderName, prefix, caseName, out outputDataType))
                return true;

        outputDataType = string.Empty;
        return false;
    }

    private static bool TryFormatType(string folderName, string folderPrefix, string caseName, out string outputDataType)
    {
        if (string.Equals(folderName, folderPrefix, StringComparison.OrdinalIgnoreCase))
        {
            outputDataType = caseName;
            return true;
        }

        var parameterPrefix = folderPrefix + "_";
        if (folderName.StartsWith(parameterPrefix, StringComparison.OrdinalIgnoreCase))
        {
            outputDataType = $"{caseName} ({folderName[parameterPrefix.Length..]})";
            return true;
        }

        outputDataType = string.Empty;
        return false;
    }
}

public sealed class MessagePackNode
{
    public string Label { get; }
    public string Value { get; }
    public ObservableCollection<MessagePackNode> Children { get; } = [];

    private MessagePackNode(string label, string value)
    {
        Label = label;
        Value = value;
    }

    public int Count() => 1 + Children.Sum(child => child.Count());

    public static MessagePackNode FromJson(string label, JsonElement element)
    {
        var node = new MessagePackNode(label, element.ValueKind switch
        {
            JsonValueKind.Object => "{…}",
            JsonValueKind.Array => $"[{element.GetArrayLength()}]",
            JsonValueKind.String => element.GetString() ?? "null",
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => "null"
        });

        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
                node.Children.Add(FromJson(property.Name, property.Value));
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in element.EnumerateArray())
                node.Children.Add(FromJson($"[{index++}]", item));
        }
        return node;
    }
}

