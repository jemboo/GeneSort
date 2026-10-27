namespace GeneSort.Project.V1.Catalogs.Setup.SorterEval

open System
open FSharp.UMX
open FsToolkit.ErrorHandling
open GeneSort.Core
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.SortingLib.Sorter
open GeneSort.Model.Sortable.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Catalogs.Setup

module SortableTestsMakers =

    /// Persists evaluation setup; callers load it through the same query catalog.
    let storeTests
            (makeTests: runParameters -> Async<Result<sortableTests * ce array, string>>)
            (storage: runStorage)
            (allowOverwrite: bool<allowOverwrite>)
            (rp: runParameters) : Async<Result<unit, string>> =
        asyncResult {
            let! query =
                storage.QueryParamsFromRunParams rp (geneSortDataType.SortableTestsWithPrefix "")
                |> Result.ofOption "Failed to create QueryParams for SortableTestsWithPrefix."
            return! SetupArtifacts.saveOnce storage.RunDb (SetupInputReferences.resolveRecorded rp query) (fun () ->
                makeTests rp |> Async.map (Result.map outputData.SortableTestsWithPrefix))
        }

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

    let getMergeTests (sources: evolutionSetupSources) (rp: runParameters) : Async<Result<sortableTests * (ce array), string>> =
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
                let! res = sources.LoadMergeTests repl mrgLibId sdf
                return Result.map (fun st -> (st, ces |> Array.concat)) res
            | None ->
                return Error "Failed: One or more RunParameters for MergeTests were missing."
        }

    let getPrefixTests (sources: evolutionSetupSources) (rp: runParameters) : Async<Result<sortableTests * (ce array), string>> =
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
                let! res = sources.LoadPrefixTests repl pfxId sdf
                return Result.map (fun st -> (st, ces)) res
            | None ->
                return Error "Failed: One or more RunParameters for PrefixTests were missing."
        }

        
        
        
        
        
        
