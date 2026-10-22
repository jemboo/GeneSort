namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Evolution

/// Continue each matching source execution without ranking sources.
module EvoAppend =
    let private create executionFunction (config: evoContinuationConfig) =
        if config.Source.ParameterSpans @ config.ExecutionParameterSpans
           |> List.exists (fst >> (=) ParamKeys.selectedSorterPoolCountKey) then
            invalidArg "SelectedSorterPoolCount" "Use EvoCondense.createStandard or EvoCondense.createSoss for ranked source selection."
        EvoContinuation.create executionFunction config

    let createStandard config = create ExecuteFunctionNames.standard config

    let createSoss config = create ExecuteFunctionNames.soss config
