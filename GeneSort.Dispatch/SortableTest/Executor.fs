namespace GeneSort.Dispatch.V1.SortableTest

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1
open GeneSort.Model.Sortable.V1


module Executor =
    let makeSortableTest
        (host: IRunHost)
        (collectTests: bool)
        (rp: runParameters) 
        (allowOverwrite: bool<allowOverwrite>) 
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) : Async<Result<runParameters, string>> =
        
        asyncResult {
            try
                let! (_: unit) = checkCancellation cts.Token
                let runId = rp |> RunParameters.getIdString
                OpsUtils.report progress (sprintf "%s Starting Sorter Run %s" (MathUtils.getTimestampString()) %runId)

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

                // 4. Save (Using Host DB)
                let! (_: unit) = checkCancellation cts.Token
                let! (_: unit) = host.ProjectDb.saveAsync qpForSortableTest (sortableTests |> outputData.SortableTest) allowOverwrite

                return rp.WithRunFinished (Some true)
            with e -> 
                return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
        }


