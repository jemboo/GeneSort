using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using GeneSort.Dispatch.V1;
using GeneSort.FileDb.V1;
using GeneSort.Project.V1;

namespace GeneSort.WPF.ViewModels;

public sealed record RunBreadcrumb(string Name, string ModulePath, bool IsRoot, bool IsCurrent);

public sealed record RunSelectionTile(string Name, string Icon, string Detail,
    string ModulePath, run? Definition = null, string? SavedPath = null,
    bool IsCodeDefined = false, bool CreateNew = false)
{
    public bool IsModule => Definition is null && !CreateNew;
    public string Status => IsModule ? "Module" : CreateNew ? "Create your own" :
        SavedPath is null ? "Code only · not saved" : IsCodeDefined ? "Code + saved on disk" : "Saved on disk";
    public string AccessibleName => $"{Name}. {Status}. {Detail}";
}

/// Folder-scoped navigation; browsing never writes definitions or starts a run.
public sealed class RunSelectionViewModel : INotifyPropertyChanged
{
    private readonly runType[] types;
    private List<RunSelectionTile> leaves = [];
    private string folder = "", currentPath = "", message = "";
    private bool isLoading, folderStep = true, creating;
    private Task? initialization;
    public RunSelectionViewModel(runType[] types) => this.types = types;
    public Task InitializeAsync() => initialization ??= InitializeCoreAsync();
    private async Task InitializeCoreAsync()
    {
        if (!CanContinue) return;
        folderStep = false;
        Refresh();
        await LoadAsync();
    }
    public string Folder
    {
        get => folder;
        set { folder = value; Changed(); Changed(nameof(CanContinue)); }
    }
    public string CurrentPath => currentPath;
    public bool IsFolderStep => folderStep;
    public bool IsHierarchyStep => !folderStep;
    public bool IsLoading => isLoading;
    public bool IsReady => !isLoading;
    public bool CanContinue => !isLoading && Path.IsPathFullyQualified(Folder) && Directory.Exists(Folder);
    public string Message { get => message; private set { message = value; Changed(); } }
    public string Heading => folderStep ? "Choose your projects folder" :
        "Modules" + (currentPath.Length == 0 ? "" : " / " + currentPath.Replace(".", " / "));
    public string Breadcrumb => folderStep ? "1 · Project folder" : "2 · Modules / " + currentPath.Replace(".", " / ");
    public RunBreadcrumb[] Breadcrumbs
    {
        get
        {
            var result = new List<RunBreadcrumb> { new("Modules", "", true, currentPath.Length == 0) };
            var path = "";
            foreach (var segment in currentPath.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                path = path.Length == 0 ? segment : path + "." + segment;
                result.Add(new(segment, path, false, path == currentPath));
            }
            return result.ToArray();
        }
    }
    public void JumpTo(RunBreadcrumb breadcrumb)
    {
        if (isLoading || folderStep || !Breadcrumbs.Contains(breadcrumb)) return;
        currentPath = breadcrumb.ModulePath;
        creating = false;
        Refresh();
    }
    public string Help => folderStep ? "Choose the folder containing your projects. Code definitions and runs saved there will appear together." :
        creating ? "Choose a template for your new leaf. You can edit its name and parameters before saving." :
        "Open a module to continue. Select a run to configure and execute it, or create your own leaf.";
    public RunSelectionTile[] Tiles { get; private set; } = [];
    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<string>? FolderSelected;
    private void Changed([CallerMemberName] string? property = null) => PropertyChanged?.Invoke(this, new(property));
    private void Refresh()
    {
        var here = leaves.Where(leaf => leaf.ModulePath == currentPath).ToArray();
        if (creating)
            Tiles = here.Select(leaf => leaf with { CreateNew = true, Detail = "Start from " + leaf.Name }).ToArray();
        else
        {
            var prefix = currentPath.Length == 0 ? "" : currentPath + ".";
            var modules = leaves.Where(leaf => leaf.ModulePath.StartsWith(prefix, StringComparison.Ordinal) && leaf.ModulePath != currentPath)
                .Select(leaf => leaf.ModulePath[prefix.Length..].Split('.')[0]).Distinct().Order(StringComparer.OrdinalIgnoreCase)
                .Select(name => new RunSelectionTile(name, ModuleIcon(name), "Explore modules and run definitions", prefix + name));
            Tiles = [.. modules, .. here.OrderBy(leaf => leaf.Name, StringComparer.OrdinalIgnoreCase),
                .. (here.Length == 0 ? Array.Empty<RunSelectionTile>() : [new RunSelectionTile("Create your own", "\uE710", "Start with a definition in this module", currentPath, CreateNew: true)])];
        }
        foreach (var property in new[] { nameof(Tiles), nameof(Heading), nameof(Breadcrumb), nameof(Breadcrumbs), nameof(Help), nameof(IsFolderStep), nameof(IsHierarchyStep), nameof(CurrentPath) }) Changed(property);
    }
    private static string ModuleIcon(string name) => name switch
    {
        "SortableTests" => "\uE9D9", "SorterEval" => "\uE9D2", "SorterMutate" => "\uE8AB",
        "SorterSgd" => "\uE9D9", _ when name.StartsWith('p') && int.TryParse(name[1..], out _) => "\uE80A",
        _ => "\uE8B7"
    };
    public void ChangeFolder() { if (isLoading) return; folderStep = true; creating = false; Refresh(); }
    public void Back()
    {
        if (isLoading) return;
        if (creating) creating = false;
        else if (currentPath.Length == 0) folderStep = true;
        else currentPath = currentPath.Contains('.') ? currentPath[..currentPath.LastIndexOf('.')] : "";
        Refresh();
    }
    public void Home() { if (isLoading) return; creating = false; currentPath = ""; Refresh(); }
    public bool Navigate(RunSelectionTile tile)
    {
        if (isLoading || !Tiles.Contains(tile)) return false;
        if (tile.IsModule) { currentPath = tile.ModulePath; creating = false; Refresh(); return true; }
        if (tile.CreateNew && tile.Definition is null) { creating = true; Refresh(); return true; }
        return false;
    }
    public async Task LoadAsync(bool reset = true)
    {
        if (!CanContinue) { Message = "Choose an existing absolute projects folder."; return; }
        var root = Path.GetFullPath(Folder);
        isLoading = true;
        foreach (var property in new[] { nameof(IsLoading), nameof(IsReady), nameof(CanContinue) }) Changed(property);
        Message = "Reading run definitions…";
        try
        {
            var result = await Task.Run(() => ReadAsync(root));
            leaves = result.Leaves;
            folderStep = false;
            FolderSelected?.Invoke(root);
            if (reset) currentPath = "";
            creating = false;
            Message = result.Errors.Count == 0 ? $"{leaves.Count} run definitions · {leaves.Count(leaf => leaf.SavedPath is not null)} saved on disk" :
                string.Join(Environment.NewLine, result.Errors);
            Refresh();
        }
        catch (Exception error)
        {
            Message = "Could not read this folder: " + error.GetBaseException().Message;
            folderStep = true;
            Refresh();
        }
        finally
        {
            isLoading = false;
            foreach (var property in new[] { nameof(IsLoading), nameof(IsReady), nameof(CanContinue) }) Changed(property);
        }
    }
    private async Task<(List<RunSelectionTile> Leaves, List<string> Errors)> ReadAsync(string root)
    {
        var result = new List<RunSelectionTile>();
        var errors = new List<string>();
        foreach (var type in types)
            foreach (var example in type.Examples)
                try { result.Add(new(example.Name, "\uE943", example.Run.Description, type.Name, example.Run, IsCodeDefined: true)); }
                catch (Exception error) { errors.Add($"{type.Name}.{example.Name}: {error.GetBaseException().Message}"); }
        var code = result.ToArray();
        // Only inspect definition directories; parameter/results trees can contain millions of files.
        string[] Directories(string path)
        {
            try { return Directory.Exists(path) ? Directory.GetDirectories(path).Where(p => (File.GetAttributes(p) & FileAttributes.ReparsePoint) == 0).ToArray() : []; }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException) { errors.Add($"{path}: {error.Message}"); return []; }
        }
        foreach (var project in Directories(root))
            foreach (var database in Directories(project))
                foreach (var runFolder in Directories(Path.Combine(database, "Data", "Project", "Run")))
                {
                    string[] paths;
                    try { paths = Directory.GetFiles(runFolder, "Run_*.msgpack"); }
                    catch (Exception error) when (error is IOException or UnauthorizedAccessException) { errors.Add($"{runFolder}: {error.Message}"); continue; }
                    foreach (var path in paths)
                        try
                        {
                            var definition = await RunDefinitionFiles.loadAsync(path);
                            if (!string.Equals(RunDefinitionFiles.pathFor(root, definition), path, StringComparison.OrdinalIgnoreCase))
                                throw new InvalidDataException("Definition identity does not match its storage location.");
                            var matches = code.Where(leaf => SameIdentity(leaf.Definition!, definition)).ToArray();
                            if (matches.Length > 0)
                            {
                                foreach (var match in matches)
                                {
                                    result.Remove(match);
                                    result.Add(match with { Definition = definition, SavedPath = path, Icon = "\uE74E", Detail = definition.Description });
                                }
                                continue;
                            }
                            string? module = null;
                            try { module = ReadModule(path); }
                            catch (Exception error) { errors.Add($"{path}: could not read module label ({error.GetBaseException().Message})."); }
                            if (module is null || !types.Any(type => type.Name == module))
                            {
                                var candidates = code.Where(leaf => leaf.Definition!.ProjectName == definition.ProjectName && leaf.Definition.DatabaseName == definition.DatabaseName)
                                    .Select(leaf => leaf.ModulePath).Distinct().ToArray();
                                module = candidates.Length == 1 ? candidates[0] : $"Saved runs.{definition.ProjectName}.{definition.DatabaseName}";
                            }
                            result.Add(new(definition.RunName, "\uE74E", definition.Description, module, definition, path));
                        }
                        catch (Exception error) { errors.Add($"{path}: {error.GetBaseException().Message}"); }
                }
        return (result, errors);
    }
    private static bool SameIdentity(run a, run b) => a.ProjectName == b.ProjectName && a.DatabaseName == b.DatabaseName && a.RunName == b.RunName;
    private sealed record ModuleOrigin(string Module);
    internal static string? ReadModule(string path)
    {
        var metadata = path + ".module.json";
        return File.Exists(metadata) ? JsonSerializer.Deserialize<ModuleOrigin>(File.ReadAllText(metadata))?.Module : null;
    }
    internal static async Task SaveModuleAsync(string path, string? module)
    {
        if (string.IsNullOrEmpty(module)) return;
        await File.WriteAllTextAsync(path + ".module.json", JsonSerializer.Serialize(new ModuleOrigin(module)));
    }
}
