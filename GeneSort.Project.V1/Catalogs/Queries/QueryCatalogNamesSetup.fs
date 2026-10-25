namespace GeneSort.Project.V1.Catalogs.Queries

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters

/// Stable names for setup query builders, independent of the run query registry.
module QueryCatalogNamesSetup =
    [<Literal>]
    let sorterSgdUf6MutationRate = "sorter-sgd.uf6-mutation-rate"
    [<Literal>]
    let sorterSgdMsrsOrthoPara = "sorter-sgd.msrs-ortho-para"
    [<Literal>]
    let sorterSgdMssiOrthoPara = "sorter-sgd.mssi-ortho-para"
    [<Literal>]
    let sorterSgdMsufStandard = "sorter-sgd.msuf-standard"
    [<Literal>]
    let sorterSgdMsufSoss = "sorter-sgd.msuf-soss"
    [<Literal>]
    let sossMsuf32Seed = "sorter-sgd.soss-msuf32-seed"
    [<Literal>]
    let sorterSgdMsrs32MutationRate = "sorter-sgd.msrs32-mutation-rate"
    [<Literal>]
    let sorterSgdMsrsPoolModComp = "sorter-sgd.msrs-pool-mod-comp"
    [<Literal>]
    let sortableTestMerge = "sortable-test.merge"
    [<Literal>]
    let sortableTestPrefix = "sortable-test.prefix"
    [<Literal>]
    let sorterEvalStandard = "sorter-eval.standard"
    [<Literal>]
    let sorterEvalMerge = "sorter-eval.merge"
    [<Literal>]
    let sorterEvalPrefix = "sorter-eval.prefix"
    [<Literal>]
    let sorterMutateStandard = "sorter-mutate.standard"
    [<Literal>]
    let sorterMutateMerge = "sorter-mutate.merge"
    [<Literal>]
    let sorterMutatePrefix = "sorter-mutate.prefix"

