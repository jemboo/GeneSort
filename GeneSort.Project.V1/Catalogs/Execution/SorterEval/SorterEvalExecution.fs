namespace GeneSort.Project.V1.Catalogs.Execution.SorterEval

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SorterModelSet
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Runtime.OpsUtils



module SorterEvalExecution =
    let _makeSorterEvals 
            (storage: runStorage)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log (msg: string) : unit = 
            OpsUtils.report progress 
                (sprintf "%s [%s] %s" (StringUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                
                // 1. Unpack run configuration
                let! qpModels =
                    storage.QueryParamsFromRunParams rp (geneSortDataType.SorterModelSet "")
                    |> Result.ofOption "Failed to create QueryParams for SorterModelSet."
                log "Loading saved SorterModelSet..."
                let! savedModels = storage.RunDb.loadAsync geneSortDataMode.Setup (SetupInputReferences.resolveRecorded rp qpModels)
                let! modelSet = savedModels |> OutputData.asSorterModelSet

                let! sWidth = 
                    rp.GetSortingWidth() 
                    |> Result.ofOption "Missing sorting width."

                let! sorterEvalType =
                    rp.GetSorterEvalType() 
                    |> Result.ofOption "Missing sorterEvalType."

                let! collectSortableTests =
                    rp.GetCollectNewSortableTests() 
                    |> Result.ofOption "Missing collectNewSortableTests."

                // 2. Load saved evaluation dependencies
                do! checkCancellation cts.Token
                let! qpTests =
                    storage.QueryParamsFromRunParams rp (geneSortDataType.SortableTestsWithPrefix "")
                    |> Result.ofOption "Failed to create QueryParams for SortableTestsWithPrefix."
                log "Loading saved Sortable Tests with prefix..."
                let! savedTests = storage.RunDb.loadAsync geneSortDataMode.Setup (SetupInputReferences.resolveRecorded rp qpTests)
                let! tests, ces = savedTests |> OutputData.asSortableTestsWithPrefix
                let prefix = ceBlock.create (Guid.Empty |> UMX.tag) sWidth ces

                let! qpSorterSet = 
                    storage.QueryParamsFromRunParams rp (geneSortDataType.SorterSet "")
                    |> Result.ofOption "Failed to create QueryParams for SorterSet."

                let! qpEval = 
                    storage.QueryParamsFromRunParams rp (geneSortDataType.SorterSetEval "")
                    |> Result.ofOption "Failed to create QueryParams for SorterSetEval."

                let testId = tests |> SortableTests.getId

                // 3. Materialize full SorterSet and Evaluate
                log (sprintf "Evaluating SorterModelSet with %d sorters..." modelSet.SorterModels.Length)
                
                let maxCeCount = None
                let fullSorterSet = SorterModelSet.makeSorterSet (Guid.Empty |> UMX.tag) maxCeCount modelSet

                do! checkCancellation cts.Token
                let sorterEvalsArray = 
                    SorterSetEval.makeSorterEvals fullSorterSet.Sorters prefix tests sorterEvalType collectSortableTests

                // 4. Build Master SorterSetEval
                log "Compiling final SorterSetEval structure..."
                let correctSorterSetId = (%qpSorterSet.Id) |> UMX.tag<sorterSetId>

                let finalSorterSetEval = 
                    sorterSetEval.create 
                        (%qpEval.Id |> UMX.tag) 
                        correctSorterSetId 
                        testId 
                        sorterEvalsArray

                // 5. Persistence
                log (sprintf "Saving Combined SorterSetEval %s" (string %qpEval.Id))
                do! storage.RunDb.saveAsync geneSortDataMode.Results qpEval (finalSorterSetEval |> outputData.SorterSetEval) allowOverwrite
                
                log "Run Complete."
                return rp.WithRunFinished (Some true)

            with e -> 
                let errorMsg = sprintf "Fatal Error in %s: %s" (rp |> RunParameters.getIdString) e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (logResult progress log)



        
