namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Parameters

/// Continue selected source executions using the shared continuation contract.
module EvoAppend =
    let private create executionFunction (config: evoContinuationConfig) =
        EvoContinuation.create executionFunction config

    let createStandard config = create ExecuteFunctionNames.standard config

    let createSoss config = create ExecuteFunctionNames.soss config
