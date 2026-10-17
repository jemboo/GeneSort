namespace GeneSort.Project.V1.Catalogs.Evolution

/// Stable names persisted by SGD runs; implementations are registered in Dispatch.
module ExecuteFunctionNames =
    [<Literal>]
    let standard = "evolution.standard"
    [<Literal>]
    let soss = "evolution.soss"
    [<Literal>]
    let sorterEval = "sorter-eval.evaluate"
    [<Literal>]
    let sorterMutate = "sorter-mutate.evaluate"
    [<Literal>]
    let sortableTests = "sortable-tests.publish"
