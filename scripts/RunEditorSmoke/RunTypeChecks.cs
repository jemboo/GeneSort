using System.Text;
using System.Windows.Automation;
using System.Windows.Controls;
using GeneSort.FileDb.V1;
using GeneSort.Project.V1.Editing;
using GeneSort.Project.V1.Parameters;
using GeneSort.WPF.ViewModels;
using GeneSort.WPF.Views;

internal static partial class Program
{
    private static void CheckRunTypes(string root, RunEditorViewModel model, RunEditorView view, StringBuilder bindingErrors)
    {
        var typePicker = Descendants<ComboBox>(view).Single(box => AutomationProperties.GetName(box) == "Run type");
        var examplePicker = Descendants<ComboBox>(view).Single(box => AutomationProperties.GetName(box) == "Example run");
        Check(typePicker.Items.Count == model.RunTypes.Length, "Run type picker did not populate.");
        typePicker.SetCurrentValue(System.Windows.Controls.Primitives.Selector.SelectedItemProperty,
            model.RunTypes.Single(type => type.Name == "SortableTests.Prefix"));
        Layout(view);
        Check(model.SelectedRunType?.Name == "SortableTests.Prefix", "Picking a run type did not update the editor.");
        model.New();
        examplePicker.SetCurrentValue(System.Windows.Controls.Primitives.Selector.SelectedItemProperty, model.Examples.Last());
        Layout(view);
        Check(ReferenceEquals(model.SelectedExample, model.Examples.Last()), "Picking an example did not update the editor.");
        model.ProjectsFolder = root;
        var count = 0;
        foreach (var kind in model.RunTypes)
        {
            model.SelectRunType(kind);
            foreach (var example in kind.Examples)
            {
                model.SelectExample(example);
                Layout(view);
                Check(ReferenceEquals(typePicker.SelectedItem, kind), "Run type selection binding is stale.");
                Check(ReferenceEquals(examplePicker.SelectedItem, example), "Example selection binding is stale.");
                var built = model.Validate();
                Check(built is not null, $"Invalid {kind.Name}.{example.Name}: {string.Join("; ", model.Issues)}");
                Check(RunTypeEditor.matches(example.Run, built!), "Catalog selections changed.");
                Check(model.IsEvolution == example.Run.IsEvolution, "Evolution controls use the wrong run type.");
                Check(model.HasSetup == !example.Run.IsDirectRun, "Setup controls use the wrong workflow.");
                Check(Section(view, model.SetupSection).Visibility == (model.HasSetup ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed), "Setup editor visibility is incorrect.");
                Check(model.SetupParameters.Concat(model.ExecutionParameters).All(row => row.Definition is not null), "Parameter metadata was not refreshed.");
                count++;
            }
        }
        foreach (var name in new[] { "SortableTests.Prefix", "SorterSgd.p32.Msuf4a.SimpleS.Soss", "SorterSgd.p32.Msuf4a.Standard_append" })
        {
            var kind = model.RunTypes.Single(type => type.Name == name);
            model.SelectRunType(kind);
            model.RunName = "EditorRoundTrip";
            if (model.HasSgdSourceSelection)
            {
                Layout(view);
                var testsDatabase = Descendants<TextBox>(view).Single(box => AutomationProperties.GetName(box) == "SortableTests source database");
                var evalsDatabase = Descendants<TextBox>(view).Single(box => AutomationProperties.GetName(box) == "SorterSetEvals source database");
                testsDatabase.SetCurrentValue(TextBox.TextProperty, "MyTests");
                evalsDatabase.SetCurrentValue(TextBox.TextProperty, "MySeedEvals");
                Pump();
                Check(model.SortableTestsDatabaseName == "MyTests" && model.SorterSetEvalsDatabaseName == "MySeedEvals", "Source database fields are not bound.");
                Check(model.Catalogs.Any(choice => choice.Role == "Setup.FunctionName"), "SGD setup function is not displayed.");
            }
            var original = model.Validate()!;
            var save = model.SaveAsync(original, false);
            while (!save.IsCompleted) Pump();
            save.GetAwaiter().GetResult();
            var open = model.OpenAsync(RunDefinitionFiles.pathFor(root, original));
            while (!open.IsCompleted) Pump();
            open.GetAwaiter().GetResult();
            Check(Equals(original, model.Validate()), $"Saved {name} changed on reopening.");
        }
        var soss = model.RunTypes.Single(type => type.Name == "SorterSgd.p32.Msuf4a.SimpleS.Soss");
        model.SelectRunType(soss);
        model.SelectExample(soss.Examples.Single(example => example.Name == "SossS"));
        var rate = model.ExecutionParameters.Single(row => row.Key == ParamKeys.activatorMutationRateKey);
        Check(!rate.IsReadOnly, "Activator mutation rate must remain editable in evolution.");
        var inherited = model.ExecutionSection.InheritedRows;
        Check(inherited.All(row => row.IsReadOnly), "Inherited settings are editable.");
        Check(inherited.Any(row => row.Key == ParamKeys.rngTypeKey), "Enhancer defaults are absent from inherited settings.");
        Check(model.ExecutionSection.AvailableParameters.All(spec => spec.Key != ParamKeys.activationProbabilityKey), "Fixed Setup value can be added as an override.");
        var activation = model.SetupParameters.Single(row => row.Key == ParamKeys.activationProbabilityKey);
        activation.Values[0].Text = "0.75";
        Check(model.ExecutionSection.InheritedRows.Single(row => row.Key == ParamKeys.activationProbabilityKey).DisplayValues == "0.75", "Read-only value did not follow Setup edits.");
        Check(model.Snapshot().ExecutionParameters.All(row => row.Key != ParamKeys.activationProbabilityKey), "Inherited display row became an execution sweep dimension.");
        Check(rate.Values.Count == 2 && rate.Values[0].Kind == parameterValueKind.Number, "Activator sweep did not get numeric editors.");
        rate.Values[0].Text = "1.5";
        Check(model.Validate() is null, "Invalid activation probability accepted.");
        model.SetupSection.BeginAdd();
        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SortableTests.Merge"));
        Check(!model.HasPendingParameter && model.RunParameterList.Count == 0 && model.Issues.Count == 0, "Type change retained stale editor state.");
        Layout(view);
        Check(bindingErrors.Length == 0, "WPF binding errors: " + bindingErrors);
        Console.WriteLine($"Checked {model.RunTypes.Length} modules and {count} examples, dynamic bindings, validation, and saved-file round trips.");
    }
}
