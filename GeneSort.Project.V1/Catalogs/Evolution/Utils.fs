namespace GeneSort.Project.V1.Catalogs.Evolution

open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Soss
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Catalogs.Setup

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
            return! genDb.loadAsync geneSortDataMode.Setup (SetupInputReferences.resolveRecorded rp query)
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
            let! saved = SetupArtifacts.getOrCreate genDb (SetupInputReferences.resolveRecorded rp query) (fun () ->
                makeTests setupRp |> Async.map (Result.map outputData.SortableTestsWithPrefix))
            return! OutputData.asSortableTestsWithPrefix saved
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
    /// Stops at the configured horizon even when another run has saved later slices.
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
            let lastGeneration =
                rp.GetGenerationIntervalLast()
                |> Option.map (fun interval ->
                    GenIntervalConfig.getSampleAtIndex saveConfig (int interval)
                    |> Option.defaultWith (fun () -> invalidArg "rp" "Invalid final generation interval."))
            let genSequence =
                GenIntervalConfig.getSamplesWithMinBound saveConfig %startingGen
                |> Seq.takeWhile (fun generation -> lastGeneration |> Option.forall (fun last -> generation <= last))
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



    /// Search the run's complete save schedule, loading only the highest saved slice.
    /// File databases list existing IDs once; other stores use existence probes.
    let loadOutputDataWithSchedules<'T>
            (checkpointConfig: genIntervalConfig option)
            (extractFn: outputData -> Result<'T, string>)
            (dataType: geneSortDataType)
            (saveConfig: genIntervalConfig)
            (generationalDb: IGeneSortDb)
            (rp: runParameters) : Async<Result<'T option, string>> =
        async {
            // GenerationIntervalLast is a zero-based schedule index, not a generation number.
            // MaxCount limits finite schedules; unbounded schedules use the run's endpoint.
            let targetCount =
                match rp.GetGenerationIntervalLast(), saveConfig.MaxCount with
                | Some last, _ when int last < 0 || int last = System.Int32.MaxValue ->
                    Error "Invalid last generation interval for checkpoint search."
                | _, Some count when count < 0 -> Error "Invalid save interval MaxCount."
                | Some last, Some count -> Ok (min (int last + 1) count)
                | Some last, None -> Ok (int last + 1)
                | None, Some count -> Ok count
                | None, None -> Error "Checkpoint search requires GenerationIntervalLast or a finite save interval MaxCount."

            match targetCount with
            | Error error -> return Error error
            | Ok count ->
                let intervals =
                    IntSampleMethod.generate saveConfig.Method saveConfig.Min count
                    |> Seq.map (fun gen -> UMX.tag<generationNumber> (int (ceil (float gen * saveConfig.Scale))))
                    // An imported initial pool lives only in Setup. Results start after it.
                    |> Seq.filter (fun gen -> rp.GetGenerationFirst() |> Option.forall (fun first -> gen > first))
                    |> Seq.toArray
                let intervals =
                    match checkpointConfig, Array.tryLast intervals with
                    | Some config, Some last ->
                        GenIntervalConfig.getSampleSetMaxBound config (int last)
                        |> Seq.map UMX.tag<generationNumber>
                        |> Seq.append intervals
                        |> Seq.filter (fun gen -> rp.GetGenerationFirst() |> Option.forall (fun first -> gen > first))
                        |> Seq.distinct |> Seq.sort |> Seq.toArray
                    | _ -> intervals

                let getQueryParams index =
                    generationalDb.makeQueryParamsFromRunParams (rp.WithGenerationCurrent(Some intervals.[index])) dataType

                let! existingIds = async {
                    match generationalDb with
                    | :? IOutputDataListing as listing ->
                        match intervals |> Array.tryPick (fun gen ->
                            generationalDb.makeQueryParamsFromRunParams (rp.WithGenerationCurrent(Some gen)) dataType) with
                        | None -> return Some Set.empty
                        | Some query ->
                            let! ids = listing.getOutputIds geneSortDataMode.Results query
                            return Some ids
                    | _ -> return None
                }

                // Sparse retained checkpoints and stale saved progress are supported.
                // Do not stop at a gap or assume that GenerationCurrent is up to date.
                let rec findHighest index = async {
                    if index < 0 then return None
                    else
                        match getQueryParams index with
                        | None -> return! findHighest (index - 1)
                        | Some query ->
                            let! exists =
                                match existingIds with
                                | Some ids -> async.Return (Set.contains query.Id ids)
                                | None -> generationalDb.doesOutPutDataExist geneSortDataMode.Results query
                            if exists then
                                let! raw = generationalDb.loadIfFoundAsync geneSortDataMode.Results query
                                match raw with
                                | Some data ->
                                    match extractFn data with
                                    | Ok value -> return Some value
                                    | Error _ -> return! findHighest (index - 1)
                                | None -> return! findHighest (index - 1)
                            else return! findHighest (index - 1)
                }

                let! highest =
                    if existingIds |> Option.exists Set.isEmpty then async.Return None
                    else findHighest (intervals.Length - 1)
                return Ok highest
        }

    let loadOutputDataWithHighestGenerationNumber extractFn dataType saveConfig db rp =
        loadOutputDataWithSchedules None extractFn dataType saveConfig db rp

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
