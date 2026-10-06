using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace GeneSort.WPF.ViewModels;


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
            ("SortableTests", OutputDataKind.SortableTests),
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
            ("SortableTests", "SortableTests"),
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

