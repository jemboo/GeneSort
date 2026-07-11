namespace GeneSort.Dispatch.V1.SortableTest

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open GeneSort.Model.Sortable.V1
open GeneSort.Dispatch.V1.OpsUtils


module SortableTestExecutor =

    let _makeSortableTestMerge
        (host: IRunHost)
        (rp: runParameters) 
        (allowOverwrite: bool<allowOverwrite>) 
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        // Local reporting helper 
        let log msg = OpsUtils.report 
                            progress 
                            (sprintf "%s [%s] %s" 
                            (MathUtils.getTimestampString()) 
                            (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                // 1. Initial Check & Sorter Model Creation
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                log "Creating SortableTest..."

                // 2. Safe extraction
                let! (sortingWidth, mergeDim, mergeSufixType, sortableDataFormat) = 
                    maybe {
                        let! width = rp.GetSortingWidth()
                        let! mergeDim = rp.GetMergeDimension()
                        let! suffixFill = rp.GetMergeSuffixType()
                        let! dataFormat = rp.GetSortableDataFormat()
                        return (width, mergeDim, suffixFill, dataFormat)
                    } |> Result.ofOption "Missing domain parameters required for generation"

                // 3. Create SortableTestModel
                let sortableTestModel = msasM.create sortingWidth mergeDim mergeSufixType |> sortableTestModel.MsasMi
            
                let! qpForSortableTest = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SortableTest "") 
                                         |> Result.ofOption "Failed to create query parameters for SortableTest"
                let sortableTests = SortableTestModel.makeSortableTest 
                                            (%qpForSortableTest.Id |> UMX.tag) 
                                            sortableTestModel 
                                            sortableDataFormat

                // 4. Save
                log (sprintf "Saving SortableTest %s" (string %qpForSortableTest.Id))

                do! host.RunDb.saveAsync qpForSortableTest (sortableTests |> outputData.SortableTest) allowOverwrite
                
                log "Run Complete."
                return rp.WithRunFinished (Some true)
            with e -> 
                return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
        } |> Async.map (logResult progress log)



    let _makeSortableTestPrefix
        (host: IRunHost)
        (rp: runParameters) 
        (allowOverwrite: bool<allowOverwrite>) 
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        // Local reporting helper 
        let log msg = OpsUtils.report 
                            progress 
                            (sprintf "%s [%s] %s" 
                            (MathUtils.getTimestampString()) 
                            (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                // 1. Initial Check & Sorter Model Creation
                do! checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                log "Creating SortableTest..."

                // 2. Safe extraction
                let! (sorterLibId, sortableDataFormat) = 
                    maybe {
                        let! _slLibId = rp.GetSorterLibId()
                        let! _dataFmt = rp.GetSortableDataFormat()
                        return (_slLibId, _dataFmt)
                    } |> Result.ofOption "Missing domain parameters required for generation"

                // 3. Create SortableTestModel
                let sortableTestModel = msasPfx.create sorterLibId |> sortableTestModel.MsasPfx
            
                let! qpForSortableTest = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SortableTest "") 
                                         |> Result.ofOption "Failed to create query parameters for SortableTest"
                let sortableTests = SortableTestModel.makeSortableTest 
                                            (%qpForSortableTest.Id |> UMX.tag) 
                                            sortableTestModel 
                                            sortableDataFormat

                // 4. Save
                log (sprintf "Saving SortableTest %s" (string %qpForSortableTest.Id))

                do! host.RunDb.saveAsync qpForSortableTest (sortableTests |> outputData.SortableTest) allowOverwrite
                
                log "Run Complete."
                return rp.WithRunFinished (Some true)
            with e -> 
                return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
        } |> Async.map (logResult progress log)


    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeSortableTestMerge 
                    host rp allowOverwrite cts progress }

    let prefixExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeSortableTestPrefix 
                    host rp allowOverwrite cts progress }

    let getExecutor (executorType: sortableTestExecutorType) : IRunParamsExecutor =
        match executorType with
        | GenMerge -> mergeExecutor
        | GenPrefix -> prefixExecutor
