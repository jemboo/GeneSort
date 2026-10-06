using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Data;
using System.IO;
using GeneSort.WPF.Views;
using GeneSort.WPF.ViewModels;

namespace GeneSort.WPF.Controls;

/// <summary>Shared structure for the output-specific viewers. Each concrete control
/// gives the serialized output a type-specific heading and context.</summary>
public abstract class OutputDataViewerControl : UserControl
{
    private readonly MessagePackNode[] rootNodes;
    private readonly TreeView tree;

    public event Action<MessagePackNode, IReadOnlyList<MessagePackNodePathSegment>>? NodeSelected;

    protected OutputDataViewerControl(string heading, string description, IEnumerable<MessagePackNode> nodes)
    {
        rootNodes = nodes.ToArray();
        var layout = new DockPanel();
        var intro = new StackPanel { Margin = new Thickness(2, 0, 2, 10) };
        intro.Children.Add(new TextBlock
        {
            Text = heading,
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(23, 43, 67))
        });
        intro.Children.Add(new TextBlock
        {
            Text = description,
            Margin = new Thickness(0, 4, 0, 0),
            Foreground = new SolidColorBrush(Color.FromRgb(89, 102, 117)),
            TextWrapping = TextWrapping.Wrap
        });
        DockPanel.SetDock(intro, Dock.Top);
        layout.Children.Add(intro);

        tree = new TreeView { ItemsSource = rootNodes, BorderThickness = new Thickness(0) };
        tree.Resources.Add(new DataTemplateKey(typeof(MessagePackNode)), CreateNodeTemplate());
        tree.SelectedItemChanged += (_, args) =>
        {
            if (args.NewValue is MessagePackNode selectedNode)
                NodeSelected?.Invoke(selectedNode, GetPath(selectedNode));
        };
        layout.Children.Add(tree);
        Content = layout;
    }

    public bool SelectNodePath(IReadOnlyList<MessagePackNodePathSegment> path)
    {
        if (path.Count == 0)
            return false;

        tree.UpdateLayout();
        ItemsControl owner = tree;
        IReadOnlyList<MessagePackNode> currentNodes = rootNodes;
        TreeViewItem? container = null;
        for (var depth = 0; depth < path.Count; depth++)
        {
            var segment = path[depth];
            var index = FindMatchingIndex(currentNodes, segment);
            if (index < 0)
                return false;

            container = owner.ItemContainerGenerator.ContainerFromIndex(index) as TreeViewItem;
            if (container is null)
                return false;
            if (depth < path.Count - 1)
            {
                container.IsExpanded = true;
                container.UpdateLayout();
                owner = container;
                currentNodes = ((MessagePackNode)container.DataContext).Children.ToArray();
            }
        }

        if (container is null)
            return false;
        container.IsSelected = true;
        container.BringIntoView();
        return true;
    }

    private IReadOnlyList<MessagePackNodePathSegment> GetPath(MessagePackNode target)
    {
        var nodes = new List<MessagePackNode>();
        foreach (var root in rootNodes)
            if (FindPath(root, target, nodes))
                break;

        var path = new List<MessagePackNodePathSegment>(nodes.Count);
        for (var index = 0; index < nodes.Count; index++)
        {
            IEnumerable<MessagePackNode> siblings = index == 0 ? rootNodes : nodes[index - 1].Children;
            var occurrence = 0;
            foreach (var sibling in siblings)
            {
                if (ReferenceEquals(sibling, nodes[index]))
                    break;
                if (string.Equals(sibling.Label, nodes[index].Label, StringComparison.Ordinal))
                    occurrence++;
            }
            path.Add(new MessagePackNodePathSegment(nodes[index].Label, occurrence));
        }
        return path;
    }

    private static bool FindPath(MessagePackNode current, MessagePackNode target, List<MessagePackNode> path)
    {
        path.Add(current);
        if (ReferenceEquals(current, target))
            return true;
        foreach (var child in current.Children)
            if (FindPath(child, target, path))
                return true;
        path.RemoveAt(path.Count - 1);
        return false;
    }

    private static int FindMatchingIndex(IReadOnlyList<MessagePackNode> siblings, MessagePackNodePathSegment segment)
    {
        var occurrence = 0;
        for (var index = 0; index < siblings.Count; index++)
        {
            if (!string.Equals(siblings[index].Label, segment.Label, StringComparison.Ordinal))
                continue;
            if (occurrence == segment.Occurrence)
                return index;
            occurrence++;
        }
        return -1;
    }

    private static HierarchicalDataTemplate CreateNodeTemplate()
    {
        var row = new FrameworkElementFactory(typeof(StackPanel));
        row.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        row.SetValue(FrameworkElement.MarginProperty, new Thickness(2));

        var label = new FrameworkElementFactory(typeof(TextBlock));
        label.SetBinding(TextBlock.TextProperty, new Binding(nameof(MessagePackNode.Label)));
        label.SetValue(TextBlock.FontWeightProperty, FontWeights.SemiBold);
        label.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(36, 70, 107)));
        row.AppendChild(label);

        var value = new FrameworkElementFactory(typeof(TextBlock));
        value.SetBinding(TextBlock.TextProperty, new Binding(nameof(MessagePackNode.Value)) { StringFormat = "  {0}" });
        value.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(61, 71, 82)));
        row.AppendChild(value);

        return new HierarchicalDataTemplate(typeof(MessagePackNode))
        {
            ItemsSource = new Binding(nameof(MessagePackNode.Children)),
            VisualTree = row
        };
    }
}

