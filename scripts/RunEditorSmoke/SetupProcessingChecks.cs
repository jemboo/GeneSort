using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using GeneSort.FileDb.V1;
using GeneSort.Project.V1;
using GeneSort.Project.V1.Parameters;
using GeneSort.WPF.ViewModels;
using GeneSort.WPF.Views;

internal static partial class Program
{
    private static void SelectEditorTab(RunEditorView view, string header)
    {
        var tabs = Descendants<TabControl>(view).Single();
        tabs.SelectedItem = tabs.Items.Cast<TabItem>().Single(tab => Equals(tab.Header, header));
        Layout(view);
    }

    private static void CheckSetupProcessing(string root, RunEditorViewModel model, RunEditorView view, StringBuilder bindingErrors)
    {
        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SorterSgd.p32.Msuf4a.SimpleS.Soss"));
        model.SelectExample(model.Examples.Single(example => example.Name == "SossS"));
        model.ProjectsFolder = root;
        var configured = model.Validate() ?? throw new Exception(string.Join("; ", model.Issues));
        var savedRows = RunParameterGeneration.generate(configured, 0, 2);
        var runs = new RunParameterListViewModel();
        runs.SetParameters(savedRows, configured);
        var setups = new SetupParameterListViewModel();
        setups.SetRuns(configured, runs.Items.Cast<RunParameterItem>());
        Check(setups.Count * 2 == savedRows.Length, "SossS did not group the two activator mutation rates by shared inputs.");
        foreach (var item in setups.Items.Cast<SetupParameterItem>())
        {
            Check(item.Runs.Length == 2 && item.Values["LinkedRuns"] == "2", "A shared setup has an incorrect linked run count.");
            Check(item.Runs.Select(row => row.Values[ParamKeys.activatorMutationRateKey]).Order().SequenceEqual(new[] { "0", "0.01" }),
                "A setup grouped different inputs instead of the two mutation rates.");
            Check(!item.Values.ContainsKey(ParamKeys.activatorMutationRateKey), "An execution mutation rate was displayed as a Setup input.");
        }
        setups.Select([setups.Items.Cast<SetupParameterItem>().First()]);
        var selected = setups.SelectedRuns.ToHashSet();
        setups.Sort(ParamKeys.replKey);
        setups.Sort(ParamKeys.replKey);
        Check(selected.SetEquals(setups.SelectedRuns), "Sorting changed the selected linked runs.");

        // Exercise real Setup and Execution with a tiny generated evaluation library.
        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SorterEval.Standard"));
        void Set(string key, string value)
        {
            var row = model.SetupParameters.Single(row => row.Key == key);
            while (row.Values.Count > 1) row.Values.RemoveAt(row.Values.Count - 1);
            row.Values[0].Text = value;
        }
        Set(ParamKeys.sortingWidthKey, "4");
        Set(ParamKeys.sorterCountKey, "2");
        Set(ParamKeys.simpleSorterModelTypeKey, "Msce");
        var evalType = model.ExecutionParameters.Single(row => row.Key == ParamKeys.sorterEvalTypeKey);
        evalType.AddValue();
        evalType.Values[1].Text = "V1";
        model.RunName = "SharedSetup";
        model.ReplicaCount = "1";
        var preparedTask = model.PrepareGenerationAsync(); Wait(preparedTask);
        var prepared = preparedTask.Result ?? throw new Exception(string.Join("; ", model.Issues));
        Wait(model.GenerateParametersAsync(prepared, false));
        Check(model.RunParameterList.Count == 2 && model.SetupParameterList.Count == 1, "Evaluation overrides did not share their setup.");
        var originalRows = model.RunParameterList.Items.Cast<RunParameterItem>().ToArray();
        var ids = originalRows.Select(row => row.Parameters.GetId()).ToArray();
        SelectEditorTab(view, "Setup parameters");
        Check(!Descendants<Button>(view).Any(button => Equals(button.Content, "Run Execution")), "Execution is offered on the Setup tab.");
        var setupList = Descendants<ListView>(view).Single(control => AutomationProperties.GetName(control) == "Shared SetupParameters");
        setupList.SelectedItems.Add(setupList.Items[0]); Pump();
        Check(model.CanRunSetup && !model.CanRunExecution && model.SetupParameterList.SelectedRuns.Length == 2,
            "Setup selection enabled Execution or omitted linked runs.");
        SelectEditorTab(view, "Run parameters");
        Check(model.CanRunSetup && model.SetupParameterList.SelectedCount == 1, "Switching tabs discarded the Setup selection.");
        SelectEditorTab(view, "Setup parameters");
        Click(Button(view, "Run Setup")); WaitUntil(() => !model.IsBusy); Layout(view);
        Check(model.PhaseSummary == "Setup finished: 2 succeeded, 0 failed, 0 skipped." && model.SetupParameterList.SelectedCount == 0,
            "Setup failed to prepare both linked runs: " + model.ErrorMessages);
        Check(originalRows.SequenceEqual(model.RunParameterList.Items.Cast<RunParameterItem>())
            && ids.SequenceEqual(originalRows.Select(row => row.Parameters.GetId())), "Setup replaced live run items or changed their IDs.");
        SelectEditorTab(view, "Run parameters");
        Check(!Descendants<Button>(view).Any(button => Equals(button.Content, "Run Setup")), "Setup is offered on the Run tab.");
        var runList = Descendants<ListView>(view).Single(control => AutomationProperties.GetName(control) == "Saved RunParameters");
        runList.SelectedItems.Add(runList.Items[0]); Pump();
        Check(model.CanRunExecution && !model.CanRunSetup, "Run selection changed Setup selection.");
        Click(Button(view, "Run Execution")); WaitUntil(() => !model.IsBusy); Layout(view);
        Check(model.PhaseSummary == "Execution finished: 1 succeeded, 0 failed, 0 skipped.", "Execution failed after shared Setup: " + model.ErrorMessages);
        var savedTask = RunParameterFiles.loadAllAsync(root, model.Build().ResultValue); Wait(savedTask);
        Check(savedTask.Result.Count(row => row.GetRunFinished()?.Value == true) == 1, "Execution processed unselected linked runs.");

        SelectEditorTab(view, "Setup parameters");
        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SortableTests.Prefix")); Layout(view);
        var tabs = Descendants<TabControl>(view).Single();
        Check(tabs.Items.Cast<TabItem>().Single(tab => Equals(tab.Header, "Setup parameters")).Visibility == Visibility.Collapsed
            && Equals(((TabItem)tabs.SelectedItem).Header, "Run parameters"), "A direct run retained a selected Setup tab.");
        Check(model.SetupParameterList.Count == 0 && model.RunParameterList.Count == 0 && !model.CanRunSetup, "Changing run types retained shared setups.");
        Check(bindingErrors.Length == 0, "Processing tab binding errors: " + bindingErrors);
    }
}
