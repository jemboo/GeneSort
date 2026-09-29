using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Data;
using System.IO;

namespace GeneSort.WPF;

public enum OutputDataKind
{
    Unknown,
    Run,
    RunParameters,
    SortableTest,
    SorterPoolSet,
    SorterPoolSetSummarySet,
    SorterSet,
    SorterSetEval,
    SorterPoolBinsSetSeries,
    SorterPoolSetHistory,
    TextReport
}

/// <summary>Shared structure for the output-specific viewers. Each concrete control
/// gives the serialized output a type-specific heading and context.</summary>
public abstract class OutputDataViewerControl : UserControl
{
    public event Action<MessagePackNode>? NodeSelected;

    protected OutputDataViewerControl(string heading, string description, IEnumerable<MessagePackNode> nodes)
    {
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

        var tree = new TreeView { ItemsSource = nodes, BorderThickness = new Thickness(0) };
        tree.Resources.Add(new DataTemplateKey(typeof(MessagePackNode)), CreateNodeTemplate());
        tree.SelectedItemChanged += (_, args) =>
        {
            if (args.NewValue is MessagePackNode selectedNode)
                NodeSelected?.Invoke(selectedNode);
        };
        layout.Children.Add(tree);
        Content = layout;
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
        AddBoundDetail(details, "Name", nameof(MainWindow.SelectionTitle), FontWeights.SemiBold);
        AddBoundDetail(details, "Type", nameof(MainWindow.SelectionType));
        AddBoundDetail(details, "Path", nameof(MainWindow.SelectionPath));
        AddBoundDetail(details, "Size", nameof(MainWindow.SelectionSize));
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

public sealed class SortableTestOutputDataControl(IEnumerable<MessagePackNode> nodes)
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
            Text = $"{table.Rows.Count:N0} files in {folderName}; each row is a file and each parameter key is a column.",
            Margin = new Thickness(0, 4, 0, 0),
            Foreground = new SolidColorBrush(Color.FromRgb(89, 102, 117)),
            TextTrimming = TextTrimming.CharacterEllipsis
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

public static class OutputDataViewerFactory
{
    public static OutputDataViewerControl Create(OutputDataKind kind, IEnumerable<MessagePackNode> nodes) => kind switch
    {
        OutputDataKind.Run => new RunOutputDataControl(nodes),
        OutputDataKind.RunParameters => new RunParametersOutputDataControl(nodes),
        OutputDataKind.SortableTest => new SortableTestOutputDataControl(nodes),
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