public sealed class FileSelectionDetailsControl : UserControl
{
    public FileSelectionDetailsControl()
    {
        var details = new StackPanel();
        details.Children.Add(new TextBlock
        {
            Text = "Details",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(23, 43, 67)),
            Margin = new Thickness(0, 0, 0, 16)
        });
        AddBoundDetail(details, "Name", "SelectionTitle", FontWeights.SemiBold);
        AddBoundDetail(details, "Type", "SelectionType");
        AddBoundDetail(details, "Path", "SelectionPath");
        AddBoundDetail(details, "Size", "SelectionSize");
        Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = details
        };
    }

    private static void AddBoundDetail(Panel panel, string label, string bindingPath, FontWeight? valueWeight = null)
    {
        panel.Children.Add(new TextBlock
        {
            Text = label,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromRgb(104, 117, 132))
        });
        var value = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 3, 0, 12)
        };
        if (valueWeight is not null)
            value.FontWeight = valueWeight.Value;
        value.SetBinding(TextBlock.TextProperty, new Binding(bindingPath));
        panel.Children.Add(value);
    }
}

public sealed class MessagePackNodeChildrenGridControl : UserControl
{
    public MessagePackNodeChildrenGridControl(MessagePackNode node)
    {
        var layout = new DockPanel();
        var heading = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
        heading.Children.Add(new TextBlock
        {
            Text = node.Label,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(23, 43, 67)),
            TextWrapping = TextWrapping.Wrap
        });
        heading.Children.Add(new TextBlock
        {
            Text = $"{node.Children.Count:N0} child values",
            Margin = new Thickness(0, 3, 0, 0),
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromRgb(89, 102, 117))
        });
        DockPanel.SetDock(heading, Dock.Top);
        layout.Children.Add(heading);
        layout.Children.Add(new DataGrid
        {
            ItemsSource = CreateTable(node).DefaultView,
            AutoGenerateColumns = true,
            IsReadOnly = true,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserSortColumns = true,
            EnableRowVirtualization = true,
            EnableColumnVirtualization = true,
            HeadersVisibility = DataGridHeadersVisibility.All,
            GridLinesVisibility = DataGridGridLinesVisibility.All
        });
        Content = layout;
    }

    private static DataTable CreateTable(MessagePackNode node)
    {
        var children = node.Children.ToArray();
        var table = new DataTable { CaseSensitive = true };
        if (children.All(child => child.Children.Count > 0))
        {
            table.Columns.Add("Item", typeof(string));
            var fields = children.SelectMany(child => child.Children)
                .Select(field => field.Label)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var fieldColumns = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var field in fields)
            {
                var columnName = UniqueColumnName(table, field);
                table.Columns.Add(columnName, typeof(string));
                fieldColumns[field] = columnName;
            }

            foreach (var child in children)
            {
                var row = table.NewRow();
                row["Item"] = child.Label;
                foreach (var field in child.Children)
                    row[fieldColumns[field.Label]] = field.Value;
                table.Rows.Add(row);
            }
        }
        else
        {
            table.Columns.Add("Child", typeof(string));
            table.Columns.Add("Value", typeof(string));
            foreach (var child in children)
                table.Rows.Add(child.Label, child.Value);
        }
        return table;
    }

    private static string UniqueColumnName(DataTable table, string name)
    {
        var candidate = name;
        var suffix = 2;
        while (table.Columns.Contains(candidate))
            candidate = $"{name} ({suffix++})";
        return candidate;
    }
}

