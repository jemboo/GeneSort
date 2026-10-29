namespace GeneSort.Project.V1.Catalogs.Execution.SorterMutate

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SorterModelSet
open GeneSort.Project.V1.ParamBundles
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Runtime.OpsUtils


module SorterMutateExecution =
    let _evaluateMutants
            (storage: runStorage)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log msg = OpsUtils.report progress 
                        (sprintf "%s [%s] %s" (StringUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let collectTests = false |> UMX.tag<collectNewSortableTests>
                // 1. Load the inputs prepared by the setup phase.
                let! qpModels =
                    storage.QueryParamsFromRunParams rp (geneSortDataType.SorterModelSet "")
                    |> Result.ofOption "Failed to create QueryParams for mutant SorterModelSet."
                let! savedModels = storage.RunDb.loadAsync geneSortDataMode.Setup (SetupInputReferences.resolveRecorded rp qpModels)
                let! modelSet = savedModels |> OutputData.asSorterModelSet
                
                let sortersPerSplit = 1000
                
                let! sorterEvalType =
                    rp.GetSorterEvalType() 
                    |> Result.ofOption "Missing sorterEvalType."

                do! checkCancellation cts.Token
                log "Loading saved Sortable Tests with prefix..."
                let! qpTests =
                    storage.QueryParamsFromRunParams rp (geneSortDataType.SortableTestsWithPrefix "")
                    |> Result.ofOption "Failed to create QueryParams for SortableTestsWithPrefix."
                let! savedTests = storage.RunDb.loadAsync geneSortDataMode.Setup (SetupInputReferences.resolveRecorded rp qpTests)
                let! tests, ces = savedTests |> OutputData.asSortableTestsWithPrefix
                let prefixBlock = ces |> ceBlock.create (Guid.Empty |> UMX.tag) (tests |> SortableTests.getChannelCount)

                let! qpSorterSet = 
                    storage.QueryParamsFromRunParams rp (geneSortDataType.SorterSet "")
                    |> Result.ofOption "Failed to create QueryParams for SorterSet."

                let! qpEval = 
                    storage.QueryParamsFromRunParams rp (geneSortDataType.SorterSetEval "")
                    |> Result.ofOption "Failed to create QueryParams for SorterSetEval."

                let testId = tests |> SortableTests.getId
                
                // 2. Setup Accumulators and Lazy Chunk Loop via Seq.chunkBySize
                log "Running Split Sorter Generation, Stream Chunk Evaluations, & Aggregation..."
                let allChunksEvals = ResizeArray<sorterEval[]>()
                let mutable chunkCounter = 0

                let chunkedMutants = modelSet.SorterModels |> Seq.chunkBySize sortersPerSplit

                for modelChunk in chunkedMutants do
                    do! checkCancellation cts.Token
                    chunkCounter <- chunkCounter + 1
                    log (sprintf "Processing mutant chunk %d..." chunkCounter)
                    
                    // Wrap the subset models into an explicit SorterModelSet container
                    let modelSetChunk = sorterModelSet.create 
                                            (Guid.Empty |> UMX.tag) 
                                            modelChunk
                                            (modelChunk.[0] |> SorterModel.getCeLength)

                    // Materialize into a functional SorterSet chunk
                    let maxCeCount = None
                    let fullSorterSetChunk = 
                        SorterModelSet.makeSorterSet (Guid.Empty |> UMX.tag) maxCeCount modelSetChunk

                    // Compute sorter evaluations directly from the targeted network chunk
                    let sorterEvalsChunk = 
                        SorterSetEval.makeSorterEvals fullSorterSetChunk.Sorters prefixBlock tests sorterEvalType collectTests

                    // Accumulate transient array chunk results
                    allChunksEvals.Add(sorterEvalsChunk)
                    
                    // Explicit GC collection cycle over the finished slice to drop garbage immediately
                    System.Runtime.GCSettings.LargeObjectHeapCompactionMode <- System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce
                    GC.Collect(2, GCCollectionMode.Forced, true, true)

                // 3. Compile Master SorterSetEval structure
                log "Compiling final Master Mutant SorterSetEval structure..."
                let correctSorterSetId = (%qpSorterSet.Id) |> UMX.tag<sorterSetId>
                
                let finalEvalsArray = allChunksEvals |> Array.concat
                let finalSorterSetEval = 
                    sorterSetEval.create 
                        (%qpEval.Id |> UMX.tag) 
                        correctSorterSetId 
                        testId 
                        finalEvalsArray

                // 4. Persistence
                log (sprintf "Saving Combined Mutant SorterSetEval %s" (string %qpEval.Id))
                do! storage.RunDb.saveAsync geneSortDataMode.Results qpEval (finalSorterSetEval |> outputData.SorterSetEval) allowOverwrite
                
                log "Mutant Evaluation Run Complete."
                return rp.WithRunFinished (Some true)

            with e -> 
                let errorMsg = sprintf "Fatal Error in %s: %s" (rp |> RunParameters.getIdString) e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (logResult progress log)




