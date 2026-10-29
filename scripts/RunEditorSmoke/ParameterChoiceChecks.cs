using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GeneSort.Project.V1.Parameters;
using GeneSort.Project.V1;
using GeneSort.FileDb.V1;
using GeneSort.Project.V1.Editing;
using GeneSort.Project.V1.Catalogs;
using GeneSort.Project.V1.Catalogs.Queries;
using GeneSort.SortingLib.Sorter;
using Microsoft.FSharp.Collections;
using GeneSort.WPF.ViewModels;
using GeneSort.WPF.Views;

internal static partial class Program
{
    private static void CheckParameterChoices(string root, RunEditorViewModel model, RunEditorView view, StringBuilder bindingErrors)
    {
        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SortableTests.Merge"));
        var row = model.ExecutionParameters.Single(row => row.Key == ParamKeys.mergeLibIdKey);
        var original = row.Snapshot().Values;
        Layout(view);
        var section = Section(view, model.ExecutionSection);
        var combos = Descendants<ComboBox>(section).Where(box => AutomationProperties.GetName(box) == "Merge library value").ToArray();
        Check(combos.Length == row.Values.Count, "Merge IDs are not dropdowns.");
        Check(combos.All(combo => combo.ActualWidth > 300), "Merge ID control remains narrow.");
        Check(row.Values.All(value => value.Choices.All(choice => choice == value.Text || !row.Values.Any(other => other.Text == choice))),
            "Merge dropdown offers an already selected ID.");
        var first = row.Values[0];
        var replacement = first.Choices.First(choice => row.Values.All(other => other.Text != choice));
        combos[0].SetCurrentValue(Selector.SelectedItemProperty, replacement);
        Pump();
        Check(first.Text == replacement && !row.HasErrors, "Selecting a merge ID did not update the run.");
        Check(row.Values.Skip(1).All(value => !value.Choices.Contains(replacement) && value.Choices.Contains(original[0])),
            "Dropdowns did not refresh after changing another selection.");
        row.AddValue(); Layout(view);
        Check(row.Snapshot().Values.Distinct().Count() == row.Values.Count, "Add value duplicated a merge selection.");
        var removed = row.Values[1].Text;
        row.Values.RemoveAt(1); Layout(view);
        Check(row.Values.All(value => value.Choices.Contains(removed)), "Removing a value did not make it selectable again.");
        while (row.CanAddValue) row.AddValue();
        var total = row.Values.Count;
        row.AddValue();
        Check(row.Values.Count == total && !row.HasErrors, "An exhausted choice list can add duplicates.");
        row.Values[1].Text = row.Values[0].Text;
        Check(row.HasErrors && !row.Values[0].IsValid && !row.Values[1].IsValid && model.Validate() is null,
            "Imported/programmatic duplicate merge IDs were accepted.");
        row.Values.RemoveAt(1);
        Check(row.Values[0].IsValid, "Duplicate error did not clear after removal.");
        view.Width = 760; Layout(view);
        foreach (var combo in Descendants<ComboBox>(section).Where(box => AutomationProperties.GetName(box) == "Merge library value"))
        {
            var bounds = combo.TransformToAncestor(section).TransformBounds(new Rect(combo.RenderSize));
            Check(bounds.Right <= section.ActualWidth && combo.ActualWidth > 150, "Merge dropdown is clipped at a narrower editor width.");
        }
        view.Width = 1060;
        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SortableTests.Prefix"));
        model.SelectExample(model.Examples.Single(example => example.Name == "Channels24"));
        CheckLibrarySelection(model, view, ParamKeys.prefixLibIdKey, "Prefix library value");
        CheckPrefixChannelCountSelection(root, model, view);
        CheckPreparedPrefixChannelCountSelection(root, model, view, "SorterEval.Prefix", "Channels32");
        foreach (var example in new[] { "Msce", "Mssi", "Msrs", "Msuf4" })
            CheckPreparedPrefixChannelCountSelection(root, model, view, "SorterMutate.Prefix", example);
        CheckMergeChannelCountSelection(root, model, view);

        // SorterLibId is supported by saved definitions even though current Dispatch examples
        // do not declare it. Open such a definition through the actual editor boundary.
        model.New();
        var source = model.Validate()!;
        var sorterSpan = Tuple.Create(ParamKeys.sorterLibIdKey,
            ListModule.OfArray(new[] { "SorterLibId(channelCount=32, Variant=VariantA)" }));
        var configured = Run.ofSimple(SimpleRun.create(new simpleRunConfig(
            source.DatabaseName, source.ProjectName, "SorterLibraryEditor", source.Description, false,
            new executionConfig(source.ExecuteFunctionName, source.RunParameterSpans, source.RunQueryCatalogName, source.RunEnhancerFunctionName),
            new preparedSetupConfig(source.SetupFunctionName.Value, ListModule.OfSeq(source.SetupParameterSpans.Append(sorterSpan)),
                source.SetupQueryCatalogName, source.SetupEnhancerFunctionName))));
        var save = RunDefinitionFiles.saveAsync(root, configured, false);
        Wait(save);
        Wait(model.OpenAsync(save.Result));
        CheckLibrarySelection(model, view, ParamKeys.sorterLibIdKey, "Sorter library value");

        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SorterSgd.p32.Msuf4a.SimpleS.Soss"));
        model.SelectExample(model.Examples.Single(example => example.Name == "SossS"));
        var rates = model.ExecutionParameters.Single(row => row.Key == ParamKeys.activatorMutationRateKey);
        rates.Values[0].Text = "0.010";
        Check(rates.HasErrors && rates.Values.All(value => !value.IsValid) && model.Validate() is null,
            "Numerically equivalent rates were accepted.");
        rates.Values[0].Text = "0.02";
        Check(!rates.HasErrors && rates.Values.All(value => value.IsValid), "Corrected values retained duplicate errors.");
        var labels = model.SetupParameters.Concat(model.ExecutionParameters).First(row => row.Key == ParamKeys.codeModKey);
        labels.AddValue(); labels.Values[1].Text = labels.Values[0].Text;
        Check(labels.HasErrors && model.Validate() is null, "Duplicate text values were accepted.");
        Layout(view);
        Check(bindingErrors.Length == 0, "Parameter editor binding errors: " + bindingErrors);
    }

