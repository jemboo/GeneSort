namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Core
open GeneSort.Db.V1
open FSharp.UMX
open GeneSort.Dispatch.V1
open System.Threading
open GeneSort.Eval.V1.Sgd


module EvolutionOrchestrator =

    /// Reusable engine that runs an evolution algorithm in chunks defined by genReportInterval and saves progress checkpoints.
    let runSlicesInLoop
            (host: IRunHost)
            (rp: runParameters)
            (genFirst: int<generationNumber>)
            (genLast: int<generationNumber>)
            (genReportInterval: int<generationNumber>)
            (initialSeedPoolSet: sorterPoolSet)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationToken)
            (log: string -> unit)
            (runSliceAsync: int<generationNumber> -> int<generationNumber> -> sorterPoolSet -> 
                                          Async<Result<sorterRunResult, string>>) 
            : Async<Result<runParameters, string>> =

        let rec stepLoop 
                    (currentGenFirst: int<generationNumber>) 
                    (currentSeedPoolSet: sorterPoolSet) 
                    (currentRp: runParameters) : Async<Result<runParameters, string>> =
            asyncResult {
                if currentGenFirst >= genLast then
                    return currentRp
                else
                    do! checkCancellation cts

                    let stepSize = min genReportInterval (genLast - currentGenFirst)
                    let currentGenLast = currentGenFirst + stepSize

                    log (sprintf "Stepping evolution: Generation %d -> %d (Report interval: %d)..." 
                            currentGenFirst currentGenLast stepSize)

                    // Execute the engine payload passed in by the executor module
                    let! (runResult: sorterRunResult) = runSliceAsync currentGenFirst stepSize currentSeedPoolSet

                    do! checkCancellation cts

                    let stepRp = 
                        currentRp
                            .WithGenerationCurrent(Some currentGenLast)

                    let! qpSorterRunResult = 
                        host.RunDb.MakeQueryParamsFromRunParams stepRp (outputDataType.SorterRunResult "")
                        |> Result.ofOption "Failed to create QueryParams for SorterRunResult."

                    log (sprintf "Saving SorterRunResult for generation block %d-%d - Id: %s" 
                            currentGenFirst currentGenLast (string qpSorterRunResult.Id))

                    do! host.RunDb.saveAsync qpSorterRunResult (runResult |> outputData.SorterRunResult) allowOverwrite

                    return! stepLoop currentGenLast runResult.FinalPoolSet stepRp
            }

        stepLoop genFirst initialSeedPoolSet rp