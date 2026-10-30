namespace GeneSort.Project.V1.Catalogs.RunParameters

/// Registration and lookup of run parameter enhancers.
module RunParametersEnhancerCatalog =
    let private syncRoot = obj ()
    let mutable private enhancers: Map<string, runParametersEnhancer> = Map.empty

    let register name (enhancer: runParametersEnhancer) =
        if System.String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "A run parameters enhancer name cannot be empty."
        lock syncRoot (fun () -> enhancers <- enhancers.Add(name, enhancer))
        name

    let get name : runParametersEnhancer =
        lock syncRoot (fun () ->
            match enhancers.TryFind name with
            | Some enhancer -> enhancer
            | None -> invalidOp (sprintf "No run parameters enhancer named '%s' is registered." name))

    let private registrationLock = obj ()
    let mutable private registered = false

    let registerAll () =
        lock registrationLock (fun () ->
            if not registered then
                register RunParametersEnhancerNames.sortableTests RunParametersEnhancers.sortableTests |> ignore
                register RunParametersEnhancerNames.sorterEvalStandard RunParametersEnhancers.sorterEvalStandard |> ignore
                register RunParametersEnhancerNames.sorterEvalMerge RunParametersEnhancers.sorterEvalMerge |> ignore
                register RunParametersEnhancerNames.sorterEvalPrefix RunParametersEnhancers.sorterEvalPrefix |> ignore
                register RunParametersEnhancerNames.sorterMutateStandard RunParametersEnhancers.sorterMutateStandard |> ignore
                register RunParametersEnhancerNames.sorterMutateStandardFormat RunParametersEnhancers.sorterMutateStandardFormat |> ignore
                register RunParametersEnhancerNames.msuf32MutationRate RunParametersEnhancers.Sgd.msuf32MutationRate |> ignore
                register RunParametersEnhancerNames.sossMsuf32 RunParametersEnhancers.Sgd.msuf32FromSetup |> ignore
                register RunParametersEnhancerNames.evoMsuf32 RunParametersEnhancers.Sgd.msuf32FromSetup |> ignore
                register RunParametersEnhancerNames.evoMsuf32R RunParametersEnhancers.Sgd.msuf32RFromSetup |> ignore
                register RunParametersEnhancerNames.sossMsuf32R RunParametersEnhancers.Sgd.msuf32RFromSetup |> ignore
                register RunParametersEnhancerNames.evoMsuf32S RunParametersEnhancers.Sgd.msuf32SFromSetup |> ignore
                register RunParametersEnhancerNames.sossMsuf32S RunParametersEnhancers.Sgd.msuf32SFromSetup |> ignore
                register RunParametersEnhancerNames.msrs32MutationRate RunParametersEnhancers.Sgd.msrs32MutationRate |> ignore
                register RunParametersEnhancerNames.msrs32MutationRateMax RunParametersEnhancers.Sgd.msrs32MutationRateMax |> ignore
                register RunParametersEnhancerNames.msuf624p3bMutationRate RunParametersEnhancers.Sgd.msuf624p3bMutationRate |> ignore
                register RunParametersEnhancerNames.msrs24p3aOrthoPara RunParametersEnhancers.Sgd.msrs24p3aOrthoPara |> ignore
                register RunParametersEnhancerNames.msrs24p3aPoolModComp RunParametersEnhancers.Sgd.msrs24p3aPoolModComp |> ignore
                register RunParametersEnhancerNames.msrs24p3bOrthoPara RunParametersEnhancers.Sgd.msrs24p3bOrthoPara |> ignore
                register RunParametersEnhancerNames.mssi24p3bOrthoPara RunParametersEnhancers.Sgd.mssi24p3bOrthoPara |> ignore
                registered <- true)