    private static void CheckPrefixChannelCountSelection(string root, RunEditorViewModel model, RunEditorView view)
    {
        model.SelectExample(model.Examples.Single(example => example.Name == "Channels24"));
        model.ProjectsFolder = root;
        Layout(view);
        var widthPicker = Descendants<ComboBox>(view).Single(box => AutomationProperties.GetName(box) == "Prefix channel count");
        Check(model.HasPrefixChannelCountSelection && model.SelectedPrefixChannelCount == 24 && Equals(widthPicker.SelectedItem, 24)
            && widthPicker.Items.Contains(32), "The width picker is restricted to the example width or has the wrong default.");
        var prefix24 = model.ExecutionParameters.Single(row => row.Key == ParamKeys.prefixLibIdKey).Values[0].Text;
        Check(model.Validate() is not null && model.CanSave, "The example is invalid before changing its width.");
        widthPicker.SetCurrentValue(Selector.SelectedItemProperty, 32);
        Layout(view);
        var row = model.ExecutionParameters.Single(row => row.Key == ParamKeys.prefixLibIdKey);
        Check(model.SelectedPrefixChannelCount == 32 && row.Values.Count == 1 && row.Values[0].Text == ""
            && model.IsDirty && !model.CanSave && model.Validate() is null,
            "Changing width retained incompatible prefixes or allowed saving before choosing new ones.");
        var expected32 = PrefixLib.ids().Where(id => id.ChannelCount == 32).Select(PrefixLibId.toString).ToArray();
        Check(row.Values[0].Choices.Where(choice => choice.Length > 0).SequenceEqual(expected32),
            "Prefix choices did not follow the selected channel count.");
        var prefixPicker = Descendants<ComboBox>(view).Single(box => box.DataContext == row.Values[0]);
        var fourStage32 = expected32.Single(value => PrefixLibId.fromString(value).StageLength == 4);
        prefixPicker.SetCurrentValue(Selector.SelectedItemProperty, fourStage32);
        Pump();
        Check(!row.HasErrors && row.CanAddValue, $"The second 32-channel prefix is unavailable: text={row.Values[0].Text}; error={row.Error}; choices={string.Join(";", row.Definition!.Choices)}; selected={prefixPicker.SelectedItem}.");
        row.Values[0].Text = prefix24;
        Check(model.Validate() is null, "A prefix from another width bypassed validation.");
        row.Values[0].Text = fourStage32;
        var configured = model.Validate() ?? throw new Exception(string.Join("; ", model.Issues));
        Check(model.Snapshot().ExecutionParameters.All(parameter => parameter.Key != ParamKeys.channelCountKey),
            "The width picker created a redundant sweep parameter.");
        ProjectCatalogs.registerAll();
        var expectedTemplate = model.Examples.Single(example => example.Name == "FourStage_32").Run;
        var expected = RunTypeEditor.build(expectedTemplate, RunTypeEditor.newDraft(expectedTemplate)).ResultValue;
        var actualRows = RunParameterGeneration.generate(configured, 0, 1);
        var expectedRows = RunParameterGeneration.generate(expected, 0, 1);
        Check(actualRows.Length == 1 && Equals(actualRows[0], expectedRows[0]),
            "Choosing width 32 changed generated parameter identities or added combinations.");
        var actualQuery = QueryParamsBuildersRun.SortableTests.Prefix.makeSortableTestsQueryParams(
            configured.ProjectName, configured.DatabaseName, actualRows[0], "").Value;
        var expectedQuery = QueryParamsBuildersRun.SortableTests.Prefix.makeSortableTestsQueryParams(
            expected.ProjectName, expected.DatabaseName, expectedRows[0], "").Value;
        Check(actualQuery.Id == expectedQuery.Id, "The width picker changed the existing result file ID.");
        model.RunName = "PrefixChannelCountSelection";
        configured = model.Validate()!;
        Wait(model.SaveAsync(configured, false));
        model.SelectExample(model.Examples.Single(example => example.Name == "Channels24"));
        Wait(model.OpenAsync(RunDefinitionFiles.pathFor(root, configured)));
        Layout(view);
        Check(model.SelectedPrefixChannelCount == 32 && Equals(configured, model.Validate()) && !model.HasUnsavedChanges,
            "Reopening a saved run lost its width, changed its values, or marked it edited.");
        // Removing and adding the parameter must also use the active width, not the example's defaults.
        row = model.ExecutionParameters.Single(row => row.Key == ParamKeys.prefixLibIdKey);
        model.ExecutionParameters.Remove(row);
        model.SelectedPrefixChannelCount = 24;
        model.ExecutionSection.BeginAdd();
        model.ExecutionSection.SelectedParameter = model.ExecutionSection.AvailableParameters.Single(spec => spec.Key == ParamKeys.prefixLibIdKey);
        var pending = model.ExecutionSection.PendingRow!;
        Check(pending.Values[0].Choices.Where(choice => choice.Length > 0).All(choice => PrefixLibId.fromString(choice).ChannelCount == 24),
            "Add parameter retained the old width's prefix choices.");
        Check(!model.CanSelectPrefixChannelCount, "Width changes can discard a pending parameter edit.");
        pending.Values[0].Text = prefix24;
        Check(model.ExecutionSection.Add(), "A matching prefix could not be added back.");
        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SortableTests.Merge"));
        Check(!model.HasPrefixChannelCountSelection, "The prefix width picker leaked into another workflow.");
    }

