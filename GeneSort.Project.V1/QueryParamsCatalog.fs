namespace GeneSort.Project.V1

open FSharp.UMX
open GeneSort.Project.V1

/// A normalized adapter for turning run parameters into the storage path metadata.
type queryParamsBuilder = runParameters -> outputDataType -> queryParams option
type queryParamsCatalogBuilder = string<projectName> -> string<databaseName> -> queryParamsBuilder

module QueryCatalogNames =
    [<Literal>]
    let sorterSgdUf6MutationRate = "sorter-sgd.uf6-mutation-rate"
    [<Literal>]
    let sorterSgdMsrsOrthoPara = "sorter-sgd.msrs-ortho-para"
    [<Literal>]
    let sorterSgdMssiOrthoPara = "sorter-sgd.mssi-ortho-para"
    [<Literal>]
    let sorterSgdMsuf32MutationRate = "sorter-sgd.msuf32-mutation-rate"
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
module QueryParamsCatalog =

    let private syncRoot = obj ()
    let mutable private builders: Map<string, queryParamsCatalogBuilder> = Map.empty

    let nameForDatabase (projectName: string) (databaseName: string) =
        sprintf "%s.%s" projectName databaseName

    let register (name: string) (builder: queryParamsCatalogBuilder) =
        if System.String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "A query parameter catalog name cannot be empty."

        lock syncRoot (fun () ->
            if Map.containsKey name builders then
                invalidOp (sprintf "A query parameter builder named '%s' is already registered." name)
            builders <- Map.add name builder builders)

    let get (name: string) : queryParamsCatalogBuilder =
        lock syncRoot (fun () ->
            match Map.tryFind name builders with
            | Some builder -> builder
            | None -> invalidOp (sprintf "No query parameter builder named '%s' is registered." name))
