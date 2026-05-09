namespace GeneSort.Dispatch.V1.SortableTest

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open GeneSort.Model.Sortable.V1


module Executor =

    let _makeSortableMergeTest
        (host: IRunHost)
        (rp: runParameters) 
        (allowOverwrite: bool<allowOverwrite>) 
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        // Local reporting helper 
        let log msg = OpsUtils.report progress (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                // 1. Initial Check & Sorter Model Creation
                let! (_: unit) = checkCancellation cts.Token
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
            
                let qpForSortableTest = host.MakeQueryParamsFromRunParams rp (outputDataType.SortableTest "") 
                let sortableTests = SortableTestModel.makeSortableTest 
                                            (%qpForSortableTest.Id |> UMX.tag) 
                                            sortableTestModel 
                                            sortableDataFormat

                // 4. Save

                log (sprintf "Saving SortableTest %s" (string %qpForSortableTest.Id))

                let! (_: unit) = host.ProjectDb.saveAsync qpForSortableTest (sortableTests |> outputData.SortableTest) allowOverwrite
                
                log "Run Complete."
                return rp.WithRunFinished (Some true)
            with e -> 
                return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
        }



    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                _makeSortableMergeTest 
                    host rp allowOverwrite cts progress }



    let getExecutor (executorType: executorType) : IRunParamsExecutor =
        match executorType with
        | Merge -> mergeExecutor
        | Unknown -> failwith "Unknown executor type"
