namespace GeneSort.Project.V1

open System
open FSharp.UMX
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters

module SetupOutputs =
    /// The generated inputs used by each execution family. Their IDs come directly
    /// from the configured Setup constructors, using fully enhanced parameters.
    let generatedFiles (configured: run) (parameters: runParameters) =
        let outputs =
            match configured.ExecuteFunctionName with
            | ExecuteFunctionNames.standard | ExecuteFunctionNames.soss ->
                [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
            | ExecuteFunctionNames.sorterEval | ExecuteFunctionNames.sorterMutate ->
                [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
            | _ -> []
        match outputs with
        | [] -> []
        | _ ->
            let query = QueryParamsCatalogSetup.get configured.SetupQueryCatalogName configured.ProjectName configured.DatabaseName
            outputs |> List.choose (query (setupParameters.create parameters.ParamMap))

    /// Inventory of concrete inputs, with no requested-file aliases.
    let files (configured: run) (parameters: runParameters) =
        let recorded =
            [geneSortDataType.SorterModelSet ""; geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
            |> List.choose (SetupInputReferences.tryRecordedInput configured.ProjectName configured.DatabaseName parameters)
        let generated =
            if configured.ParameterSource.IsSome || (QueryParamsCatalogSetup.tryGet configured.SetupQueryCatalogName).IsNone then []
            else generatedFiles configured parameters
        recorded @ (generated |> List.filter (fun query -> recorded |> List.exists (fun saved -> saved.OutputDataType = query.OutputDataType) |> not))

    /// Carry finalizer-supplied file references alongside the identity of the setup settings.
    let referencesFor (configured: run) (parameters: setupParameters) =
        let settings = SetupFileReferences.withoutReferences parameters
        let identity =
            queryParams.create configured.DatabaseName configured.ProjectName (parameters.GetRepl()) None
                (geneSortDataType.Run configured.RunName) (Map.toArray settings.ParamMap)
        let files = SetupFileReferences.fromSetupParameters parameters |> List.filter (fun (key, _) -> key <> ParamKeys.setupParametersIdKey)
        (ParamKeys.setupParametersIdKey, string identity.Id) :: files

    /// Predicts one output ID per enhanced setup parameter combination for a single replica.
    /// Preserves order and duplicate IDs. Does not execute setup or access storage.
    let getIds
            (simple: simpleRun)
            (repl: int<replNumber>)
            (outputType: geneSortDataType) : Result<Guid<queryParamsId> array, string> =
        try
            ProjectCatalogs.registerAll ()
            let configured = Run.ofSimple simple
            let query =
                QueryParamsCatalogSetup.get configured.SetupQueryCatalogName
                    configured.ProjectName configured.DatabaseName
            let applyPipeline = SetupParametersPipelineCatalog.get configured.SetupParametersPipelineName |> ParameterPipeline.apply
            let context = { Run = configured; QueryParamsFromSetupParams = query }
            let parameters =
                SimpleRun.makeSetupParameters repl (Checked.(+) repl 1<replNumber>)
                    (Seq.map (applyPipeline context)) simple
            parameters
            |> Array.mapi (fun index parameters ->
                match query parameters outputType with
                | Some queryParams -> queryParams.Id
                | None ->
                    invalidOp (sprintf "Setup query '%s' could not build %A for run '%s', replica %d, combination %d.\n%s"
                        configured.SetupQueryCatalogName outputType (%configured.RunName) (%repl) index
                        (SetupParameters.reportKvps parameters)))
            |> Ok
        with error -> Error error.Message
