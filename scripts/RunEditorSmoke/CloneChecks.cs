using System.IO;
using System.Text;
using System.Windows.Automation;
using System.Windows.Controls;
using GeneSort.FileDb.V1;
using GeneSort.Project.V1.Parameters;
using GeneSort.WPF.ViewModels;
using GeneSort.WPF.Views;

internal static partial class Program
{
    private static void CheckCloning(string root, RunEditorViewModel model, RunEditorView view, StringBuilder errors)
    {
        model.Selection.Folder = root;
        Wait(model.Selection.LoadAsync());
        model.Selection.OpenModule("SortableTests.Prefix");
        var tile = model.Selection.Tiles.Single(tile => tile.Name == "Channels24");
        Wait(model.ChooseRunAsync(tile));
        Layout(view);
        Click(Button(view, "Clone"));
        Check(model.RunName == "Channels24_clone" && model.ModulePropertiesLocked, "Code clone was not named or locked correctly.");
        Check(model.RunParameterList.Count == 0 && model.SetupParameterList.Count == 0, "Clone retained generated rows.");
        Check(Descendants<TextBox>(view).Single(box => AutomationProperties.GetName(box) == "Project name").IsReadOnly,
            "Module-owned project is editable.");
        Check(!Descendants<TextBox>(view).Single(box => AutomationProperties.GetName(box) == "Run name").IsReadOnly,
            "Clone name is not editable.");
        var originalDescription = model.Description;
        model.Description = "Forbidden";
        Check(model.Validate() is null, "Module metadata mutation bypassed validation.");
        model.Description = originalDescription;
        model.RunName = "CustomPrefixClone";
        var row = model.ExecutionParameters.Single(row => row.Key == ParamKeys.prefixLibIdKey);
        while (row.Values.Count > 1) row.Values.RemoveAt(row.Values.Count - 1);
        var clone = model.Validate() ?? throw new Exception(string.Join("; ", model.Issues));
        Wait(model.SaveAsync(clone, false));
        var path = RunDefinitionFiles.pathFor(root, clone);
        Check(RunSelectionViewModel.ReadModule(path) == tile.ModulePath, "Clone moved to another module.");
        Check(!File.Exists(RunDefinitionFiles.pathFor(root, tile.Definition!)), "Cloning saved the source code definition.");
        Wait(model.OpenAsync(path));
        model.Clone();
        Check(model.RunName == "CustomPrefixClone_clone" && model.ExecutionParameters.Single(row => row.Key == ParamKeys.prefixLibIdKey).Values.Count == 1,
            "Saved clone did not preserve its actual saved spans.");
        var second = model.Validate() ?? throw new Exception(string.Join("; ", model.Issues));
        Wait(model.SaveAsync(second, false));
        Check(File.Exists(path) && File.Exists(RunDefinitionFiles.pathFor(root, second)), "Clone replaced its source definition.");
        Wait(model.BeginSelectionAsync());
        Check(model.Selection.CurrentPath == tile.ModulePath && model.Selection.Tiles.Any(tile => tile.Name == "CustomPrefixClone_clone"),
            "Saved clone is not visible in the source module.");
        Layout(view);
        Check(errors.Length == 0, errors.ToString());
    }
}
