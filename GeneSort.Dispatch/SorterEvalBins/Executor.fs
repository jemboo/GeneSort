namespace GeneSort.Dispatch.V1.SorterEvalBins

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
open GeneSort.Model.Sorting.Simple.V1


module Executor =

    type executorType = 
        | Standard
        | Merge
        | BinsReport


    let makeMsasFTests (host:IRunHost) (rp:runParameters) : Sortable.sortableTest option =
        maybe {
            let! sdt = rp.GetSortableDataFormat()
            let! sortingWidth = rp.GetSortingWidth()
            let qpTests = host.MakeQueryParamsFromRunParams rp (outputDataType.SortableTest "")
            let testModel = msasF.create sortingWidth |> sortableTestModel.MsasF
            return SortableTestModel.makeSortableTest (%qpTests.Id |> UMX.tag) testModel sdt
        }


    let makeSorterModelSet (host:IRunHost) (rp:runParameters) : sorterModelSet option =
        maybe {
            let! sortingWidth = rp.GetSortingWidth()
            let! stageLength = rp.GetStageLength()
            let! simpleSorterModelType = rp.GetSimpleSorterModelType()
            let! repl = rp.GetRepl()
            let! sorterCount = rp.GetSorterCount()
            let firstIdx = (%repl * %sorterCount) |> UMX.tag<sorterCount>
            let sorterModelGen = SimpleSorterModelGen.makeUniform 
                                        rngFactory.LcgFactory 
                                        sortingWidth stageLength simpleSorterModelType
                                 |> sorterModelGen.Simple

            let qpModelSet = host.MakeQueryParamsFromRunParams rp (outputDataType.SorterModelSet "")
            return SorterModelGen.makeSorterModelSet 
                            (%qpModelSet.Id |> UMX.tag) firstIdx sorterCount sorterModelGen
        }


    let _makeSorterEvalBins 
        (makeSorterModel: IRunHost -> runParameters -> sorterModelSet option)
        (makeTests: IRunHost ->runParameters -> Sortable.sortableTest option)
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

                let! modelSet = makeSorterModel host rp |> Result.ofOption "Failed to create SorterModelSet"
                let fullSorterSet = SorterModelSet.makeSorterSet (%modelSet.Id |> UMX.tag) modelSet

                let! (_: unit) = checkCancellation cts.Token
                let! tests = makeTests host rp |> Result.ofOption "Failed to create SortableTests"

                let qpEval = host.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                let sorterSetEval = SorterSetEval.makeSorterSetEval (%qpEval.Id |> UMX.tag) fullSorterSet tests collectTests

                let qpBins = host.MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                let sorterEvalBins = sorterEvalBinsV1.createFromEvals 
                                            (%qpBins.Id |> UMX.tag) 
                                            (tests |> SortableTests.getId) 
                                            sorterSetEval.SorterEvals 
                                     |> sorterEvalBins.V1

                let! (_: unit) = host.ProjectDb.saveAsync qpBins (sorterEvalBins |> outputData.SorterEvalBins) allowOverwrite
                return rp.WithRunFinished (Some true)
            with e -> 
                return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
        }


    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host collectTests rp allowOverwrite cts progress =
                _makeSorterEvalBins 
                    makeSorterModelSet
                    makeMsasFTests
                    host collectTests rp allowOverwrite cts progress }

    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host collectTests rp allowOverwrite cts progress =
                _makeSorterEvalBins 
                    makeSorterModelSet
                    makeMsasFTests
                    host collectTests rp allowOverwrite cts progress }

    // --- The Dispatcher ---

    let getExecutor (executorType: executorType) : IRunParamsExecutor =
        match executorType with
        | Standard -> standardExecutor
        | Merge -> mergeExecutor
        | BinsReport -> failwith "BinsReport executor not implemented yet"


