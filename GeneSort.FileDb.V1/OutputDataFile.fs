namespace GeneSort.FileDb.V1
open System
open System.IO
open System.Threading
open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Project.Mp.V1
open GeneSort.Eval.Mp.V1
open GeneSort.Sorting.Mp.Sortable
open GeneSort.Sorting.Sortable

[<Measure>] type fullPathToFolder
[<Measure>] type pathToRootFolder
[<Measure>] type runParamsType
[<Measure>] type fileNameWithoutExtension
[<Measure>] type fullPathToFile // e.g., "C:\GeneSortData\Project1\RunParameters_0_0.msgpack"

module OutputDataFile =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let getPathToOutputDataFolder
                (pathToRootFolder:string<pathToRootFolder>)
                (replString: string)
                (outputDataType: outputDataType)
                 :string<fullPathToFolder> =
        match outputDataType with
        | outputDataType.RunParameters _   ->
            Path.Combine(%pathToRootFolder, "Run", outputDataType |> OutputDataType.toFolderName, replString) 
            |> UMX.tag<fullPathToFolder>
        | _ -> 
            Path.Combine(%pathToRootFolder, outputDataType |> OutputDataType.toFolderName, replString) 
            |> UMX.tag<fullPathToFolder>

    let makeOutputDataName (queryParams: queryParams) : string =
        let idPart = queryParams.Id.ToString()
        match queryParams.OutputDataType with
        | outputDataType.Run runName -> sprintf "Run_%s" %runName
        | _ -> idPart

    let getFullOutputDataFilePath
            (pathToRootFolder: string<pathToRootFolder>)
            (queryParams: queryParams) : string<fullPathToFile> =
        let fileNameWithExtension =
            match queryParams.OutputDataType with
            | outputDataType.TextReport reportName -> sprintf "%s_%s.txt" %reportName (DateTime.Now.ToString("yyyyMMdd_HHmm"))
            | _ -> makeOutputDataName queryParams + ".msgpack"
        let outputDataFolder = getPathToOutputDataFolder pathToRootFolder queryParams.ReplAsString queryParams.OutputDataType
        Path.Combine(%outputDataFolder, fileNameWithExtension) |> UMX.tag<fullPathToFile>

    /// Helper to deserialize DTO and convert to domain.
    let private deserializeDto<'Dto, 'Domain> (stream: Stream) (token: CancellationToken) (toDomain: 'Dto -> 'Domain) =
        async {
            let! dto = MessagePackSerializer.DeserializeAsync<'Dto>(stream, options, token).AsTask() |> Async.AwaitTask
            return toDomain dto
        }

    let getOutputDataAsync
            (pathToProjectFolder :string<pathToRootFolder>)
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
                    | outputDataType.MutationSegmentEvalBinsSet _ ->
                        failwith "Not implemented: SorterSet deserialization"
                        //async {
                        //    let! domain = deserializeDto<mutationSegmentEvalBinsSetDto, mutationSegmentEvalBinsSet> stream token MutationSegmentEvalBinsSetDto.toDomain
                        //    return outputData.MutationSegmentEvalBinsSet domain
                        //}
                    | outputDataType.RunParameters _ ->
                        failwith "Not implemented: SorterSet deserialization"
                        //async {
                        //    let! domain = deserializeDto<runParametersDto, runParameters> stream token RunParametersDto.toDomain
                        //    return outputData.RunParameters domain
                        //}
                    | outputDataType.SorterSet _ ->
                        failwith "Not implemented: SorterSet deserialization"
                        //async {
                        //    let! domain = deserializeDto<sorterSetDto, sorterSet> stream token SorterSetDto.toDomain
                        //    return outputData.SorterSet domain
                        //}
                    | outputDataType.SortableTestSet _ ->
                        failwith "Not implemented: SorterSet deserialization"
                        //async {
                        //    let! domain = deserializeDto<sortableTestSetDto, sortableTestSet> stream token SortableTestSetDto.toDomain
                        //    return outputData.SortableTestSet domain
                        //}
                    | outputDataType.SortableTest _ ->
                        async {
                            let! domain = deserializeDto<sortableTestDto, sortableTest> stream token SortableTestDto.toDomain
                            return outputData.SortableTest domain
                        }
                    | outputDataType.SortingSet _ ->
                        failwith "Not implemented: SorterSet deserialization"
                        //async {
                        //    let! domain = deserializeDto<sortingSetDto, sortingSet> stream token SortingSetDto.toDomain
                        //    return outputData.SortingSet domain
                        //}
                    | outputDataType.SorterModelSetGen _ ->
                        failwith "Not implemented: SorterSet deserialization"
                        //async {
                        //    let! domain = deserializeDto<sortingSetGenDto, sortingGenSegment> stream token SortingSetGenDto.toDomain
                        //    return outputData.SortingSetGen domain
                        //}
                    | outputDataType.SortableTestModelSet _ ->
                        failwith "Not implemented: SorterSet deserialization"
                        //async {
                        //    let! domain = deserializeDto<sortableTestModelSetDto, sortableTestModelSet> stream token SortableTestModelSetDto.toDomain
                        //    return outputData.SortableTestModelSet domain
                        //}
                    | outputDataType.SortableTestModelSetGen _ ->
                        failwith "Not implemented: SorterSet deserialization"
                        //async {
                        //    let! domain = deserializeDto<sortableTestModelSetGenDto, sortableTestModelSetGen> stream token SortableTestModelSetGenDto.toDomain
                        //    return outputData.SortableTestModelSetGen domain
                        //}
                    | outputDataType.SorterSetEval _ ->
                        failwith "Not implemented: SorterSet deserialization"
                        //async {
                        //    let! domain = deserializeDto<sorterModelSetEvalDto, sorterSetEval> stream token SorterSetEvalDto.toDomain
                        //    return outputData.SorterSetEval domain
                        //}
                    | outputDataType.SorterEvalBins _ ->
                        async {
                            let! domain = deserializeDto<sorterEvalBinsDto, sorterEvalBins> stream token SorterEvalBinsDto.fromDto
                            return outputData.SorterEvalBins domain
                        }
                    | outputDataType.Run _ ->
                        async {
                            let! domain = deserializeDto<runDto, run> stream token RunDto.toDomain
                            return outputData.Run domain
                        }
                    | outputDataType.TextReport _ ->
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
        MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask

    let saveToFileAsync
        (pathToProjectFolder:string<pathToRootFolder>)
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
                            //| outputData.MutationSegmentEvalBinsSet msr ->
                            //    failwith "Not implemented: SorterSetEval serialization"
                            //   // serializeDto stream msr MutationSegmentEvalBinsSetDto.fromDomain
                            | outputData.RunParameters r ->
                                serializeDto stream r RunParametersDto.fromDomain
                            //| outputData.SorterSet ss ->
                            //    failwith "Not implemented: SorterSetEval serialization"
                            //    //serializeDto stream ss SorterSetDto.fromDomain
                            //| outputData.SortableTestSet sts ->
                            //    failwith "Not implemented: SorterSetEval serialization"
                            //    //serializeDto stream sts SortableTestSetDto.fromDomain
                            | outputData.SortableTest sts ->
                                 serializeDto stream sts SortableTestDto.fromDomain
                            //| outputData.SortingSet sms ->
                            //    failwith "Not implemented: SorterSetEval serialization"
                            //    //serializeDto stream sms SortingSetDto.fromDomain
                            //| outputData.SortingSetGen sms ->
                            //   //serializeDto stream sms SorterModelSetGenDto.fromDomain
                            //    failwith "Not implemented: SorterModelSetGen serialization"
                            //| outputData.SortableTestModelSet sts ->
                            //    failwith "Not implemented: SorterSetEval serialization"
                            //    //serializeDto stream sts SortableTestModelSetDto.fromDomain
                            //| outputData.SortableTestModelSetGen stsm ->
                            //    failwith "Not implemented: SorterSetEval serialization"
                            //   // serializeDto stream stsm SortableTestModelSetGenDto.fromDomain
                            //| outputData.SorterSetEval sse ->
                            //    failwith "Not implemented: SorterSetEval serialization"
                            //   // serializeDto stream sse SorterSetEvalDto.fromDomain
                            | outputData.SorterEvalBins sse ->
                                serializeDto stream sse SorterEvalBinsDto.toDto
                            | outputData.Run p ->
                                serializeDto stream p RunDto.fromDomain
                            //| outputData.TextReport dataTableReport ->
                            //    failwith "Not implemented: SorterSetEval serialization"
                            //    //async {
                            //    //    dataTableReport.SaveToStream stream
                            //    //    //let textBytes = Encoding.UTF8.GetBytes("")
                            //    //    //do! stream.WriteAsync(textBytes, 0, textBytes.Length) |> Async.AwaitTask
                            //    //}
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
            let folder = getPathToOutputDataFolder rootFolder (repl |> queryParams.ReplString) (outputDataType.RunParameters %runName)
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