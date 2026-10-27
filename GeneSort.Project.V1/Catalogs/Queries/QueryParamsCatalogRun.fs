namespace GeneSort.Project.V1.Catalogs.Queries

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters

/// A normalized adapter for turning run parameters into the storage path metadata.


module QueryCatalogNamesRun =
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
    let sorterSgdMsufSossAppend = "sorter-sgd.msuf-soss-append"
    [<Literal>]
    let sorterSgdMsufStandardAppend = "sorter-sgd.msuf-standard-append"
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

/// Process-wide catalog of query parameter builders, addressed by stable names stored in runs.
module QueryParamsCatalogRun =

    let private syncRoot = obj ()
    let mutable private builders: Map<string, runQueryParamsCatalogBuilder> = Map.empty

    let nameForDatabase (projectName: string) (databaseName: string) =
        sprintf "%s.%s" projectName databaseName

    /// Register complete, standalone functions. The catalog never changes their output.
    let register name builder =
        if System.String.IsNullOrWhiteSpace name then
            invalidArg "name" "A query parameter catalog name cannot be empty."
        lock syncRoot (fun () ->
            if Map.containsKey name builders then
                invalidOp (sprintf "A query parameter builder named '%s' is already registered." name)
            builders <- Map.add name builder builders)

    let get name =
        lock syncRoot (fun () ->
            match Map.tryFind name builders with
            | Some builder -> builder
            | None -> invalidOp (sprintf "No query parameter builder named '%s' is registered." name))
