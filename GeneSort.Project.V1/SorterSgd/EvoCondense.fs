namespace GeneSort.Project.V1.SorterSgd

open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Setup

/// Rank source checkpoints during preparation, then continue the winners.
module EvoCondense =
    let createStandard = EvoContinuation.create ExecuteFunctionNames.standard SetupCatalogNames.sgdStandard

    let createSoss = EvoContinuation.create ExecuteFunctionNames.soss SetupCatalogNames.sgdSoss
