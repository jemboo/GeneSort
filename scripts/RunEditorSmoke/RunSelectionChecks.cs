using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using GeneSort.FileDb.V1;
using GeneSort.WPF.ViewModels;
using GeneSort.WPF.Views;

internal static partial class Program
{
    private static void CheckRunSelection(string root, RunEditorViewModel model, RunEditorView view, StringBuilder errors)
    {
        var wizard = model.Selection;
        Check(model.RunTypes.All(type => !string.IsNullOrWhiteSpace(type.Description)), "A run module has no description.");
        Check(model.IsSelectingRun && wizard.IsFolderStep && !wizard.CanContinue, "The first step must require a folder.");
        wizard.Folder = "relative-path";
        Check(!wizard.CanContinue, "A relative folder was accepted.");
        wizard.Folder = root;
        Wait(wizard.LoadAsync());
        Layout(view);
        Check(!wizard.IsFolderStep && wizard.Tiles.All(tile => tile.IsModule), "Root should display modules.");
        Check(wizard.Tiles.Select(tile => tile.Name).SequenceEqual(new[] { "SortableTests", "SorterEval", "SorterMutate", "SorterSgd" }), "Root module hierarchy is wrong.");
        var selectionView = Descendants<RunSelectionView>(view).Single();
        Check(selectionView.Visibility == Visibility.Visible && Descendants<WrapPanel>(selectionView).Any(), "Wizard is not rendered as wrap panels. " + errors);
        var rootTile = Descendants<Button>(selectionView).Single(button => button.DataContext is RunSelectionTile { Name: "SortableTests" });
        Click(rootTile);
        Check(wizard.CurrentPath == "SortableTests", "Tile click did not navigate.");
        wizard.Navigate(wizard.Tiles.Single(tile => tile.Name == "Prefix"));
        Check(wizard.ModuleDescription == model.RunTypes.Single(type => type.Name == wizard.CurrentPath).Description,
            "The module page lost its description.");
        Layout(view);
        Check(Descendants<TextBlock>(selectionView).Any(text => text.Text == wizard.ModuleDescription), "Module description is not visible on the page.");
        var original = wizard.Tiles.First(tile => tile.Definition is not null);
        Check(original.IsCodeDefined && original.SavedPath is null && original.Status.Contains("not saved"), "Unsaved code leaf status is incorrect.");
        var codeRunName = original.Definition!.RunName;
        Wait(model.ChooseRunAsync(original));
        Check(!model.IsSelectingRun && model.RunName == codeRunName, "Code selection must retain the actual run name rather than NewRun.");
        Check(model.ProjectsFolder == root && model.IsDirty, "Code selection lost its folder or unsaved state.");
        model.Description = "Saved values take precedence over code defaults";
        var configured = model.Validate() ?? throw new Exception(string.Join("; ", model.Issues));
        Wait(model.SaveAsync(configured, false));
        Wait(model.BeginSelectionAsync());
        var saved = wizard.Tiles.Single(tile => tile.Name == original.Name);
        Check(saved.IsCodeDefined && saved.SavedPath is not null && saved.Status == "Code + saved on disk", "Saved counterpart was not merged into the code leaf.");
        Check(!saved.IsMaterialized, "Saving a definition alone made it look completed.");
        var completionRows = GeneSort.Project.V1.RunParameterGeneration.generate(configured, 0, 2);
        Check(completionRows.Length >= 2, "Completion fixture needs multiple processing rows.");
        void WriteCompletion(bool allFinished)
        {
            var parameterFolder = RunParameterFiles.folderFor(root, configured);
            for (var index = 0; index < completionRows.Length; index++)
            {
                var row = completionRows[index].WithRunFinished(Microsoft.FSharp.Core.FSharpOption<bool>.Some(allFinished || index == 0));
                var replicaFolder = Path.Combine(parameterFolder, row.GetRepl().Value.ToString());
                Directory.CreateDirectory(replicaFolder);
                File.WriteAllBytes(Path.Combine(replicaFolder, row.GetId().Value + ".msgpack"),
                    MessagePack.MessagePackSerializer.Serialize(GeneSort.Project.Mp.V1.RunParametersDto.fromDomain(row)));
            }
        }
        WriteCompletion(false);
        Wait(wizard.LoadAsync(false));
        Check(!wizard.Tiles.Single(tile => tile.Name == original.Name).IsMaterialized, "Partial processing made the whole run look completed.");
        WriteCompletion(true);
        Wait(wizard.LoadAsync(false));
        saved = wizard.Tiles.Single(tile => tile.Name == original.Name);
        Check(saved.IsMaterialized, "Completed processing did not materialize the run tile.");
        Layout(view);
        var completedButton = Descendants<Button>(selectionView).Single(button => ReferenceEquals(button.DataContext, saved));
        Check(completedButton.BorderThickness.Left == 1, "Completed run incorrectly received a selection-like border.");
        Check(!Descendants<TextBlock>(completedButton).Any(text => text.Text == saved.Status && text.Visibility == Visibility.Visible), "Run status text remains visible.");
        var completionScreens = Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "run-selection");
        Directory.CreateDirectory(completionScreens);
        Render(view, Path.Combine(completionScreens, "completed.png"));
        Wait(model.ChooseRunAsync(saved));
        Check(!model.HasUnsavedChanges && model.RunName == codeRunName, "Saved leaf did not load cleanly.");
        Check(model.Description == "Saved values take precedence over code defaults", "Code defaults replaced the saved counterpart.");
        Wait(model.BeginSelectionAsync());
        wizard.Navigate(wizard.Tiles.Single(tile => tile.CreateNew));
        var template = wizard.Tiles.Single(tile => tile.Name == original.Name);
        Check(template.CreateNew && template.Definition is not null, "Create leaf did not expose templates.");
        Wait(model.ChooseRunAsync(template));
        model.RunName = "WizardCustom";
        model.ProjectName = "MyCustomProject";
        model.DatabaseName = "MyCustomDatabase";
        var custom = model.Validate() ?? throw new Exception(string.Join("; ", model.Issues));
        Wait(model.SaveAsync(custom, false));
        Check(File.Exists(RunDefinitionFiles.pathFor(root, custom) + ".module.json"), "Custom leaf did not retain its module.");
        Wait(model.OpenAsync(RunDefinitionFiles.pathFor(root, custom)));
        Check(model.SelectedRunType?.Name == original.ModulePath, "Opening a saved file directly lost its recorded module.");
        Wait(model.BeginSelectionAsync());
        var customTile = wizard.Tiles.Single(tile => tile.Name == "WizardCustom");
        Check(!customTile.IsCodeDefined && customTile.SavedPath is not null && customTile.ModulePath == original.ModulePath, "Renamed custom leaf was not restored to its source module.");
        Wait(model.ChooseRunAsync(customTile));
        Check(model.SelectedRunType?.Name == original.ModulePath, "Custom leaf was associated with an unrelated compatible module.");
        model.Description = "Keep this draft";
        wizard.Home(); // The editor must return to its module, not this stale wizard location.
        Layout(view);
        Click(Descendants<Button>(view).Single(button => Equals(button.Content, "Back") && button.ToolTip is string tip && tip.StartsWith("Back to")));
        WaitUntil(() => model.IsSelectingRun && !wizard.IsLoading);
        Check(wizard.CurrentPath == original.ModulePath, "Back did not return to the containing module.");
        model.CancelSelection();
        Check(model.Description == "Keep this draft" && model.HasUnsavedChanges, "Returning from the wizard discarded edits.");
        Wait(model.BeginSelectionAsync());
        // Corrupt definitions should not hide healthy runs or abort the wizard.
        var corruptFolder = Path.Combine(root, "Broken", "Database", "Data", "Project", "Run", "Broken");
        Directory.CreateDirectory(corruptFolder);
        File.WriteAllText(Path.Combine(corruptFolder, "Run_Broken.msgpack"), "invalid data");
        Wait(wizard.LoadAsync(false));
        Check(wizard.Message.Contains("Run_Broken.msgpack") && wizard.Tiles.Any(tile => tile.Name == "WizardCustom"), "Corrupt file handling hid healthy leaves.");
        var otherRoot = Path.Combine(root, "OtherProjects");
        Directory.CreateDirectory(otherRoot);
        wizard.ChangeFolder();
        wizard.Folder = otherRoot;
        Wait(wizard.LoadAsync());
        NavigateTo(wizard, original.ModulePath);
        Check(!wizard.Tiles.Any(tile => tile.Name == "WizardCustom") && wizard.Tiles.Where(tile => tile.Definition is not null).All(tile => tile.SavedPath is null), "Switching folders retained saved leaves from the old folder.");
        wizard.Back();
        Check(wizard.CurrentPath == "SortableTests", "Back skipped a module layer.");
        wizard.Home();
        NavigateTo(wizard, "SorterSgd.p32.Msuf4a.SimpleR.Soss");
        Check(wizard.Tiles.Any(tile => tile.Name == "Test") && wizard.Tiles.Any(tile => tile.Name == "ActivatorRates"), "Deep module lost its leaves.");
        Layout(view);
        Check(Descendants<Button>(selectionView).Any(button => button.DataContext is RunSelectionTile { Name: "Test" }), "Leaf tiles were not rendered.");
        Click(Descendants<Button>(selectionView).Single(button => button.DataContext is RunBreadcrumb { Name: "p32" }));
        Check(wizard.CurrentPath == "SorterSgd.p32" && wizard.Tiles.Any(tile => tile.Name == "Msuf4a"), "Ancestor breadcrumb did not show that module's children.");
        Layout(view);
        Click(Descendants<Button>(selectionView).Single(button => button.DataContext is RunBreadcrumb { IsRoot: true }));
        Check(wizard.CurrentPath == "" && wizard.Tiles.Any(tile => tile.Name == "SorterSgd"), "Root breadcrumb did not return to all modules.");
        NavigateTo(wizard, "SorterSgd.p32.Msuf4a.SimpleR.Soss");
        Layout(view);
        var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "run-selection");
        Directory.CreateDirectory(screenshotDir);
        Render(view, Path.Combine(screenshotDir, "leaves.png"));
        view.Width = 680; Layout(view); Render(view, Path.Combine(screenshotDir, "leaves-narrow.png"));
        view.Width = 1060;
        wizard.Home(); Layout(view); Render(view, Path.Combine(screenshotDir, "modules.png"));
        wizard.ChangeFolder(); Layout(view); Render(view, Path.Combine(screenshotDir, "folder.png"));
        Check(errors.Length == 0, "WPF binding errors: " + errors);
    }

    private static void NavigateTo(RunSelectionViewModel wizard, string path)
    {
        wizard.Home();
        foreach (var segment in path.Split('.'))
            Check(wizard.Navigate(wizard.Tiles.Single(tile => tile.IsModule && tile.Name == segment)), "Could not navigate to " + segment);
    }
}
