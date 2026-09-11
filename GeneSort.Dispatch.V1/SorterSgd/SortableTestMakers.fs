namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1
open FsToolkit.ErrorHandling
open FSharp.UMX
open System
open GeneSort.Sorting
open GeneSort.Model.Sortable.V1
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1.SortableTest
open GeneSort.Sorting.Sorter
open GeneSort.SortingLib.Sorter


module SortableTestMakers =

    let makeStandardTests (rp:runParameters) : Async<Result<sortableTest * (ce array), string>> =
        async {
            let paramsOpt = option {
                let! sortingWidth = rp.GetSortingWidth()
                let sortableTestId = Guid.NewGuid() |> UMX.tag<sortableTestId>
                return (sortingWidth, sortableTestId)
            }
            match paramsOpt with
            | Some (sortingWidth, sortableTestId) ->
                let testModel = msasF.create sortingWidth |> sortableTestModel.MsasF
                return Ok (( SortableTestModel.makeSortableTest 
                                    sortableTestId
                                    testModel 
                                    sortableDataFormat.BitVector512), [||])
            | None ->
                return Error "Failed: One or more RunParameters for StandardTests were missing."
        }

        
    let makeMergeTests (rp: runParameters) : Async<Result<sortableTest * (ce array), string>> =
        async {
            let paramsOpt = option {
                let repl = 0 |> UMX.tag<replNumber>   // only repl 0 is relevant
                let! mrgLibId = rp.GetMergeLibId()
                let! sdf = rp.GetSortableDataFormat()
                return (repl, mrgLibId, sdf)
            }

            match paramsOpt with
            | Some (repl, mrgLibId, sdf) ->
                let! res = SortableTestDbs.Merge.getMergeSorterTestSet repl mrgLibId sdf  
                return Result.map (fun st -> (st, [||])) res
            | None ->
                return Error "Failed: One or more RunParameters for MergeTests were missing."
        }

        
    let makePrefixTests (rp: runParameters) : Async<Result<sortableTest * (ce array), string>> =
        async {
            let paramsOpt = option {
                    let repl = 0 |> UMX.tag<replNumber>  // only repl 0 is relevant
                    let! pfxId = rp.GetPrefixLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    let! ces = SorterDataParse.getCeArrayFromPrefixLib pfxId
                    return (repl, pfxId, sdf, ces)
                }

            match paramsOpt with
            | Some (repl, pfxId, sdf, ces) ->
                let! res = SortableTestDbs.Prefix.getPrefixSorterTestSet repl pfxId  sdf 
                return Result.map (fun st -> (st, ces)) res
            | None ->
                return Error "Failed: One or more RunParameters for PrefixTests were missing."
        }