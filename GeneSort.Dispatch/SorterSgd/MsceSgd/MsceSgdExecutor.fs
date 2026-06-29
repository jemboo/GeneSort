namespace GeneSort.Dispatch.V1.SorterSgd.Msce

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
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Dispatch.V1.SorterSgd


module MsceSgdExecutor =

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
                                    _dataFormatBitVector512)
            | None ->
                return Error "Failed: One or more RunParameters for StandardTests were missing."
        }


    let makeMergeTests (rp: runParameters) : Async<Result<sortableTest, string>> =
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
                return! SortableMergeTestDb.getMergeSorterTestSet 
                                        repl sw md mst sdf  
            | None ->
                return Error "Failed: One or more RunParameters for MergeTests were missing."
        }


    let private createSeedSorterPoolSetStandard 
            (rp:runParameters) : Async<Result<sorterPoolSet, string>> =
        asyncResult {
            let! sortingWidth = rp.GetSortingWidth() |> Result.ofOption "Missing sorting width."
            let! poolCount = rp.GetSorterPoolCount() |> Result.ofOption "Missing poolCount."
            let! sortersPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
            let! sorterEvalMeasureInitial = rp.GetSorterEvalMeasureInitial() |> Result.ofOption "Missing sorterEvalMeasureInitial."
            let! rngType = rp.GetRngType() |> Result.ofOption "Missing rngType."
            let! simpleSorterModelType = 
                                rp.GetSimpleSorterModelType() 
                                |> Result.ofOption "Missing simpleSorterModelType."

            let! (sorterEvalSelectionType: sorterEvalSelectionType) =
                                rp.GetSorterEvalSelectionType() 
                                |> Result.ofOption "Missing sorterEvalSelectionType"

            
            let! (parentSorterSetEval: sorterSetEval) = 
                SorterEvalDbs.getStandardSorterEvals 
                    sortingWidth 
                    simpleSorterModelType 
                    GeneSort.SortingOps.sorterEvalType.V2

            let seedSorterModelGen = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                    rngType 
                    sortingWidth 
                    simpleSorterModelType

            let sorterEvalSelection = 
                SorterEvalSelection.makeSelection 
                    sorterEvalMeasureInitial 
                    sorterEvalSelectionType 
                    parentSorterSetEval.SorterEvals 
                    parentSorterSetEval.SorterTestId
                
            let seedSorterModelSet = 
                sorterEvalSelection.MakeSorterModelSet 
                    (Guid.Empty |> UMX.tag) 
                    seedSorterModelGen

            return SorterPoolSet.fromSorterModelSet 
                (Guid.NewGuid() |> UMX.tag)
                poolCount
                sortersPerPool
                (0 |> UMX.tag<generationNumber>)
                seedSorterModelSet
        }


    let private createSeedSorterPoolSetMerge
            (rp:runParameters) : Async<Result<sorterPoolSet, string>> =
        asyncResult {
            let! sortingWidth = rp.GetSortingWidth() |> Result.ofOption "Missing sorting width."
            let! poolCount = rp.GetSorterPoolCount() |> Result.ofOption "Missing poolCount."
            let! sortersPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
            let! mergeDimension = rp.GetMergeDimension() |> Result.ofOption "Missing sortersPerPool."
            let! mergeSuffixType = rp.GetMergeSuffixType() |> Result.ofOption "Missing sortersPerPool."
            let! sorterEvalMeasureInitial = rp.GetSorterEvalMeasureInitial() |> Result.ofOption "Missing sorterEvalMeasureInitial."
            let! rngType = rp.GetRngType() |> Result.ofOption "Missing rngType."
            let! simpleSorterModelType = 
                                rp.GetSimpleSorterModelType() 
                                |> Result.ofOption "Missing simpleSorterModelType."

            let! (sorterEvalSelectionType: sorterEvalSelectionType) =
                                rp.GetSorterEvalSelectionType() 
                                |> Result.ofOption "Missing sorterEvalSelectionType"

            
            let! (parentSorterSetEval: sorterSetEval) = 
                SorterEvalDbs.getMergeSorterEvals 
                    sortingWidth 
                    simpleSorterModelType 
                    mergeDimension
                    mergeSuffixType
                    GeneSort.SortingOps.sorterEvalType.V2

            let seedSorterModelGen = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                    rngType 
                    sortingWidth 
                    simpleSorterModelType

            let sorterEvalSelection = 
                SorterEvalSelection.makeSelection 
                    sorterEvalMeasureInitial 
                    sorterEvalSelectionType 
                    parentSorterSetEval.SorterEvals 
                    parentSorterSetEval.SorterTestId
                
            let seedSorterModelSet = 
                sorterEvalSelection.MakeSorterModelSet 
                    (Guid.Empty |> UMX.tag) 
                    seedSorterModelGen

            return SorterPoolSet.fromSorterModelSet 
                (Guid.NewGuid() |> UMX.tag)
                poolCount
                sortersPerPool
                (0 |> UMX.tag<generationNumber>)
                seedSorterModelSet
        }

    /// Dispatches the evolution history run parameters, executes the generative loop via asyncResult,
    /// and manages final state serialization/reporting pipelines.
    let evaluateEvolutionRun
            (makeSortableTests: runParameters -> Async<Result<sortableTest, string>>)
            (sorterPoolSetCreator: runParameters -> Async<Result<sorterPoolSet, string>>)
            (host: IRunHost)
            (rp: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log msg = 
            OpsUtils.report progress 
                (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token

                // 1. Gather all required run metrics and options out of your parameters block securely
                let! genLast = rp.GetGenerationLast() |> Result.ofOption "Missing genLast."
                let! genFirst = rp.GetGenerationFirst() |> Result.ofOption "Missing genFirst."
                let! sortersPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
                let! sorterChildCount = rp.GetSorterChildCount() |> Result.ofOption "Missing sorter child count"
                let! sorterEvalMeasure = rp.GetSorterEvalMeasure() |> Result.ofOption "Missing sorterEvalMeasure."
                let! sorterEvalType = rp.GetSorterEvalType() |> Result.ofOption "Missing sorterEvalType."
                let! rngType = rp.GetRngType() |> Result.ofOption "Missing rngType."

                // 2. Resolve target seed sorterPoolSet collection state depending on genFirst criteria
                let! seedSorterPoolSet = 
                    if %genFirst > 0 then
                        log "Looking up historical sorterPoolSet from database..."
                        let newRp = rp.WithQueryWithGenFirst (Some true)
                        
                        let qpSRRResult = 
                            host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.SorterRunResult "")
                            |> Result.ofOption "Failed to create QueryParams for SorterRunResult."

                        asyncResult {
                            let! qpSRR = qpSRRResult 
                            let! (outData: outputData) = 
                                host.RunDb.loadAsync qpSRR 
                                |> AsyncResult.mapError (fun err -> sprintf "Database load error: %A" err)

                            let! sorterRunRes = outData |> OutputData.asSorterRunResult
                            return sorterRunRes.FinalPoolSet
                        }
                    else
                        // Otherwise create it ..
                        log "Make seedSorterPoolSet..."
                        sorterPoolSetCreator rp

                do! checkCancellation cts.Token
                log "Executing makeSortableTests..."
                let! sortableTest = makeSortableTests rp
                
                log "Making sorterModelMutator..."
                // Rates for mutator creation
                let! mutationRate = rp.GetMutationRate() |> Result.ofOption "Missing mutationRate."
                let! insertionRate = rp.GetInsertionRate() |> Result.ofOption "Missing insertionRate."
                let! deletionRate = rp.GetDeletionRate() |> Result.ofOption "Missing deletionRate."
                let! modificationRate = rp.GetModificationRate() |> Result.ofOption "Missing modificationRate."

                let sorterModelMutator = 
                    SimpleSorterModelMutator.getMsceModelMutator
                                (RngFactory.create rngType) 
                                ExcludeSelfCe 
                                modificationRate 
                                mutationRate
                                insertionRate
                                deletionRate
                    |> sorterModelMutator.Simple

                log "Executing SorterRunResult.runEvolutionAsync..."
                let! (runResult: sorterRunResult) = SorterRunResult.runEvolutionAsync
                                                        genFirst
                                                        (genLast - genFirst)
                                                        sorterModelMutator
                                                        sortersPerPool
                                                        sorterChildCount
                                                        sortableTest
                                                        sorterEvalType
                                                        sorterEvalMeasure
                                                        seedSorterPoolSet
                                                        cts.Token
                                                        log

                // 4. Persistence of run stats mapping results out to output streams

                let newRp = rp.WithQueryWithGenFirst (Some false)
                let! qpSorterRunResult = 
                    host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.SorterRunResult "")
                    |> Result.ofOption "Failed to create QueryParams for SorterRunResult."

                log (sprintf "Saving SorterRunResult - Id: %s" (string qpSorterRunResult.Id))
                do! host.RunDb.saveAsync qpSorterRunResult (runResult |> outputData.SorterRunResult) allowOverwrite
                
                
                log "evaluateEvolutionRun completed."
                return newRp.WithRunFinished (Some true)

            with e -> 
                let errorMsg = sprintf "Error in evaluateEvolutionRun: %s" e.Message
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
                let newRp = rp.WithQueryWithGenFirst (Some false)
                let! qpSorterRunResult = host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.SorterRunResult "")
                                        |> Result.ofOption "Failed to create QueryParams for SorterRunResult."
                let! outB = host.RunDb.loadAsync qpSorterRunResult
                let! (srResult : sorterRunResult) = outB |> OutputData.asSorterRunResult |> Async.singleton

                let reportName = (sprintf "SorterRunResult_report" |> UMX.tag<textReportName>)

                let! qpReport = host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.TextReport reportName)
                                |> Result.ofOption "Failed to create QueryParams for Report."
                let leadCols = qpReport |> QueryParams.makeDataTableRecord
                let details = srResult |> SorterRunResult.toDataTableRecords ""
                let dtrs = dataTableRecord.combineWithMany details leadCols
                let report = DataTableReport.fromDataTableRecords dtrs

                let! (_:unit) = host.RunDb.saveAsync qpReport (report |> outputData.TextReport) allowOverwrite
                let yab = (newRp : runParameters).WithRunFinished(Some true)
                return yab
            with e -> 
               return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message)
        } |> Async.map (logResult progress log)


    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                evaluateEvolutionRun
                    makeStandardTests
                    createSeedSorterPoolSetStandard
                    host rp allowOverwrite cts progress }

    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                evaluateEvolutionRun
                    makeMergeTests
                    createSeedSorterPoolSetMerge
                    host rp allowOverwrite cts progress }

    let fullReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                makeFullReport
                    host rp allowOverwrite cts progress }



    let getExecutor (executorType: sorterSgdExecutorType) : IRunParamsExecutor =
        match executorType with
        | sorterSgdExecutorType.GenStandard -> standardExecutor
        | sorterSgdExecutorType.GenMerge -> mergeExecutor
        | sorterSgdExecutorType.FullReport -> fullReportExecutor
