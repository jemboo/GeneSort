using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace GeneSort.WPF.Settings;

// A null key is the row-number column; a null width means automatic sizing.
internal sealed record ColumnLayoutSetting(string? Key, double? Width);

internal sealed class TableLayoutSettings
{
    public List<ColumnLayoutSetting> Columns { get; set; } = [];
    public string? SortKey { get; set; }
    public bool Descending { get; set; }
    internal Action? CaptureLayout { get; set; }
}

/// Per-user presentation settings, independent of run definitions and project files.
internal sealed class TableLayoutStore
{
    private readonly string path;
    private readonly Dictionary<string, TableLayoutSettings> tables;
    public TableLayoutSettings RunParameters => Get("RunParameters");
    public TableLayoutSettings SummaryReports => Get("SummaryReports");

    public TableLayoutStore(string path)
    {
        this.path = path;
        try
        {
            tables = File.Exists(path)
                ? JsonSerializer.Deserialize<Dictionary<string, TableLayoutSettings>>(File.ReadAllText(path)) ?? [] : [];
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
        {
            Trace.TraceWarning($"Could not read table display settings from {path}: {error.Message}");
            tables = [];
        }
        foreach (var key in tables.Keys.ToArray())
        {
            if (tables[key] is not { } settings) { tables.Remove(key); continue; }
            settings.Columns = (settings.Columns ?? []).Where(column => column is not null)
                .DistinctBy(column => column.Key, StringComparer.Ordinal)
                .Select(column => column with { Width = column.Width is double width && double.IsFinite(width) && width >= 0 ? width : null }).ToList();
            if (string.IsNullOrEmpty(settings.SortKey)) { settings.SortKey = null; settings.Descending = false; }
        }
    }

    private TableLayoutSettings Get(string key)
    {
        if (!tables.TryGetValue(key, out var settings)) tables[key] = settings = new();
        return settings;
    }

    // Called on the UI thread at shutdown, including when a table is still visible.
    public string? Save()
    {
        foreach (var settings in tables.Values) settings.CaptureLayout?.Invoke();
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(temporary, JsonSerializer.Serialize(tables, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporary, path, overwrite: true);
            return null;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return $"Could not save table display settings to {path}: {error.Message}";
        }
        finally
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { Trace.TraceWarning(error.Message); }
        }
    }
}
