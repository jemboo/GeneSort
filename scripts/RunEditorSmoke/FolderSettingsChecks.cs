using System.IO;
using GeneSort.WPF.Settings;
using GeneSort.WPF.ViewModels;

internal static partial class Program
{
    private static void CheckFolderSettings(string root)
    {
        var settingsPath = Path.Combine(root, "preferences", "workspace.json");
        var projects = Path.Combine(root, "Projects");
        Directory.CreateDirectory(projects);
        var first = new RunEditorViewModel(new WorkspaceSettingsStore(settingsPath));
        first.Selection.Folder = projects;
        Wait(first.Selection.LoadAsync());
        var next = new RunEditorViewModel(new WorkspaceSettingsStore(settingsPath));
        Wait(next.Selection.InitializeAsync());
        Check(next.ProjectsFolder == projects && next.Selection.Folder == projects,
            "Folder selected in the wizard was not restored in both folder fields.");
        Check(!next.HasUnsavedChanges && next.Selection.IsHierarchyStep && next.Selection.CurrentPath == ""
            && next.Selection.Tiles.Any(tile => tile.IsModule),
            "A valid remembered folder did not open module selection cleanly.");
        next.Selection.ChangeFolder();
        Wait(next.Selection.InitializeAsync());
        Check(next.Selection.IsFolderStep, "Reloading the view overrode an explicit Change folder action.");
        next.ProjectsFolder = root;
        Check(new WorkspaceSettingsStore(settingsPath).ProjectsFolder == root, "Editor folder changes were not persisted.");
        next.ProjectsFolder = "relative";
        Check(new WorkspaceSettingsStore(settingsPath).ProjectsFolder == root, "Incomplete input erased the saved folder.");
        var unavailable = Path.Combine(root, "Unavailable");
        File.WriteAllText(settingsPath, System.Text.Json.JsonSerializer.Serialize(new WorkspaceSettings { ProjectsFolder = unavailable }));
        var missing = new RunEditorViewModel(new WorkspaceSettingsStore(settingsPath));
        Wait(missing.Selection.InitializeAsync());
        Check(missing.Selection.IsFolderStep && missing.Selection.Folder == unavailable && !missing.Selection.CanContinue && !missing.HasUnsavedChanges,
            "An unavailable folder should remain visible and require reselection.");
        File.WriteAllText(settingsPath, "invalid json");
        Check(new WorkspaceSettingsStore(settingsPath).ProjectsFolder == "", "Malformed preferences were not handled.");
    }
}
