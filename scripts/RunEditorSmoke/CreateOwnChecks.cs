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
    // Exercise the same template selection as the Create your own tile.
    private static void CreateOwn(string root, RunEditorViewModel model)
    {
        var module = model.SelectedRunType!.Name;
        var sourceName = model.RunName;
        model.ProjectsFolder = root;
        Wait(model.BeginSelectionAsync());
        model.Selection.OpenModule(module);
        Check(model.Selection.Navigate(model.Selection.Tiles.Single(tile => tile.CreateNew)), "Create your own did not open template selection.");
        var template = model.Selection.Tiles.FirstOrDefault(tile => tile.Name == sourceName)
            ?? model.Selection.Tiles.First(tile => tile.Name == model.SelectedExample!.Name);
        Wait(model.ChooseRunAsync(template));
    }

    private static void CheckCreateOwn(string root, RunEditorViewModel model, RunEditorView view, StringBuilder errors)
    {
        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SortableTests.Prefix.Channels24"));
        var source = model.SelectedExample!.Run;
        Layout(view);
        Check(model.DefinitionReadOnly && !model.CanEditDefinition, "Code run is editable.");
        Check(!Descendants<Button>(view).Any(button => Equals(button.Content, "Clone")), "Clone button remains visible.");
        CreateOwn(root, model);
        Layout(view);
        Check(model.CanEditDefinition && model.RunName == "NewRun", "New draft is not editable.");
        Check(model.RunParameterList.Count == 0 && model.SetupParameterList.Count == 0, "New draft retained generated rows.");
        Check(Descendants<TextBox>(view).Single(box => AutomationProperties.GetName(box) == "Project name").IsReadOnly,
            "Module-owned project is editable.");
        var description = Descendants<TextBox>(view).Single(box => AutomationProperties.GetName(box) == "Description");
        Check(!description.IsReadOnly && description.IsEnabled, "New run description is not editable.");
        description.Text = "My custom prefix experiment";
        Pump();
        Check(model.Description == description.Text && model.HasUnsavedChanges, "Description binding did not update the draft.");
        var row = model.ExecutionParameters.Single(row => row.Key == ParamKeys.prefixLibIdKey);
        while (row.Values.Count > 1) row.Values.RemoveAt(row.Values.Count - 1);
        var configured = model.Validate() ?? throw new Exception(string.Join("; ", model.Issues));
        Check(configured.Description == description.Text, "Validation lost the custom description.");
        Wait(model.SaveAsync(configured, false));
        var path = RunDefinitionFiles.pathFor(root, configured);
        Check(!File.Exists(RunDefinitionFiles.pathFor(root, source)), "Creating a run saved its source preset.");
        Wait(model.OpenAsync(path));
        Layout(view);
        Check(model.Description == "My custom prefix experiment" && description.IsReadOnly, "Saved description changed or remained editable.");
        model.Description = "Forbidden saved edit";
        Check(model.Validate() is null, "Saved description edits bypassed validation.");
        model.Description = configured.Description;
        CreateOwn(root, model);
        Layout(view);
        Check(model.RunName == "NewRun1" && model.Description == configured.Description,
            "Saved template lost its description or collided with the saved name.");
        Check(model.ExecutionParameters.Single(row => row.Key == ParamKeys.prefixLibIdKey).Values.Count == 1,
            "Saved template lost its actual parameter spans.");
        description.Text = "Second experiment";
        Pump();
        var second = model.Validate() ?? throw new Exception(string.Join("; ", model.Issues));
        Wait(model.SaveAsync(second, false));
        Wait(model.BeginSelectionAsync());
        Check(model.Selection.Tiles.Any(tile => tile.Name == second.RunName && tile.Detail == "Second experiment"),
            "Saved run or its description is missing from its module.");
        var reloadedSource = RunDefinitionFiles.loadAsync(path);
        Wait(reloadedSource);
        Check(reloadedSource.Result.Description == configured.Description,
            "Creating a second run changed its saved template.");
        Check(errors.Length == 0, errors.ToString());
    }
}
