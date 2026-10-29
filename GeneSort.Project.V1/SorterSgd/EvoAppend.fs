namespace GeneSort.Project.V1.SorterSgd

open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Setup

/// Continue selected source executions using the shared continuation contract.
module EvoAppend =
    let createStandard = EvoContinuation.create ExecuteFunctionNames.standard SetupCatalogNames.sgdStandard

    let createSoss = EvoContinuation.create ExecuteFunctionNames.soss SetupCatalogNames.sgdSoss
