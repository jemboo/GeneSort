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
                let repl = 0 |> UMX.tag<replNumber>   
                let! sw = rp.GetSortingWidth()
                let! md = rp.GetMergeDimension()
                let! mst = rp.GetMergeSuffixType()
                let! sdf = rp.GetSortableDataFormat()
                return (repl, sw, md, mst, sdf)
            }

            match paramsOpt with
            | Some (repl, sw, md, mst, sdf) ->
                let! res = SortableTestDbs.Merge.getMergeSorterTestSet repl sw md mst sdf  
                return Result.map (fun st -> (st, [||])) res
            | None ->
                return Error "Failed: One or more RunParameters for MergeTests were missing."
        }

        
    let makePrefixTests (rp: runParameters) : Async<Result<sortableTest * (ce array), string>> =
        async {
            let paramsOpt = option {
                    let repl = 0 |> UMX.tag<replNumber>   
                    let! sorterLibId = rp.GetSorterLibId()
                    let! sdf = rp.GetSortableDataFormat()
                    let! ces = SorterDataParse.getCeArrayFromLib sorterLibId
                    return (repl, sorterLibId, sdf, ces)
                }

            match paramsOpt with
            | Some (repl, sorterLibId, sdf, ces) ->
                let! res = SortableTestDbs.Prefix.getPrefixSorterTestSet repl sorterLibId sdf 
                return Result.map (fun st -> (st, ces)) res
            | None ->
                return Error "Failed: One or more RunParameters for MergeTests were missing."
        }