using GeneSort.Project.V1;
using GeneSort.Project.V1.Editing;
using GeneSort.Project.V1.Parameters;
using GeneSort.Project.V1.Runtime;
using GeneSort.WPF.ViewModels;
using Microsoft.FSharp.Core;

internal static partial class Program
{
    private static void CheckDirectEvolutionSelection()
    {
        var configured = RunEditor.build(RunEditor.newDraft()).ResultValue;
        var rows = RunParameterGeneration.generate(configured, 0, 1);
        var list = new RunParameterListViewModel();
        list.SetParameters(rows, configured);
        var items = list.Items.Cast<RunParameterItem>().ToArray();
        var untouched = items[0].Evolution;
        RunParameterItem[] selected = [items[2], items[0]];
        list.Select(selected);
        list.Sort(ParamKeys.mutationModKey);
        list.Sort(ParamKeys.mutationModKey);

        var start = items[2].Evolution.Item1.Start;
        var setup = EvoPath.fromStart(configured, start, EvoStart.initializeExecution(start, items[2].Parameters));
        list.ApplyUpdates(selected, [Tuple.Create(0, runPhaseOutput.NewEvolution(setup.Item1, setup.Item2))]);
        Check(ReferenceEquals(items[2].Evolution.Item1, setup.Item1)
            && ReferenceEquals(items[0].Evolution.Item1, untouched.Item1),
            "A partial Setup result did not update the original selected item after sorting.");
        Check(list.SelectedCount == 0, "Completing a phase retained its selection.");

        var advanced = setup.Item2.WithCurrentGeneration(FSharpOption<int>.Some(100)).WithFinished(FSharpOption<bool>.Some(true));
        list.ApplyUpdates([items[2]], [Tuple.Create(0, runPhaseOutput.NewEvolution(setup.Item1, advanced))]);
        Check(ReferenceEquals(items[2].Evolution.Item1, setup.Item1)
            && ReferenceEquals(items[2].Evolution.Item1.Start, start)
            && items[2].Parameters.GetGenerationCurrent().Value == 100
            && setup.Item2.CurrentGeneration.Value == 0,
            "Execution rebuilt the path or failed to advance progress separately.");

        var saved = items[2].Parameters;
        list.SetParameters([saved], configured);
        var reopened = list.Items.Cast<RunParameterItem>().Single().Evolution;
        Check(!ReferenceEquals(reopened.Item1, setup.Item1)
            && reopened.Item1.Id == setup.Item1.Id
            && reopened.Item1.Start.Tests.Id == start.Tests.Id
            && reopened.Item1.Start.Pool.Id == start.Pool.Id
            && EvoPath.toParameters(reopened.Item1, reopened.Item2).Equals(saved),
            "Explicit refresh did not reopen the saved path with unchanged IDs and progress.");
    }
}
