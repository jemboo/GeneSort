using System.IO;
using System.Text.Json;

namespace GeneSort.WPF.Settings;

/// Execution history is separate from generated parameter values. Regeneration explicitly clears this history.
internal static class ProcessingFailureStore
{
    private static string FilePath(string parameterFolder) => parameterFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + ".processing-failures.json";

    internal const string MissingMessage = "No error message was recorded for this older failure.";

    internal static async Task<Dictionary<Guid, string>> LoadAsync(string parameterFolder)
    {
        var path = FilePath(parameterFolder);
        if (!File.Exists(path)) return [];
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(path));
        // Previous versions saved only failed row IDs.
        if (document.RootElement.ValueKind == JsonValueKind.Array)
            return (document.RootElement.Deserialize<HashSet<Guid>>() ?? []).ToDictionary(id => id, _ => MissingMessage);
        return document.RootElement.Deserialize<Dictionary<Guid, string>>() ?? [];
    }

    internal static async Task SaveAsync(string parameterFolder, IReadOnlyDictionary<Guid, string> failures)
    {
        var path = FilePath(parameterFolder);
        var messages = failures.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
        if (messages.Count == 0) { File.Delete(path); return; }
        Directory.CreateDirectory(parameterFolder);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(messages));
            File.Move(temporary, path, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
