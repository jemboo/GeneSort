namespace GeneSort.Project.V1.Catalogs.Queries

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters

/// Process-wide catalog of query parameter builders, addressed by stable names stored in runs.
module QueryParamsCatalogSetup =

    let private syncRoot = obj ()
    let mutable private builders: Map<string, setupQueryParamsCatalogBuilder> = Map.empty

    let nameForDatabase (projectName: string) (databaseName: string) =
        sprintf "%s.%s" projectName databaseName

    let register (name: string) (builder: setupQueryParamsCatalogBuilder) =
        if System.String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "A query parameter catalog name cannot be empty."

        lock syncRoot (fun () ->
            if Map.containsKey name builders then
                invalidOp (sprintf "A query parameter builder named '%s' is already registered." name)
            builders <- Map.add name builder builders)

    let tryGet name = lock syncRoot (fun () -> Map.tryFind name builders)

    let get (name: string) : setupQueryParamsCatalogBuilder =
        tryGet name |> Option.defaultWith (fun () -> invalidOp (sprintf "No query parameter builder named '%s' is registered." name))