    private static void CheckPreparedPrefixChannelCountSelection(string root, RunEditorViewModel model, RunEditorView view, string typeName, string exampleName)
    {
        model.SelectRunType(model.RunTypes.Single(type => type.Name == typeName));
        model.SelectExample(model.Examples.Single(example => example.Name == exampleName));
        model.ProjectsFolder = root;
        Layout(view);
        var widthPicker = Descendants<ComboBox>(view).Single(box => AutomationProperties.GetName(box) == "Prefix channel count");
        var section = Section(view, model.SetupSection);
        Check(model.HasPrefixChannelCountSelection && model.SelectedPrefixChannelCount == 32 && Equals(widthPicker.SelectedItem, 32)
            && ((FrameworkElement)widthPicker.Parent).Visibility == Visibility.Visible, "Evaluation does not expose its Setup channel count.");
        var original = model.Validate()!;
        var originalRows = RunParameterGeneration.generate(original, 0, 1);
        var execution = model.Snapshot().ExecutionParameters;
        var prefix32 = model.SetupParameters.Single(row => row.Key == ParamKeys.prefixLibIdKey).Values[0].Text;
        widthPicker.SetCurrentValue(Selector.SelectedItemProperty, 24);
        Layout(view);
        CheckMutatorChannelCount(model, original, 24);
        var row = model.SetupParameters.Single(row => row.Key == ParamKeys.prefixLibIdKey);
        Check(row.Values.Count == 1 && row.Values[0].Text == "" && !model.CanSave && model.Validate() is null,
            "Evaluation retained a width-32 prefix or allowed saving after switching to width 24.");
        Check(!model.ExecutionSection.InheritedRows.Single(row => row.Key == ParamKeys.prefixLibIdKey).DisplayValues.Contains(prefix32),
            "Execution still displays the incompatible Setup prefix.");
        var expected24 = RunTypeEditor.parametersForPrefixChannelCount(GeneSort.Dispatch.V1.SortableTests.Prefix.Channels24(), 24)
            .Single(spec => spec.Key == ParamKeys.prefixLibIdKey).Choices;
        Check(row.Values[0].Choices.SequenceEqual(expected24), "Evaluation and SortableTests offer different width-24 prefixes.");
        var prefixPicker = Descendants<ComboBox>(section).Single(box => box.DataContext == row.Values[0]);
        Check(prefixPicker.ActualWidth > 300, "Evaluation's PrefixLibId control is too narrow.");
        prefixPicker.SetCurrentValue(Selector.SelectedItemProperty, expected24[0]);
        Pump();
        row.AddValue();
        Check(row.Values.Count == 2 && row.Snapshot().Values.Distinct().Count() == 2
            && row.Values.All(value => value.Choices.All(choice => choice == value.Text || row.Values.All(other => other.Text != choice))),
            "Evaluation offers duplicate prefix selections.");
        var inherited = model.ExecutionSection.InheritedRows.Single(row => row.Key == ParamKeys.prefixLibIdKey);
        Check(inherited.IsReadOnly && inherited.Snapshot().Values.SequenceEqual(row.Snapshot().Values)
            && model.ExecutionSection.AvailableParameters.All(spec => spec.Key != ParamKeys.prefixLibIdKey)
            && execution.SequenceEqual(model.Snapshot().ExecutionParameters),
            "The selected Setup prefixes were not inherited read-only or became extra Execution dimensions.");
        var chosen = row.Snapshot().Values;
        row.Values[0].Text = prefix32;
        Check(model.Validate() is null && model.Issues.Any(issue => issue.StartsWith("Setup.PrefixLibId:")),
            "A mismatched Setup prefix bypassed evaluation width validation.");
        row.Values[0].Text = chosen[0];
        var configured = model.Validate() ?? throw new Exception(string.Join("; ", model.Issues));
        var expectedSetup = ListModule.OfSeq(original.SetupParameterSpans.Select(span => span.Item1 == ParamKeys.prefixLibIdKey
            ? Tuple.Create(span.Item1, ListModule.OfArray(chosen)) : MutatorSpanAtWidth(span, 24)));
        var expected = Run.withParameterSpans(expectedSetup, original.RunParameterSpans, original);
        var actualRows = RunParameterGeneration.generate(configured, 0, 1);
        Check(actualRows.Length == originalRows.Length * chosen.Length && actualRows.SequenceEqual(RunParameterGeneration.generate(expected, 0, 1)),
            "Evaluation width selection changed existing file identities or added sweep dimensions.");
        Check(model.Snapshot().SetupParameters.All(parameter => parameter.Key != ParamKeys.channelCountKey),
            "The evaluation picker added a redundant ChannelCount parameter.");
        model.RunName = typeName + exampleName + "WidthSelection";
        configured = model.Validate()!;
        Wait(model.SaveAsync(configured, false));
        model.SelectExample(model.Examples.Single(example => example.Name == exampleName));
        Wait(model.OpenAsync(RunDefinitionFiles.pathFor(root, configured)));
        Layout(view);
        Check(model.SelectedPrefixChannelCount == 24 && Equals(configured, model.Validate()) && !model.HasUnsavedChanges,
            "Reopening an evaluation run lost its Setup width or changed its saved configuration.");
        Check(model.ExecutionSection.InheritedRows.Single(row => row.Key == ParamKeys.prefixLibIdKey).Snapshot().Values.SequenceEqual(chosen),
            "Reopening lost the read-only Execution prefix values.");
        // Changing back to the original values must restore the original generated identities.
        model.SelectedPrefixChannelCount = 32;
        row = model.SetupParameters.Single(row => row.Key == ParamKeys.prefixLibIdKey);
        row.Values[0].Text = prefix32;
        model.RunName = original.RunName;
        Check(originalRows.SequenceEqual(RunParameterGeneration.generate(model.Validate()!, 0, 1)),
            "Returning to width 32 changed the original Setup references or run parameters.");
        // The Setup add panel must follow the selector too.
        model.SetupParameters.Remove(row);
        model.SelectedPrefixChannelCount = 24;
        model.SetupSection.BeginAdd();
        model.SetupSection.SelectedParameter = model.SetupSection.AvailableParameters.Single(spec => spec.Key == ParamKeys.prefixLibIdKey);
        var pending = model.SetupSection.PendingRow!;
        Check(pending.Values[0].Choices.SequenceEqual(expected24) && !model.CanSelectPrefixChannelCount,
            "The Setup add panel retained the old width or permits losing a pending edit.");
        pending.Values[0].Text = chosen[0];
        Check(model.SetupSection.Add() && model.Validate() is not null, "A matching Setup prefix could not be added back.");
        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SortableTests.Merge"));
    }

