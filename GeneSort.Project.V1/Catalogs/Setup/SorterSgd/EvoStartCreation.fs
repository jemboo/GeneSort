namespace GeneSort.Project.V1.Catalogs.Setup.SorterSgd

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage
open FsToolkit.ErrorHandling

module EvoStartCreation =
    /// Setup owns these recorded identities; no Run query is needed to reopen them.
    let private files project database (rp: runParameters) = result {
        let! testsId = rp.GetSetupSortableTestsWithPrefixId()
                       |> Result.ofOption "Missing or invalid SetupSortableTestsWithPrefixId. Regenerate run parameters before Setup."
        let! poolId = rp.GetSetupSorterPoolSetId()
                      |> Result.ofOption "Missing or invalid SetupSorterPoolSetId. Regenerate run parameters before Setup."
        let tests = queryParams.create database project None None (geneSortDataType.SortableTestsWithPrefix "") [||]
                    |> queryParams.withId testsId
        let pool = queryParams.create database project None None (geneSortDataType.SorterPoolSet "") [||]
                   |> queryParams.withId poolId
        return { Tests = tests; Pool = pool; FirstGeneration = UMX.tag 0 }
    }

    let evaluateStandardSeed (rp: runParameters) tests prefix seed = result {
        let! pool = SorterPoolSet.asStandard seed
        let! measure = rp.GetSorterEvalMeasure() |> Result.ofOption "Missing sorterEvalMeasure."
        let evaluations = PoolRunner_Standard.evaluatePoolSet tests prefix sorterEvalType.V2 true (UMX.tag false) pool
        return pool |> SorterPoolSet_Standard.updateSorterEvals evaluations
                    |> SorterPoolSet_Standard.withSelectionMeasure measure |> sorterPoolSet.Standard
    }

    let evaluateSossSeed (rp: runParameters) tests prefix seed = result {
        let! pool = SorterPoolSet.asSoss seed
        let! measure = rp.GetSorterEvalMeasure() |> Result.ofOption "Missing sorterEvalMeasure."
        let evaluations =
            PoolRunner_Soss.evaluatePoolSet tests prefix sorterEvalType.V2 true (UMX.tag false) pool
            |> Map.map (fun _ evaluation -> SorterEval.withSortableTestsSubsetId CommonSgdSoss.mergedSubsetId evaluation)
        return pool |> SorterPoolSet_Soss.updateSorterEvals evaluations
                    |> SorterPoolSet_Soss.withSelectionMeasure measure |> sorterPoolSet.Soss
    }

    let createStandardStart project database makeTests (makeSeed: runParameters -> Async<Result<sorterPoolSet, string>>) (db: IGeneSortDb) (parameters: runParameters) (ct: CancellationToken) = asyncResult {
        do! checkCancellation ct
        let rp = parameters.WithGenerationCurrent(Some (UMX.tag 0))
        let! start = files project database rp
        let! width = rp.GetChannelCount() |> Result.ofOption "Missing channel count."
        let! testsData = SetupArtifacts.getOrCreate db start.Tests (fun () ->
            makeTests rp |> Async.map (Result.map outputData.SortableTestsWithPrefix))
        let! tests, ces = OutputData.asSortableTestsWithPrefix testsData
        do! checkCancellation ct
        let! poolData = SetupArtifacts.getOrCreate db start.Pool (fun () -> asyncResult {
            let! seed = makeSeed rp
            let! pool = evaluateStandardSeed rp tests (ceBlock.create (UMX.tag Guid.Empty) width ces) seed
            return outputData.SorterPoolSet pool
        })
        let! _ = OutputData.asSorterPoolSetStandard poolData
        return start
    }

    let createSossStart project database makeTests (makeSeed: runParameters -> Async<Result<sorterPoolSet, string>>) (db: IGeneSortDb) (parameters: runParameters) (ct: CancellationToken) = asyncResult {
        do! checkCancellation ct
        let rp = parameters.WithGenerationCurrent(Some (UMX.tag 0))
        let! start = files project database rp
        let! width = rp.GetChannelCount() |> Result.ofOption "Missing channel count."
        let! testsData = SetupArtifacts.getOrCreate db start.Tests (fun () ->
            makeTests rp |> Async.map (Result.map outputData.SortableTestsWithPrefix))
        let! tests, ces = OutputData.asSortableTestsWithPrefix testsData
        do! checkCancellation ct
        let! poolData = SetupArtifacts.getOrCreate db start.Pool (fun () -> asyncResult {
            let! seed = makeSeed rp
            let! pool = evaluateSossSeed rp tests (ceBlock.create (UMX.tag Guid.Empty) width ces) seed
            return outputData.SorterPoolSet pool
        })
        let! _ = OutputData.asSorterPoolSetSoss poolData
        return start
    }
