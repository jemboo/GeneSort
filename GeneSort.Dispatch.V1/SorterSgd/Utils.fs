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
            let yab = genSequence |> Seq.toList
            let qua = yab.Length
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

    let loadAvailableSorterPoolSets
            (generationalDb: IGeneSortGenDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<sorterPoolSet>> =
        loadAvailableOutputData
            OutputData.asSorterPoolSet 
            (outputDataType.SorterPoolSet "") 
            generationalDb startingGen rp cts log


    let loadAvailableSorterPoolSetSummarySets
            (generationalDb: IGeneSortGenDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<sorterPoolSetSummarySet>> =
        loadAvailableOutputData
            OutputData.asSorterPoolSetSummarySet 
            (outputDataType.SorterPoolSetSummarySet "") 
            generationalDb startingGen rp cts log


    let loadAvailableSorterPoolSetHistories
            (generationalDb: IGeneSortGenDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<sorterPoolSetHistory>> =
        loadAvailableOutputData
            OutputData.asSorterPoolSetHistory 
            (outputDataType.SorterPoolSetHistory "") 
            generationalDb startingGen rp cts log


    let loadAvailableSorterPoolBins
            (generationalDb: IGeneSortGenDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<sorterPoolBinsSetSeries>> =
        loadAvailableOutputData
            OutputData.asSorterPoolBinsSetSeries 
            (outputDataType.SorterPoolBinsSetSeries "") 
            generationalDb startingGen rp cts log



    /// Optimized search to locate and load ONLY the slice with the highest generation number.
    /// Uses fast file-existence probes to skip expensive MessagePack deserialization.
    /// Loads and extracts a specific output data slice at the highest saved generation.
    let loadOutputDataWithHighestGenerationNumber<'T>
            (extractFn: outputData -> Result<'T, string>)
            (dataType: outputDataType)
            (generationalDb: IGeneSortGenDb)
            (rp: runParameters) : Async<Result<'T option, string>> =
        async {
            let! rawDataOpt = generationalDb.getNextGenSavePointAsync rp dataType
            match rawDataOpt with
            | None -> return Ok None
            | Some rawData -> return extractFn rawData |> Result.map Some
        }

    /// Ergonomic 1-liner wrapper for SorterRunResult.
    let loadHighestGenSorterPoolSet
            (generationalDb: IGeneSortGenDb)
            (rp: runParameters) : Async<Result<sorterPoolSet option, string>> =
        loadOutputDataWithHighestGenerationNumber 
            OutputData.asSorterPoolSet 
            (outputDataType.SorterPoolSet "") 
            generationalDb rp