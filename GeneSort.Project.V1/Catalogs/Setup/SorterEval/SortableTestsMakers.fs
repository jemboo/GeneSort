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
                let! channelCount = rp.GetChannelCount()
                let! sdf = rp.GetSortableDataFormat()
                let sortableTestsId = Guid.NewGuid() |> UMX.tag
                return (channelCount, sdf, sortableTestsId)
            }

            match paramsOpt with
            | Some (channelCount, sdf, sortableTestsId) ->
                let testModel = msasF.create channelCount |> sortableTestsModel.MsasF
                let test = SortableTestsModel.makeSortableTests sortableTestsId testModel sdf
                return Ok (test, [||])
            | None ->
                return Error "Failed: One or more RunParameters for StandardTests were missing."
        }

    /// The producer module supplies the database and catalog; setup supplies the query inputs.
    let getModuleTests (sources: workflowSources) (rp: runParameters) : Async<Result<sortableTests * ce array, string>> =
        asyncResult {
            let! moduleName = rp.ParamMap.TryFind ParamKeys.sortableTestsModuleKey
                              |> Result.ofOption "Setup requires SortableTestsModule."
            let! moduleName = ParameterValidation.validateValue ParamKeys.sortableTestsModuleKey moduleName
            let! comparators =
                match rp.GetMergeLibId(), rp.GetPrefixLibId() with
                | Some library, None -> SorterDataParse.getCeArrayFromMergeLib library |> Option.map Array.concat
                | None, Some library -> SorterDataParse.getCeArrayFromPrefixLib library
                | _ -> None
                |> Result.ofOption "Setup requires exactly one valid merge or prefix library."
            let! output = sources.LoadModuleOutput moduleName (setupParameters.create rp.ParamMap) (geneSortDataType.SortableTests "")
            let! tests = OutputData.asSortableTests output
            return tests, comparators
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

        
        
        
        
        
        
