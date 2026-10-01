namespace GeneSort.Project.V1

open GeneSort.Project.V1

/// A normalized adapter for turning run parameters into the storage path metadata.
type queryParamsBuilder = runParameters -> outputDataType -> queryParams option

/// Process-wide catalog of query parameter builders, addressed by stable names stored in runs.
module QueryParamsCatalog =

    let private syncRoot = obj ()
    let mutable private builders: Map<string, queryParamsBuilder> = Map.empty

    let nameForDatabase (projectName: string) (databaseName: string) =
        sprintf "%s.%s" projectName databaseName

    let register (name: string) (builder: queryParamsBuilder) =
        if System.String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "A query parameter catalog name cannot be empty."

        lock syncRoot (fun () ->
            if Map.containsKey name builders then
                invalidOp (sprintf "A query parameter builder named '%s' is already registered." name)
            builders <- Map.add name builder builders)

    let get (name: string) : queryParamsBuilder =
        lock syncRoot (fun () ->
            match Map.tryFind name builders with
            | Some builder -> builder
            | None -> invalidOp (sprintf "No query parameter builder named '%s' is registered." name))
