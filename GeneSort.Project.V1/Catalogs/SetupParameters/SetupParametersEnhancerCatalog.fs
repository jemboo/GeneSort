namespace GeneSort.Project.V1.Catalogs.SetupParameters

/// Registration and lookup of setup parameter enhancers.
module SetupParametersEnhancerCatalog =
    let private syncRoot = obj ()
    let mutable private enhancers: Map<string, setupParametersEnhancer> =
        Map.ofList [
            SetupParametersEnhancerNames.noOp, SetupParametersEnhancers.noOp
            SetupParametersEnhancerNames.sortableTests, SetupParametersEnhancers.sortableTests
            SetupParametersEnhancerNames.sorterEval, SetupParametersEnhancers.sorterEval
            SetupParametersEnhancerNames.sorterEvalPrefix, SetupParametersEnhancers.sorterEvalPrefix
            SetupParametersEnhancerNames.sorterMutate, SetupParametersEnhancers.sorterMutate
            SetupParametersEnhancerNames.sorterSgd, SetupParametersEnhancers.sorterSgd
            SetupParametersEnhancerNames.sossMsuf32, SetupParametersEnhancers.sossMsuf32
            SetupParametersEnhancerNames.standardMsuf32, SetupParametersEnhancers.standardMsuf32
            SetupParametersEnhancerNames.standardMsuf32R, SetupParametersEnhancers.standardMsuf32R
            SetupParametersEnhancerNames.sossMsuf32R, SetupParametersEnhancers.sossMsuf32R
            SetupParametersEnhancerNames.standardMsuf32S, SetupParametersEnhancers.standardMsuf32S
            SetupParametersEnhancerNames.sossMsuf32S, SetupParametersEnhancers.sossMsuf32S
        ]

    let register name (enhancer: setupParametersEnhancer) =
        if System.String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "A setup parameters enhancer name cannot be empty."
        lock syncRoot (fun () ->
            if enhancers.ContainsKey name then
                invalidOp (sprintf "Setup parameters enhancer '%s' is already registered." name)
            enhancers <- enhancers.Add(name, enhancer))

    let get name : setupParametersEnhancer =
        lock syncRoot (fun () ->
            match enhancers.TryFind name with
            | Some enhancer -> enhancer
            | None -> invalidOp (sprintf "No setup parameters enhancer named '%s' is registered." name))
