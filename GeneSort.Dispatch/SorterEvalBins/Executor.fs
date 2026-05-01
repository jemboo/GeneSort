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


module Executor =



    let makeSorterEvalBins 
        (makeSorterModel: runParameters -> sorterModelSet option)
        (makeTests: runParameters -> Sortable.sortableTest option)
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

                let! modelSet = makeSorterModel rp |> Result.ofOption "Failed to create SorterModelSet"
                let fullSorterSet = SorterModelSet.makeSorterSet (%modelSet.Id |> UMX.tag) modelSet

                let! (_: unit) = checkCancellation cts.Token
                let! tests = makeTests rp |> Result.ofOption "Failed to create SortableTests"

                let qpEval = host.MakeQueryParamsFromRunParams rp (outputDataType.SorterSetEval "")
                let sorterSetEval = SorterSetEval.makeSorterSetEval (%qpEval.Id |> UMX.tag) fullSorterSet tests collectTests

                let qpBins = host.MakeQueryParamsFromRunParams rp (outputDataType.SorterEvalBins "")
                let sorterEvalBins = sorterEvalBinsV1.createFromEvals (%qpBins.Id |> UMX.tag) (tests |> SortableTests.getId) sorterSetEval.SorterEvals |> sorterEvalBins.V1

                let! (_: unit) = host.ProjectDb.saveAsync qpBins (sorterEvalBins |> outputData.SorterEvalBins) allowOverwrite
                return rp.WithRunFinished (Some true)
            with e -> 
                return! Error (sprintf "Error in %s: %s" (rp |> RunParameters.getIdString) e.Message) |> async.Return
        }


