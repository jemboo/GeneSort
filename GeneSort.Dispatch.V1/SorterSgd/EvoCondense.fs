namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Evolution

/// Rank source checkpoints during preparation, then continue the winners.
module EvoCondense =
    let private createFor executionFunction (config: evoContinuationConfig) =
        if config.ExecutionParameterSpans |> List.exists (fst >> (=) ParamKeys.selectedSorterPoolCountKey) then
            invalidArg "ExecutionParameterSpans" "SelectedSorterPoolCount belongs in Source.ParameterSpans."
        match config.Source.ParameterSpans |> List.tryFind (fst >> (=) ParamKeys.selectedSorterPoolCountKey) with
        | Some (_, [value]) ->
            match System.Int32.TryParse value with
            | true, count when count > 0 ->
                match config.Source.ParameterSpans |> List.tryFind (fst >> (=) ParamKeys.mutationModKey) with
                | Some (_, values) when not values.IsEmpty && List.distinct values = values && count <= values.Length -> ()
                | _ -> invalidArg "Source.ParameterSpans" "Condense requires distinct MutationMod values and SelectedSorterPoolCount <= their count."
            | _ -> invalidArg "SelectedSorterPoolCount" "SelectedSorterPoolCount must be a positive integer."
        | _ -> invalidArg "SelectedSorterPoolCount" "Condense requires a single selection count in Source.ParameterSpans."
        EvoContinuation.create executionFunction config

    let createStandard config = createFor ExecuteFunctionNames.standard config

    let createSoss config = createFor ExecuteFunctionNames.soss config
