namespace GeneSort.Dispatch.V1.SorterEval

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Db.V1
open GeneSort.Dispatch.V1
open GeneSort.Project.V1.Parameters

module SorterModelSetMakers =

    /// Generates and persists the model set used by sorter evaluation.
    let makeUniformSorterModelSet
            (host: runHost)
            (allowOverwrite: bool<allowOverwrite>)
            (rp: runParameters) : Async<Result<unit, string>> =
        asyncResult {
          try
            let! sortingWidth = rp.GetSortingWidth() |> Result.ofOption "Missing SortingWidth."
            let! simpleSorterModelType = rp.GetSimpleSorterModelType() |> Result.ofOption "Missing SimpleSorterModelType."
            let! rngType = rp.GetRngType() |> Result.ofOption "Missing RngType."
            let! excludeSelfCe = rp.GetExcludeSelfCe() |> Result.ofOption "Missing ExcludeSelfCe."
            let! totalSorterCount = rp.GetSorterCount() |> Result.ofOption "Missing SorterCount."
            let! repl = rp.GetRepl() |> Result.ofOption "Missing Repl."
            let! query =
                host.QueryParamsFromRunParams rp (geneSortDataType.SorterModelSet "")
                |> Result.ofOption "Failed to create QueryParams for SorterModelSet."

            let modelGen = CommonSorterEval.getSimpleUniformSorterModelGen rngType sortingWidth simpleSorterModelType excludeSelfCe
            let baseFirstIdx = (%repl * %totalSorterCount) |> UMX.tag<sorterCount>

            let modelSet = SorterModelGen.makeSorterModelSetFromIndexSpan (Guid.Empty |> UMX.tag) baseFirstIdx totalSorterCount modelGen
            return! host.RunDb.saveAsync geneSortDataMode.Setup query (outputData.SorterModelSet modelSet) allowOverwrite
          with error ->
            return! Error (sprintf "Failed to generate or save SorterModelSet: %s" error.Message)
        }
