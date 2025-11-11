
namespace GeneSort.Project

open System
open System.IO
open System.Threading

open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers

open GeneSort.Core
open GeneSort.Db
open GeneSort.Model.Mp.Sorter
open GeneSort.Model.Mp.Sortable
open GeneSort.Sorter.Mp.Sorter
open GeneSort.Sorter.Mp.Sortable
open GeneSort.SortingResults.Mp
open GeneSort.SortingOps.Mp
open GeneSort.Runs
open GeneSort.Runs.Mp


[<Measure>] type fullPathToFolder
[<Measure>] type pathToRootFolder
[<Measure>] type pathToProjectFolder
[<Measure>] type fileNameWithoutExtension
[<Measure>] type fullPathToFile // e.g., "C:\GeneSortData\Project1\RunParameters_0_0.msgpack"
     
module OutputDataFile =
    /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let getPathToOutputDataFolder 
                (projectFolder:string<pathToProjectFolder>) 
                (outputDataType: outputDataType) 
                 : string<fullPathToFolder> =
        Path.Combine(%projectFolder, outputDataType |> OutputDataType.toFolderName) |> UMX.tag<fullPathToFolder>


    let makeIndexAndReplPathFromQueryParams 
                (projectFolder:string<pathToProjectFolder>)
                (queryParams:queryParams) : string<fullPathToFile> =

        let outputDataFolder = getPathToOutputDataFolder projectFolder queryParams.OutputDataType
        match queryParams.Index, queryParams.Repl with
        | Some index, Some repl ->
            let outputDataName = queryParams.OutputDataType |> OutputDataType.toFolderName
            let fileName = sprintf "%s_%d_%d.msgpack" outputDataName repl index 
            Path.Combine(%outputDataFolder, fileName) |> UMX.tag<fullPathToFile>
        | _ -> 
            failwithf "Index and Repl must be provided in queryParams for output data type %s" 
                            (queryParams.OutputDataType |> OutputDataType.toFolderName)


    let getFullOutputDataFilePath
            (projectFolder: string<pathToProjectFolder>)
            (queryParams: queryParams) : string<fullPathToFile> =

        let fileNameWithExtension =
            match queryParams.OutputDataType with
            | outputDataType.TextReport reportName -> sprintf "%s.txt" %reportName
            | _ -> 
                GeneSortDb.makeOutputDataName queryParams + ".msgpack"

        let outputDataFolder = getPathToOutputDataFolder projectFolder queryParams.OutputDataType
        Path.Combine(%outputDataFolder, fileNameWithExtension) |> UMX.tag<fullPathToFile>



    let getOutputDataAsync
            (projectFolder:string<pathToProjectFolder>)
            (queryParams: queryParams)
                : Async<Result<outputData, OutputError>> =
        async {
            let filePath = getFullOutputDataFilePath projectFolder queryParams
            if not (File.Exists %filePath) then
                return Error (sprintf "File not found: %s" %filePath)
            else
            try
                use stream = new FileStream(%filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync = true)
                let! fileBytes = 
                    async {
                        use ms = new MemoryStream()
                        do! stream.CopyToAsync(ms) |> Async.AwaitTask
                        return ms.ToArray()
                    }
            
                let domainData =
                    match queryParams.OutputDataType with
                    | outputDataType.RunParameters ->
                        let dto = MessagePackSerializer.Deserialize<runParametersDto>(fileBytes, options)
                        outputData.RunParameters (RunParametersDto.fromDto dto)
                    | outputDataType.SorterSet _ ->
                        let dto = MessagePackSerializer.Deserialize<sorterSetDto>(fileBytes, options)
                        outputData.SorterSet (SorterSetDto.toDomain dto)
                    | outputDataType.SortableTestSet _ ->
                        let dto = MessagePackSerializer.Deserialize<sortableTestSetDto>(fileBytes, options)
                        outputData.SortableTestSet (SortableTestSetDto.toDomain dto)
                    | outputDataType.SorterModelSet _ ->
                        let dto = MessagePackSerializer.Deserialize<sorterModelSetDto>(fileBytes, options)
                        outputData.SorterModelSet (SorterModelSetDto.toDomain dto)
                    | outputDataType.SorterModelSetMaker _ ->
                        let dto = MessagePackSerializer.Deserialize<sorterModelSetMakerDto>(fileBytes, options)
                        outputData.SorterModelSetMaker (SorterModelSetMakerDto.toDomain dto)
                    | outputDataType.SortableTestModelSet _ ->
                        let dto = MessagePackSerializer.Deserialize<sortableTestModelSetDto>(fileBytes, options)
                        outputData.SortableTestModelSet (SortableTestModelSetDto.toDomain dto)
                    | outputDataType.SortableTestModelSetMaker _ ->
                        let dto = MessagePackSerializer.Deserialize<sortableTestModelSetMakerDto>(fileBytes, options)
                        outputData.SortableTestModelSetMaker (SortableTestModelSetMakerDto.toDomain dto)
                    | outputDataType.SorterSetEval _ ->
                        let dto = MessagePackSerializer.Deserialize<sorterSetEvalDto>(fileBytes, options)
                        outputData.SorterSetEval (SorterSetEvalDto.toDomain dto)
                    | outputDataType.SorterSetEvalBins _ ->
                        let dto = MessagePackSerializer.Deserialize<sorterSetEvalBinsDto>(fileBytes, options)
                        outputData.SorterSetEvalBins (SorterSetEvalBinsDto.toDomain dto)
                    | outputDataType.Project ->
                        let dto = MessagePackSerializer.Deserialize<projectDto>(fileBytes, options)
                        outputData.Project (ProjectDto.toDomain dto)
                    | outputDataType.TextReport _ ->
                        let text = System.Text.Encoding.UTF8.GetString(fileBytes)
                        failwith "TextReport should be handled separately"
            
                return Ok domainData
            with e ->
                return Error (sprintf "Error reading file %s: %s" %filePath e.Message)
        }



    let saveToFileAsync 
            (projectFolder:string<pathToProjectFolder>)
            (queryParams: queryParams)
            (outputData: outputData) : Async<unit> =
        async {
            let filePath = getFullOutputDataFilePath projectFolder queryParams
            let directory = Path.GetDirectoryName %filePath
            Directory.CreateDirectory directory |> ignore
            try
                use stream = new FileStream(%filePath, FileMode.Create, FileAccess.Write, FileShare.None)
                match outputData with
                | outputData.RunParameters r -> 
                    let dto = RunParametersDto.fromDomain r
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | outputData.SorterSet ss ->
                    let dto = SorterSetDto.fromDomain ss
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | outputData.SortableTestSet sts ->
                    let dto = SortableTestSetDto.fromDomain sts
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | outputData.SorterModelSet sms ->
                    let dto = SorterModelSetDto.fromDomain sms
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | outputData.SorterModelSetMaker sms -> 
                    let dto = SorterModelSetMakerDto.fromDomain sms
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | outputData.SortableTestModelSet sts ->
                    let dto = SortableTestModelSetDto.fromDomain sts
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | outputData.SortableTestModelSetMaker stsm ->
                    let dto = SortableTestModelSetMakerDto.fromDomain stsm
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | outputData.SorterSetEval sse ->
                    let dto = SorterSetEvalDto.fromDomain sse
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | outputData.SorterSetEvalBins sse ->
                    let dto = SorterSetEvalBinsDto.fromDomain sse
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | outputData.Project p ->
                    let dto = ProjectDto.fromDomain p
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | outputData.TextReport dataTableFile ->
                    let textBytes = System.Text.Encoding.UTF8.GetBytes(dataTableFile.ToText())
                    do! stream.WriteAsync(textBytes, 0, textBytes.Length) |> Async.AwaitTask

            with e ->
                printfn "Error saving to file %s: %s" %filePath e.Message
                raise e // Re-throw to ensure the caller is aware of the failure
        }



    let getFilesSortedByCreationTime (pathToFolder: string) : Result<string list, string> =
        try
            if not (Directory.Exists(pathToFolder)) then
                Error (sprintf "Directory does not exist: %s" pathToFolder)
            else
                let files = 
                    Directory.GetFiles(pathToFolder)
                    |> Array.map (fun filePath -> filePath, File.GetCreationTime(filePath))
                    |> Array.sortBy snd
                    |> Array.map fst
                    |> Array.toList
                Ok files
        with
        | :? UnauthorizedAccessException as e -> 
            Error (sprintf "Access denied to directory %s: %s" pathToFolder e.Message)
        | :? IOException as e -> 
            Error (sprintf "IO error accessing directory %s: %s" pathToFolder e.Message)
        | e -> 
            Error (sprintf "Unexpected error accessing directory %s: %s" pathToFolder e.Message)



    let getAllProjectRunParametersAsync 
                (projectFolder: string<pathToProjectFolder>)
                (ct: CancellationToken option) 
                (progress: IProgress<string> option) : Async<Result<runParameters[], string>> =
    
        let _getRPFileAsync (runFilePath: string) (ct: CancellationToken option) : Async<Result<runParameters, string>> =
            async {
                try
                    use stream = new FileStream(runFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                    let token = defaultArg ct CancellationToken.None
                    let! dto = MessagePackSerializer.DeserializeAsync<runParametersDto>(stream, options, token).AsTask() |> Async.AwaitTask
                    let runParameters = RunParametersDto.fromDto dto
                    return Ok runParameters
                with
                | :? FileNotFoundException -> 
                    return Error (sprintf "File not found: %s" runFilePath)
                | :? UnauthorizedAccessException as e -> 
                    return Error (sprintf "Access denied to %s: %s" runFilePath e.Message)
                | :? IOException as e -> 
                    return Error (sprintf "IO error reading %s: %s" runFilePath e.Message)
                | :? MessagePackSerializationException as e -> 
                    return Error (sprintf "Deserialization error in %s: %s" runFilePath e.Message)
                | :? OperationCanceledException -> 
                    return Error (sprintf "Operation cancelled while reading %s" runFilePath)
                | e -> 
                    return Error (sprintf "Unexpected error in %s: %s" runFilePath e.Message)
            }
    
        async {
            try
                let folder = getPathToOutputDataFolder projectFolder outputDataType.RunParameters
            
                match progress with
                | Some p -> p.Report(sprintf "Scanning directory: %s" %folder)
                | None -> ()
            
                // Get file paths with error handling
                let! filePaths = 
                    async {
                        match getFilesSortedByCreationTime %folder with
                        | Ok files -> return Ok (files |> List.toArray)
                        | Error msg -> return Error msg
                    }
            
                match filePaths with
                | Error msg ->
                    match progress with
                    | Some p -> p.Report(sprintf "Error: %s" msg)
                    | None -> ()
                    return Error msg
                
                | Ok files ->
                    match progress with
                    | Some p -> p.Report(sprintf "Found %d RunParameter files" files.Length)
                    | None -> ()
                
                    if files.Length = 0 then
                        return Ok [||]
                    else
                        let mutable successCount = 0
                        let mutable errorCount = 0
                        let mutable results = []
                    
                        for i = 0 to files.Length - 1 do
                            // Check cancellation
                            match ct with
                            | Some t -> t.ThrowIfCancellationRequested()
                            | None -> ()
                        
                            let filePath = files.[i]
                            let! result = _getRPFileAsync filePath ct
                        
                            match result with
                            | Ok runParams -> 
                                results <- runParams :: results
                                successCount <- successCount + 1
                                match progress with
                                | Some p when (i + 1) % 10 = 0 || (i + 1) = files.Length -> 
                                    p.Report(sprintf "Loaded %d/%d RunParameter files" (i + 1) files.Length)
                                | _ -> ()
                            
                            | Error msg ->  
                                errorCount <- errorCount + 1
                                match progress with
                                | Some p -> p.Report(sprintf "⚠ Skipped file %d/%d: %s" (i + 1) files.Length msg)
                                | None -> ()
                    
                        // Final summary
                        match progress with
                        | Some p -> 
                            if errorCount > 0 then
                                p.Report(sprintf "Completed: %d loaded, %d skipped due to errors" successCount errorCount)
                            else
                                p.Report(sprintf "Successfully loaded all %d RunParameter files" successCount)
                        | None -> ()
                    
                        return Ok (results |> List.rev |> List.toArray)
        
            with 
            | :? OperationCanceledException ->
                let msg = "Loading RunParameters was cancelled"
                match progress with
                | Some p -> p.Report(msg)
                | None -> ()
                return Error msg
            | e ->
                let msg = sprintf "Unexpected error loading RunParameters: %s" e.Message
                match progress with
                | Some p -> p.Report(msg)
                | None -> ()
                return Error msg
        }