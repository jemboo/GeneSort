namespace GeneSort.Dispatch.V1

open System
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Dispatch.V1.SortableTests
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.SorterSgd

/// Selects setup from the run's execution family; individual factories honor its setup catalog and makers.
module SetupExecutorCatalog =
    let private syncRoot = obj ()
    let mutable private builders: Map<string, runHost -> IRunParamsExecutor> = Map.ofList [
        ExecuteFunctionNames.sortableTests, SortableTestsExecutor.getSetupExecutor
        ExecuteFunctionNames.sorterEval, (fun _ -> SorterEvalExecutor.setupExecutor)
        ExecuteFunctionNames.sorterMutate, (fun _ -> SorterMutateExecutor.setupExecutor)
        ExecuteFunctionNames.standard, SorterSgdSetup.getSetupExecutor
        ExecuteFunctionNames.soss, SorterSgdSetup.getSetupExecutor
    ]

    let register name builder =
        if String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "Setup executor registration requires an execute function name."
        lock syncRoot (fun () ->
            if Map.containsKey name builders then
                invalidOp (sprintf "A setup executor for '%s' is already registered." name)
            builders <- Map.add name builder builders)

    let get (host: runHost) =
        let name = host.Run.ExecuteFunctionName |> Option.defaultWith (fun () -> invalidOp "Setup requires an ExecuteFunctionName.")
        let builder = lock syncRoot (fun () ->
            Map.tryFind name builders
            |> Option.defaultWith (fun () -> invalidOp (sprintf "No setup executor is registered for execute function '%s'." name)))
        builder host

[<AutoOpen>]
module RunHostSetupExecutor =
    type runHost with
        member this.SetupExecutor = SetupExecutorCatalog.get this
