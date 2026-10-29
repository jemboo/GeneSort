using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace GeneSort.WPF.Settings;

internal sealed class WorkspaceSettings
{
    public string ProjectsFolder { get; set; } = "";
}

/// Per-user folder preference; it is not part of a run definition.
internal sealed class WorkspaceSettingsStore
{
    private readonly string path;
    public string ProjectsFolder { get; private set; } = "";

    public WorkspaceSettingsStore(string path)
    {
        this.path = path;
        try
        {
            var saved = File.Exists(path) ? JsonSerializer.Deserialize<WorkspaceSettings>(File.ReadAllText(path)) : null;
            if (!string.IsNullOrWhiteSpace(saved?.ProjectsFolder) && Path.IsPathFullyQualified(saved.ProjectsFolder))
                ProjectsFolder = saved.ProjectsFolder;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
        {
            Trace.TraceWarning($"Could not read workspace settings: {error.Message}");
        }
    }

    public string? RememberFolder(string folder)
    {
        // Ignore partially typed paths and temporarily unavailable locations.
        if (!Path.IsPathFullyQualified(folder) || !Directory.Exists(folder)) return null;
        var normalized = Path.GetFullPath(folder);
        if (string.Equals(ProjectsFolder, normalized, StringComparison.OrdinalIgnoreCase)) return null;
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(temporary, JsonSerializer.Serialize(new WorkspaceSettings { ProjectsFolder = normalized }));
            File.Move(temporary, path, overwrite: true);
            ProjectsFolder = normalized;
            return null;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return $"Could not remember the Projects folder: {error.Message}";
        }
        finally
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { Trace.TraceWarning(error.Message); }
        }
    }
}
