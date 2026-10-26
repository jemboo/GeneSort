namespace GeneSort.Project.V1.Catalogs.Setup

open System
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Catalogs.Setup.SorterEval
open GeneSort.Project.V1.Catalogs.Setup.SorterMutate
open GeneSort.Project.V1.Catalogs.Setup.SorterSgd

/// Selects setup from the run's execution family; individual factories honor its setup catalog and makers.
module SetupExecutorCatalog =
    let private syncRoot = obj ()
    let mutable private builders: Map<string, workflowSources -> run -> runStorage -> runFunction> = Map.ofList [
        ExecuteFunctionNames.sortableTests, (fun _ _ _ rp _ _ _ -> async { return Ok rp })
        ExecuteFunctionNames.sorterEval, SorterEvalSetup.bind
        ExecuteFunctionNames.sorterMutate, SorterMutateSetup.bind
        ExecuteFunctionNames.standard, SorterSgdSetup.bind
        ExecuteFunctionNames.soss, SorterSgdSetup.bind
    ]

    let register name builder =
        if String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "Setup executor registration requires an execute function name."
        lock syncRoot (fun () ->
            if Map.containsKey name builders then
                invalidOp (sprintf "A setup executor for '%s' is already registered." name)
            builders <- Map.add name builder builders)

    let get sources (configured: run) storage =
        let name = configured.ExecuteFunctionName
        let builder = lock syncRoot (fun () ->
            Map.tryFind name builders
            |> Option.defaultWith (fun () -> invalidOp (sprintf "No setup executor is registered for execute function '%s'." name)))
        builder sources configured storage
