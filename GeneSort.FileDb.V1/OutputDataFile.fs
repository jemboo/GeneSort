namespace GeneSort.FileDb.V1
open System
open System.IO
open System.Text
open System.Threading
open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open GeneSort.Core
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1
open GeneSort.Sorting.Mp.Sortable
open GeneSort.Sorting.Mp.Sorter
open GeneSort.Model.Sorting.Mp.V1
open GeneSort.SortingOps
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Bins.Standard
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.SortingOps.Mp
open GeneSort.Eval.Mp.V1.Bins
open GeneSort.Eval.Mp.V1.Sgd
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage
open GeneSort.Project.Mp.V1

[<Measure>] type fullPathToFolder
[<Measure>] type pathToRootFolder
[<Measure>] type fileNameWithoutExtension
[<Measure>] type fullPathToFile // e.g., "C:\GeneSortData\Project1\RunParameters_0_0.msgpack"

module OutputDataFile =

    let getPathToOutputDataFolder
                (pathToRootFolder:string<pathToRootFolder>)
                (dataMode: geneSortDataMode)
                (replString: string)
                (outputDataType: geneSortDataType)
                 :string<fullPathToFolder> =
        let effectiveMode =
            match outputDataType with
            | geneSortDataType.Run _ | geneSortDataType.RunParameters _ -> geneSortDataMode.Project
            | _ -> dataMode
        let pathToRootFolder =
            Path.Combine(%pathToRootFolder, GeneSortDataMode.toFolderName effectiveMode)
            |> UMX.tag<pathToRootFolder>
        // Setup references address one immutable file within the database, across all replicas.
        let replString = if effectiveMode = geneSortDataMode.Setup then "" else replString
        match outputDataType with
        | geneSortDataType.Run runName ->
            let folderPath = Path.Combine(%pathToRootFolder, "Run", %runName)
            UMX.tag<fullPathToFolder> folderPath
        | geneSortDataType.RunParameters _   ->
            Path.Combine(%pathToRootFolder, "Run", outputDataType |> GeneSortDataType.toFolderName, replString)
            |> UMX.tag<fullPathToFolder>
        | _ ->
            Path.Combine(%pathToRootFolder, outputDataType |> GeneSortDataType.toFolderName, replString)
            |> UMX.tag<fullPathToFolder>

    let makeOutputDataName (queryParams: queryParams) : string =
        let idPart = queryParams.Id.ToString()
        match queryParams.OutputDataType with
        | geneSortDataType.Run runName -> sprintf "Run_%s" %runName
        | _ -> idPart

    let getFullOutputDataFilePath
            (pathToRootFolder: string<pathToRootFolder>)
            (dataMode: geneSortDataMode)
            (queryParams: queryParams) : string<fullPathToFile> =
        let fileNameWithExtension =
            match queryParams.OutputDataType with
            | geneSortDataType.TextReport reportName ->
                sprintf "%s.txt" (string %queryParams.Id)
            | _ -> makeOutputDataName queryParams + ".msgpack"
        let outputDataFolder = getPathToOutputDataFolder pathToRootFolder dataMode queryParams.ReplAsString queryParams.OutputDataType
        Path.Combine(%outputDataFolder, fileNameWithExtension) |> UMX.tag<fullPathToFile>


    /// Helper to deserialize DTO and convert to domain.
    let private deserializeDto<'Dto, 'Domain> (stream: Stream) (token: CancellationToken) (toDomain: 'Dto -> 'Domain) =
        async {
            // Uses global DefaultOptions set in Program.fs
            let! dto = MessagePackSerializer.DeserializeAsync<'Dto>(stream, MessagePackSerializer.DefaultOptions, token).AsTask() |> Async.AwaitTask
            return toDomain dto
        }


    let rec private isSerializationFailure (error: exn) =
        match error with
        | :? MessagePackSerializationException -> true
        | :? AggregateException as aggregate -> aggregate.InnerExceptions |> Seq.exists isSerializationFailure
        | _ -> false

    /// New files carry a workflow tag; older files contain a Standard payload directly.
    let private deserializeUnionDto<'Dto, 'LegacyDto, 'Domain>
            (stream: Stream) (token: CancellationToken)
            (toDomain: 'Dto -> 'Domain) (fromLegacy: 'LegacyDto -> 'Domain) =
        async {
            let startPosition = stream.Position
            try
                return! deserializeDto<'Dto, 'Domain> stream token toDomain
            with error when isSerializationFailure error ->
                stream.Position <- startPosition
                return! deserializeDto<'LegacyDto, 'Domain> stream token fromLegacy
        }

    let getOutputDataAsync
            (pathToProjectFolder :string<pathToRootFolder>)
            (dataMode: geneSortDataMode)
            (queryParams :queryParams)
            (ct :CancellationToken option)
                :Async<Result<outputData, string>> =
        async {
            let filePath = getFullOutputDataFilePath pathToProjectFolder dataMode queryParams
            if not (File.Exists %filePath) || not (CheckpointIntegrity.isReadable %filePath) then
                return Error (sprintf "File not found: %s" %filePath)
            else
            let token = defaultArg ct CancellationToken.None
            try
                use stream = new FileStream(%filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync = true)
                let! domainData =
                    match queryParams.OutputDataType with
                    | geneSortDataType.RunParameters _ ->
                        async {
                            let! domain = deserializeDto<runParametersDto, runParameters> stream token RunParametersDto.toDomain
                            return outputData.RunParameters domain
                        }
                    | geneSortDataType.SorterPoolSet _ ->
                        async {
                            let! domain = deserializeUnionDto<sorterPoolSetDto, sorterPoolSetDto_Standard, sorterPoolSet> stream token SorterPoolSetDto.toDomain (SorterPoolSetDto_Standard.fromDto >> sorterPoolSet.Standard)
                            return outputData.SorterPoolSet domain
                        }
                    | geneSortDataType.SorterPoolSetSummarySet _ ->
                        async {
                            let! domain = deserializeUnionDto<spSummarySetDto, sorterPoolSetSummarySetDto_Standard, spSummarySet> stream token SpSummarySetDto.toDomain (SorterPoolSetSummarySetDto_Standard.fromDto >> spSummarySet.Standard)
                            return outputData.SorterPoolSetSummarySet domain
                        }
                    | geneSortDataType.SorterSet _ ->
                        async {
                            let! domain = deserializeDto<sorterSetDto, sorterSet> stream token SorterSetDto.toDomain
                            return outputData.SorterSet domain
                        }
                    | geneSortDataType.SorterModelSet _ ->
                        async {
                            let! domain = deserializeDto<sorterModelSetDto, sorterModelSet> stream token SorterModelSetDto.toDomain
                            return outputData.SorterModelSet domain
                        }
                    | geneSortDataType.SortableTests _ ->
                        async {
                            let! domain = deserializeDto<sortableTestsDto, sortableTests> stream token SortableTestsDto.toDomain
                            return outputData.SortableTests domain
                        }
                    | geneSortDataType.SortableTestsWithPrefix _ ->
                        async {
                            let! domain = deserializeDto<sortableTestsWithPrefixDto, sortableTests * ce array> stream token SortableTestsWithPrefixDto.toDomain
                            return outputData.SortableTestsWithPrefix domain
                        }
                    | geneSortDataType.SorterSetEval _ ->
                        async {
                            let! domain = deserializeDto<sorterSetEvalDto, sorterSetEval> stream token SorterSetEvalDto.toDomain
                            return outputData.SorterSetEval domain
                        }
                    | geneSortDataType.SorterPoolBinsSetSeries _ ->
                        async {
                            let! domain = deserializeUnionDto<sorterPoolBinsSetSeriesDto, sorterPoolBinsSetSeriesDto_Standard, sorterPoolBinsSetSeries>
                                                stream token SorterPoolBinsSetSeriesDto.toDomain (SorterPoolBinsSetSeriesDto_Standard.toDomain >> sorterPoolBinsSetSeries.Standard)
                            return outputData.SorterPoolBinsSetSeries domain
                        }
                    | geneSortDataType.SorterPoolSetHistory _ ->
                        async {
                            let! domain = deserializeUnionDto<spshDto, sorterPoolSetHistoryDto_Standard, spsh>
                                                stream token SpshDto.toDomain (SorterPoolSetHistoryDto_Standard.toDomain >> spsh.Standard)
                            return outputData.SorterPoolSetHistory domain
                        }
                    | geneSortDataType.Run _ ->
                        async {
                            let! domain = deserializeDto<runDto, run> stream token RunDto.toDomain
                            return outputData.Run domain
                        }
                    | geneSortDataType.TextReport _ ->
                        failwith "Not implemented: SorterSet deserialization"
                        //async {
                        //    use reader = new StreamReader(stream, Encoding.UTF8)
                        //    let! text = reader.ReadToEndAsync() |> Async.AwaitTask
                        //    return outputData.TextReport (DataTableReport.create "" [||]) // Assume FromText; adjust if needed.
                        //}
                return Ok domainData
            with
            | :? OperationCanceledException -> return Error "Operation cancelled"
            | e -> return Error (sprintf "Error reading file %s: %s" %filePath e.Message)
        }

            /// Helper to serialize domain to DTO.
    let private serializeDto<'Domain, 'Dto> (stream: Stream) (domain: 'Domain) (fromDomain: 'Domain -> 'Dto) =
        let dto = fromDomain domain
        // Uses global DefaultOptions set in Program.fs
        MessagePackSerializer.SerializeAsync(stream, dto, MessagePackSerializer.DefaultOptions) |> Async.AwaitTask


    let rec buildDetailedMessage (ex: exn) =
        let currentMsg = sprintf "[%s] %s" (ex.GetType().FullName) ex.Message
        if isNull ex.InnerException then
            currentMsg
        else
            sprintf "%s\n  ---> %s" currentMsg (buildDetailedMessage ex.InnerException)

    let saveToFileAsync
        (pathToProjectFolder:string<pathToRootFolder>)
        (dataMode: geneSortDataMode)
        (queryParams: queryParams)
        (outputData: outputData)
        (allowOverwrite: bool<allowOverwrite>) : Async<Result<unit, string>> =

        async {
            let filePath = getFullOutputDataFilePath pathToProjectFolder dataMode queryParams
            let sharedSetup =
                match queryParams.OutputDataType with
                | geneSortDataType.Run _ | geneSortDataType.RunParameters _ -> false
                | _ -> dataMode = geneSortDataMode.Setup
            let directory = Path.GetDirectoryName %filePath
            let dirRes =
                try
                    Directory.CreateDirectory directory |> ignore
                    Ok ()
                with e ->
                    Error (sprintf "Directory creation failed: %s" e.Message)
            match dirRes with
            | Error err -> return Error err
            | Ok () ->
                if sharedSetup && File.Exists %filePath then
                    return Ok ()
                // A shared Setup file may appear after the first check. In that case,
                // let atomic publication below retain the winning file and succeed.
                elif not sharedSetup && (not %allowOverwrite) && File.Exists %filePath then
                    return Error "Overwrite not allowed"
                else
                    // Publish every file atomically; the previous version survives interrupted serialization.
                    let writePath = %filePath + "." + Guid.NewGuid().ToString("N") + ".tmp"
                    try
                        try
                            use stream = new FileStream(writePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync = true)
                            do!
                                match outputData with
                                | outputData.RunParameters r ->
                                    serializeDto stream r RunParametersDto.fromDomain
                                | outputData.SorterPoolSet ss ->
                                    serializeDto stream ss SorterPoolSetDto.fromDomain
                                | outputData.SorterPoolSetSummarySet ss ->
                                    serializeDto stream ss SpSummarySetDto.fromDomain
                                | outputData.SorterSet ss ->
                                    serializeDto stream ss SorterSetDto.fromDomain
                                | outputData.SorterModelSet models ->
                                    serializeDto stream models SorterModelSetDto.fromDomain
                                | outputData.SortableTests sts ->
                                     serializeDto stream sts SortableTestsDto.fromDomain
                                | outputData.SortableTestsWithPrefix (tests, prefix) ->
                                    serializeDto stream (tests, prefix) SortableTestsWithPrefixDto.fromDomain
                                | outputData.SorterSetEval sse ->
                                    serializeDto stream sse SorterSetEvalDto.fromDomain
                                | outputData.SorterPoolBinsSetSeries sse ->
                                    serializeDto stream sse SorterPoolBinsSetSeriesDto.fromDomain
                                | outputData.SorterPoolSetHistory sse ->
                                    serializeDto stream sse SpshDto.fromDomain
                                | outputData.Run p ->
                                    serializeDto stream p RunDto.fromDomain
                                | outputData.TextReport dataTableReport ->
                                    async {
                                        dataTableReport.SaveToStream stream
                                        let textBytes = Encoding.UTF8.GetBytes("")
                                        do! stream.WriteAsync(textBytes, 0, textBytes.Length) |> Async.AwaitTask
                                    }
                            do! stream.FlushAsync() |> Async.AwaitTask
                            stream.Flush(true)
                            stream.Dispose()
                            if sharedSetup then
                                try File.Move(writePath, %filePath, false)
                                with :? IOException when File.Exists %filePath -> ()
                            else File.Move(writePath, %filePath, %allowOverwrite)
                            return Ok ()
                        with e ->
                            let detailedError = buildDetailedMessage e
                            return Error (sprintf "Error saving file %s:\n%s" %filePath detailedError)
                    finally
                        if writePath <> %filePath && File.Exists writePath then File.Delete writePath
        }


    let getFilesSortedByCreationTimeAsync (pathToFolder: string) (ct: CancellationToken option) : Async<Result<string list, string>> =
        async {
            let token = defaultArg ct CancellationToken.None
            try
                token.ThrowIfCancellationRequested()
                if not (Directory.Exists(pathToFolder)) then return Error (sprintf "Directory does not exist: %s" pathToFolder)
                else
                    let files = Directory.GetFiles(pathToFolder)
                    let! fileTimes =
                        files
                        |> Array.map (fun fp -> async { return fp, File.GetCreationTime(fp) })
                        |> Async.Parallel
                    let sorted = fileTimes |> Array.sortBy snd |> Array.map fst |> Array.toList
                    return Ok sorted
            with e -> return Error (sprintf "Error accessing directory %s: %s" pathToFolder e.Message)
        }

    let _getRPFileAsync (runFilePath: string) (ct: CancellationToken option) : Async<Result<runParameters, string>> =
        async {
            try
                use stream = new FileStream(runFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync = true)
                let token = defaultArg ct CancellationToken.None
                // Pass MessagePackSerializer.DefaultOptions here
                let! dto = MessagePackSerializer.DeserializeAsync<runParametersDto>(stream, MessagePackSerializer.DefaultOptions, token).AsTask() |> Async.AwaitTask
                return Ok (RunParametersDto.toDomain dto)
            with e -> return Error (sprintf "Error loading file %s: %s" runFilePath e.Message)
        }

    let getProjectRunParametersForReplAsync
                (rootFolder: string<pathToRootFolder>)
                (runName:string<runName>)
                (repl: int<replNumber> option)
                (ct: CancellationToken option)
                (progress: IProgress<string> option) : Async<Result<runParameters[], string>> =
        async {
            let folder = getPathToOutputDataFolder rootFolder geneSortDataMode.Project (repl |> UmxExt.intOptionToString) (geneSortDataType.RunParameters %runName)
            progress |> Option.iter (fun p -> p.Report(sprintf "Scanning directory: %s" %folder))
            let! filePathsRes = getFilesSortedByCreationTimeAsync %folder ct
            match filePathsRes with
            | Error msg ->
                progress |> Option.iter (fun p -> p.Report(sprintf "Error: %s" msg))
                return Error msg
            | Ok files ->
                let fileArray = files |> List.toArray
                progress |> Option.iter (fun p -> p.Report(sprintf "Found %d RunParameter files" fileArray.Length))
                if fileArray.Length = 0 then return Ok [||]
                else
                    let! resultsRes =
                        fileArray
                        |> Array.mapi (fun i fp -> async {
                            defaultArg ct CancellationToken.None |> ignore // Check CT.
                            let! res = _getRPFileAsync fp ct
                            if (i + 1) % 10 = 0 || (i + 1) = fileArray.Length then
                                progress |> Option.iter (fun p -> p.Report(sprintf "Loaded %d/%d RunParameter files" (i + 1) fileArray.Length))
                            return res
                        })
                        |> Async.Parallel
                    let results = resultsRes |> Array.choose (function Ok rp -> Some rp | _ -> None)
                    let errors = resultsRes |> Array.choose (function Error msg -> Some msg | _ -> None)
                    let successCount = results.Length
                    let errorCount = errors.Length
                    progress |> Option.iter (fun p ->
                        if errorCount > 0 then p.Report(sprintf "Completed: %d loaded, %d skipped due to errors" successCount errorCount)
                        else p.Report(sprintf "Successfully loaded all %d RunParameter files" successCount))
                    if errors.Length > 0 then return Error (String.concat "; " errors)
                    else return Ok results
        }


    let getRunParameters
            (rootFolder: string<pathToRootFolder>)
            (runName:string<runName>)
            (minRepl: int<replNumber> option)
            (maxRepl: int<replNumber> option) // Note: ensured unit matching
            (ct: CancellationToken option)
            (progress: IProgress<string> option) : Async<Result<runParameters[], string>> =
        async {
            // 1. Resolve replication range
            let start = defaultArg minRepl 0<replNumber>
            let stop = defaultArg maxRepl start

            let repls = [| %start .. (%stop - 1) |] |> Array.map UMX.tag<replNumber>

            progress |> Option.iter (fun p ->
                p.Report(sprintf "Starting bulk load for %d replicates (Range: %d to %d)" repls.Length %start %stop))

            // 2. Map replicates to your existing async function
            let! allResults =
                repls
                |> Array.map (fun r ->
                    getProjectRunParametersForReplAsync rootFolder runName (Some r) ct progress)
                |> Async.Parallel

            // 3. Aggregate Results
            let successes =
                allResults
                |> Array.choose (function Ok rps -> Some rps | _ -> None)
                |> Array.concat

            let errors =
                allResults
                |> Array.choose (function Error msg -> Some msg | _ -> None)

            // 4. Final Reporting
            if errors.Length > 0 then
                let errorMsg = sprintf "Partial failure: %d replicate(s) failed. Errors: %s" errors.Length (String.concat " | " errors)
                progress |> Option.iter (fun p -> p.Report errorMsg)
                return Error errorMsg
            else
                progress |> Option.iter (fun p ->
                    p.Report(sprintf "Bulk load complete. Total parameters loaded: %d" successes.Length))
                return Ok successes
        }
