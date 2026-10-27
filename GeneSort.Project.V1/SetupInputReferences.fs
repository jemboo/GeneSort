namespace GeneSort.Project.V1.Parameters

open System
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries

/// Saved input references and compatibility for imported or older datasets.
/// Generated Setup file identities come directly from the Setup constructors.
module SetupInputReferences =
    let fileIdentity (query: queryParams) = query.ProjectName, query.DbName, query.OutputDataType, query.Id

    let tryRecordedInput project database (parameters: runParameters) output =
        let keys =
            match output with
            | geneSortDataType.SorterModelSet _ -> [ParamKeys.setupSorterModelSetIdKey]
            | geneSortDataType.SorterPoolSet _ -> [ParamKeys.setupSorterPoolSetIdKey]
            | geneSortDataType.SortableTestsWithPrefix _ -> [ParamKeys.setupSortableTestsWithPrefixIdKey]
            | _ -> []
        keys |> List.tryPick (fun key ->
            parameters.ParamMap.TryFind key |> Option.map (fun value ->
                let id = Guid.Parse value |> UMX.tag<queryParamsId>
                queryParams.create database project None None output [||] |> queryParams.withId id))

    /// Use a recorded file when present. The supplied query is the legacy fallback.
    /// This applies only to Setup; Results keep their own queries.
    let resolveRecorded parameters (fallback: queryParams) =
        tryRecordedInput fallback.ProjectName fallback.DbName parameters fallback.OutputDataType
        |> Option.defaultValue fallback

    /// Legacy pools without references used the execution query at the origin.
    /// Legacy models/tests used the same identities now returned by Setup constructors.
    let resolveSavedInput (setupQuery: setupQueryParamsBuilder) parameters (legacy: queryParams) =
        tryRecordedInput legacy.ProjectName legacy.DbName parameters legacy.OutputDataType
        |> Option.orElseWith (fun () ->
            match legacy.OutputDataType with
            | geneSortDataType.SorterModelSet _ | geneSortDataType.SortableTestsWithPrefix _ ->
                setupQuery (setupParameters.create parameters.ParamMap) legacy.OutputDataType
            | _ -> None)
        |> Option.defaultValue legacy

    /// Preserve existing metadata, including SetupParametersId, when recording inputs.
    let recordFiles (files: queryParams seq) (parameters: runParameters) =
        let references = files |> Seq.map (fun query -> SetupFileReferences.keyForOutputType query.OutputDataType, string query.Id) |> Seq.toArray
        for key, values in references |> Array.groupBy fst do
            if values |> Array.map snd |> Array.distinct |> Array.length > 1 then
                invalidArg "files" (sprintf "Cannot record multiple files for %s on one execution." key)
        references |> Array.fold (fun (rp: runParameters) (key, value) -> runParameters.create (rp.ParamMap.Add(key, value))) parameters

    let copyPool project database (source: queryParams) =
        queryParams.createForImportedFile database project None (geneSortDataType.SorterPoolSet "") source.Id

    let copyTests project database (source: queryParams) =
        queryParams.createForImportedFile database project None (geneSortDataType.SortableTestsWithPrefix "") source.Id

    let convertedPool project database executionFunction (pool: queryParams) (tests: queryParams) =
        queryParams.create database project None None (geneSortDataType.SorterPoolSet "")
            [| "SourceFileId", string pool.Id
               "SourceTestsFileId", string tests.Id
               "ExecutionFunction", executionFunction |]
