
namespace GeneSort.Project

open FSharp.UMX
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open System.IO
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
    | Run
    | SorterSet
    | SortableTestSet
    | SorterModelSetMaker
    | SortableTestModelSet
    | SortableTestModelSetMaker
    | SorterSetEval
    | SorterSetEvalBins
   // | SorterSetCeUseProfile


     
module OutputDataType =
    
    let toString (outputDataType: outputDataType) : string =
        match outputDataType with
        | Run -> "Run"
        | SorterSet -> "SorterSet"
        | SortableTestSet -> "SortableTestSet"
        | SorterModelSetMaker -> "SorterModelSet"
        | SortableTestModelSet -> "SortableTestModelSet"
        | SortableTestModelSetMaker -> "SortableTestModelSetMaker"
        | SorterSetEval -> "SorterSetEval"
        | SorterSetEvalBins -> "SorterSetEvalBins"
       // | SorterSetCeUseProfile -> "SorterSetCeUseProfile"
        | _ -> failwith "Unknown OutputData type"


type outputData =
    | Run of Run
    | SorterSet of sorterSet
    | SortableTestSet of sortableTestSet
    | SorterModelSetMaker of sorterModelSetMaker
    | SortableTestModelSet of sortableTestModelSet
    | SortableTestModelSetMaker of sortableTestModelSetMaker
    | SorterSetEval of sorterSetEval
    | SorterSetEvalBins of sorterSetEvalBins
    //| SorterSetCeUseProfile of sorterSetCeUseProfile


     
module OutputData =

    let getOutputDataType (outputData: outputData) : outputDataType =
        match outputData with
        | Run _ -> outputDataType.Run
        | SorterSet _ -> outputDataType.SorterSet
        | SortableTestSet _ -> outputDataType.SortableTestSet
        | SorterModelSetMaker _ -> outputDataType.SorterModelSetMaker
        | SortableTestModelSet _ -> outputDataType.SortableTestModelSet
        | SortableTestModelSetMaker _ -> outputDataType.SortableTestModelSetMaker
        | SorterSetEval _ -> outputDataType.SorterSetEval
        | SorterSetEvalBins _ -> outputDataType.SorterSetEvalBins
       // | SorterSetCeUseProfile _ -> outputDataType.SorterSetCeUseProfile

        /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

         

    let getOutputDataFolder (workspace:workspace) (outputDataType: outputDataType) 
                    : string =
        Path.Combine(workspace.WorkspaceFolder, outputDataType |> OutputDataType.toString)

    let getOutputFileName 
                (folder:string) (index:int) (cycle: int<cycleNumber>) 
                (outputDataName: string) 
                : string =
        let fileName = sprintf "%s_%d_%d.msgpack" outputDataName %cycle index 
        Path.Combine(folder, outputDataName, fileName)


    let getRunFileNameForOutputDataFileName  (outputFilePath:string) : string =
        let folder = Path.GetDirectoryName (Path.GetDirectoryName outputFilePath)
        let outputName = Path.GetFileName outputFilePath
        let pcs = outputName.Split('_')
        let cycle = pcs.[1]
        let index = pcs.[2].Split('.').[0]
        let fileName = sprintf "Run_%s_%s.msgpack" cycle index 
        Path.Combine(folder, "Run", fileName)


    let saveToFile 
            (workspaceFolder: string) 
            (index: int) 
            (cycle: int<cycleNumber>) 
            (outputData: outputData) : Async<unit> =
        async {
            let filePath = getOutputFileName workspaceFolder index cycle (outputData |> getOutputDataType |> OutputDataType.toString)
            let directory = Path.GetDirectoryName filePath
            Directory.CreateDirectory directory |> ignore
            try
                use stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None)
                match outputData with
                | Run r -> 
                    let dto = RunDto.toRunDto r
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
                    let dto = SorterTestModelSetMakerDto.fromDomain stsm
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SorterSetEval sse ->
                    let dto = SorterSetEvalDto.fromDomain sse
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SorterSetEvalBins sse ->
                    let dto = SorterSetEvalBinsDto.fromDomain sse
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask

            with e ->
                printfn "Error saving to file %s: %s" filePath e.Message
                raise e // Re-throw to ensure the caller is aware of the failure
        }


    let getRunParametersForOutputDataPath
            (outputDataPath: string) =
            let runPath = getRunFileNameForOutputDataFileName outputDataPath
            if not (File.Exists runPath) then
                failwith (sprintf "Run file %s does not exist" runPath)
            try
                use stream = new FileStream(runPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                let dto = MessagePackSerializer.Deserialize<RunDto>(stream, options)
                let run = RunDto.fromDto dto
                run.Parameters
            with e ->
                failwith "Error reading Run file %s: %s" runPath e.Message