public sealed class RunOutputDataControl(IEnumerable<MessagePackNode> nodes)
    : OutputDataViewerControl("Run", "Run metadata: database, project, run name, and description.", nodes);

public sealed class RunParametersOutputDataControl(IEnumerable<MessagePackNode> nodes)
    : OutputDataViewerControl("Run parameters", "Parameter names and values used by this run.", nodes);

public sealed class SortableTestsOutputDataControl(IEnumerable<MessagePackNode> nodes)
    : OutputDataViewerControl("Sortable test", "Sortable test inputs and their associated data.", nodes);

public sealed class SorterPoolSetOutputDataControl(IEnumerable<MessagePackNode> nodes)
    : OutputDataViewerControl("Sorter pool set", "Generation, pool membership, and sorter evaluation data.", nodes);

public sealed class SorterPoolSetSummarySetOutputDataControl(IEnumerable<MessagePackNode> nodes)
    : OutputDataViewerControl("Sorter pool summary set", "Generation summaries and aggregate pool measurements.", nodes);

public sealed class SorterSetOutputDataControl(IEnumerable<MessagePackNode> nodes)
    : OutputDataViewerControl("Sorter set", "Sorter definitions and their shared configuration.", nodes);

public sealed class SorterSetEvalOutputDataControl(IEnumerable<MessagePackNode> nodes)
    : OutputDataViewerControl("Sorter set evaluation", "Evaluation results for the sorters in this set.", nodes);

public sealed class SorterPoolBinsSetSeriesOutputDataControl(IEnumerable<MessagePackNode> nodes)
    : OutputDataViewerControl("Sorter pool bins series", "Evaluation-bin snapshots collected across generations.", nodes);

public sealed class SorterPoolSetHistoryOutputDataControl(IEnumerable<MessagePackNode> nodes)
    : OutputDataViewerControl("Sorter pool history", "Pool member history, lineage, and saved evaluations.", nodes);

public sealed class TextReportOutputDataControl : UserControl
{
    public TextReportOutputDataControl(string text, string reportName)
    {
        var layout = new DockPanel();
        var heading = new StackPanel { Margin = new Thickness(2, 0, 2, 10) };
        heading.Children.Add(new TextBlock
        {
            Text = $"Text report: {reportName}",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(23, 43, 67))
        });
        heading.Children.Add(new TextBlock
        {
            Text = "Report output",
            Margin = new Thickness(0, 4, 0, 0),
            Foreground = new SolidColorBrush(Color.FromRgb(89, 102, 117))
        });
        DockPanel.SetDock(heading, Dock.Top);
        layout.Children.Add(heading);

        var reportTable = ParseTabSeparatedReport(text);
        layout.Children.Add(new DataGrid
        {
            ItemsSource = reportTable.DefaultView,
            AutoGenerateColumns = true,
            IsReadOnly = true,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserReorderColumns = true,
            CanUserSortColumns = true,
            EnableRowVirtualization = true,
            EnableColumnVirtualization = true,
            HeadersVisibility = DataGridHeadersVisibility.All,
            GridLinesVisibility = DataGridGridLinesVisibility.All
        });
        Content = layout;
    }

    private static DataTable ParseTabSeparatedReport(string text)
    {
        var lines = new List<string>();
        using (var reader = new StringReader(text.TrimStart('\uFEFF')))
        {
            string? line;
            while ((line = reader.ReadLine()) is not null)
                lines.Add(line);
        }
        while (lines.Count > 0 && string.IsNullOrEmpty(lines[^1]))
            lines.RemoveAt(lines.Count - 1);

        var table = new DataTable { CaseSensitive = true };
        if (lines.Count == 0)
        {
            table.Columns.Add("Report", typeof(string));
            return table;
        }

        var headers = lines[0].Split('\t', StringSplitOptions.None);
        var rows = lines.Skip(1).Select(line => line.Split('\t', StringSplitOptions.None)).ToArray();
        var columnCount = Math.Max(headers.Length, rows.Select(row => row.Length).DefaultIfEmpty(0).Max());
        for (var index = 0; index < columnCount; index++)
        {
            var baseName = index < headers.Length && !string.IsNullOrWhiteSpace(headers[index])
                ? headers[index]
                : $"Column {index + 1}";
            var columnName = baseName;
            var duplicate = 2;
            while (table.Columns.Contains(columnName))
                columnName = $"{baseName} ({duplicate++})";
            table.Columns.Add(columnName, typeof(string));
        }

        foreach (var values in rows)
        {
            var row = table.NewRow();
            for (var index = 0; index < Math.Min(values.Length, table.Columns.Count); index++)
                row[index] = values[index];
            table.Rows.Add(row);
        }
        return table;
    }
}

