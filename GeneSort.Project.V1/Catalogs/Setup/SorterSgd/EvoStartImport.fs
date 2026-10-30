namespace GeneSort.Project.V1.Catalogs.Setup.SorterSgd

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting.Sortable
open GeneSort.SortingOps
open GeneSort.Eval.V1.Sgd
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage
open FsToolkit.ErrorHandling

module EvoStartImport =
    let private validate (configured: run) (rp: runParameters) (checkpoint: evoCheckpoint) = result {
        let! width = rp.GetChannelCount() |> Result.ofOption "Missing ChannelCount."
        do! if SortableTests.getChannelCount checkpoint.Tests = width then Ok () else Error "Source test width does not match the destination."
        let! intervals = Runtime.RunIntervals.forRun configured |> Result.ofOption "An evolutionary start requires an SgdRun."
        let! last = rp.GetGenerationIntervalLast() |> Result.ofOption "Missing GenerationIntervalLast."
        let! final = GenIntervalConfig.getSampleAtIndex intervals (int last) |> Result.ofOption "Destination GenerationIntervalLast is unavailable."
        do! if final > int checkpoint.Generation then Ok () else Error "Destination final generation must be after the imported generation."
        do! match rp.GetSorterCountPerPool(), rp.GetSorterPoolCount() with
            | Some perPool, Some count when int perPool > 0 && int count > 0 -> Ok ()
            | _ -> Error "SorterPoolCount and SorterCountPerPool must both be positive."
        return ceBlock.create (UMX.tag Guid.Empty) width checkpoint.Prefix
    }

    let importStandardStart (configured: run) (db: IGeneSortDb) rp (checkpoint: evoCheckpoint) (ct: CancellationToken) = asyncResult {
        do! checkCancellation ct
        let! prefix = validate configured rp checkpoint
        let poolFile =
            match checkpoint.Pool with
            | sorterPoolSet.Standard _ -> SetupInputReferences.copyPool configured.ProjectName configured.DatabaseName checkpoint.PoolFile
            | sorterPoolSet.Soss _ -> SetupInputReferences.convertedPool configured.ProjectName configured.DatabaseName ExecuteFunctionNames.standard checkpoint.PoolFile checkpoint.TestsFile
        let testsFile = SetupInputReferences.copyTests configured.ProjectName configured.DatabaseName checkpoint.TestsFile
        do! SetupArtifacts.saveOnce db poolFile (fun () -> async {
            let pool = EvoAppendPools.convertForSetup ExecuteFunctionNames.standard checkpoint.Tests prefix checkpoint.Pool
            return Ok (outputData.SorterPoolSet pool)
        })
        do! checkCancellation ct
        do! SetupArtifacts.saveOnce db testsFile (fun () -> async { return Ok (outputData.SortableTestsWithPrefix(checkpoint.Tests, checkpoint.Prefix)) })
        return { Tests = testsFile; Pool = poolFile; FirstGeneration = checkpoint.Generation }
    }

    let importSossStart (configured: run) (db: IGeneSortDb) rp (checkpoint: evoCheckpoint) (ct: CancellationToken) = asyncResult {
        do! checkCancellation ct
        let! prefix = validate configured rp checkpoint
        let poolFile =
            match checkpoint.Pool with
            | sorterPoolSet.Soss _ -> SetupInputReferences.copyPool configured.ProjectName configured.DatabaseName checkpoint.PoolFile
            | sorterPoolSet.Standard _ -> SetupInputReferences.convertedPool configured.ProjectName configured.DatabaseName ExecuteFunctionNames.soss checkpoint.PoolFile checkpoint.TestsFile
        let testsFile = SetupInputReferences.copyTests configured.ProjectName configured.DatabaseName checkpoint.TestsFile
        do! SetupArtifacts.saveOnce db poolFile (fun () -> async {
            let pool = EvoAppendPools.convertForSetup ExecuteFunctionNames.soss checkpoint.Tests prefix checkpoint.Pool
            return Ok (outputData.SorterPoolSet pool)
        })
        do! checkCancellation ct
        do! SetupArtifacts.saveOnce db testsFile (fun () -> async { return Ok (outputData.SortableTestsWithPrefix(checkpoint.Tests, checkpoint.Prefix)) })
        return { Tests = testsFile; Pool = poolFile; FirstGeneration = checkpoint.Generation }
    }
