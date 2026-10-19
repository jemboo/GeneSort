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

    let register (name: string) (builder: runQueryParamsCatalogBuilder) =
        if System.String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "A query parameter catalog name cannot be empty."

        lock syncRoot (fun () ->
            if Map.containsKey name builders then
                invalidOp (sprintf "A query parameter builder named '%s' is already registered." name)
            builders <- Map.add name builder builders)

    let private lookup (name: string) : runQueryParamsCatalogBuilder =
        lock syncRoot (fun () ->
            match Map.tryFind name builders with
            | Some builder -> builder
            | None -> invalidOp (sprintf "No query parameter builder named '%s' is registered." name))

    /// Setup linkage is part of every run query's identity, even for builders that select only a subset of keys.
    let get name : runQueryParamsCatalogBuilder =
        let builder = lookup name
        fun project database parameters outputType ->
            builder project database parameters outputType
            |> Option.map (fun query ->
                let references = SetupFileReferences.fromRunParameters parameters
                if List.isEmpty references then query
                else
                    let properties = references |> List.fold (fun map (key, value) -> Map.add key value map) query.Properties
                    queryParams.create query.DbName query.ProjectName query.Repl query.Generation query.OutputDataType (Map.toArray properties))

    /// Setup and run builders share the named catalog and the same parameter keys.
    let registerForSetup name (builder: setupQueryParamsCatalogBuilder) =
        register name (fun project database rp dataType ->
            builder project database (setupParameters.create rp.ParamMap) dataType)

    let getForSetup name project database : setupQueryParamsBuilder =
        let builder = lookup name project database
        fun parameters dataType -> builder (runParameters.create parameters.ParamMap) dataType
