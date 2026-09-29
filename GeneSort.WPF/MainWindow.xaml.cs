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
using System.Windows.Threading;
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
    private OutputFolderFilesGridControl? activeFolderGrid;
    private CancellationTokenSource? childLoadCancellation;
    private DataTable? runParametersTable;
    private readonly HashSet<string> loadedRunParametersPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> runParameterColumns = new(StringComparer.Ordinal);
    private string activeFolderPath = string.Empty;
    private IReadOnlyList<MessagePackNodePathSegment>? selectedNodePath;
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
        childLoadCancellation?.Cancel();
        activeFolderGrid = null;
        runParametersTable = null;
        loadedRunParametersPaths.Clear();
        runParameterColumns.Clear();
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

    private void CenterOutputNode_Selected(MessagePackNode node, IReadOnlyList<MessagePackNodePathSegment> path)
    {
        selectedNodePath = path.ToArray();
        DetailsViewer = SelectedFile?.Kind == OutputDataKind.SorterPoolSet && node.Children.Count > 0
            ? new MessagePackNodeChildrenGridControl(node)
            : new FileSelectionDetailsControl();
    }

    private async Task DisplayRunParametersFolderAsync(string folderPath)
    {
        childLoadCancellation?.Cancel();
        var version = Interlocked.Increment(ref folderLoadVersion);
        activeFolderGrid = null;
        runParametersTable = null;
        loadedRunParametersPaths.Clear();
        runParameterColumns.Clear();
        activeFolderPath = folderPath;
        SelectedFile = null;
        FileSummary = folderPath;
        CurrentViewer = null;
        DetailsViewer = new FileSelectionDetailsControl();
        StatusMessage = $"Loading file details in {folderPath}…";

        try
        {
            var result = await Task.Run(() => BuildFolderContents(folderPath));
            if (version != folderLoadVersion)
                return;

            var grid = new OutputFolderFilesGridControl(Path.GetFileName(folderPath), result.FilesTable);
            grid.FileSelected += path =>
            {
                var file = new MessagePackFile(path);
                SelectedFile = file;
                SelectionTitle = file.Name;
                SelectionType = file.OutputDataType;
                SelectionPath = file.FullPath;
                SelectionSize = new FileInfo(path).Length.ToString("N0") + " bytes";
                if (runParametersTable is null)
                    DetailsViewer = new FileSelectionDetailsControl();
                _ = LoadRootPropertiesForFileAsync(grid, path, version);
            };
            grid.StartRequested += () => _ = StartLazyLoadingAsync(grid, version);
            grid.StopRequested += StopLazyLoading;
            activeFolderGrid = grid;
            CurrentViewer = grid;
            FileSummary = $"{folderPath}  •  {result.FileCount:N0} output file(s)";
            if (grid.GetAllFilePaths().Any(path => MessagePackFile.GetKind(path) == OutputDataKind.RunParameters))
            {
                runParametersTable = CreateEmptyRunParametersTable();
                DetailsViewer = new RunParametersTableControl(Path.GetFileName(folderPath), runParametersTable);
            }
            else
                StatusMessage = $"Loaded {result.FileCount:N0} file-level records. Select a row or start loading root fields.";

            var firstFile = grid.GetAllFilePaths().FirstOrDefault(path => Path.GetExtension(path).Equals(".msgpack", StringComparison.OrdinalIgnoreCase));
            if (firstFile is not null)
                await LoadRootPropertiesForFileAsync(grid, firstFile, version);
            else
                grid.SetProgressMessage($"0 of {result.FileCount:N0} files loaded");
        }
        catch (Exception ex)
        {
            if (version == folderLoadVersion)
                StatusMessage = $"Unable to search this folder: {ex.Message}";
        }
    }

    private static FolderOutputContentsResult BuildFolderContents(string folderPath)
    {
        var table = new DataTable { CaseSensitive = true };
        table.Columns.Add("File", typeof(string));
        table.Columns.Add("Output data type", typeof(string));
        table.Columns.Add("Size (bytes)", typeof(long));
        table.Columns.Add("Last modified", typeof(DateTime));
        table.Columns.Add("Path", typeof(string));

        var outputFiles = EnumerateOutputFiles(folderPath)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var path in outputFiles)
        {
            var info = new FileInfo(path);
            var row = table.NewRow();
            row["File"] = info.Name;
            row["Output data type"] = MessagePackFile.OutputDataTypeForPath(info.FullName);
            row["Size (bytes)"] = info.Length;
            row["Last modified"] = info.LastWriteTime;
            row["Path"] = info.FullName;
            table.Rows.Add(row);
        }

        return new FolderOutputContentsResult(table, outputFiles.Length);
    }

    private static IEnumerable<string> EnumerateOutputFiles(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            var folder = pending.Pop();
            IEnumerable<string> files;
            try { files = Directory.EnumerateFiles(folder).ToArray(); }
            catch (UnauthorizedAccessException) { files = []; }
            catch (IOException) { files = []; }
            foreach (var path in files)
            {
                var extension = Path.GetExtension(path);
                if (extension.Equals(".msgpack", StringComparison.OrdinalIgnoreCase)
                    || extension.Equals(".txt", StringComparison.OrdinalIgnoreCase)
                        && MessagePackFile.GetKind(path) == OutputDataKind.TextReport)
                    yield return path;
            }

            IEnumerable<string> directories;
            try { directories = Directory.EnumerateDirectories(folder).ToArray(); }
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

    private static MessagePackFileProperties ReadRootNodeChildren(string path)
    {
        if (!Path.GetExtension(path).Equals(".msgpack", StringComparison.OrdinalIgnoreCase))
            return new MessagePackFileProperties(new Dictionary<string, string>(StringComparer.Ordinal), new Dictionary<string, string>(StringComparer.Ordinal));

        var bytes = File.ReadAllBytes(path);
        var json = MessagePackSerializer.ConvertToJson(new ReadOnlyMemory<byte>(bytes));
        using var document = JsonDocument.Parse(json);
        var root = MessagePackNode.FromJson("root", document.RootElement);
        var rootValues = root.Children.ToDictionary(child => child.Label, child => child.Value, StringComparer.Ordinal);
        var paramMap = root.Children.FirstOrDefault(child => child.Label.Equals("paramMap", StringComparison.OrdinalIgnoreCase));
        var runParameters = paramMap?.Children.ToDictionary(child => child.Label, child => child.Value, StringComparer.Ordinal)
            ?? new Dictionary<string, string>(StringComparer.Ordinal);
        return new MessagePackFileProperties(rootValues, runParameters);
    }

    private async Task LoadRootPropertiesForFileAsync(OutputFolderFilesGridControl grid, string path, long version, CancellationToken token = default)
    {
        if (!grid.TryBeginLoading(path))
            return;

        try
        {
            var properties = await Task.Run(() => ReadRootNodeChildren(path), token);
            token.ThrowIfCancellationRequested();
            if (version != folderLoadVersion || activeFolderGrid != grid)
            {
                grid.FailLoading(path);
                return;
            }

            grid.CompleteLoading(path, properties.RootValues);
            AddRunParametersRow(path, properties.RunParameters);
            grid.SetProgressMessage($"{grid.LoadedFileCount:N0} of {grid.GetAllFilePaths().Count:N0} files loaded");
            StatusMessage = $"Loaded root fields from {Path.GetFileName(path)}.";
        }
        catch (OperationCanceledException)
        {
            grid.FailLoading(path);
        }
        catch (Exception ex)
        {
            grid.FailLoading(path);
            if (version == folderLoadVersion && activeFolderGrid == grid)
                StatusMessage = $"Unable to read {Path.GetFileName(path)}: {ex.Message}";
        }
    }

    private async Task StartLazyLoadingAsync(OutputFolderFilesGridControl grid, long version)
    {
        childLoadCancellation?.Cancel();
        var cancellation = new CancellationTokenSource();
        childLoadCancellation = cancellation;
        var token = cancellation.Token;
        var paths = grid.GetUnloadedFilePaths();
        grid.SetLoadingState(true);
        grid.SetProgressMessage($"Loading remaining files (0 of {paths.Count:N0})…");
        var processed = 0;
        try
        {
            foreach (var path in paths)
            {
                token.ThrowIfCancellationRequested();
                await LoadRootPropertiesForFileAsync(grid, path, version, token);
                processed++;
                grid.SetProgressMessage($"Loading remaining files ({processed:N0} of {paths.Count:N0})…");
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            grid.SetLoadingState(false);
            grid.SetProgressMessage($"{grid.LoadedFileCount:N0} of {grid.GetAllFilePaths().Count:N0} files loaded");
            if (ReferenceEquals(childLoadCancellation, cancellation))
                childLoadCancellation = null;
            cancellation.Dispose();
        }
    }

    private void StopLazyLoading() => childLoadCancellation?.Cancel();

    private static DataTable CreateEmptyRunParametersTable()
    {
        var table = new DataTable { CaseSensitive = true };
        table.Columns.Add("File path", typeof(string));
        return table;
    }

    private void AddRunParametersRow(string path, IReadOnlyDictionary<string, string> parameters)
    {
        if (runParametersTable is null
            || MessagePackFile.GetKind(path) != OutputDataKind.RunParameters
            || !loadedRunParametersPaths.Add(path))
            return;

        var columnNames = new HashSet<string>(runParametersTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName), StringComparer.OrdinalIgnoreCase);
        foreach (var key in parameters.Keys)
        {
            if (runParameterColumns.ContainsKey(key))
                continue;
            var columnName = key;
            var suffix = 2;
            while (!columnNames.Add(columnName))
                columnName = $"{key} (value {suffix++})";
            runParametersTable.Columns.Add(columnName, typeof(string));
            runParameterColumns[key] = columnName;
        }

        var row = runParametersTable.NewRow();
        row["File path"] = Path.GetRelativePath(activeFolderPath, path);
        foreach (var (key, value) in parameters)
            row[runParameterColumns[key]] = value;
        runParametersTable.Rows.Add(row);
        DetailsViewer = new RunParametersTableControl(Path.GetFileName(activeFolderPath), runParametersTable);
    }

    private async Task DisplayFileAsync(MessagePackFile file)
    {
        var version = Interlocked.Increment(ref folderLoadVersion);
        var nodePathToRestore = selectedNodePath?.ToArray();
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
                if (nodePathToRestore is { Length: > 0 })
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                    {
                        if (version == folderLoadVersion && ReferenceEquals(CurrentViewer, viewer))
                            viewer.SelectNodePath(nodePathToRestore);
                    }));
                }
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

internal sealed record FolderOutputContentsResult(DataTable FilesTable, int FileCount);
internal sealed record MessagePackFileProperties(Dictionary<string, string> RootValues, Dictionary<string, string> RunParameters);

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

