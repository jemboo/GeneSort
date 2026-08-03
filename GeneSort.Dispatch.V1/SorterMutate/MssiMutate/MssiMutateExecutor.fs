namespace GeneSort.Dispatch.V1.SorterMutate.Mssi

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
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.CommonParams


module MssiMutateExecutor =

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
                                    sortableDataFormat.BitVector512)
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
                return! SortableTestDbs.Merge.getMergeSorterTestSet 
                                        repl sw md mst sdf  
            | None ->
                return Error "Failed: One or more RunParameters for MergeTests were missing."
        }


    let makeMutantSorterModels (rp:runParameters) : Async<Result<sorterModel seq, string>> =
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

            let! (sorterChildCount: int<sorterChildCount>) = 
                        rp.GetSorterChildCount()
                        |> Result.ofOption "Missing parent sorterChildCount in run parameters"

            let! (orthoRate: float<orthoRate>) =  
                        rp.GetOrthoRate()
                        |> Result.ofOption "Missing orthoRate in run parameters"

            let! (paraRate: float<paraRate>) =  
                        rp.GetParaRate()
                        |> Result.ofOption "Missing paraRate in run parameters"

            let! (modificationRate: float<modificationRate>) =  
                        rp.GetModificationRate()
                        |> Result.ofOption "Missing modificationRate in run parameters"

            let! (sest: sorterEvalSelectionType) = 
                        rp.GetSorterEvalSelectionType()
                        |> Result.ofOption "Missing sorterEvalSelectionType in run parameters"

            let! (sem:sorterEvalMeasure) = 
                        rp.GetSorterEvalMeasure()
                        |> Result.ofOption "Missing sorterEvalMeasure in run parameters"

            let! (mutationMod: int<mutationMod>) = 
                        rp.GetMutationMod() 
                        |> Result.ofOption "Missing mutationMod in run parameters"

            let rngFactory = rngType |> RngFactory.create

            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getStandardSorterEvals 
                                            sortingWidth 
                                            simpleSorterModelType
                                            sorterEvalType.V2

            let _sorterEvalSelection = 
                            SorterEvalSelection.makeSelection 
                                        sem 
                                        sest
                                        parentSorterSetEval.SorterEvals
                                        parentSorterSetEval.SorterTestId

            let (parentSorterModelGen: sorterModelGen) = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                                        rngType 
                                        sortingWidth 
                                        simpleSorterModelType

            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen

            let sorterModelMutator = SimpleSorterModelMutator.getMssiModelMutator
                                            rngFactory
                                            ExcludeSelfCe
                                            modificationRate
                                            orthoRate
                                            paraRate
                                     |> sorterModelMutator.Simple

            let childIndexes = [| 0 .. (%sorterChildCount - 1) |]

            // Streaming engine via sequence expression
            let generateMutantStream (parents: sorterModel[]) =
                seq {
                    for parentModel in parents do
                        for dex in childIndexes do
                            yield SorterModelMutator.makeMutantSorterModelFromIndexAndMod
                                        sorterModelMutator
                                        parentModel
                                        (dex |> UMX.tag<mutationIndex>)
                                        mutationMod
                }

            return generateMutantStream parentSorterModelSet.SorterModels
        }



    let makeMutantMergeSorterModels (rp:runParameters) : Async<Result<sorterModel seq, string>> =
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

            let! (sorterChildCount: int<sorterChildCount>) = 
                        rp.GetSorterChildCount()
                        |> Result.ofOption "Missing parent sorterChildCount in run parameters"

            let! (orthoRate: float<orthoRate>) =  
                        rp.GetOrthoRate()
                        |> Result.ofOption "Missing orthoRate in run parameters"

            let! (paraRate: float<paraRate>) =  
                        rp.GetParaRate()
                        |> Result.ofOption "Missing paraRate in run parameters"

            let! (modificationRate: float<modificationRate>) =  
                        rp.GetModificationRate()
                        |> Result.ofOption "Missing modificationRate in run parameters"

            let! (sest: sorterEvalSelectionType) = 
                        rp.GetSorterEvalSelectionType()
                        |> Result.ofOption "Missing sorterEvalSelectionType in run parameters"

            let! (sem:sorterEvalMeasure) = 
                        rp.GetSorterEvalMeasure()
                        |> Result.ofOption "Missing sorterEvalMeasure in run parameters"

            let! (mutationMod: int<mutationMod>) = 
                        rp.GetMutationMod() 
                        |> Result.ofOption "Missing mutationMod in run parameters"
                                        
            let rngFactory = rngType |> RngFactory.create

            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getMergeSorterEvals 
                                        sortingWidth 
                                        simpleSorterModelType 
                                        mergeDimension
                                        mergeSuffixType
                                        sorterEvalType.V2

            let _sorterEvalSelection = 
                            SorterEvalSelection.makeSelection 
                                        sem 
                                        sest
                                        parentSorterSetEval.SorterEvals   
                                        parentSorterSetEval.SorterTestId

            let (parentSorterModelGen: sorterModelGen) = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                                        rngType 
                                        sortingWidth 
                                        simpleSorterModelType

            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen

            let sorterModelMutator = SimpleSorterModelMutator.getMssiModelMutator
                                            rngFactory
                                            ExcludeSelfCe
                                            modificationRate
                                            orthoRate
                                            paraRate
                                     |> sorterModelMutator.Simple


            let childIndexes = [| 0 .. (%sorterChildCount - 1) |]

            // Streaming engine via sequence expression
            let generateMutantStream (parents: sorterModel[]) =
                seq {
                    for parentModel in parents do
                        for dex in childIndexes do
                            yield SorterModelMutator.makeMutantSorterModelFromIndexAndMod
                                        sorterModelMutator
                                        parentModel
                                        (dex |> UMX.tag<mutationIndex>)
                                        mutationMod
                }

            return generateMutantStream parentSorterModelSet.SorterModels
        }


    let _evaluateMutants 
            (makeMutantSorterModels: runParameters -> Async<Result<sorterModel seq, string>> )
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
                
                // 1. Fetch mutant sorter models as a lazy stream sequence
                log "Generating Mutant Sorter Models Stream..."
                let! (allMutantStream: sorterModel seq) = makeMutantSorterModels rp
                
                let sortersPerSplit = 1000
                
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

                let collectTests = CollectSortableTests
                let testId = tests |> SortableTests.getId
                
                // 2. Setup Accumulators and Lazy Chunk Loop via Seq.chunkBySize
                log "Running Split Sorter Generation, Stream Chunk Evaluations, & Aggregation..."
                let allChunksEvals = ResizeArray<sorterEval[]>()
                let mutable chunkCounter = 0

                let chunkedMutants = allMutantStream |> Seq.chunkBySize sortersPerSplit

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
                        SorterSetEval.makeSorterEvals fullSorterSetChunk.Sorters tests sorterEvalType collectTests

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
                do! host.RunDb.saveAsync qpEval (finalSorterSetEval |> outputData.SorterSetEval) allowOverwrite
                
                log "Mutant Evaluation Run Complete."
                return rp.WithRunFinished (Some true)

            with e -> 
                let errorMsg = sprintf "Fatal Error in %s: %s" (rp |> RunParameters.getIdString) e.Message
                log errorMsg 
                return! Error errorMsg
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

    let mergeReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeMutantReport
                    Reporting.makeMergeMutantDetails
                    host rp allowOverwrite cts progress }

    let standardReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeMutantReport
                    Reporting.makeStandardMutantDetails
                    host rp allowOverwrite cts progress }



    let getExecutor (executorType: sorterMutateExecutorType) : IRunParamsExecutor =
        match executorType with
        | sorterMutateExecutorType.GenStandard -> standardExecutor
        | sorterMutateExecutorType.GenMerge -> mergeExecutor
        | sorterMutateExecutorType.MergeReport -> mergeReportExecutor
        | sorterMutateExecutorType.StandardReport -> standardReportExecutor





