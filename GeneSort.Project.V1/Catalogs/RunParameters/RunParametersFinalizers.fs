namespace GeneSort.Project.V1.Catalogs.RunParameters

open GeneSort.Project.V1

/// Assign execution identity/state only after effective parameter validation.
module RunParametersFinalizers =
    let private initialize includeDatabase : runParametersTransform = fun context parameters ->
        let query = context.QueryParamsFromRunParams parameters (geneSortDataType.Run context.Run.RunName)
                    |> Option.defaultWith (fun () -> invalidOp "The run query could not identify the validated parameters.")
        let initialized = parameters.WithRunName(Some context.Run.RunName).WithRunFinished(Some false).WithId(Some query.Id)
        if includeDatabase then initialized.WithDatabaseName(Some context.Run.DatabaseName) else initialized

    let standard = initialize true
    let sgd = initialize false
