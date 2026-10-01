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
            (generationalDb: IGeneSortDb)
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
            (saveConfig: samplingConfig)
            (generationalDb: IGeneSortDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<'T>> =
        async {
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
            (saveConfig: samplingConfig)
            (generationalDb: IGeneSortDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<sorterPoolSet>> =
        loadAvailableOutputData
            OutputData.asSorterPoolSet 
            (outputDataType.SorterPoolSet "") 
            saveConfig generationalDb startingGen rp cts log


    let loadAvailableSorterPoolSetSummarySets
            (saveConfig: samplingConfig)
            (generationalDb: IGeneSortDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<sorterPoolSetSummarySet>> =
        loadAvailableOutputData
            OutputData.asSorterPoolSetSummarySet 
            (outputDataType.SorterPoolSetSummarySet "") 
            saveConfig generationalDb startingGen rp cts log


    let loadAvailableSorterPoolSetHistories
            (saveConfig: samplingConfig)
            (generationalDb: IGeneSortDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<sorterPoolSetHistory>> =
        loadAvailableOutputData
            OutputData.asSorterPoolSetHistory 
            (outputDataType.SorterPoolSetHistory "") 
            saveConfig generationalDb startingGen rp cts log


    let loadAvailableSorterPoolBins
            (saveConfig: samplingConfig)
            (generationalDb: IGeneSortDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<sorterPoolBinsSetSeries>> =
        loadAvailableOutputData
            OutputData.asSorterPoolBinsSetSeries 
            (outputDataType.SorterPoolBinsSetSeries "") 
            saveConfig generationalDb startingGen rp cts log



    /// Optimized search to locate and load ONLY the slice with the highest generation number.
    /// Uses fast file-existence probes to skip expensive MessagePack deserialization.
    /// Loads and extracts a specific output data slice at the highest saved generation.
    let loadOutputDataWithHighestGenerationNumber<'T>
            (extractFn: outputData -> Result<'T, string>)
            (dataType: outputDataType)
            (saveConfig: samplingConfig)
            (generationalDb: IGeneSortDb)
            (rp: runParameters) : Async<Result<'T option, string>> =
        async {
            let targetCount = defaultArg saveConfig.MaxCount 200
            let intervals =
                IntSampleMethod.generate saveConfig.Method saveConfig.Min targetCount
                |> Seq.map (fun gen -> int (ceil (float gen * saveConfig.Scale)))
                |> Seq.toArray
                |> Array.map (fun gen -> %gen : int<generationNumber>)

            let getQueryParams index =
                let currentGen = intervals.[index]
                let wrp = rp.WithGenerationCurrent(Some currentGen)
                generationalDb.MakeQueryParamsFromRunParams wrp dataType

            let rec findHighestExistingIndex low high bestIdx =
                async {
                    if low > high then return bestIdx
                    else
                        let mid = low + (high - low) / 2
                        match getQueryParams mid with
                        | None -> return! findHighestExistingIndex low (mid - 1) bestIdx
                        | Some qp ->
                            let! exists = generationalDb.doesOutPutDataExist qp
                            if exists then return! findHighestExistingIndex (mid + 1) high (Some mid)
                            else return! findHighestExistingIndex low (mid - 1) bestIdx
                }

            if intervals.Length = 0 then return Ok None
            else
                let! highestIndexOpt = findHighestExistingIndex 0 (intervals.Length - 1) None
                match highestIndexOpt with
                | None -> return Ok None
                | Some idx ->
                    match getQueryParams idx with
                    | None -> return Error "Failed to create query parameters for the highest saved generation."
                    | Some qp ->
                        let! rawDataOpt = generationalDb.loadIfFoundAsync qp
                        match rawDataOpt with
                        | None -> return Ok None
                        | Some rawData -> return extractFn rawData |> Result.map Some
        }

    /// Ergonomic 1-liner wrapper for SorterRunResult.
    let loadHighestGenSorterPoolSet
            (saveConfig: samplingConfig)
            (generationalDb: IGeneSortDb)
            (rp: runParameters) : Async<Result<sorterPoolSet option, string>> =
        loadOutputDataWithHighestGenerationNumber 
            OutputData.asSorterPoolSet 
            (outputDataType.SorterPoolSet "") 
            saveConfig generationalDb rp
