namespace GeneSort.Project.V1.SorterSgd

open GeneSort.Project.V1.Catalogs.Evolution

/// Continue selected source executions using the shared continuation contract.
module EvoAppend =
    let createStandard = EvoContinuation.create ExecuteFunctionNames.standard

    let createSoss = EvoContinuation.create ExecuteFunctionNames.soss
