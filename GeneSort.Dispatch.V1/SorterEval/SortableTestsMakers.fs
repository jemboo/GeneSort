namespace GeneSort.Dispatch.V1.SorterEval

open System
open FsToolkit.ErrorHandling
open FSharp.UMX
open GeneSort.Sorting.Sorter
open GeneSort.Project.V1
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sortable.V1
open GeneSort.Dispatch.V1.SortableTests
open GeneSort.SortingLib.Sorter

module SortableTestsMakers =

    let makeStandardTests (rp: runParameters) : Async<Result<sortableTests * (ce array), string>> =
        async {
            let paramsOpt = option {
                let! sortingWidth = rp.GetSortingWidth()
                let! sdf = rp.GetSortableDataFormat()
                let sortableTestsId = Guid.NewGuid() |> UMX.tag
                return (sortingWidth, sdf, sortableTestsId)
            }

            match paramsOpt with
            | Some (sortingWidth, sdf, sortableTestsId) ->
                let testModel = msasF.create sortingWidth |> sortableTestsModel.MsasF
                let test = SortableTestsModel.makeSortableTests sortableTestsId testModel sdf
                return Ok (test, [||])
            | None ->
                return Error "Failed: One or more RunParameters for StandardTests were missing."
        }

    let makeMergeTests (rp: runParameters) : Async<Result<sortableTests * (ce array), string>> =
        async {
            let paramsOpt = option {
                let repl = 0 |> UMX.tag<replNumber>   
                let! mrgLibId = rp.GetMergeLibId()
                let! sdf = rp.GetSortableDataFormat()
                let! ces = SorterDataParse.getCeArrayFromMergeLib mrgLibId
                return (repl, mrgLibId, sdf, ces)
            }

            match paramsOpt with
            | Some (repl, mrgLibId, sdf, ces) ->
                let! res = SortableTestsDbs.Merge.getMergeSortableTests repl mrgLibId sdf
                return Result.map (fun st -> (st, ces |> Array.concat)) res
            | None ->
                return Error "Failed: One or more RunParameters for MergeTests were missing."
        }

    let getPrefixTests (rp: runParameters) : Async<Result<sortableTests * (ce array), string>> =
        async {
            let paramsOpt = option {
                let repl = 0 |> UMX.tag<replNumber>   
                let! pfxId = rp.GetPrefixLibId()
                let! sdf = rp.GetSortableDataFormat()
                let! ces = SorterDataParse.getCeArrayFromPrefixLib pfxId
                return (repl, pfxId, sdf, ces)
            }

            match paramsOpt with
            | Some (repl, pfxId, sdf, ces) ->
                let! res = SortableTestsDbs.Prefix.getPrefixSortableTests repl pfxId sdf
                return Result.map (fun st -> (st, ces)) res
            | None ->
                return Error "Failed: One or more RunParameters for PrefixTests were missing."
        }

        
        
        
        
        
        
