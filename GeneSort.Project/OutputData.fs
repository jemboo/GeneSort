
namespace GeneSort.Project

open System
open System.IO
open System.Threading

open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers

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


type outputDataType =
    | Run2
    | SorterSet
    | SortableTestSet
    | SorterModelSetMaker
    | SortableTestModelSet
    | SortableTestModelSetMaker
    | SorterSetEval
    | SorterSetEvalBins
    | Workspace


     
module OutputDataType =
    
    let toString (outputDataType: outputDataType) : string =
        match outputDataType with
        | Run2 -> "Run2"
        | SorterSet -> "SorterSet"
        | SortableTestSet -> "SortableTestSet"
        | SorterModelSetMaker -> "SorterModelSet"
        | SortableTestModelSet -> "SortableTestModelSet"
        | SortableTestModelSetMaker -> "SortableTestModelSetMaker"
        | SorterSetEval -> "SorterSetEval"
        | SorterSetEvalBins -> "SorterSetEvalBins"
        | Workspace -> "Workspace"




type outputData =
    | Run2 of run2
    | SorterSet of sorterSet
    | SortableTestSet of sortableTestSet
    | SorterModelSetMaker of sorterModelSetMaker
    | SortableTestModelSet of sortableTestModelSet
    | SortableTestModelSetMaker of sortableTestModelSetMaker
    | SorterSetEval of sorterSetEval
    | SorterSetEvalBins of sorterSetEvalBins
    | Workspace of workspace


     
