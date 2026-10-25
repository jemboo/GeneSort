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

    let private lookup (name: string) : setupQueryParamsCatalogBuilder =
        lock syncRoot (fun () ->
            match Map.tryFind name builders with
            | Some builder -> builder
            | None -> invalidOp (sprintf "No query parameter builder named '%s' is registered." name))

    /// Setup identities depend on setup settings, excluding source file references.
    let get name : setupQueryParamsCatalogBuilder =
        let builder = lookup name
        fun project database parameters outputType ->
            match SetupArtifactQueries.tryCreate name project database (runParameters.create parameters.ParamMap) outputType with
            | Some query -> Some query
            | None ->
                builder project database (SetupFileReferences.withoutReferences parameters) outputType
                |> Option.map (fun query ->
                    let properties =
                        [ParamKeys.aMaxKey; ParamKeys.activationProbabilityKey; ParamKeys.activatorMutationRateKey; ParamKeys.controlledSeedAMaxKey]
                        |> List.fold (fun map key ->
                            match Map.tryFind key parameters.ParamMap with
                            | Some value -> Map.add key value map
                            | None -> map) query.Properties
                    if properties = query.Properties then query
                    else queryParams.create query.DbName query.ProjectName query.Repl query.Generation query.OutputDataType (Map.toArray properties))
