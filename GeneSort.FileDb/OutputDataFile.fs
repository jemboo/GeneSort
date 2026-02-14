namespace GeneSort.Project
open System
open System.IO
open System.Text
open System.Threading
open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open GeneSort.Core
open GeneSort.Db
open GeneSort.Model.Mp.Sortable
open GeneSort.Sorting.Mp.Sorter
open GeneSort.Sorting.Mp.Sortable
open GeneSort.SortingResults.Mp
open GeneSort.SortingOps.Mp
open GeneSort.Runs
open GeneSort.Runs.Mp
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.SortingResults
open GeneSort.Model.Sorting
open GeneSort.Model.Mp.Sorting

[<Measure>] type fullPathToFolder
[<Measure>] type pathToRootFolder
[<Measure>] type pathToProjectFolder
[<Measure>] type fileNameWithoutExtension
[<Measure>] type fullPathToFile // e.g., "C:\GeneSortData\Project1\RunParameters_0_0.msgpack"

module OutputDataFile =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let getPathToOutputDataFolder
                (pathToProjectFolder:string<pathToProjectFolder>)
                (replString: string)
                (outputDataType: outputDataType)
                 :string<fullPathToFolder> =
        Path.Combine(%pathToProjectFolder, outputDataType |> OutputDataType.toFolderName, replString) |> UMX.tag<fullPathToFolder>

    let makeOutputDataName (queryParams: queryParams) : string =
        let idPart = queryParams.Id.ToString()
        match queryParams.OutputDataType with
        | outputDataType.Project -> "Project"
        | _ -> idPart

    let getFullOutputDataFilePath
            (pathToProjectFolder: string<pathToProjectFolder>)
            (queryParams: queryParams) : string<fullPathToFile> =
        let fileNameWithExtension =
            match queryParams.OutputDataType with
            | outputDataType.TextReport reportName -> sprintf "%s_%s.txt" %reportName (DateTime.Now.ToString("yyyyMMdd_HHmm"))
            | _ -> makeOutputDataName queryParams + ".msgpack"
        let outputDataFolder = getPathToOutputDataFolder pathToProjectFolder queryParams.ReplAsString queryParams.OutputDataType
        Path.Combine(%outputDataFolder, fileNameWithExtension) |> UMX.tag<fullPathToFile>

    /// Helper to deserialize DTO and convert to domain.
    let private deserializeDto<'Dto, 'Domain> (stream: Stream) (token: CancellationToken) (fromDto: 'Dto -> 'Domain) =
        async {
            let! dto = MessagePackSerializer.DeserializeAsync<'Dto>(stream, options, token).AsTask() |> Async.AwaitTask
            return fromDto dto
        }

    let getOutputDataAsync
            (pathToProjectFolder :string<pathToProjectFolder>)
            (queryParams :queryParams)
            (ct :CancellationToken option)
                :Async<Result<outputData, string>> =
        async {
            let filePath = getFullOutputDataFilePath pathToProjectFolder queryParams
            if not (File.Exists %filePath) then
                return Error (sprintf "File not found: %s" %filePath)
            else
            let token = defaultArg ct CancellationToken.None
            try
                use stream = new FileStream(%filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync = true)
                let! domainData =
                    match queryParams.OutputDataType with
                    | outputDataType.RunParameters ->
                        async {
                            let! domain = deserializeDto<runParametersDto, runParameters> stream token RunParametersDto.fromDto
                            return outputData.RunParameters domain
                        }
                    | outputDataType.SorterSet _ ->
                        async {
                            let! domain = deserializeDto<sorterSetDto, sorterSet> stream token SorterSetDto.toDomain
                            return outputData.SorterSet domain
                        }
                    | outputDataType.SortableTestSet _ ->
                        async {
                            let! domain = deserializeDto<sortableTestSetDto, sortableTestSet> stream token SortableTestSetDto.toDomain
                            return outputData.SortableTestSet domain
                        }
                    | outputDataType.SortableTest _ ->
                        async {
                            let! domain = deserializeDto<sortableTestDto, sortableTest> stream token SortableTestDto.toDomain
                            return outputData.SortableTest domain
                        }
                    | outputDataType.SortingModelSet _ ->
                        async {
                            let! domain = deserializeDto<sortingModelSetDto, sortingModelSet> stream token SortingModelSetDto.toDomain
                            return outputData.SortingModelSet domain
                        }
                    | outputDataType.SorterModelSetMaker _ ->
                        async {
                            let! domain = deserializeDto<sortingModelSetMakerDto, sortingModelSetMaker> stream token SortingModelSetMakerDto.toDomain
                            return outputData.SortingModelSetMaker domain
                        }
                    | outputDataType.SortableTestModelSet _ ->
                        async {
                            let! domain = deserializeDto<sortableTestModelSetDto, sortableTestModelSet> stream token SortableTestModelSetDto.toDomain
                            return outputData.SortableTestModelSet domain
                        }
                    | outputDataType.SortableTestModelSetMaker _ ->
                        async {
                            let! domain = deserializeDto<sortableTestModelSetMakerDto, sortableTestModelSetMaker> stream token SortableTestModelSetMakerDto.toDomain
                            return outputData.SortableTestModelSetMaker domain
                        }
                    | outputDataType.SorterSetEval _ ->
                        async {
                            let! domain = deserializeDto<sorterModelSetEvalDto, sorterSetEval> stream token SorterSetEvalDto.toDomain
                            return outputData.SorterSetEval domain
                        }
                    | outputDataType.SorterSetEvalBins _ ->
                        async {
                            let! domain = deserializeDto<sorterSetEvalBinsDto, sorterSetEvalBins> stream token SorterSetEvalBinsDto.toDomain
                            return outputData.SorterSetEvalBins domain
                        }
                    | outputDataType.Project ->
                        async {
                            let! domain = deserializeDto<projectDto, project> stream token ProjectDto.toDomain
                            return outputData.Project domain
                        }
                    | outputDataType.TextReport _ ->
                        async {
                            use reader = new StreamReader(stream, Encoding.UTF8)
                            let! text = reader.ReadToEndAsync() |> Async.AwaitTask
                            return outputData.TextReport (DataTableReport.create "" [||]) // Assume FromText; adjust if needed.
                        }
                return Ok domainData
            with
            | :? OperationCanceledException -> return Error "Operation cancelled"
            | e -> return Error (sprintf "Error reading file %s: %s" %filePath e.Message)
        }

    /// Helper to serialize domain to DTO.
    let private serializeDto<'Domain, 'Dto> (stream: Stream) (domain: 'Domain) (toDto: 'Domain -> 'Dto) =
        let dto = toDto domain
        MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask

    let saveToFileAsync
        (pathToProjectFolder:string<pathToProjectFolder>)
        (queryParams: queryParams)
        (outputData: outputData)
        (allowOverwrite: bool<allowOverwrite>) : Async<Result<unit, string>> =

        async {

            let filePath = getFullOutputDataFilePath pathToProjectFolder queryParams
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
                if (not %allowOverwrite) && File.Exists %filePath then
                    return Error "Overwrite not allowed"
                else
                    try
                        use stream = new FileStream(%filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync = true)
                        do!
                            match outputData with
                            | outputData.RunParameters r ->
                                serializeDto stream r RunParametersDto.fromDomain
                            | outputData.SorterSet ss ->
                                serializeDto stream ss SorterSetDto.fromDomain
                            | outputData.SortableTestSet sts ->
                                serializeDto stream sts SortableTestSetDto.fromDomain
                            | outputData.SortableTest sts ->
                                serializeDto stream sts SortableTestDto.fromDomain
                            | outputData.SortingModelSet sms ->
                                serializeDto stream sms SortingModelSetDto.fromDomain
                            | outputData.SortingModelSetMaker sms ->
                              //  serializeDto stream sms SorterModelSetMakerDto.fromDomain
                                failwith "Not implemented: SorterModelSetMaker serialization"
                            | outputData.SortableTestModelSet sts ->
                                serializeDto stream sts SortableTestModelSetDto.fromDomain
                            | outputData.SortableTestModelSetMaker stsm ->
                                serializeDto stream stsm SortableTestModelSetMakerDto.fromDomain
                            | outputData.SorterSetEval sse ->
                                serializeDto stream sse SorterSetEvalDto.fromDomain
                            | outputData.SorterSetEvalBins sse ->
                                serializeDto stream sse SorterSetEvalBinsDto.fromDomain
                            | outputData.Project p ->
                                serializeDto stream p ProjectDto.fromDomain
                            | outputData.TextReport dataTableReport ->
                                async {
                                    dataTableReport.SaveToStream stream
                                    //let textBytes = Encoding.UTF8.GetBytes("")
                                    //do! stream.WriteAsync(textBytes, 0, textBytes.Length) |> Async.AwaitTask
                                }
                        return Ok ()
                    with e -> return Error (sprintf "Error saving file %s: %s" %filePath e.Message)
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
                let! dto = MessagePackSerializer.DeserializeAsync<runParametersDto>(stream, options, token).AsTask() |> Async.AwaitTask
                return Ok (RunParametersDto.fromDto dto)
            with e -> return Error (sprintf "Error loading file %s: %s" runFilePath e.Message)
        }

    let getProjectRunParametersForReplAsync
                (projectFolder: string<pathToProjectFolder>)
                (repl: int<replNumber> option)
                (ct: CancellationToken option)
                (progress: IProgress<string> option) : Async<Result<runParameters[], string>> =
        async {
            let folder = getPathToOutputDataFolder projectFolder (repl |> queryParams.ReplString) outputDataType.RunParameters
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


    let getProjectRunParametersForReplRangeAsync
            (projectFolder: string<pathToProjectFolder>)
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
                    getProjectRunParametersForReplAsync projectFolder (Some r) ct progress)
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