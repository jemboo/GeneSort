namespace GeneSort.Dispatch.V1.SorterSgd

open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd

module Utils =

    /// Helper to evaluate one generation step asynchronously without accumulating history.
    let private tryLoadOutputDataForGen<'T>
            (extractFn: outputData -> Result<'T, string>)
            (dataType: outputDataType)
            (generationalDb: IGeneSortGenDb)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit)
            (genInt: int) : Async<Result<'T option, string>> =

        asyncResult {
            do! checkCancellation cts
            let currentGen = %genInt : int<generationNumber>
            let sliceRp = rp.WithGenerationCurrent(Some currentGen)

            match generationalDb.MakeQueryParamsFromRunParams sliceRp dataType with
            | None -> 
                log (sprintf "Failed to make query params at generation %d. Stopping search." genInt)
                return None
            | Some qpSlice ->
                let! loadedDataOpt = generationalDb.loadIfFoundAsync qpSlice
                match loadedDataOpt with
                | None ->
                    log (sprintf "No file found for Gen %d. Sequence complete." genInt)
                    return None
                | Some outData ->
                    match extractFn outData with
                    | Ok sliceResult ->
                        log (sprintf "Successfully loaded slice for Gen %d." genInt)
                        return Some sliceResult
                    | Error err ->
                        log (sprintf "Failed to parse data at Gen %d (%s). Ending search sequence." genInt err)
                        return None
        }

    /// Dynamically discovers and yields contiguous slices lazily as an Async sequence generator.
    /// Uses SamplingConfig.getSamplesWithMinBound without artificially capping sequence length.
    let loadAvailableOutputData<'T>
            (extractFn: outputData -> Result<'T, string>)
            (dataType: outputDataType)
            (generationalDb: IGeneSortGenDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<'T>> =
        async {
            let saveConfig = generationalDb.getGenSaveIntervals()
            let genSequence = SamplingConfig.getSamplesWithMinBound saveConfig %startingGen

            let rec discoverLazy (gens: int seq) = seq {
                match Seq.tryHead gens with
                | None -> ()
                | Some currentGenInt ->
                    let stepAsync = tryLoadOutputDataForGen extractFn dataType generationalDb rp cts log currentGenInt
                    match Async.RunSynchronously stepAsync with
                    | Ok (Some slice) -> 
                        yield slice
                        yield! discoverLazy (Seq.tail gens)
                    | Ok None -> ()
                    | Error _ -> ()
            }

            return discoverLazy genSequence
        }

    /// Backwards-compatible SorterRunResult slice loader returning a lazy sequence.
    let loadAvailableSorterRunResults
            (generationalDb: IGeneSortGenDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<sorterRunResult>> =
        loadAvailableOutputData
            OutputData.asSorterRunResult 
            (outputDataType.SorterRunResult "") 
            generationalDb startingGen rp cts log

    /// Generic function to load only the slice with the highest available generation number,
    /// traversing the unbounded sequence lazily without storing prior slices in memory.
    let loadOutputDataWithHighestGenerationNumber<'T>
            (extractFn: outputData -> Result<'T, string>)
            (dataType: outputDataType)
            (generationalDb: IGeneSortGenDb)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<Result<'T option, string>> =
        async {
            let saveConfig = generationalDb.getGenSaveIntervals()
            let genSequence = SamplingConfig.getSamplesWithMinBound saveConfig -1

            let rec findLast (genSeq: int seq) (lastSeen: 'T option) = async {
                match Seq.tryHead genSeq with
                | None -> return Ok lastSeen
                | Some currentGenInt ->
                    let! stepResult = tryLoadOutputDataForGen extractFn dataType generationalDb rp cts log currentGenInt
                    match stepResult with
                    | Ok (Some slice) -> return! findLast (Seq.tail genSeq) (Some slice)
                    | Ok None -> return Ok lastSeen
                    | Error err -> return Error err
            }

            return! findLast genSequence None
        }

    /// Convenience wrapper to retrieve the highest SorterRunResult slice.
    let loadSorterRunResultWithHighestGenerationNumber
            (generationalDb: IGeneSortGenDb)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<Result<sorterRunResult option, string>> =
        loadOutputDataWithHighestGenerationNumber 
            OutputData.asSorterRunResult 
            (outputDataType.SorterRunResult "") 
            generationalDb rp cts log