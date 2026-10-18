namespace GeneSort.Dispatch.V1.SorterSgd

open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.SortingOps
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter

module Utils =

    /// Initial pools may start at an imported generation; test inputs are stored at zero.
    let loadSetupOutputData (genDb: IGeneSortDb) (rp: runParameters) dataType =
        asyncResult {
            let initialGeneration =
                match dataType with
                | geneSortDataType.SorterPoolSet _ -> rp.GetGenerationFirst() |> Option.defaultValue (UMX.tag 0)
                | _ -> UMX.tag 0
            let setupRp = rp.WithGenerationCurrent(Some initialGeneration)
            let! query =
                genDb.makeQueryParamsFromRunParams setupRp dataType
                |> Result.ofOption "Failed to create QueryParams for evolution Setup."
            return! genDb.loadAsync geneSortDataMode.Setup query
        }

    let loadSortableTestsWithPrefix genDb rp =
        asyncResult {
            let! saved = loadSetupOutputData genDb rp (geneSortDataType.SortableTestsWithPrefix "")
            return! OutputData.asSortableTestsWithPrefix saved
        }

    /// Reuses run setup across checkpoints, or creates and persists it before evolution.
    let loadOrCreateSortableTestsWithPrefix
            (makeTests: runParameters -> Async<Result<sortableTests * ce array, string>>)
            (genDb: IGeneSortDb)
            (rp: runParameters) : Async<Result<sortableTests * ce array, string>> =
        asyncResult {
            let setupRp = rp.WithGenerationCurrent(Some (UMX.tag<generationNumber> 0))
            let! query =
                genDb.makeQueryParamsFromRunParams setupRp (geneSortDataType.SortableTestsWithPrefix "")
                |> Result.ofOption "Failed to create QueryParams for Setup SortableTestsWithPrefix."
            let! exists = genDb.doesOutPutDataExist geneSortDataMode.Setup query
            if exists then
                let! savedTests = genDb.loadAsync geneSortDataMode.Setup query
                return! OutputData.asSortableTestsWithPrefix savedTests
            else
                let! (tests, prefix) = makeTests setupRp
                do! genDb.saveAsync geneSortDataMode.Setup query
                        (outputData.SortableTestsWithPrefix (tests, prefix)) (UMX.tag<allowOverwrite> false)
                return tests, prefix
        }

    /// Helper to evaluate one generation step asynchronously without accumulating history.
    let private tryLoadOutputDataForGen<'T>
            (extractFn: outputData -> Result<'T, string>)
            (dataType: geneSortDataType)
            (generationalDb: IGeneSortDb)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit)
            (genInt: int) : Async<Result<'T option, string>> =

        asyncResult {
            do! checkCancellation cts
            let currentGen = %genInt : int<generationNumber>
            let sliceRp = rp.WithGenerationCurrent(Some currentGen)

            match generationalDb.makeQueryParamsFromRunParams sliceRp dataType with
            | None -> 
                log (sprintf "Failed to make query params at generation %d. Stopping search." genInt)
                return None
            | Some qpSlice ->
                let! loadedDataOpt = generationalDb.loadIfFoundAsync geneSortDataMode.Results qpSlice
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
    /// Uses GenIntervalConfig.getSamplesWithMinBound without artificially capping sequence length.
    let loadAvailableOutputData<'T>
            (extractFn: outputData -> Result<'T, string>)
            (dataType: geneSortDataType)
            (saveConfig: genIntervalConfig)
            (generationalDb: IGeneSortDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<'T>> =
        async {
            let genSequence =
                GenIntervalConfig.getSamplesWithMinBound saveConfig %startingGen
                |> Seq.filter (fun generation ->
                    rp.GetGenerationFirst() |> Option.forall (fun first -> generation > %first))
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
            (saveConfig: genIntervalConfig)
            (generationalDb: IGeneSortDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<sorterPoolSet>> =
        loadAvailableOutputData
            OutputData.asSorterPoolSet 
            (geneSortDataType.SorterPoolSet "") 
            saveConfig generationalDb startingGen rp cts log


    let loadAvailableSorterPoolSetSummarySets
            (saveConfig: genIntervalConfig)
            (generationalDb: IGeneSortDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<spSummarySet>> =
        loadAvailableOutputData
            OutputData.asSorterPoolSetSummarySet 
            (geneSortDataType.SorterPoolSetSummarySet "") 
            saveConfig generationalDb startingGen rp cts log


    let loadAvailableSorterPoolSetHistories
            (saveConfig: genIntervalConfig)
            (generationalDb: IGeneSortDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<spsh>> =
        loadAvailableOutputData
            OutputData.asSorterPoolSetHistory 
            (geneSortDataType.SorterPoolSetHistory "") 
            saveConfig generationalDb startingGen rp cts log


    let loadAvailableSorterPoolBins
            (saveConfig: genIntervalConfig)
            (generationalDb: IGeneSortDb)
            (startingGen: int<generationNumber>)
            (rp: runParameters)
            (cts: CancellationToken)
            (log: string -> unit) : Async<seq<sorterPoolBinsSetSeries>> =
        loadAvailableOutputData
            OutputData.asSorterPoolBinsSetSeries 
            (geneSortDataType.SorterPoolBinsSetSeries "") 
            saveConfig generationalDb startingGen rp cts log



    /// Optimized search to locate and load ONLY the slice with the highest generation number.
    /// Uses fast file-existence probes to skip expensive MessagePack deserialization.
    /// Loads and extracts a specific output data slice at the highest saved generation.
    let loadOutputDataWithHighestGenerationNumber<'T>
            (extractFn: outputData -> Result<'T, string>)
            (dataType: geneSortDataType)
            (saveConfig: genIntervalConfig)
            (generationalDb: IGeneSortDb)
            (rp: runParameters) : Async<Result<'T option, string>> =
        async {
            let targetCount = defaultArg saveConfig.MaxCount 200
            let intervals =
                IntSampleMethod.generate saveConfig.Method saveConfig.Min targetCount
                |> Seq.map (fun gen -> int (ceil (float gen * saveConfig.Scale)))
                |> Seq.toArray
                |> Array.map (fun gen -> %gen : int<generationNumber>)
                // An imported initial pool lives only in Setup. Results start after it.
                |> Array.filter (fun gen -> rp.GetGenerationFirst() |> Option.forall (fun first -> gen > first))

            let getQueryParams index =
                let currentGen = intervals.[index]
                let wrp = rp.WithGenerationCurrent(Some currentGen)
                generationalDb.makeQueryParamsFromRunParams wrp dataType

            let rec findHighestExistingIndex low high bestIdx =
                async {
                    if low > high then return bestIdx
                    else
                        let mid = low + (high - low) / 2
                        match getQueryParams mid with
                        | None -> return! findHighestExistingIndex low (mid - 1) bestIdx
                        | Some qp ->
                            let! exists = generationalDb.doesOutPutDataExist geneSortDataMode.Results qp
                            if exists then return! findHighestExistingIndex (mid + 1) high (Some mid)
                            // Generation zero is now stored as Setup, so its absence from
                            // Results must not hide the first evolved checkpoint.
                            elif intervals.[mid] = UMX.tag<generationNumber> 0 then
                                return! findHighestExistingIndex (mid + 1) high bestIdx
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
                        let! rawDataOpt = generationalDb.loadIfFoundAsync geneSortDataMode.Results qp
                        match rawDataOpt with
                        | None -> return Ok None
                        | Some rawData -> return extractFn rawData |> Result.map Some
        }

    /// Ergonomic 1-liner wrapper for SorterRunResult.
    let loadHighestGenSorterPoolSet
            (saveConfig: genIntervalConfig)
            (generationalDb: IGeneSortDb)
            (rp: runParameters) : Async<Result<sorterPoolSet_Standard option, string>> =
        loadOutputDataWithHighestGenerationNumber
            OutputData.asSorterPoolSetStandard
            (geneSortDataType.SorterPoolSet "")
            saveConfig generationalDb rp

    let loadHighestGenSorterPoolSetSoss
            (saveConfig: genIntervalConfig)
            (generationalDb: IGeneSortDb)
            (rp: runParameters) : Async<Result<GeneSort.Eval.V1.Sgd.Soss.sorterPoolSet_Soss option, string>> =
        loadOutputDataWithHighestGenerationNumber
            OutputData.asSorterPoolSetSoss
            (geneSortDataType.SorterPoolSet "")
            saveConfig generationalDb rp
