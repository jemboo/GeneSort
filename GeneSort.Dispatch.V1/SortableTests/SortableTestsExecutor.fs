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

    let _makeSortableTestsMerge
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
            
                let! qpForSortableTests = host.QueryParamsFromRunParams rp (outputDataType.SortableTests "")
                                         |> Result.ofOption "Failed to create query parameters for SortableTests"
                let sortableTests = SortableTestsModel.makeSortableTests 
                                            (%qpForSortableTests.Id |> UMX.tag) 
                                            sortableTestsModel 
                                            sortableDataFormat

                // 4. Save
                log (sprintf "Saving SortableTests %s" (string %qpForSortableTests.Id))

                do! host.RunDb.saveAsync qpForSortableTests (sortableTests |> outputData.SortableTests) allowOverwrite
                
                log "Run Complete."
                return rp.WithRunFinished (Some true)
            with e -> 
                return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
        } |> Async.map (logResult progress log)



    let _makeSortableTestsPrefix
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
            
                let! qpForSortableTests = host.QueryParamsFromRunParams rp (outputDataType.SortableTests "")
                                         |> Result.ofOption "Failed to create query parameters for SortableTests"
                let sortableTests = SortableTestsModel.makeSortableTests 
                                            (%qpForSortableTests.Id |> UMX.tag) 
                                            sortableTestsModel 
                                            sortableDataFormat

                // 4. Save
                log (sprintf "Saving SortableTests %s" (string %qpForSortableTests.Id))

                do! host.RunDb.saveAsync qpForSortableTests (sortableTests |> outputData.SortableTests) allowOverwrite
                
                log "Run Complete."
                return rp.WithRunFinished (Some true)
            with e -> 
                return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
        } |> Async.map (logResult progress log)


    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeSortableTestsMerge 
                    host rp allowOverwrite cts progress }

    let prefixExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeSortableTestsPrefix 
                    host rp allowOverwrite cts progress }

    let getExecutor (executorType: sortableTestsExecutorType) : IRunParamsExecutor =
        match executorType with
        | GenMerge -> mergeExecutor
        | GenPrefix -> prefixExecutor