public sealed class RunParametersTableControl : UserControl
{
    public RunParametersTableControl(string folderName, DataTable table)
    {
        var layout = new DockPanel();
        var heading = new StackPanel { Margin = new Thickness(2, 0, 2, 10) };
        heading.Children.Add(new TextBlock
        {
            Text = "Run parameters",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(23, 43, 67))
        });
        heading.Children.Add(new TextBlock
        {
            Text = $"{table.Rows.Count:N0} RunParameters file(s) loaded from {folderName}; each parameter key is a column.",
            Margin = new Thickness(0, 4, 0, 0),
            Foreground = new SolidColorBrush(Color.FromRgb(89, 102, 117)),
            TextWrapping = TextWrapping.Wrap
        });
        DockPanel.SetDock(heading, Dock.Top);
        layout.Children.Add(heading);
        layout.Children.Add(new DataGrid
        {
            ItemsSource = table.DefaultView,
            AutoGenerateColumns = true,
            IsReadOnly = true,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserReorderColumns = true,
            CanUserSortColumns = true,
            EnableRowVirtualization = true,
            EnableColumnVirtualization = true,
            FrozenColumnCount = table.Columns.Count > 0 ? 1 : 0,
            HeadersVisibility = DataGridHeadersVisibility.All,
            GridLinesVisibility = DataGridGridLinesVisibility.All
        });
        Content = layout;
    }

}

public sealed record MessagePackNodePathSegment(string Label, int Occurrence);

public sealed class OutputFolderFilesGridControl : UserControl
{
    private readonly DataTable table;
    private readonly DataGrid dataGrid;
    private readonly Button startButton;
    private readonly Button stopButton;
    private readonly TextBlock progressText;
    private readonly HashSet<string> loadedPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> loadingPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> rootColumns = new(StringComparer.Ordinal);
    private bool isLoading;

    public event Action<string>? FileSelected;
    public event Action? StartRequested;
    public event Action? StopRequested;

    public OutputFolderFilesGridControl(string folderName, DataTable table)
    {
        this.table = table;
        var layout = new DockPanel();
        var heading = new StackPanel { Margin = new Thickness(2, 0, 2, 10) };
        heading.Children.Add(new TextBlock
        {
            Text = $"Files in {folderName}",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(23, 43, 67))
        });
        heading.Children.Add(new TextBlock
        {
            Text = $"{table.Rows.Count:N0} output file(s). Load root fields on demand.",
            Margin = new Thickness(0, 4, 0, 0),
            Foreground = new SolidColorBrush(Color.FromRgb(89, 102, 117))
        });
        DockPanel.SetDock(heading, Dock.Top);
        layout.Children.Add(heading);

