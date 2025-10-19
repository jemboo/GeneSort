
namespace GeneSort.Project

open System
open System.IO
open System.Threading

open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers

open GeneSort.Db
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Model.Sorter
open GeneSort.Model.Sortable
open GeneSort.Model.Mp.Sorter
open GeneSort.Model.Mp.Sortable
open GeneSort.Sorter.Mp.Sorter
open GeneSort.Sorter.Mp.Sortable
open GeneSort.SortingResults
open GeneSort.SortingResults.Mp
open GeneSort.SortingOps
open GeneSort.SortingOps.Mp
open GeneSort.Runs.Params
open GeneSort.Runs.Mp
open GeneSort.Runs
open GeneSort.Db.OutputData


     
module OutputDataFile =

    let getFilesSortedByCreationTime (directoryPath: string) : string list =
        Directory.GetFiles(directoryPath)
        |> Array.map (fun filePath -> filePath, File.GetCreationTime(filePath))
        |> Array.sortBy snd
        |> Array.map fst
        |> Array.toList


        /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)


    let getOutputDataFolder (projectFolder:string) (outputDataType: outputDataType) 
                    : string =
        Path.Combine(projectFolder, outputDataType |> OutputDataType.toString)


    let makeIndexAndReplName 
                (projectFolder:string) 
                (runParameters:runParameters)
                (outputDataType: outputDataType) : string =

        let outputDataFolder = getOutputDataFolder projectFolder outputDataType
        let index = runParameters.GetIndex()
        let repl = runParameters.GetRepl()
        let outputDataName = outputDataType |> OutputDataType.toString
        let fileName = sprintf "%s_%d_%d.msgpack" outputDataName %repl %index 
        Path.Combine(outputDataFolder, fileName)


    let getOutputDataFileName
            (projectFolder: string)
            (runParameters: runParameters option)
            (outputDataType: outputDataType) 
                : string =

        match outputDataType with
        | outputDataType.Project -> 
            let fileName = sprintf "%s.msgpack" (outputDataType |> OutputDataType.toString)
            Path.Combine(projectFolder, fileName)
        | _ -> 
            if runParameters.IsNone then
                failwithf "Run parameters must be provided for output data type %s" (outputDataType |> OutputDataType.toString)
            makeIndexAndReplName projectFolder runParameters.Value outputDataType


    let getOutputData
            (projectFolder: string)
            (runParameters: runParameters option)
            (outputDataType: outputDataType) 
                : outputData =
        let filePath = getOutputDataFileName projectFolder runParameters outputDataType
        if not (File.Exists filePath) then
            failwithf "File not found: %s" filePath
        
        try
            use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            match outputDataType with
            | outputDataType.RunParameters ->
                let dto = MessagePackSerializer.Deserialize<runParametersDto>(stream, options)
                RunParameters (RunParametersDto.fromDto dto)
            | outputDataType.SorterSet ->
                let dto = MessagePackSerializer.Deserialize<sorterSetDto>(stream, options)
                SorterSet (SorterSetDto.toDomain dto)
            | outputDataType.SortableTestSet ->
                let dto = MessagePackSerializer.Deserialize<sortableTestSetDto>(stream, options)
                SortableTestSet (SortableTestSetDto.toDomain dto)
            | outputDataType.SorterModelSetMaker ->
                let dto = MessagePackSerializer.Deserialize<sorterModelSetMakerDto>(stream, options)
                SorterModelSetMaker (SorterModelSetMakerDto.toDomain dto)
            | outputDataType.SortableTestModelSet ->
                let dto = MessagePackSerializer.Deserialize<sortableTestModelSetDto>(stream, options)
                SortableTestModelSet (SortableTestModelSetDto.toDomain dto)
            | outputDataType.SortableTestModelSetMaker ->
                let dto = MessagePackSerializer.Deserialize<sortableTestModelSetMakerDto>(stream, options)
                SortableTestModelSetMaker (SortableTestModelSetMakerDto.toDomain dto)
            | outputDataType.SorterSetEval ->
                let dto = MessagePackSerializer.Deserialize<sorterSetEvalDto>(stream, options)
                SorterSetEval (SorterSetEvalDto.toDomain dto)
            | outputDataType.SorterSetEvalBins ->
                let dto = MessagePackSerializer.Deserialize<sorterSetEvalBinsDto>(stream, options)
                SorterSetEvalBins (SorterSetEvalBinsDto.toDomain dto)
            | outputDataType.Project ->
                let dto = MessagePackSerializer.Deserialize<projectDto>(stream, options)
                Project (ProjectDto.toDomain dto)
        with e ->
            failwithf "Error reading file %s: %s" filePath e.Message


    let getRunParameters (projectFolder: string) (runParameters: runParameters) : runParameters =
        match getOutputData projectFolder (Some runParameters) outputDataType.RunParameters with
        | RunParameters r -> r
        | _ -> failwith "Unexpected output data type: expected RunParameters"

    let getSorterSet (projectFolder: string) (runParameters: runParameters) : sorterSet =
        match getOutputData projectFolder (Some runParameters) outputDataType.SorterSet with
        | SorterSet ss -> ss
        | _ -> failwith "Unexpected output data type: expected SorterSet"

    let getSortableTestSet (projectFolder: string) (runParameters: runParameters) : sortableTestSet =
        match getOutputData projectFolder (Some runParameters) outputDataType.SortableTestSet with
        | SortableTestSet sts -> sts
        | _ -> failwith "Unexpected output data type: expected SortableTestSet"

    let getSorterModelSetMaker (projectFolder: string) (runParameters: runParameters) : sorterModelSetMaker =
        match getOutputData projectFolder (Some runParameters) outputDataType.SorterModelSetMaker with
        | SorterModelSetMaker sms -> sms
        | _ -> failwith "Unexpected output data type: expected SorterModelSetMaker"

    let getSortableTestModelSet (projectFolder: string) (runParameters: runParameters) : sortableTestModelSet =
        match getOutputData projectFolder (Some runParameters) outputDataType.SortableTestModelSet with
        | SortableTestModelSet sts -> sts
        | _ -> failwith "Unexpected output data type: expected SortableTestModelSet"

    let getSortableTestModelSetMaker (projectFolder: string) (runParameters: runParameters) : sortableTestModelSetMaker =
        match getOutputData projectFolder (Some runParameters) outputDataType.SortableTestModelSetMaker with
        | SortableTestModelSetMaker stsm -> stsm
        | _ -> failwith "Unexpected output data type: expected SortableTestModelSetMaker"

    let getSorterSetEval (projectFolder: string) (runParameters: runParameters) : sorterSetEval =
        match getOutputData projectFolder (Some runParameters) outputDataType.SorterSetEval with
        | SorterSetEval sse -> sse
        | _ -> failwith "Unexpected output data type: expected SorterSetEval"

    let getSorterSetEvalBins (projectFolder: string) (runParameters: runParameters) : sorterSetEvalBins =
        match getOutputData projectFolder (Some runParameters) outputDataType.SorterSetEvalBins with
        | SorterSetEvalBins sse -> sse
        | _ -> failwith "Unexpected output data type: expected SorterSetEvalBins"

    let getProject (projectFolder: string) (runParameters: runParameters) : project =
        match getOutputData projectFolder None outputDataType.Project with
        | Project w -> w
        | _ -> failwith "Unexpected output data type: expected Project"



    let saveToFile 
            (projectFolder: string)
            (runParameters: runParameters option)
            (outputData: outputData) : Async<unit> =
        async {
            let filePath = getOutputDataFileName projectFolder runParameters (outputData |> getOutputDataType)
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

    let getRunParametersFileAsync (runFilePath: string) (ct: CancellationToken option) : Async<Result<runParameters, string>> =
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


    let getRunParametersAsync 
                (projectFolder: string) 
                (ct: CancellationToken option) 
                (progress: IProgress<string> option) : Async<runParameters[]> =
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
                let! result = getRunParametersFileAsync filePath ct
                match result with
                | Ok runParams -> results <- runParams :: results
                | Error msg ->  
                    match progress with
                    | None -> ()
                    | Some p -> p.Report(sprintf "Skipped due to error: %s" msg)  // Log error, continue
        
            return results |> List.rev |> List.toArray  // Reverse to maintain original order
        }

