
namespace GeneSort.Project

open System
open System.IO
open System.Threading

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
open GeneSort.Runs.Params
open GeneSort.Runs.Mp


     
module OutputDataFile =
    /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let getOutputDataFolder (projectFolder:string) (outputDataType: outputDataType) 
                    : string =
        Path.Combine(projectFolder, outputDataType |> OutputDataType.toString)

    let makeIndexAndReplPathFromQueryParams 
                (projectFolder:string) 
                (queryParams:queryParams)
                (outputDataType: outputDataType) : string =

        let outputDataFolder = getOutputDataFolder projectFolder outputDataType
        match queryParams.Index, queryParams.Repl with
        | Some index, Some repl ->
            let outputDataName = outputDataType |> OutputDataType.toString
            let fileName = sprintf "%s_%d_%d.msgpack" outputDataName repl index 
            Path.Combine(outputDataFolder, fileName)
        | _ -> 
            failwithf "Index and Repl must be provided in queryParams for output data type %s" (outputDataType |> OutputDataType.toString)

    let getOutputDataFilePath
            (projectFolder: string)
            (queryParams: queryParams)
            (outputDataType: outputDataType) 
                : string =

        match outputDataType with
        | outputDataType.Project -> 
            let fileName = sprintf "%s.msgpack" (outputDataType |> OutputDataType.toString)
            Path.Combine(projectFolder, fileName)
        | _ -> 
            makeIndexAndReplPathFromQueryParams projectFolder queryParams outputDataType

    let getOutputDataAsync
            (projectFolder: string)
            (queryParams: queryParams)
            (outputDataType: outputDataType) 
                : Async<Result<outputData, OutputError>> =
        async {
            let filePath = getOutputDataFilePath projectFolder queryParams outputDataType
            if not (File.Exists filePath) then
                return Error (sprintf "File not found: %s" filePath)
            else
            try
                use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync = true)
                let! fileBytes = 
                    async {
                        use ms = new MemoryStream()
                        do! stream.CopyToAsync(ms) |> Async.AwaitTask
                        return ms.ToArray()
                    }
            
                let domainData =
                    match outputDataType with
                    | outputDataType.RunParameters ->
                        let dto = MessagePackSerializer.Deserialize<runParametersDto>(fileBytes, options)
                        RunParameters (RunParametersDto.fromDto dto)
                    | outputDataType.SorterSet ->
                        let dto = MessagePackSerializer.Deserialize<sorterSetDto>(fileBytes, options)
                        SorterSet (SorterSetDto.toDomain dto)
                    | outputDataType.SortableTestSet ->
                        let dto = MessagePackSerializer.Deserialize<sortableTestSetDto>(fileBytes, options)
                        SortableTestSet (SortableTestSetDto.toDomain dto)
                    | outputDataType.SorterModelSetMaker ->
                        let dto = MessagePackSerializer.Deserialize<sorterModelSetMakerDto>(fileBytes, options)
                        SorterModelSetMaker (SorterModelSetMakerDto.toDomain dto)
                    | outputDataType.SortableTestModelSet ->
                        let dto = MessagePackSerializer.Deserialize<sortableTestModelSetDto>(fileBytes, options)
                        SortableTestModelSet (SortableTestModelSetDto.toDomain dto)
                    | outputDataType.SortableTestModelSetMaker ->
                        let dto = MessagePackSerializer.Deserialize<sortableTestModelSetMakerDto>(fileBytes, options)
                        SortableTestModelSetMaker (SortableTestModelSetMakerDto.toDomain dto)
                    | outputDataType.SorterSetEval ->
                        let dto = MessagePackSerializer.Deserialize<sorterSetEvalDto>(fileBytes, options)
                        SorterSetEval (SorterSetEvalDto.toDomain dto)
                    | outputDataType.SorterSetEvalBins ->
                        let dto = MessagePackSerializer.Deserialize<sorterSetEvalBinsDto>(fileBytes, options)
                        SorterSetEvalBins (SorterSetEvalBinsDto.toDomain dto)
                    | outputDataType.Project ->
                        let dto = MessagePackSerializer.Deserialize<projectDto>(fileBytes, options)
                        Project (ProjectDto.toDomain dto)
            
                return Ok domainData
            with e ->
                return Error (sprintf "Error reading file %s: %s" filePath e.Message)
        }

    let saveToFileAsync 
            (projectFolder: string)
            (queryParams: queryParams)
            (outputData: outputData) : Async<unit> =
        async {
            let outputDataType = outputData |> OutputData.getOutputDataType
            let filePath = getOutputDataFilePath projectFolder queryParams outputDataType
            let directory = Path.GetDirectoryName filePath
            Directory.CreateDirectory directory |> ignore
            try
                use stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None)
                match outputData with
                | RunParameters r -> 
                    let dto = RunParametersDto.fromDomain r
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SorterSet ss ->
                    let dto = SorterSetDto.fromDomain ss
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SortableTestSet sts ->
                    let dto = SortableTestSetDto.fromDomain sts
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SorterModelSetMaker sms -> 
                    let dto = SorterModelSetMakerDto.fromDomain sms
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SortableTestModelSet sts ->
                    let dto = SortableTestModelSetDto.fromDomain sts
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SortableTestModelSetMaker stsm ->
                    let dto = SortableTestModelSetMakerDto.fromDomain stsm
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SorterSetEval sse ->
                    let dto = SorterSetEvalDto.fromDomain sse
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SorterSetEvalBins sse ->
                    let dto = SorterSetEvalBinsDto.fromDomain sse
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | Project p ->
                    let dto = ProjectDto.fromDomain p
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask

            with e ->
                printfn "Error saving to file %s: %s" filePath e.Message
                raise e // Re-throw to ensure the caller is aware of the failure
        }

    let getFilesSortedByCreationTime (directoryPath: string) : string list =
        Directory.GetFiles(directoryPath)
        |> Array.map (fun filePath -> filePath, File.GetCreationTime(filePath))
        |> Array.sortBy snd
        |> Array.map fst
        |> Array.toList

    let getAllRunParametersAsync 
                (projectFolder: string) 
                (ct: CancellationToken option) 
                (progress: IProgress<string> option) : Async<runParameters[]> =

        let _getRPFileAsync (runFilePath: string) (ct: CancellationToken option) : Async<Result<runParameters, string>> =
            async {
                try
                    use stream = new FileStream(runFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)

                    let token = defaultArg ct CancellationToken.None
                    let! dto = MessagePackSerializer.DeserializeAsync<runParametersDto>(stream, options, token).AsTask() |> Async.AwaitTask

                    let runParameters = RunParametersDto.fromDto dto
                    return Ok runParameters
                with
                | :? FileNotFoundException -> return Error (sprintf "File not found: %s" runFilePath)
                | :? IOException as e -> return Error (sprintf "IO error reading %s: %s" runFilePath e.Message)
                | :? MessagePackSerializationException as e -> return Error (sprintf "Deserialization error in %s: %s" runFilePath e.Message)
                | e -> return Error (sprintf "Unexpected error in %s: %s" runFilePath e.Message)
            }

        async {
            let folder = getOutputDataFolder projectFolder outputDataType.RunParameters
            let filePaths = getFilesSortedByCreationTime folder |> List.toArray  // Snapshot to array for safety
        
            match progress with
            | None -> ()
            | Some p -> p.Report(sprintf "Found %d files in %s" filePaths.Length folder)
        
            let mutable results = []
            for i = 0 to filePaths.Length - 1 do
                match ct with
                | None -> ()
                | Some t -> t.ThrowIfCancellationRequested()  // Check cancellation
                let filePath = filePaths.[i]
                let! result = _getRPFileAsync filePath ct
                match result with
                | Ok runParams -> results <- runParams :: results
                | Error msg ->  
                    match progress with
                    | None -> ()
                    | Some p -> p.Report(sprintf "Skipped due to error: %s" msg)  // Log error, continue
        
            return results |> List.rev |> List.toArray  // Reverse to maintain original order
        }



    //let saveAllRunParametersAsync 
    //        (projectFolder: string) 
    //        (runParametersArray: runParameters[])
    //        (ct: CancellationToken option) 
    //        (progress: IProgress<string> option) : Async<unit> =
    //    async {
    //        let folder = getOutputDataFolder projectFolder outputDataType.RunParameters
    //        Directory.CreateDirectory folder |> ignore
            
    //        match progress with
    //        | None -> ()
    //        | Some p -> p.Report(sprintf "Saving %d run parameters to %s" runParametersArray.Length folder)
            
    //        for i = 0 to runParametersArray.Length - 1 do
    //            match ct with
    //            | None -> ()
    //            | Some t -> t.ThrowIfCancellationRequested()
                
    //            let runParams = runParametersArray.[i]
    //            let qps = queryParams.Create(runParams.GetProjectName(), (Some runParams.GetIndex()), (Some runParams.GetRepl()), None, outputDataType.RunParameters) |> ignore

                
    //            let filePath = getOutputDataFilePath projectFolder queryParams outputDataType.RunParameters
                
    //            try
    //                use stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None)
    //                let dto = RunParametersDto.fromDomain runParams
    //                do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                    
    //                match progress with
    //                | None -> ()
    //                | Some p -> p.Report(sprintf "Saved %d/%d" (i + 1) runParametersArray.Length)
    //            with e ->
    //                let errorMsg = sprintf "Error saving run parameters %d/%d to %s: %s" (i + 1) runParametersArray.Length filePath e.Message
    //                match progress with
    //                | None -> ()
    //                | Some p -> p.Report(errorMsg)
    //                // Decide: continue or fail-fast? Currently continuing...
    //    }