        var actions = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        stopButton = new Button { Content = "Stop", Padding = new Thickness(12, 5, 12, 5), IsEnabled = false };
        stopButton.Click += (_, _) => StopRequested?.Invoke();
        DockPanel.SetDock(stopButton, Dock.Right);
        actions.Children.Add(stopButton);
        startButton = new Button { Content = "Start loading remaining files", Padding = new Thickness(12, 5, 12, 5), IsEnabled = table.Rows.Count > 0 };
        startButton.Click += (_, _) => StartRequested?.Invoke();
        DockPanel.SetDock(startButton, Dock.Left);
        actions.Children.Add(startButton);
        progressText = new TextBlock
        {
            Text = $"0 of {table.Rows.Count:N0} files loaded",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 4, 0),
            Foreground = new SolidColorBrush(Color.FromRgb(89, 102, 117))
        };
        actions.Children.Add(progressText);
        DockPanel.SetDock(actions, Dock.Top);
        layout.Children.Add(actions);

        dataGrid = new DataGrid
        {
            ItemsSource = table.DefaultView,
            AutoGenerateColumns = true,
            IsReadOnly = true,
            CanUserAddRows = false,
            CanUserDeleteRows = false,
            CanUserSortColumns = true,
            EnableRowVirtualization = true,
            EnableColumnVirtualization = true,
            HeadersVisibility = DataGridHeadersVisibility.All,
            GridLinesVisibility = DataGridGridLinesVisibility.All
        };
        dataGrid.SelectionChanged += (_, _) =>
        {
            if (dataGrid.SelectedItem is DataRowView row && row.Row.Table.Columns.Contains("Path"))
                FileSelected?.Invoke(Convert.ToString(row["Path"]) ?? string.Empty);
        };
        layout.Children.Add(dataGrid);
        Content = layout;
    }

    public IReadOnlyList<string> GetUnloadedFilePaths() => table.Rows.Cast<DataRow>()
        .Select(row => Convert.ToString(row["Path"]) ?? string.Empty)
        .Where(path => path.Length > 0 && !loadedPaths.Contains(path) && !loadingPaths.Contains(path))
        .ToArray();

    public IReadOnlyList<string> GetAllFilePaths() => table.Rows.Cast<DataRow>()
        .Select(row => Convert.ToString(row["Path"]) ?? string.Empty)
        .Where(path => path.Length > 0)
        .ToArray();

    public int LoadedFileCount => loadedPaths.Count;

    public bool TryBeginLoading(string path)
    {
        if (loadedPaths.Contains(path) || !loadingPaths.Add(path))
            return false;
        return true;
    }

    public void CompleteLoading(string path, IReadOnlyDictionary<string, string> values)
    {
        var row = FindRow(path);
        if (row is null)
            return;

        foreach (var key in values.Keys)
        {
            if (!rootColumns.TryGetValue(key, out var columnName))
            {
                columnName = CreateRootColumnName(key);
                table.Columns.Add(columnName, typeof(string));
                rootColumns[key] = columnName;
                RefreshGridColumns();
            }
        }
        foreach (var (key, value) in values)
            row[rootColumns[key]] = value;

        loadingPaths.Remove(path);
        loadedPaths.Add(path);
        UpdateProgress();
    }

    public void FailLoading(string path)
    {
        loadingPaths.Remove(path);
        UpdateProgress();
    }

    public void SetLoadingState(bool isLoading)
    {
        this.isLoading = isLoading;
        startButton.IsEnabled = !isLoading && GetUnloadedFilePaths().Count > 0;
        stopButton.IsEnabled = isLoading;
    }

    public void SetProgressMessage(string message) => progressText.Text = message;

    private DataRow? FindRow(string path) => table.Rows.Cast<DataRow>()
        .FirstOrDefault(row => string.Equals(Convert.ToString(row["Path"]), path, StringComparison.OrdinalIgnoreCase));

    private string CreateRootColumnName(string key)
    {
        var candidate = key;
        var suffix = 2;
        while (table.Columns.Cast<DataColumn>().Any(column => string.Equals(column.ColumnName, candidate, StringComparison.OrdinalIgnoreCase)))
            candidate = $"{key} (value {suffix++})";
        return candidate;
    }

    private void RefreshGridColumns()
    {
        dataGrid.ItemsSource = null;
        dataGrid.Columns.Clear();
        dataGrid.AutoGenerateColumns = true;
        dataGrid.ItemsSource = table.DefaultView;
    }

    private void UpdateProgress()
    {
        progressText.Text = $"{loadedPaths.Count:N0} of {table.Rows.Count:N0} files loaded";
        startButton.IsEnabled = !isLoading && GetUnloadedFilePaths().Count > 0;
    }
}

public static class OutputDataViewerFactory
{
    public static OutputDataViewerControl Create(OutputDataKind kind, IEnumerable<MessagePackNode> nodes) => kind switch
    {
        OutputDataKind.Run => new RunOutputDataControl(nodes),
        OutputDataKind.RunParameters => new RunParametersOutputDataControl(nodes),
        OutputDataKind.SortableTests => new SortableTestsOutputDataControl(nodes),
        OutputDataKind.SorterPoolSet => new SorterPoolSetOutputDataControl(nodes),
        OutputDataKind.SorterPoolSetSummarySet => new SorterPoolSetSummarySetOutputDataControl(nodes),
        OutputDataKind.SorterSet => new SorterSetOutputDataControl(nodes),
        OutputDataKind.SorterSetEval => new SorterSetEvalOutputDataControl(nodes),
        OutputDataKind.SorterPoolBinsSetSeries => new SorterPoolBinsSetSeriesOutputDataControl(nodes),
        OutputDataKind.SorterPoolSetHistory => new SorterPoolSetHistoryOutputDataControl(nodes),
        _ => new GenericMessagePackOutputDataControl(nodes)
    };

    private sealed class GenericMessagePackOutputDataControl(IEnumerable<MessagePackNode> nodes)
        : OutputDataViewerControl("MessagePack data", "Serialized values.", nodes);
}
