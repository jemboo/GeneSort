namespace GeneSort.Project.V1.Catalogs.Setup.SorterEval

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Catalogs.Setup

module SorterModelSetMakers =

    /// Generates and persists the model set used by sorter evaluation.
    let makeUniformSorterModelSet
            (storage: runStorage)
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
                storage.QueryParamsFromRunParams rp (geneSortDataType.SorterModelSet "")
                |> Result.ofOption "Failed to create QueryParams for SorterModelSet."

            return! SetupArtifacts.saveOnce storage.RunDb (QueryParams.forSetup rp query) (fun () -> asyncResult {
                let! modelGen = SeedSorterModelGeneration.getSimpleUniformSorterModelGen rngType sortingWidth simpleSorterModelType excludeSelfCe
                                |> SeedSorterModelGeneration.applyParameters rp
                let baseFirstIdx = (%repl * %totalSorterCount) |> UMX.tag<sorterCount>
                let modelSet = SorterModelGen.makeSorterModelSetFromIndexSpan (Guid.Empty |> UMX.tag) baseFirstIdx totalSorterCount modelGen
                return outputData.SorterModelSet modelSet
            })
          with error ->
            return! Error (sprintf "Failed to generate or save SorterModelSet: %s" error.Message)
        }
