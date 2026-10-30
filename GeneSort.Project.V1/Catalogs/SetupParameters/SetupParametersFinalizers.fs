namespace GeneSort.Project.V1.Catalogs.SetupParameters

open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters

/// Assign file references after effective parameter validation.
module SetupParametersFinalizers =
    /// Resolve required source files after applying setup settings.
    let withFileReferences outputTypes (context: setupParametersPipelineContext) (parameters: setupParameters) =
        let outputs = outputTypes |> List.map (fun outputType -> SetupFileReferences.keyForOutputType outputType, outputType)
        match outputs |> List.countBy fst |> List.tryFind (fun (_, count) -> count > 1) with
        | Some (key, _) -> invalidArg "outputTypes" (sprintf "Multiple source files use '%s'; only one file per ID key can be linked." key)
        | None -> ()
        let references = outputs |> List.map (fun (key, outputType) ->
            match context.QueryParamsFromSetupParams parameters outputType with
            | Some query -> key, string query.Id
            | None -> invalidOp (sprintf "Setup query '%s' could not build %A for run '%s'.\n%s"
                                    context.Run.SetupQueryCatalogName outputType (string context.Run.RunName)
                                    (SetupParameters.reportKvps parameters)))
        references |> List.fold (fun map (key, value) -> Map.add key value map) parameters.ParamMap |> setupParameters.create

    let noOp : setupParametersTransform = fun _ parameters -> parameters
    let sorterEval = withFileReferences [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""]
    let sorterSgd = withFileReferences [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
