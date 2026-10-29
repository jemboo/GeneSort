using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using GeneSort.FileDb.V1;
using GeneSort.Project.V1;
using GeneSort.Project.V1.Parameters;
using GeneSort.Project.V1.ParamBundles;
using GeneSort.SortingLib.Sorter;
using GeneSort.WPF.ViewModels;
using GeneSort.WPF.Views;
using Microsoft.FSharp.Collections;

internal static partial class Program
{
    private static void CheckMergeWidthSelection(string root, RunEditorViewModel model, RunEditorView view)
    {
        foreach (var (typeName, exampleName) in new[] { ("SortableTests.Merge", "Test"), ("SorterEval.Merge", "Test"),
            ("SorterMutate.Merge", "Msce"), ("SorterMutate.Merge", "Mssi"), ("SorterMutate.Merge", "Msrs"), ("SorterMutate.Merge", "Msuf4") })
        {
            model.SelectRunType(model.RunTypes.Single(type => type.Name == typeName));
            model.SelectExample(model.Examples.Single(example => example.Name == exampleName));
            model.ProjectsFolder = root;
            Layout(view);
            var section = model.HasSetup ? model.SetupSection : model.ExecutionSection;
            var sectionView = Section(view, section);
            var widthPicker = Descendants<ComboBox>(view).Single(box => AutomationProperties.GetName(box) == "Merge sorting width");
            Check(model.HasMergeWidthSelection && !model.HasPrefixWidthSelection && model.SelectedMergeSortingWidth == "32"
                && widthPicker.Text == "32" && widthPicker.Items.Contains("24")
                && ((FrameworkElement)widthPicker.Parent).Visibility == Visibility.Visible, $"{typeName} has no usable width selector.");
            var original = model.Validate()!;
            var originalRows = RunParameterGeneration.generate(original, 0, 1);
            var originalValues = section.Rows.Single(row => row.Key == ParamKeys.mergeLibIdKey).Snapshot().Values;
            var originalExecution = model.Snapshot().ExecutionParameters;
            widthPicker.SetCurrentValue(Selector.SelectedItemProperty, "24");
            Layout(view);
            CheckMutatorWidth(model, original, 24);
            var row = section.Rows.Single(row => row.Key == ParamKeys.mergeLibIdKey);
            Check(model.SelectedMergeSortingWidth == "24" && row.Values.Count == 1 && row.Values[0].Text == ""
                && model.IsDirty && !model.CanSave && model.Validate() is null,
                $"{typeName} retained incompatible merge selections: width={model.SelectedMergeSortingWidth}, values={row.DisplayValues}, selected={widthPicker.SelectedItem}, text={widthPicker.Text}.");
            var expected24 = MergeLibId.availableForWidth(24).Select(MergeLibId.toString).ToArray();
            Check(row.Values[0].Choices.SequenceEqual(expected24), $"{typeName} offers the wrong width-24 merges.");
            var libraryPicker = Descendants<ComboBox>(sectionView).Single(box => box.DataContext == row.Values[0]);
            Check(libraryPicker.ActualWidth > 300, $"{typeName} narrowed the MergeLibId control.");
            libraryPicker.SetCurrentValue(Selector.SelectedItemProperty, expected24[0]);
            Pump();
            row.AddValue();
            var chosen = row.Snapshot().Values;
            Check(chosen.Length == 2 && chosen.Distinct().Count() == 2
                && row.Values.All(value => value.Choices.All(choice => choice == value.Text || row.Values.All(other => other.Text != choice))),
                $"{typeName} offers duplicate merges.");
            row.Values[1].Text = chosen[0];
            Check(model.Validate() is null && row.HasErrors, $"{typeName} accepts duplicated merges.");
            row.Values[1].Text = chosen[1];
            row.Values[0].Text = originalValues[0];
            Check(model.Validate() is null && model.Issues.Any(issue => issue.Contains("Choose merges with sorting width 24")),
                $"{typeName} accepts a merge from another width.");
            row.Values[0].Text = chosen[0];
            if (model.HasSetup)
            {
                var inherited = model.ExecutionSection.InheritedRows.Single(row => row.Key == ParamKeys.mergeLibIdKey);
                Check(inherited.IsReadOnly && inherited.Snapshot().Values.SequenceEqual(chosen)
                    && originalExecution.SequenceEqual(model.Snapshot().ExecutionParameters)
                    && model.ExecutionSection.AvailableParameters.All(spec => spec.Key != ParamKeys.mergeLibIdKey),
                    "Merge evaluation did not inherit the selected Setup values read-only.");
            }
            var configured = model.Validate() ?? throw new Exception(string.Join("; ", model.Issues));
            var replacement = Tuple.Create(ParamKeys.mergeLibIdKey, ListModule.OfArray(chosen));
            var expected = Run.withParameterSpans(
                ListModule.OfSeq(original.SetupParameterSpans.Select(span => span.Item1 == ParamKeys.mergeLibIdKey ? replacement : MutatorSpanAtWidth(span, 24))),
                ListModule.OfSeq(original.RunParameterSpans.Select(span => span.Item1 == ParamKeys.mergeLibIdKey ? replacement : span)), original);
            Check(RunParameterGeneration.generate(configured, 0, 1).SequenceEqual(RunParameterGeneration.generate(expected, 0, 1))
                && model.Snapshot().SetupParameters.Concat(model.Snapshot().ExecutionParameters).All(parameter => parameter.Key != ParamKeys.sortingWidthKey),
                $"{typeName} width selection changed saved identities or added a sweep dimension.");
            model.RunName = typeName + exampleName + "WidthSelection";
            configured = model.Validate()!;
            Wait(model.SaveAsync(configured, false));
            model.SelectExample(model.Examples.Single(example => example.Name == exampleName));
            Wait(model.OpenAsync(RunDefinitionFiles.pathFor(root, configured)));
            Layout(view);
            Check(model.SelectedMergeSortingWidth == "24" && Equals(configured, model.Validate()) && !model.HasUnsavedChanges,
                $"{typeName} lost its width or changed its values on reopening.");
            // Wider merges use existing component networks and need not appear among the suggestions.
            widthPicker.SetCurrentValue(ComboBox.TextProperty, "96");
            Pump();
            CheckMutatorWidth(model, original, 96);
            row = section.Rows.Single(row => row.Key == ParamKeys.mergeLibIdKey);
            Check(model.SelectedMergeSortingWidth == "96" && row.Values[0].Choices.SequenceEqual(MergeLibId.availableForWidth(96).Select(MergeLibId.toString)),
                $"{typeName} does not filter a typed width larger than the library sorter widths.");
            row.Values[0].Text = row.Values[0].Choices[0];
            Check(model.Validate() is not null, $"{typeName} rejects a supported typed width.");
            foreach (var invalid in new[] { "", "0", "-1", "1.5", "abc", "2147483648", "37" })
            {
                widthPicker.SetCurrentValue(ComboBox.TextProperty, invalid);
                Pump();
                Check(model.MergeWidthError.Length > 0 && model.Validate() is null && !model.CanSave && !model.CanGenerateParameters,
                    $"{typeName} accepts unsupported width '{invalid}' or hides its error.");
            }
            // Removing and re-adding MergeLibId uses the selected width in either phase.
            row = section.Rows.Single(row => row.Key == ParamKeys.mergeLibIdKey);
            section.Rows.Remove(row);
            model.SelectedMergeSortingWidth = "24";
            section.BeginAdd();
            section.SelectedParameter = section.AvailableParameters.Single(spec => spec.Key == ParamKeys.mergeLibIdKey);
            var pending = section.PendingRow!;
            Check(pending.Values[0].Choices.SequenceEqual(expected24) && !model.CanSelectMergeWidth,
                $"{typeName}'s add panel lost the width filter.");
            pending.Values[0].Text = chosen[0];
            Check(section.Add() && model.Validate() is not null, $"{typeName} could not add a matching merge back.");
            // Restoring the original inputs restores their exact parameter and Setup reference IDs.
            model.SelectedMergeSortingWidth = "32";
            row = section.Rows.Single(row => row.Key == ParamKeys.mergeLibIdKey);
            section.Rows[section.Rows.IndexOf(row)] = new ParameterRow(row.Key, originalValues, row.Definition);
            model.RunName = original.RunName;
            Check(originalRows.ToHashSet().SetEquals(RunParameterGeneration.generate(model.Validate()!, 0, 1)),
                $"{typeName} changed its original file identities after restoring its selections.");
        }
        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SorterEval.Merge"));
        model.SelectExample(model.Examples.Single(example => example.Name == "Small"));
        model.RunName = "MixedMergeWidths";
        var mixed = model.Validate()!;
        Wait(model.SaveAsync(mixed, false));
        model.SelectedMergeSortingWidth = "24";
        Wait(model.OpenAsync(RunDefinitionFiles.pathFor(root, mixed)));
        Layout(view);
        Check(model.SelectedMergeSortingWidth == "" && model.MergeWidthError == "" && Equals(mixed, model.Validate()) && !model.HasUnsavedChanges,
            "Opening a mixed-width evaluation imposed one width or changed its saved values.");
        model.SelectRunType(model.RunTypes.Single(type => type.Name == "SortableTests.Merge"));
    }

