namespace GeneSort.Dispatch.V1

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Db.V1
open GeneSort.Dispatch.V1.SortableTests
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.SorterSgd.Soss
open GeneSort.Dispatch.V1.SorterSgd.Standard

/// Resolves run functions independently of setup and executor labels.
module ExecuteFunctionCatalog =
    let private evolution evaluate (context: executeContext) rp allowOverwrite cts progress =
        match context.MakeMutator, context.GenSaveIntervals, context.GenSaveSubIntervals with
        | Some mutator, Some intervals, Some subIntervals ->
            evaluate mutator context.RunDb intervals subIntervals rp allowOverwrite cts progress
        | _ -> async { return Error "Evolution execution requires an SGD context with a mutator and generation intervals." }

    let private syncRoot = obj ()
    let mutable private functions: Map<string, executeFunction> =
        Map.ofList [
            ExecuteFunctionNames.standard, evolution SgdEx_Standard.evaluateEvolutionRunStandard
            ExecuteFunctionNames.soss, evolution SgdEx_Soss.evaluateEvolutionRunSoss
            ExecuteFunctionNames.sorterEval, SorterEvalExecution._makeSorterEvals
            ExecuteFunctionNames.sorterMutate, SorterMutateExecution._evaluateMutants
            ExecuteFunctionNames.sortableTests, SortableTestsExecution._runSortableTests
        ]

    let register name (execute: executeFunction) =
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
