namespace GeneSort.Project.V1.SorterSgd

open GeneSort.Project.V1.Catalogs.Evolution

/// Rank source checkpoints during preparation, then continue the winners.
module EvoCondense =
    let createStandard = EvoContinuation.create ExecuteFunctionNames.standard

    let createSoss = EvoContinuation.create ExecuteFunctionNames.soss
