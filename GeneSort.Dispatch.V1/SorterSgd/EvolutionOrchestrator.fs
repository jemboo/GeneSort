namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Core
open GeneSort.Db.V1
open FSharp.UMX
open GeneSort.Dispatch.V1
open System.Threading
open GeneSort.Eval.V1.Sgd
open GeneSort.SortingOps

module EvolutionOrchestrator =

    /// Reusable engine that runs an evolution algorithm in chunks defined by genSliceSize and saves progress checkpoints.
    let runSlicesInLoop
            (host: IRunHost)
            (rp: runParameters)
            (genFirst: int<generationNumber>)
            (genLast: int<generationNumber>)
            (genSliceSize: int<generationNumber>)
            (measure: sorterEvalMeasure)
            (sorterPoolExpansionRate: int<sorterPoolExpansionRate>)
            (initialSeedPoolSet: sorterPoolSet)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationToken)
            (log: string -> unit)
            (runSliceAsync: int<generationNumber> -> int<generationNumber> -> sorterPoolSet -> 
                                        Async<Result<sorterRunResult, string>>) 
            : Async<Result<runParameters, string>> =


        let rec stepLoop 
                    (currentGenFirst: int<generationNumber>) 
                    (currentPoolSet: sorterPoolSet) 
                    (currentRp: runParameters) : Async<Result<runParameters, string>> =

            asyncResult {
                if currentGenFirst >= genLast then
                    return currentRp
                else
                    do! checkCancellation cts

                    let stepSize = min genSliceSize (genLast - currentGenFirst)
                    let currentGenLast = currentGenFirst + stepSize

                    log (sprintf "Selecting and expanding pools for slice: Generation %d -> %d (Expansion rate: %d)..." 
                            currentGenFirst currentGenLast %sorterPoolExpansionRate)

                    // Execute pool selection logic every genSliceSize boundary:
                    // 1. Trim the pools down to the top-performing fraction
                    // 2. Expand back out by multiplying pools and assigning distinct mutationMod values
                    let expandedPoolSet =
                        if sorterPoolExpansionRate = 1<sorterPoolExpansionRate> then
                            currentPoolSet
                        else
                            currentPoolSet
                            |> SorterPoolSet.trimPools sorterPoolExpansionRate measure
                            |> SorterPoolSet.expandPools sorterPoolExpansionRate

                    log (sprintf "Stepping evolution: Generation %d -> %d (Report interval: %d)..." 
                            currentGenFirst currentGenLast stepSize)

                    // Execute the engine payload passed in by the executor module
                    let! (runResult: sorterRunResult) = runSliceAsync currentGenFirst stepSize expandedPoolSet

                    do! checkCancellation cts

                    let stepRp = currentRp.WithGenerationCurrent(Some currentGenLast)

                    let! qpSorterRunResult = 
                        host.RunDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterRunResult "")
                        |> Result.ofOption "Failed to create QueryParams for SorterRunResult."

                    log (sprintf "Saving SorterRunResult for generation block %d-%d - Id: %s" 
                            currentGenFirst currentGenLast (string qpSorterRunResult.Id))

                    do! host.RunDb.saveAsync qpSorterRunResult (runResult |> outputData.SorterRunResult) allowOverwrite

                    return! stepLoop currentGenLast runResult.FinalPoolSet stepRp
            }

        if %genFirst = 0 then
            let srr0: sorterRunResult = sorterRunResult.create initialSeedPoolSet [||]
            let qpStep0 = (host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.SorterRunResult "")).Value
            host.RunDb.saveAsync qpStep0 (srr0 |> outputData.SorterRunResult) allowOverwrite |> ignore

        stepLoop genFirst initialSeedPoolSet rp