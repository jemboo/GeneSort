namespace GeneSort.Dispatch.V1.SortableTests

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open GeneSort.Model.Sortable.V1
open GeneSort.Dispatch.V1.OpsUtils
open GeneSort.Sorting
open GeneSort.SortingLib.Sorter

module SortableTestsExecutor =

    let _setupSortableTestsMerge
        (host: runHost)
        (rp: runParameters) 
        (allowOverwrite: bool<allowOverwrite>) 
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        // Local reporting helper 
        let log msg = OpsUtils.report 
                            progress 
                            (sprintf "%s [%s] %s" 
                            (StringUtils.getTimestampString()) 
                            (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                // 1. Initial Check & Sorter Model Creation
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                log "Creating Merge SortableTests..."

                // 2. Safe extraction
                let! (mrgLibId, sortableDataFormat) = 
                    maybe {
                        let! mrgLibId = rp.GetMergeLibId()
                        let! dataFormat = rp.GetSortableDataFormat()
                        return (mrgLibId, dataFormat)
                    } |> Result.ofOption "Missing domain parameters required for generation"

                // 3. Create SortableTestsModel
                let sortableTestsModel = msasM.create 
                                            mrgLibId.SortingWidth
                                            mrgLibId.MergeDimension 
                                            mergeSuffixType.NoSuffix
                                            sorterLibVariant.VariantA
                                        |> sortableTestsModel.MsasMi
            
                let! qpForSortableTests = host.QueryParamsFromRunParams rp (geneSortDataType.SortableTests "")
                                         |> Result.ofOption "Failed to create query parameters for SortableTests"
                let sortableTests = SortableTestsModel.makeSortableTests 
                                            (%qpForSortableTests.Id |> UMX.tag) 
                                            sortableTestsModel 
                                            sortableDataFormat

                // 4. Save
                log (sprintf "Saving SortableTests %s" (string %qpForSortableTests.Id))

                do! host.RunDb.saveAsync geneSortDataMode.Setup qpForSortableTests (sortableTests |> outputData.SortableTests) allowOverwrite
                
                log "Setup complete."
                return rp
            with e -> 
                return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
        } |> Async.map (logResult progress log)



    let _setupSortableTestsPrefix
        (host: runHost)
        (rp: runParameters) 
        (allowOverwrite: bool<allowOverwrite>) 
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        // Local reporting helper 
        let log msg = OpsUtils.report 
                            progress 
                            (sprintf "%s [%s] %s" 
                            (StringUtils.getTimestampString()) 
                            (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                // 1. Initial Check & Sorter Model Creation
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                log "Creating Prefix SortableTests..."

                // 2. Safe extraction
                let! (prefixLibId, sortableDataFormat) = 
                    maybe {
                        let! _pfxLibId = rp.GetPrefixLibId()
                        let! _dataFmt = rp.GetSortableDataFormat()
                        return (_pfxLibId, _dataFmt)
                    } |> Result.ofOption "Missing domain parameters required for generation"

                // 3. Create SortableTestsModel
                let sortableTestsModel = msasPfx.create prefixLibId |> sortableTestsModel.MsasPfx
            
                let! qpForSortableTests = host.QueryParamsFromRunParams rp (geneSortDataType.SortableTests "")
                                         |> Result.ofOption "Failed to create query parameters for SortableTests"
                let sortableTests = SortableTestsModel.makeSortableTests 
                                            (%qpForSortableTests.Id |> UMX.tag) 
                                            sortableTestsModel 
                                            sortableDataFormat

                // 4. Save
                log (sprintf "Saving SortableTests %s" (string %qpForSortableTests.Id))

                do! host.RunDb.saveAsync geneSortDataMode.Setup qpForSortableTests (sortableTests |> outputData.SortableTests) allowOverwrite
                
                log "Setup complete."
                return rp
            with e -> 
                return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
        } |> Async.map (logResult progress log)


    /// Publishes prepared tests as Results for downstream evaluation workflows.
    let _runSortableTests
            (host: runHost)
            (rp: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        let log msg = OpsUtils.report progress msg
        asyncResult {
            try
                do! checkCancellation cts.Token
                let! query = host.QueryParamsFromRunParams rp (geneSortDataType.SortableTests "")
                             |> Result.ofOption "Failed to create query parameters for SortableTests"
                let! saved = host.RunDb.loadAsync geneSortDataMode.Setup query
                let! tests = OutputData.asSortableTests saved
                do! checkCancellation cts.Token
                do! host.RunDb.saveAsync geneSortDataMode.Results query (outputData.SortableTests tests) allowOverwrite
                log "Sortable tests published. Run complete."
                return rp.WithRunFinished(Some true)
            with e ->
                return! Error (sprintf "Error publishing sortable tests: %s" e.Message)
        } |> Async.map (logResult progress log)

    let private setupExecutor prepare =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                prepare host rp allowOverwrite cts progress }

    let getSetupExecutor = function
        | GenMerge -> setupExecutor _setupSortableTestsMerge
        | GenPrefix -> setupExecutor _setupSortableTestsPrefix

    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _runSortableTests host rp allowOverwrite cts progress }

    let prefixExecutor = mergeExecutor

    let getExecutor (executorType: sortableTestsExecutorType) : IRunParamsExecutor =
        match executorType with
        | GenMerge -> mergeExecutor
        | GenPrefix -> prefixExecutor