module OutputData =

    let getFilesSortedByCreationTime (directoryPath: string) : string list =
        Directory.GetFiles(directoryPath)
        |> Array.map (fun filePath -> filePath, File.GetCreationTime(filePath))
        |> Array.sortBy snd
        |> Array.map fst
        |> Array.toList


    let getOutputDataType (outputData: outputData) : outputDataType =
        match outputData with
        | Run2 _ -> outputDataType.Run2
        | SorterSet _ -> outputDataType.SorterSet
        | SortableTestSet _ -> outputDataType.SortableTestSet
        | SorterModelSetMaker _ -> outputDataType.SorterModelSetMaker
        | SortableTestModelSet _ -> outputDataType.SortableTestModelSet
        | SortableTestModelSetMaker _ -> outputDataType.SortableTestModelSetMaker
        | SorterSetEval _ -> outputDataType.SorterSetEval
        | SorterSetEvalBins _ -> outputDataType.SorterSetEvalBins
        | Workspace _ -> outputDataType.Workspace


    let getOutputFilePath (rootFolder) (run:run2) (outputData: outputData) =
        let index = run.Index
        let repl = %run.RunParameters.GetRepl()
        let dataTypeName = outputData |> getOutputDataType |> OutputDataType.toString
        match outputData with
        | Run2 _ -> Path.Combine(rootFolder, dataTypeName, sprintf "%s_%d_%d.msgpack" dataTypeName %repl index)
        | SorterSet _ -> Path.Combine(rootFolder, dataTypeName, sprintf "%s_%d_%d.msgpack" dataTypeName %repl index)
        | SortableTestSet _ -> Path.Combine(rootFolder, dataTypeName, sprintf "%s_%d_%d.msgpack" dataTypeName %repl index)
        | SorterModelSetMaker _ -> Path.Combine(rootFolder, dataTypeName, sprintf "%s_%d_%d.msgpack" dataTypeName %repl index)
        | SortableTestModelSet _ -> Path.Combine(rootFolder, dataTypeName, sprintf "%s_%d_%d.msgpack" dataTypeName %repl index)
        | SortableTestModelSetMaker _ -> Path.Combine(rootFolder, dataTypeName, sprintf "%s_%d_%d.msgpack" dataTypeName %repl index)
        | SorterSetEval _ -> Path.Combine(rootFolder, dataTypeName, sprintf "%s_%d_%d.msgpack" dataTypeName %repl index)
        | SorterSetEvalBins _ -> Path.Combine(rootFolder, dataTypeName, sprintf "%s_%d_%d.msgpack" dataTypeName %repl index)
        | Workspace _ -> Path.Combine(rootFolder, dataTypeName, sprintf "%s.msgpack" dataTypeName)




        /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)


    let getOutputDataFolder (workspace:workspace) (outputDataType: outputDataType) 
                    : string =
        Path.Combine(workspace.WorkspaceFolder, outputDataType |> OutputDataType.toString)


    let makeIndexAndReplName 
                (folder:string) 
                (runParameters:runParameters)
                (outputDataType: outputDataType) 
                    : string =
        let index = runParameters.GetIndex()
        let repl = runParameters.GetRepl()
        let outputDataName = outputDataType |> OutputDataType.toString
        let fileName = sprintf "%s_%d_%d.msgpack" outputDataName %repl %index 
        Path.Combine(folder, outputDataName, fileName)


    let getOutputDataFileName
            (workspaceP: workspace)
            (runParameters: runParameters option)
            (outputDataType: outputDataType) 
                : string =

        match outputDataType with
        | outputDataType.Workspace -> 
            let fileName = sprintf "%s.msgpack" (outputDataType |> OutputDataType.toString)
            Path.Combine(workspaceP.WorkspaceFolder, fileName)
        | _ -> 
            if runParameters.IsNone then
                failwithf "Run parameters must be provided for output data type %s" (outputDataType |> OutputDataType.toString)
            makeIndexAndReplName workspaceP.WorkspaceFolder runParameters.Value outputDataType


    let getOutputData
            (workspace: workspace)
            (runParameters: runParameters option)
            (outputDataType: outputDataType) 
                : outputData =
        let filePath = getOutputDataFileName workspace runParameters outputDataType
        if not (File.Exists filePath) then
            failwithf "File not found: %s" filePath
        
        try
            use stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            match outputDataType with
            | outputDataType.Run2 ->
                let dto = MessagePackSerializer.Deserialize<run2Dto>(stream, options)
                Run2 (Run2Dto.fromDto dto)
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
            | outputDataType.Workspace ->
                let dto = MessagePackSerializer.Deserialize<workspaceDto>(stream, options)
                Workspace (WorkspaceDto.toDomain dto)
        with e ->
            failwithf "Error reading file %s: %s" filePath e.Message


    let getRun2 (workspace: workspace) (runParameters: runParameters) : run2 =
        match getOutputData workspace (Some runParameters) outputDataType.Run2 with
        | Run2 r -> r
        | _ -> failwith "Unexpected output data type: expected Run2"

    let getSorterSet (workspace: workspace) (runParameters: runParameters) : sorterSet =
        match getOutputData workspace (Some runParameters) outputDataType.SorterSet with
        | SorterSet ss -> ss
        | _ -> failwith "Unexpected output data type: expected SorterSet"

    let getSortableTestSet (workspace: workspace) (runParameters: runParameters) : sortableTestSet =
        match getOutputData workspace (Some runParameters) outputDataType.SortableTestSet with
        | SortableTestSet sts -> sts
        | _ -> failwith "Unexpected output data type: expected SortableTestSet"

    let getSorterModelSetMaker (workspace: workspace) (runParameters: runParameters) : sorterModelSetMaker =
        match getOutputData workspace (Some runParameters) outputDataType.SorterModelSetMaker with
        | SorterModelSetMaker sms -> sms
        | _ -> failwith "Unexpected output data type: expected SorterModelSetMaker"

    let getSortableTestModelSet (workspace: workspace) (runParameters: runParameters) : sortableTestModelSet =
        match getOutputData workspace (Some runParameters) outputDataType.SortableTestModelSet with
        | SortableTestModelSet sts -> sts
        | _ -> failwith "Unexpected output data type: expected SortableTestModelSet"

    let getSortableTestModelSetMaker (workspace: workspace) (runParameters: runParameters) : sortableTestModelSetMaker =
        match getOutputData workspace (Some runParameters) outputDataType.SortableTestModelSetMaker with
        | SortableTestModelSetMaker stsm -> stsm
        | _ -> failwith "Unexpected output data type: expected SortableTestModelSetMaker"

    let getSorterSetEval (workspace: workspace) (runParameters: runParameters) : sorterSetEval =
        match getOutputData workspace (Some runParameters) outputDataType.SorterSetEval with
        | SorterSetEval sse -> sse
        | _ -> failwith "Unexpected output data type: expected SorterSetEval"

    let getSorterSetEvalBins (workspace: workspace) (runParameters: runParameters) : sorterSetEvalBins =
        match getOutputData workspace (Some runParameters) outputDataType.SorterSetEvalBins with
        | SorterSetEvalBins sse -> sse
        | _ -> failwith "Unexpected output data type: expected SorterSetEvalBins"

    let getWorkspace (workspace: workspace) (runParameters: runParameters) : workspace =
        match getOutputData workspace (Some runParameters) outputDataType.Workspace with
        | Workspace w -> w
        | _ -> failwith "Unexpected output data type: expected Workspace"



    let saveToFileO 
            (workspace: workspace) 
            (runParameters: runParameters)
            (outputData: outputData) : Async<unit> =
        async {
            let filePath = getOutputDataFileName workspace (Some runParameters) (outputData |> getOutputDataType)
            let directory = Path.GetDirectoryName filePath
            Directory.CreateDirectory directory |> ignore
            try
                use stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None)
                match outputData with
                | Run2 r -> 
                    let dto = Run2Dto.toRunDto r
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
                | Workspace w ->
                    let dto = WorkspaceDto.toWorkspaceDto w
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask

            with e ->
                printfn "Error saving to file %s: %s" filePath e.Message
                raise e // Re-throw to ensure the caller is aware of the failure
        }


    let saveToFile
            (filePath: string)
            (runParameters: runParameters option)
            (outputData: outputData) : Async<unit> =
        async {
            let directory = Path.GetDirectoryName filePath
            Directory.CreateDirectory directory |> ignore
            try
                use stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None)
                match outputData with
                | Run2 r -> 
                    let dto = Run2Dto.toRunDto r
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
                | Workspace w ->
                    let dto = WorkspaceDto.toWorkspaceDto w
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask

            with e ->
                printfn "Error saving to file %s: %s" filePath e.Message
                raise e // Re-throw to ensure the caller is aware of the failure
        }


    let getRunFileAsync (runFilePath: string) (ct: CancellationToken) : Async<Result<runParameters, string>> =
        async {
            try
                use stream = new FileStream(runFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                let! dto = MessagePackSerializer.DeserializeAsync<run2Dto>(stream, options, ct).AsTask() |> Async.AwaitTask
                let run = Run2Dto.fromDto dto
                return Ok run.RunParameters
            with
            | :? FileNotFoundException -> return Error (sprintf "File not found: %s" runFilePath)
            | :? IOException as e -> return Error (sprintf "IO error reading %s: %s" runFilePath e.Message)
            | :? MessagePackSerializationException as e -> return Error (sprintf "Deserialization error in %s: %s" runFilePath e.Message)
            | e -> return Error (sprintf "Unexpected error in %s: %s" runFilePath e.Message)
        }


    let getRunParametersAsync 
                (workspace: workspace) 
                (ct: CancellationToken) 
                (progress: IProgress<string>) : Async<runParameters[]> =
        async {
            let folder = getOutputDataFolder workspace outputDataType.Run2
            let filePaths = getFilesSortedByCreationTime folder |> List.toArray  // Snapshot to array for safety
        
            progress.Report(sprintf "Found %d files in %s" filePaths.Length folder)
        
            let mutable results = []
            for i = 0 to filePaths.Length - 1 do
                ct.ThrowIfCancellationRequested()  // Check cancellation
                let filePath = filePaths.[i]
                progress.Report(sprintf "Processing file %d/%d: %s" (i+1) filePaths.Length (Path.GetFileName filePath))
            
                let! result = getRunFileAsync filePath ct
                match result with
                | Ok runParams -> results <- runParams :: results
                | Error msg -> progress.Report(sprintf "Skipped due to error: %s" msg)  // Log error, continue
        
            return results |> List.rev |> List.toArray  // Reverse to maintain original order
        }

