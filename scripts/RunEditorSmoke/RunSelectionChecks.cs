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
        wizard.Navigate(wizard.Tiles.Single(tile => tile.Name == "SorterEval"));
        wizard.Navigate(wizard.Tiles.Single(tile => tile.Name == "Standard"));
        var channelCounts = wizard.Tiles.Where(tile => tile.IsModule).Select(tile => int.Parse(tile.Name["Channels".Length..])).ToArray();
        Check(channelCounts.SequenceEqual(channelCounts.Order()), "Channel modules are not sorted numerically.");
        Check(Array.IndexOf(channelCounts, 8) < Array.IndexOf(channelCounts, 16), "Channels8 must precede Channels16.");
        wizard.JumpTo(wizard.Breadcrumbs[0]);
        Layout(view);
        var rootTile = Descendants<Button>(selectionView).Single(button => button.DataContext is RunSelectionTile { Name: "SortableTests" });
        Click(rootTile);
        Check(wizard.CurrentPath == "SortableTests", "Tile click did not navigate.");
        wizard.Navigate(wizard.Tiles.Single(tile => tile.Name == "Prefix"));
        Check(wizard.Tiles.All(tile => tile.IsModule), "Mixed Prefix module still contains run leaves.");
        wizard.Navigate(wizard.Tiles.Single(tile => tile.Name == "Channels24"));
        Check(wizard.ModuleDescription == model.RunTypes.Single(type => type.Name == wizard.CurrentPath).Description,
            "The module page lost its description.");
        Layout(view);
        Check(Descendants<TextBlock>(selectionView).Any(text => text.Text == wizard.ModuleDescription), "Module description is not visible on the page.");
        var original = wizard.Tiles.First(tile => tile.Definition is not null);
        Check(original.IsCodeDefined && original.SavedPath is null && original.Status.Contains("not saved"), "Unsaved code leaf status is incorrect.");
        var codeRunName = original.Definition!.RunName;
        Wait(model.ChooseRunAsync(original));
        Check(!model.IsSelectingRun && model.RunName == codeRunName, "Code selection must retain the actual run name rather than NewRun.");
        Check(model.ProjectsFolder == root && !model.IsDirty && model.DefinitionReadOnly, "Code selection lost its folder or read-only state.");
        Check(model.ModulePropertiesLocked, "Code module properties are editable.");
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
        Check(model.Description == original.Definition!.Description, "Code defaults replaced the saved counterpart.");
        Wait(model.BeginSelectionAsync());
        wizard.Navigate(wizard.Tiles.Single(tile => tile.CreateNew));
        var template = wizard.Tiles.Single(tile => tile.Name == original.Name);
        Check(template.CreateNew && template.Definition is not null, "Create leaf did not expose templates.");
        Wait(model.ChooseRunAsync(template));
        model.RunName = "WizardCustom";
        Check(model.ProjectName == original.Definition!.ProjectName && model.DatabaseName == original.Definition.DatabaseName, "New leaf left its module database.");
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
        model.Clone();
        model.RunName = "KeepThisDraft";
        wizard.Home(); // The editor must return to its module, not this stale wizard location.
        Layout(view);
        Click(Descendants<Button>(view).Single(button => Equals(button.Content, "Back") && button.ToolTip is string tip && tip.StartsWith("Back to")));
        WaitUntil(() => model.IsSelectingRun && !wizard.IsLoading);
        Check(wizard.CurrentPath == original.ModulePath, "Back did not return to the containing module.");
        model.CancelSelection();
        Check(model.RunName == "KeepThisDraft" && model.HasUnsavedChanges, "Returning from the wizard discarded edits.");
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
        Check(wizard.CurrentPath == "SortableTests.Prefix", "Back skipped a module layer.");
        wizard.Home();
        NavigateTo(wizard, "SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleR.Soss");
        Check(wizard.Tiles.Any(tile => tile.Name == "Test") && wizard.Tiles.Any(tile => tile.Name == "ActivatorRates"), "Deep module lost its leaves.");
        Layout(view);
        Check(Descendants<Button>(selectionView).Any(button => button.DataContext is RunSelectionTile { Name: "Test" }), "Leaf tiles were not rendered.");
        Click(Descendants<Button>(selectionView).Single(button => button.DataContext is RunBreadcrumb { Name: "cc32" }));
        Check(wizard.CurrentPath == "SorterSgd.cc32" && wizard.Tiles.Any(tile => tile.Name == "p4a"), "Ancestor breadcrumb did not show that module's children.");
        Layout(view);
        Click(Descendants<Button>(selectionView).Single(button => button.DataContext is RunBreadcrumb { IsRoot: true }));
        Check(wizard.CurrentPath == "" && wizard.Tiles.Any(tile => tile.Name == "SorterSgd"), "Root breadcrumb did not return to all modules.");
        NavigateTo(wizard, "SorterSgd.cc32.p4a.Msuf4.SimpleR.SimpleR.Soss");
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
