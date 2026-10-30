namespace GeneSort.Project.V1.Catalogs.SetupParameters

/// Registration and lookup of setup parameter pipelines.
module SetupParametersPipelineCatalog =
    let private syncRoot = obj ()
    let mutable private pipelines: Map<string, setupParametersPipeline> =
        Map.ofList [
            SetupParametersPipelineNames.noOp, SetupParametersPipelines.noOp
            SetupParametersPipelineNames.sortableTests, SetupParametersPipelines.sortableTests
            SetupParametersPipelineNames.sorterEval, SetupParametersPipelines.sorterEval
            SetupParametersPipelineNames.sorterEvalPrefix, SetupParametersPipelines.sorterEvalPrefix
            SetupParametersPipelineNames.sorterMutate, SetupParametersPipelines.sorterMutate
            SetupParametersPipelineNames.sorterSgd, SetupParametersPipelines.sorterSgd
            SetupParametersPipelineNames.sossMsuf32, SetupParametersPipelines.sossMsuf32
            SetupParametersPipelineNames.standardMsuf32, SetupParametersPipelines.standardMsuf32
            SetupParametersPipelineNames.standardMsuf32R, SetupParametersPipelines.standardMsuf32R
            SetupParametersPipelineNames.sossMsuf32R, SetupParametersPipelines.sossMsuf32R
            SetupParametersPipelineNames.standardMsuf32S, SetupParametersPipelines.standardMsuf32S
            SetupParametersPipelineNames.sossMsuf32S, SetupParametersPipelines.sossMsuf32S
        ]

    let register name (pipeline: setupParametersPipeline) =
        if System.String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "A setup parameters pipeline name cannot be empty."
        lock syncRoot (fun () ->
            if pipelines.ContainsKey name then
                invalidOp (sprintf "Setup parameters pipeline '%s' is already registered." name)
            pipelines <- pipelines.Add(name, pipeline))

    let get name : setupParametersPipeline =
        lock syncRoot (fun () ->
            match pipelines.TryFind name with
            | Some pipeline -> pipeline
            | None -> invalidOp (sprintf "No setup parameters pipeline named '%s' is registered." name))
