namespace GeneSort.Project.V1.Catalogs.Execution

open System
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Catalogs.Execution.SortableTests
open GeneSort.Project.V1.Catalogs.Execution.SorterEval
open GeneSort.Project.V1.Catalogs.Execution.SorterMutate
open GeneSort.Project.V1.Catalogs.Execution.SorterSgd.Standard
open GeneSort.Project.V1.Catalogs.Execution.SorterSgd.Soss

/// Resolves run functions independently of setup and executor labels.
module ExecuteFunctionCatalog =
    let private evolution evaluate (storage: runStorage) (dependencies: sgdExecution) : runFunction =
        evaluate dependencies.MakeMutator storage.RunDb dependencies.GenSaveIntervals dependencies.GenSaveSubIntervals

    let private syncRoot = obj ()
    let mutable private functions: Map<string, executionBinding> =
        Map.ofList [
            ExecuteFunctionNames.standard, SgdExecution (evolution SgdEx_Standard.evaluateEvolutionRunStandard)
            ExecuteFunctionNames.soss, SgdExecution (evolution SgdEx_Soss.evaluateEvolutionRunSoss)
            ExecuteFunctionNames.sorterEval, SimpleExecution SorterEvalExecution._makeSorterEvals
            ExecuteFunctionNames.sorterMutate, SimpleExecution SorterMutateExecution._evaluateMutants
            ExecuteFunctionNames.sortableTests, SimpleExecution SortableTestsExecution._runSortableTests
        ]

    let register name (execute: executionBinding) =
        if String.IsNullOrWhiteSpace name then
            invalidArg (nameof name) "Execute function name cannot be empty."
        lock syncRoot (fun () ->
            if Map.containsKey name functions then
                invalidOp (sprintf "An execute function named '%s' is already registered." name)
            functions <- Map.add name execute functions)

    let get name =
        lock syncRoot (fun () ->
            match Map.tryFind name functions with
            | Some execute -> execute
            | None -> invalidOp (sprintf "No execute function named '%s' is registered." name))

    /// Bind execution from the saved definition; execution has no source-library or setup dependency.
    let bind (configured: run) storage =
        match get configured.ExecuteFunctionName with
        | SimpleExecution bind -> bind storage
        | SgdExecution bind ->
            match configured with
            | run.SimpleRun _ ->
                invalidArg "configured" (sprintf "Execute function '%s' requires an SgdRun with a mutator and generation intervals." configured.ExecuteFunctionName)
            | run.SgdRun sgd ->
                let dependencies = sgdExecution.create {
                    MakeMutator = MutatorMakerCatalog.get (SgdRun.makerNames sgd).Mutator
                    GenSaveIntervals = RunIntervals.resolve configured.RunName (SgdRun.genSaveIntervalsName sgd)
                    GenSaveSubIntervals = RunIntervals.resolve configured.RunName (SgdRun.genSaveSubIntervalsName sgd)
                }
                bind storage dependencies
