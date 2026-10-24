namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Parameters

/// Rank source checkpoints during preparation, then continue the winners.
module EvoCondense =
    let private createFor executionFunction (config: evoContinuationConfig) =
        EvoContinuation.create executionFunction config

    let createStandard config = createFor ExecuteFunctionNames.standard config

    let createSoss config = createFor ExecuteFunctionNames.soss config