    private static Tuple<string, FSharpList<string>> MutatorSpanAtWidth(Tuple<string, FSharpList<string>> span, int width)
    {
        if (span.Item1 != ParamKeys.mutatorParamsKey) return span;
        return Tuple.Create(span.Item1, ListModule.OfSeq(span.Item2.Select(text =>
        {
            var simple = ((mutatorParams.SimpleMutatorParams)MutatorParams.fromString(text).ResultValue).Item;
            return MutatorParams.toString(mutatorParams.NewSimpleMutatorParams(
                simpleMutatorParams.create(width, simple.RngType, simple.MutatorVariant, simple.Case)));
        })));
    }

    private static void CheckMutatorWidth(RunEditorViewModel model, run original, int width)
    {
        var originalSpan = original.SetupParameterSpans.FirstOrDefault(span => span.Item1 == ParamKeys.mutatorParamsKey);
        if (originalSpan is null) return;
        var expected = MutatorSpanAtWidth(originalSpan, width).Item2;
        Check(model.SetupParameters.Single(row => row.Key == ParamKeys.mutatorParamsKey).Snapshot().Values.SequenceEqual(expected),
            $"{model.WorkflowName} did not update the mutator width while preserving its other settings.");
        var inherited = model.ExecutionSection.InheritedRows.Single(row => row.Key == ParamKeys.mutatorParamsKey);
        Check(inherited.IsReadOnly && inherited.Snapshot().Values.SequenceEqual(expected),
            $"{model.WorkflowName} did not inherit the adjusted mutator read-only.");
    }
}
