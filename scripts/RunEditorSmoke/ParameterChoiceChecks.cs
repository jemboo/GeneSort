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
        foreach (var name in new[] { "SortableTests.Prefix.Channels24", "SortableTests.Merge.Channels32", "SorterEval.Standard.Channels4",
            "SorterEval.Standard.Channels16", "SorterMutate.Prefix", "SorterMutate.Standard", "SorterSgd.p24.Msrs3a.OrthoPara.OrthoPara32",
            "SorterSgd.p32.Msuf4a.SimpleR.Standard", "SorterSgd.p32.Msuf4a.SimpleS.Standard" })
        {
            model.SelectRunType(model.RunTypes.Single(type => type.Name == name));
            model.SelectExample(model.Examples[0]);
            model.ProjectsFolder = root;
            Check(model.Validate() is not null, string.Join("; ", model.Issues));
            var count = model.ModuleChannelCount;
            model.Clone();
            Layout(view);
            Check(model.ModuleChannelCount == count && model.CanEditDefinition, "Clone changed the module channel count.");
            Check(!Descendants<Button>(view).Any(button => button.Content is "Save" or "New" or "Open…"),
                "Removed editor actions remain visible.");
            var configurationTabs = Descendants<TabControl>(view).Single();
            configurationTabs.SelectedItem = configurationTabs.Items.Cast<TabItem>().Single(tab => Equals(tab.Header, "Generate parameters"));
            Layout(view);
            var validateButton = Descendants<Button>(view).Single(button => Equals(button.Content, "Validate"));
            var generateButton = Descendants<Button>(view).Single(button => Equals(button.Content, "Generate RunParameters"));
            Check(validateButton.Parent == generateButton.Parent
                && validateButton.TransformToAncestor(view).Transform(new Point()).X < generateButton.TransformToAncestor(view).Transform(new Point()).X,
                "Validate must appear immediately left of Generate RunParameters.");
            configurationTabs.SelectedIndex = 0;
            Layout(view);
            foreach (var row in model.SetupParameters.Where(row => row.Key == ParamKeys.aMaxKey || row.Key == ParamKeys.activationProbabilityKey))
                Check(!row.IsReadOnly, $"Activation parameter {row.Key} is not editable in a clone.");
            if (name.StartsWith("SorterSgd", StringComparison.Ordinal))
                foreach (var row in model.SetupParameters.Where(row => RunTypeEditor.isModuleOwnedParameter(model.SelectedExample!.Run, row.Key)))
                {
                    Check(row.IsReadOnly, $"Module setting {row.Key} is editable in a clone.");
                    var originalValue = row.Values[0].Text;
                    row.Values[0].Text = "changed";
                    Check(model.Validate() is null, $"Module setting {row.Key} bypassed validation.");
                    row.Values[0].Text = originalValue;
                }
            Check(!Descendants<ComboBox>(view).Any(box => AutomationProperties.GetName(box) is "Prefix channel count" or "Merge channel count"),
                "An editable channel-count picker remains.");
            Check(!Descendants<TextBlock>(view).Any(text => text.Text == model.ModuleChannelCountLabel), "Redundant module channel count remains visible.");
            foreach (var spec in model.Parameters)
                foreach (var value in spec.Choices)
                {
                    var actual = spec.Key == ParamKeys.prefixLibIdKey ? PrefixLibId.fromString(value).ChannelCount
                        : spec.Key == ParamKeys.mergeLibIdKey ? MergeLibId.fromString(value).ChannelCount
                        : spec.Key == ParamKeys.sorterLibIdKey ? SorterLibId.fromString(value).ChannelCount : count;
                    Check(actual == count, "Library choices contain another module's channel count.");
                }
            var channelRow = model.SetupParameters.Concat(model.ExecutionParameters).FirstOrDefault(row => row.Key == ParamKeys.channelCountKey);
            if (channelRow is not null)
            {
                Check(channelRow.IsReadOnly, "ChannelCount parameter is editable.");
                var original = channelRow.Values[0].Text;
                channelRow.Values[0].Text = (count + 1).ToString();
                Check(model.Validate() is null, "A channel count edit bypassed module validation.");
                channelRow.Values[0].Text = original;
            }
            var configured = model.Validate() ?? throw new Exception(string.Join("; ", model.Issues));
            Wait(model.SaveAsync(configured, false));
            Wait(model.OpenAsync(RunDefinitionFiles.pathFor(root, configured)));
            Check(model.SelectedRunType?.Name == name && model.ModuleChannelCount == count && model.DefinitionReadOnly,
                "Saved clone reopened under another channel module.");
        }
        foreach (var (name, key, control) in new[] {
            ("SortableTests.Prefix.Channels24", ParamKeys.prefixLibIdKey, "Prefix library value"),
            ("SortableTests.Merge.Channels32", ParamKeys.mergeLibIdKey, "Merge library value") })
        {
            model.SelectRunType(model.RunTypes.Single(type => type.Name == name));
            model.Clone();
            CheckLibrarySelection(model, view, key, control);
        }
        Check(bindingErrors.Length == 0, bindingErrors.ToString());
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
