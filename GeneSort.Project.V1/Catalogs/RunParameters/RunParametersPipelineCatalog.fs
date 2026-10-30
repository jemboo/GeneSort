namespace GeneSort.Project.V1.Catalogs.RunParameters

/// Registration and lookup of run parameter pipelines.
module RunParametersPipelineCatalog =
    let private syncRoot = obj ()
    let mutable private pipelines: Map<string, runParametersPipeline> = Map.empty

    let register name (pipeline: runParametersPipeline) =
        if System.String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "A run parameters pipeline name cannot be empty."
        lock syncRoot (fun () -> pipelines <- pipelines.Add(name, pipeline))
        name

    let get name : runParametersPipeline =
        lock syncRoot (fun () ->
            match pipelines.TryFind name with
            | Some pipeline -> pipeline
            | None -> invalidOp (sprintf "No run parameters pipeline named '%s' is registered." name))

    let private registrationLock = obj ()
    let mutable private registered = false

    let registerAll () =
        lock registrationLock (fun () ->
            if not registered then
                register RunParametersPipelineNames.sortableTests RunParametersPipelines.sortableTests |> ignore
                register RunParametersPipelineNames.sorterEvalStandard RunParametersPipelines.sorterEvalStandard |> ignore
                register RunParametersPipelineNames.sorterEvalMerge RunParametersPipelines.sorterEvalMerge |> ignore
                register RunParametersPipelineNames.sorterEvalPrefix RunParametersPipelines.sorterEvalPrefix |> ignore
                register RunParametersPipelineNames.sorterMutateStandard RunParametersPipelines.sorterMutateStandard |> ignore
                register RunParametersPipelineNames.sorterMutateStandardFormat RunParametersPipelines.sorterMutateStandardFormat |> ignore
                register RunParametersPipelineNames.msuf32MutationRate RunParametersPipelines.Sgd.msuf32MutationRate |> ignore
                register RunParametersPipelineNames.sossMsuf32 RunParametersPipelines.Sgd.msuf32FromSetup |> ignore
                register RunParametersPipelineNames.evoMsuf32 RunParametersPipelines.Sgd.msuf32FromSetup |> ignore
                register RunParametersPipelineNames.evoMsuf32R RunParametersPipelines.Sgd.msuf32RFromSetup |> ignore
                register RunParametersPipelineNames.sossMsuf32R RunParametersPipelines.Sgd.msuf32RFromSetup |> ignore
                register RunParametersPipelineNames.evoMsuf32S RunParametersPipelines.Sgd.msuf32SFromSetup |> ignore
                register RunParametersPipelineNames.sossMsuf32S RunParametersPipelines.Sgd.msuf32SFromSetup |> ignore
                register RunParametersPipelineNames.msrs32MutationRate RunParametersPipelines.Sgd.msrs32MutationRate |> ignore
                register RunParametersPipelineNames.msrs32MutationRateMax RunParametersPipelines.Sgd.msrs32MutationRateMax |> ignore
                register RunParametersPipelineNames.msuf624p3bMutationRate RunParametersPipelines.Sgd.msuf624p3bMutationRate |> ignore
                register RunParametersPipelineNames.msrs24p3aOrthoPara RunParametersPipelines.Sgd.msrs24p3aOrthoPara |> ignore
                register RunParametersPipelineNames.msrs24p3aPoolModComp RunParametersPipelines.Sgd.msrs24p3aPoolModComp |> ignore
                register RunParametersPipelineNames.msrs24p3bOrthoPara RunParametersPipelines.Sgd.msrs24p3bOrthoPara |> ignore
                register RunParametersPipelineNames.mssi24p3bOrthoPara RunParametersPipelines.Sgd.mssi24p3bOrthoPara |> ignore
                registered <- true)
