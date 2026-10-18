namespace GeneSort.Project.V1.Catalogs.Evolution

open GeneSort.Project.V1
open FsToolkit.ErrorHandling
open FSharp.UMX
open System
open GeneSort.Sorting
open GeneSort.Model.Sortable.V1
open GeneSort.Sorting.Sortable

open GeneSort.Sorting.Sorter
open GeneSort.SortingLib.Sorter
open GeneSort.Project.V1.Parameters


module EvolutionTestMakers =

    let makeStandardTests (_sources: evolutionSetupSources) (rp:runParameters) : Async<Result<sortableTests * (ce array), string>> =
        async {
            let paramsOpt = option {
                let! sortingWidth = rp.GetSortingWidth()
                let sortableTestsId = Guid.NewGuid() |> UMX.tag<sortableTestsId>
                return (sortingWidth, sortableTestsId)
            }
            match paramsOpt with
            | Some (sortingWidth, sortableTestsId) ->
                let testModel = msasF.create sortingWidth |> sortableTestsModel.MsasF
                return Ok ((SortableTestsModel.makeSortableTests 
                                    sortableTestsId
                                    testModel 
                                    sortableDataFormat.BitVector512), [||])
            | None ->
                return Error "Failed: One or more RunParameters for StandardTests were missing."
        }

        
    let makeMergeTests (sources: evolutionSetupSources) (rp: runParameters) : Async<Result<sortableTests * (ce array), string>> =
        async {
            let paramsOpt = option {
                let repl = 0 |> UMX.tag<replNumber>   // only repl 0 is relevant
                let! mrgLibId = rp.GetMergeLibId()
                let! sdf = rp.GetSortableDataFormat()
                return (repl, mrgLibId, sdf)
            }

            match paramsOpt with
            | Some (repl, mrgLibId, sdf) ->
                let! res = sources.LoadMergeTests repl mrgLibId sdf
                return Result.map (fun st -> (st, [||])) res
            | None ->
                return Error "Failed: One or more RunParameters for MergeTests were missing."
        }

        
    let makePrefixTests (sources: evolutionSetupSources) (rp: runParameters) : Async<Result<sortableTests * (ce array), string>> =
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
                let! res = sources.LoadPrefixTests repl pfxId sdf
                return Result.map (fun st -> (st, ces)) res
            | None ->
                return Error "Failed: One or more RunParameters for PrefixTests were missing."
        }