    private static void CheckLibrarySelection(RunEditorViewModel model, RunEditorView view, string key, string controlName)
    {
        var row = model.ExecutionParameters.Single(row => row.Key == key);
        while (row.Values.Count > 1) row.Values.RemoveAt(row.Values.Count - 1);
        Layout(view);
        var section = Section(view, model.ExecutionSection);
        var combo = Descendants<ComboBox>(section).Single(box => box.DataContext == row.Values[0]);
        Check(AutomationProperties.GetName(combo) == controlName && combo.ActualWidth > 300, $"{key} is not a wide dropdown.");
        var replacement = row.Values[0].Choices.First(choice => choice != row.Values[0].Text);
        combo.SetCurrentValue(Selector.SelectedItemProperty, replacement);
        Pump();
        Check(row.Values[0].Text == replacement && !row.HasErrors, $"{key} selection failed.");
        while (row.CanAddValue) row.AddValue();
        Layout(view);
        Check(row.Values.Count == row.Snapshot().Values.Distinct().Count(), $"{key} added duplicates.");
        Check(row.Values.All(value => value.Choices.Count == 1 && value.Choices[0] == value.Text), $"{key} offers selected values again.");
        var removed = row.Values[1].Text;
        row.Values.RemoveAt(1); Layout(view);
        Check(row.Values.All(value => value.Choices.Contains(removed)), $"{key} removal did not restore its choice.");
        row.AddValue();
        row.Values[1].Text = row.Values[0].Text;
        Check(row.HasErrors && model.Validate() is null, $"Duplicate {key} values are accepted.");
        row.Values.RemoveAt(1);
        Check(!row.HasErrors, $"{key} retains stale duplicate errors.");
        view.Width = 760; Layout(view);
        foreach (var picker in Descendants<ComboBox>(section).Where(box => AutomationProperties.GetName(box) == controlName))
        {
            var bounds = picker.TransformToAncestor(section).TransformBounds(new Rect(picker.RenderSize));
            Check(bounds.Right <= section.ActualWidth && picker.ActualWidth > 150, $"{key} is clipped in a narrow editor.");
        }
        view.Width = 1060;
    }
}
