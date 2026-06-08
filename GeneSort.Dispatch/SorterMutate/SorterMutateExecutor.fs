namespace GeneSort.Dispatch.V1.SorterMutate

open System
open System.Threading
open FsToolkit.ErrorHandling
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1
open GeneSort.Model.Sortable.V1
open GeneSort.Dispatch.V1.OpsUtils
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Dispatch.V1.SortableTest
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Eval.V1




module SorterMutateExecutor =

    let makeStandardTests (rp:runParameters) : Async<Result<Sortable.sortableTest, string>> =
        async {
            let paramsOpt = option {
                let! sortingWidth = rp.GetSortingWidth()
                let sortableTestId = Guid.NewGuid() |> UMX.tag<sortableTestId>
                return (sortingWidth, sortableTestId)
            }
            match paramsOpt with
            | Some (sortingWidth, sortableTestId) ->
                let testModel = msasF.create sortingWidth |> sortableTestModel.MsasF
                return Ok ( SortableTestModel.makeSortableTest 
                                    sortableTestId
                                    testModel 
                                    CommonSorterEval.standardSortableDataFormat)
            | None ->
                return Error "Failed: One or more RunParameters for StandardTests were missing."
        }


    let makeMergeTests (rp: runParameters) : Async<Result<Sortable.sortableTest, string>> =
        async {
            let paramsOpt = option {
                let repl = 0 |> UMX.tag<replNumber>   
                let! sw = rp.GetSortingWidth()
                let! md = rp.GetMergeDimension()
                let! mst = rp.GetMergeSuffixType()
                let! sdf = rp.GetSortableDataFormat()
                return (repl, sw, md, mst, sdf)
            }

            match paramsOpt with
            | Some (repl, sw, md, mst, sdf) ->
                return! SortableTestDb.getMergeSorterTestSet 
                                        repl sw md mst sdf  
            | None ->
                return Error "Failed: One or more RunParameters for MergeTests were missing."
        }


    let makeMutantSorterModels (rp:runParameters) : Async<Result<sorterModel [], string>> =
        asyncResult {

            let! (rngType: rngType) =  
                        rp.GetRngType()
                        |> Result.ofOption "Missing RNG type in run parameters"

            let! (sortingWidth: int<sortingWidth>) = 
                        rp.GetSortingWidth() 
                        |> Result.ofOption "Missing sorting width in run parameters"
    
            let! (simpleSorterModelType: simpleSorterModelType) = 
                        rp.GetSimpleSorterModelType() 
                        |> Result.ofOption "Missing simple sorter model type in run parameters"

            let! (_sorterEvalType: sorterEvalType) =
                        rp.GetSorterEvalType()
                        |> Result.ofOption "Missing sorter eval type in run parameters"

            let! (sorterParentCount: int<sorterCount>) = 
                        rp.GetSorterParentCount()
                        |> Result.ofOption "Missing parent sorter count in run parameters"

            let! (sorterChildCount: int<sorterCount>) = 
                        rp.GetSorterChildCount()
                        |> Result.ofOption "Missing parent sorter count in run parameters"

            let! (mutationRate: float<mutationRate>) =  
                        rp.GetMutationRate()
                        |> Result.ofOption "Missing mutationRate in run parameters"

            let! (insertionRate: float<insertionRate>) =  
                        rp.GetInsertionRate()
                        |> Result.ofOption "Missing insertionRate in run parameters"

            let! (deletionRate: float<deletionRate>) =  
                        rp.GetDeletionRate()
                        |> Result.ofOption "Missing deletionRate in run parameters"

            let! (modificationRate: float<modificationRate>) =  
                        rp.GetModificationRate()
                        |> Result.ofOption "Missing modificationRate in run parameters"

            //let! (sorterEvalSelectionType: groupSelectionType) =  
            //            rp.GetGroupSelectionType()
            //            |> Result.ofOption "Missing sorterEvalSelectionType in run parameters"


            let rngFactory = rngType |> RngFactory.create
            let ranker = SorterEvalFunctions.byEqualTwoWeighted


            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getStandardSorterEvals 
                                            sortingWidth 
                                            simpleSorterModelType
                                            sorterEvalType.V2


            let labeledSorterEvals
                    = SorterEvalSelection.makeTmbSelections
                                        ranker
                                        (%sorterParentCount / 3)
                                        parentSorterSetEval.SorterEvals

            let (parentSorterModelGen: sorterModelGen) = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                                        rngType 
                                        sortingWidth 
                                        simpleSorterModelType

            let parentSorterModelSet = labeledSorterEvals.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen


            let sorterModelMutator = SimpleSorterModelMutator.getSimpleSorterModelMutator 
                                            simpleSorterModelType
                                            rngFactory
                                            CommonSorterMutate.ExcludeSelfCe
                                            modificationRate
                                            mutationRate
                                            insertionRate
                                            deletionRate
                                     |> sorterModelMutator.Simple

            let childIndexes = [| 0 .. (%sorterChildCount - 1) |]

            let makeChildren (parentSorterModel: sorterModel) : sorterModel[] =
                childIndexes
                |> Array.map (fun dex -> SorterModelMutator.makeMutantSorterModelFromIndex
                                            sorterModelMutator
                                            parentSorterModel
                                            dex)

            let allMutantSorterModels = 
                parentSorterModelSet.SorterModels
                |> Array.collect makeChildren

            return allMutantSorterModels
        }



    let makeMutantMergeSorterModels (rp:runParameters) : Async<Result<sorterModel [], string>> =
        asyncResult {

            let! (rngType: rngType) =  
                        rp.GetRngType()
                        |> Result.ofOption "Missing RNG type in run parameters"

            let! (sortingWidth: int<sortingWidth>) = 
                        rp.GetSortingWidth() 
                        |> Result.ofOption "Missing sorting width in run parameters"
    
            let! (simpleSorterModelType: simpleSorterModelType) = 
                        rp.GetSimpleSorterModelType() 
                        |> Result.ofOption "Missing simple sorter model type in run parameters"

            let! (mergeDimension: int<mergeDimension>) = 
                        rp.GetMergeDimension() 
                        |> Result.ofOption "Missing mergeDimension in run parameters"

            let! (mergeSuffixType: mergeSuffixType) = 
                        rp.GetMergeSuffixType() 
                        |> Result.ofOption "Missing mergeSuffixType in run parameters"

            let! (_sorterEvalType: sorterEvalType) =
                        rp.GetSorterEvalType()
                        |> Result.ofOption "Missing sorter eval type in run parameters"

            let! (sorterParentCount: int<sorterCount>) = 
                        rp.GetSorterParentCount()
                        |> Result.ofOption "Missing parent sorter count in run parameters"

            let! (sorterChildCount: int<sorterCount>) = 
                        rp.GetSorterChildCount()
                        |> Result.ofOption "Missing parent sorter count in run parameters"

            let! (mutationRate: float<mutationRate>) =  
                        rp.GetMutationRate()
                        |> Result.ofOption "Missing mutationRate in run parameters"

            let! (insertionRate: float<insertionRate>) =  
                        rp.GetInsertionRate()
                        |> Result.ofOption "Missing insertionRate in run parameters"

            let! (deletionRate: float<deletionRate>) =  
                        rp.GetDeletionRate()
                        |> Result.ofOption "Missing deletionRate in run parameters"

            let! (modificationRate: float<modificationRate>) =  
                        rp.GetModificationRate()
                        |> Result.ofOption "Missing modificationRate in run parameters"

            let! (parentSorterCount: int<sorterCount>) = 
                        rp.GetSorterParentCount()
                        |> Result.ofOption "Missing parent sorter count in run parameters"

                                        
            let rngFactory = rngType |> RngFactory.create
            let ranker = SorterEvalFunctions.byEqualTwoWeighted


            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getMergeSorterEvals 
                                        sortingWidth 
                                        simpleSorterModelType 
                                        mergeDimension
                                        mergeSuffixType
                                        sorterEvalType.V2

            let labeledSorterEvals
                    = SorterEvalSelection.makeTmbSelections
                                        ranker
                                        (%sorterParentCount / 3)
                                        parentSorterSetEval.SorterEvals

            let (parentSorterModelGen: sorterModelGen) = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                                        rngType 
                                        sortingWidth 
                                        simpleSorterModelType

            let parentSorterModelSet = labeledSorterEvals.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen


            let sorterModelMutator = SimpleSorterModelMutator.getSimpleSorterModelMutator 
                                            simpleSorterModelType
                                            rngFactory
                                            CommonSorterMutate.ExcludeSelfCe
                                            modificationRate
                                            mutationRate
                                            insertionRate
                                            deletionRate
                                     |> sorterModelMutator.Simple


            let childIndexes = [| 0 .. (%sorterChildCount - 1) |]

            let makeChildren (parentSorterModel: sorterModel) : sorterModel[] =
                childIndexes
                |> Array.map (fun dex -> SorterModelMutator.makeMutantSorterModelFromIndex
                                            sorterModelMutator
                                            parentSorterModel
                                            dex)

            let allMutantSorterModels = 
                    parentSorterModelSet.SorterModels
                    |> Array.collect makeChildren

            return allMutantSorterModels
        }



    let makeMutantDetails (rp:runParameters) : 
                    Async<Result<sorterEvalSelection * Map<Guid<sorterModelId>, Guid<sorterModelId>>, string>> =
        asyncResult {

            let! (rngType: rngType) =  
                        rp.GetRngType()
                        |> Result.ofOption "Missing RNG type in run parameters"

            let! (sortingWidth: int<sortingWidth>) = 
                        rp.GetSortingWidth() 
                        |> Result.ofOption "Missing sorting width in run parameters"
    
            let! (simpleSorterModelType: simpleSorterModelType) = 
                        rp.GetSimpleSorterModelType() 
                        |> Result.ofOption "Missing simple sorter model type in run parameters"

            let! (_sorterEvalType: sorterEvalType) =
                        rp.GetSorterEvalType()
                        |> Result.ofOption "Missing sorter eval type in run parameters"

            let! (sorterParentCount: int<sorterCount>) = 
                        rp.GetSorterParentCount()
                        |> Result.ofOption "Missing parent sorter count in run parameters"

            let! (sorterChildCount: int<sorterCount>) = 
                        rp.GetSorterChildCount()
                        |> Result.ofOption "Missing parent sorter count in run parameters"

            let! (mutationRate: float<mutationRate>) =  
                        rp.GetMutationRate()
                        |> Result.ofOption "Missing mutationRate in run parameters"

            let! (insertionRate: float<insertionRate>) =  
                        rp.GetInsertionRate()
                        |> Result.ofOption "Missing insertionRate in run parameters"

            let! (deletionRate: float<deletionRate>) =  
                        rp.GetDeletionRate()
                        |> Result.ofOption "Missing deletionRate in run parameters"

            let! (modificationRate: float<modificationRate>) =  
                        rp.GetModificationRate()
                        |> Result.ofOption "Missing modificationRate in run parameters"

            //let! (sorterEvalSelectionType: groupSelectionType) =  
            //            rp.GetGroupSelectionType()
            //            |> Result.ofOption "Missing sorterEvalSelectionType in run parameters"


            let rngFactory = rngType |> RngFactory.create
            let ranker = SorterEvalFunctions.byEqualTwoWeighted


            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getStandardSorterEvals 
                                            sortingWidth 
                                            simpleSorterModelType
                                            sorterEvalType.V2


            let _labeledSorterEvals
                    = SorterEvalSelection.makeTmbSelections
                                        ranker
                                        (%sorterParentCount / 30)
                                        parentSorterSetEval.SorterEvals

            let (parentSorterModelGen: sorterModelGen) = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                                        rngType 
                                        sortingWidth 
                                        simpleSorterModelType

            let parentSorterModelSet = _labeledSorterEvals.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen

            let simpleSorterModels = parentSorterModelSet.SorterModels |> Array.map (SorterModel.asSimpleSorterModel)

            let sorterModelMutator = SimpleSorterModelMutator.getSimpleSorterModelMutator 
                                            simpleSorterModelType
                                            rngFactory
                                            CommonSorterMutate.ExcludeSelfCe
                                            modificationRate
                                            mutationRate
                                            insertionRate
                                            deletionRate

            let parentMutantMap = 
                    SimpleSorterModelMutator.makeMutantIdToParentIdMap
                                        sorterModelMutator
                                        simpleSorterModels
                                        %sorterChildCount

            return (_labeledSorterEvals, parentMutantMap)
        }




    let makeMutantMergeDetails (rp:runParameters) : 
                    Async<Result<sorterEvalSelection * Map<Guid<sorterModelId>, Guid<sorterModelId>>, string>> =
        asyncResult {

            let! (rngType: rngType) =  
                        rp.GetRngType()
                        |> Result.ofOption "Missing RNG type in run parameters"

            let! (sortingWidth: int<sortingWidth>) = 
                        rp.GetSortingWidth() 
                        |> Result.ofOption "Missing sorting width in run parameters"
    
            let! (simpleSorterModelType: simpleSorterModelType) = 
                        rp.GetSimpleSorterModelType() 
                        |> Result.ofOption "Missing simple sorter model type in run parameters"

            let! (mergeDimension: int<mergeDimension>) = 
                        rp.GetMergeDimension() 
                        |> Result.ofOption "Missing mergeDimension in run parameters"

            let! (mergeSuffixType: mergeSuffixType) = 
                        rp.GetMergeSuffixType() 
                        |> Result.ofOption "Missing mergeSuffixType in run parameters"

            let! (_sorterEvalType: sorterEvalType) =
                        rp.GetSorterEvalType()
                        |> Result.ofOption "Missing sorter eval type in run parameters"

            let! (_sorterEvalType: sorterEvalType) =
                        rp.GetSorterEvalType()
                        |> Result.ofOption "Missing sorter eval type in run parameters"

            let! (sorterParentCount: int<sorterCount>) = 
                        rp.GetSorterParentCount()
                        |> Result.ofOption "Missing parent sorter count in run parameters"

            let! (sorterChildCount: int<sorterCount>) = 
                        rp.GetSorterChildCount()
                        |> Result.ofOption "Missing parent sorter count in run parameters"

            let! (mutationRate: float<mutationRate>) =  
                        rp.GetMutationRate()
                        |> Result.ofOption "Missing mutationRate in run parameters"

            let! (insertionRate: float<insertionRate>) =  
                        rp.GetInsertionRate()
                        |> Result.ofOption "Missing insertionRate in run parameters"

            let! (deletionRate: float<deletionRate>) =  
                        rp.GetDeletionRate()
                        |> Result.ofOption "Missing deletionRate in run parameters"

            let! (modificationRate: float<modificationRate>) =  
                        rp.GetModificationRate()
                        |> Result.ofOption "Missing modificationRate in run parameters"

            let! (parentSorterCount: int<sorterCount>) = 
                        rp.GetSorterParentCount()
                        |> Result.ofOption "Missing parent sorter count in run parameters"

                                        
            let rngFactory = rngType |> RngFactory.create
            let ranker = SorterEvalFunctions.byEqualTwoWeighted


            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getMergeSorterEvals 
                                        sortingWidth 
                                        simpleSorterModelType 
                                        mergeDimension 
                                        mergeSuffixType
                                        sorterEvalType.V2

            let _labeledSorterEvals
                    = SorterEvalSelection.makeTmbSelections
                                        ranker
                                        (%sorterParentCount / 30)
                                        parentSorterSetEval.SorterEvals

            let (parentSorterModelGen: sorterModelGen) = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                                        rngType 
                                        sortingWidth 
                                        simpleSorterModelType

            let parentSorterModelSet = _labeledSorterEvals.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen


            let sorterModelMutator = SimpleSorterModelMutator.getSimpleSorterModelMutator 
                                            simpleSorterModelType
                                            rngFactory
                                            CommonSorterMutate.ExcludeSelfCe
                                            modificationRate
                                            mutationRate
                                            insertionRate
                                            deletionRate

            let simpleSorterModels = parentSorterModelSet.SorterModels 
                                        |> Array.map (SorterModel.asSimpleSorterModel)

            let parentMutantMap = 
                    SimpleSorterModelMutator.makeMutantIdToParentIdMap
                                        sorterModelMutator
                                        simpleSorterModels
                                        %sorterChildCount

            return (_labeledSorterEvals, parentMutantMap)
        }





    let _evaluateMutants 
                (makeMutantSorterModels: runParameters -> Async<Result<sorterModel [], string>> )
                (makeSortableTests: runParameters -> Async<Result<sortableTest, string>>)
                (host: IRunHost)
                (rp: runParameters) 
                (allowOverwrite: bool<allowOverwrite>) 
                (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log msg = OpsUtils.report progress 
                        (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                
                // 1. Load the mutant sorter models and common dependencies
                log "Generating Mutant Sorter Models..."
                let! (allMutantModels: sorterModel []) = makeMutantSorterModels rp
                
                let totalSorterCount = allMutantModels.Length
                let sortersPerSplit = 1000
                
                // Calculate total chunks dynamically based on the number of generated mutants
                let splitFactor = Math.Max(1, int (Math.Ceiling(float totalSorterCount / float sortersPerSplit)))
                
                log (sprintf "Initializing Mutant Evaluation over %d chunks for %d total mutants..." splitFactor totalSorterCount)

                let! sorterEvalType =
                    rp.GetSorterEvalType() 
                    |> Result.ofOption "Missing sorterEvalType."

                do! checkCancellation cts.Token
                log "Generating Sortable Tests..."
                let! tests = makeSortableTests rp 

                let! qpSorterSet = 
                    host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSet "")
                    |> Result.ofOption "Failed to create QueryParams for SorterSet."

                let! qpEval = 
                    host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                    |> Result.ofOption "Failed to create QueryParams for SorterSetEval."

                let collectTests = CommonSorterEval.CollectSortableTests
                let testId = tests |> SortableTests.getId
                
                // 2. Setup Accumulators and Chunk Loop
                log "Running Split Sorter Generation, Array Map Evaluations, & Aggregation..."
                let allChunksEvals : sorterEval array[] = Array.zeroCreate splitFactor

                for i in 0 .. (splitFactor - 1) do
                    do! checkCancellation cts.Token
                    log (sprintf "Processing mutant chunk %d of %d..." (i + 1) splitFactor)
                    
                    // Determine the lower and upper bounds for the current slice safely
                    let startIdx = i * sortersPerSplit
                    let endIdx = Math.Min(startIdx + sortersPerSplit - 1, totalSorterCount - 1)
                    
                    // Extract the chunk models
                    let modelChunk = allMutantModels.[startIdx .. endIdx]
                    
                    // Wrap the subset models into an explicit SorterModelSet container
                    let modelSetChunk = sorterModelSet.create (Guid.Empty |> UMX.tag) modelChunk

                    // Materialize into a functional SorterSet chunk
                    let fullSorterSetChunk = 
                        SorterModelSet.makeSorterSet (Guid.Empty |> UMX.tag) modelSetChunk

                    // Compute sorter evaluations directly from the targeted network chunk
                    let sorterEvalsChunk = 
                        SorterSetEval.makeSorterEvals fullSorterSetChunk.Sorters tests sorterEvalType collectTests

                    // Accumulate results and trigger an explicit GC collection cycle
                    allChunksEvals.[i] <- sorterEvalsChunk
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
                do! host.RunDb.saveAsync qpEval (finalSorterSetEval |> outputData.SorterSetEval) allowOverwrite
                
                log "Mutant Evaluation Run Complete."
                return rp.WithRunFinished (Some true)

            with e -> 
                let errorMsg = sprintf "Fatal Error in %s: %s" (rp |> RunParameters.getIdString) e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (logResult progress log)



    let makeFullReport 
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =


        let log msg = OpsUtils.report progress 
                        (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Full Report for Run %s" (MathUtils.getTimestampString()) %runId)
    
                let! qpSorterSetEval = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                                        |> Result.ofOption "Failed to create QueryParams for SorterSetEval."
                let! outB = host.RunDb.loadAsync qpSorterSetEval
                let! (sorterSetEvals : sorterSetEval) = outB |> OutputData.asSorterSetEval |> Async.singleton

                let reportName = (sprintf "FullEvalReport" |> UMX.tag<textReportName>)

                let! qpReport = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = sorterSetEvals |> SorterSetEval.makeFullDataTableRecords
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                let yab = (rp : runParameters).WithRunFinished(Some true)
                return yab
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress log)



    let makeMutantReport
            (mutantDetailsMaker: runParameters -> Async<Result<sorterEvalSelection * Map<Guid<sorterModelId>, Guid<sorterModelId>>, string>> )
            (host: IRunHost)
            (rp: runParameters) 
            (allowOverwrite: bool<allowOverwrite>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =


        let log msg = OpsUtils.report progress 
                        (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Mutant Report for Run %s" (MathUtils.getTimestampString()) %runId)
                let reportName = (sprintf "MutantReport" |> UMX.tag<textReportName>)


                let! (lse, (mutantIdToParentIdMap: Map<Guid<sorterModelId>,Guid<sorterModelId>>)) = mutantDetailsMaker rp


    
                let! qpSorterSetEval = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                                        |> Result.ofOption "Failed to create QueryParams for SorterSetEval."
                let! outB = host.RunDb.loadAsync qpSorterSetEval
                let! (sorterSetEvals : sorterSetEval) = outB |> OutputData.asSorterSetEval |> Async.singleton



                let! qpReport = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let parentRecordMap = lse |> EvalReporting.toDataTableRecords leadCols "Parent_"

                let details =  
                    sorterSetEvals.SorterEvals
                    |> Array.choose (fun se -> 
                        let (sorterModelId : Guid<sorterModelId>) = se |> SorterEval.getSorterId |> UMX.untag |> UMX.tag<sorterModelId>
        
                        // Use an option workflow (or simple pattern match) to safely look up both maps
                        match mutantIdToParentIdMap |> Map.tryFind sorterModelId with
                        | None -> None // Safely ignore if the parent mapping is missing
                        | Some parentSorterModelId ->
                            let parentKey = %parentSorterModelId |> UMX.tag<sorterId>
                            match parentRecordMap |> Map.tryFind parentKey with
                            | None -> None // Safely ignore if the parent record detail is missing
                            | Some parentRecord ->
                                let childRecord = se |> SorterEval.toDataTableRecord
                                Some (dataTableRecord.combine parentRecord childRecord)
                    )

                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs



                let! (_:unit) = host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                return (rp : runParameters).WithRunFinished(Some true)
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress log)




    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _evaluateMutants 
                    makeMutantSorterModels
                    makeStandardTests
                    host rp allowOverwrite cts progress }

    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _evaluateMutants 
                    makeMutantMergeSorterModels
                    makeMergeTests
                    host rp allowOverwrite cts progress }

    let fullReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeFullReport
                    host rp allowOverwrite cts progress }

    let mutantReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeMutantReport
                    makeMutantDetails
                    host rp allowOverwrite cts progress }

    let mutantMergeReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeMutantReport
                    makeMutantMergeDetails
                    host rp allowOverwrite cts progress }



    let getExecutor (executorType: sorterMutateExecutorType) : IRunParamsExecutor =
        match executorType with
        | sorterMutateExecutorType.GenStandard -> standardExecutor
        | sorterMutateExecutorType.GenMerge -> mergeExecutor
        | sorterMutateExecutorType.FullReport -> fullReportExecutor
        | sorterMutateExecutorType.MutantReport -> mutantReportExecutor
        | sorterMutateExecutorType.MutantMergeReport -> mutantMergeReportExecutor





