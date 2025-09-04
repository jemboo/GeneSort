
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

type outputDataType =
    | Run
    | SorterSet
    | SorterTestSet
    | SorterModelSetMaker
    | SorterTestModelSet
    | SorterTestModelSetMaker
    | SorterSetEvalSamples


     
module OutputDataType =
    
    let toString (outputDataType: outputDataType) : string =
        match outputDataType with
        | Run -> "Run"
        | SorterSet -> "SorterSet"
        | SorterTestSet -> "SorterTestSet"
        | SorterModelSetMaker -> "SorterModelSet"
        | SorterTestModelSet -> "SorterTestModelSet"
        | SorterTestModelSetMaker -> "SorterTestModelSetMaker"
        | SorterSetEvalSamples -> "SorterSetEvalSamples"
        | _ -> failwith "Unknown OutputData type"


type outputData =
    | Run of Run
    | SorterSet of sorterSet
    | SorterTestSet of sortableTestSet
    | SorterModelSetMaker of sorterModelSetMaker
    | SorterTestModelSet of sorterTestModelSet
    | SorterTestModelSetMaker of sorterTestModelSetMaker
    | SorterSetEvalSamples of sorterSetEvalBins


     
module OutputData =

    let getOutputDataType (outputData: outputData) : outputDataType =
        match outputData with
        | Run _ -> outputDataType.Run
        | SorterSet _ -> outputDataType.SorterSet
        | SorterTestSet _ -> outputDataType.SorterTestSet
        | SorterModelSetMaker _ -> outputDataType.SorterModelSetMaker
        | SorterTestModelSet _ -> outputDataType.SorterTestModelSet
        | SorterTestModelSetMaker _ -> outputDataType.SorterTestModelSetMaker
        | SorterSetEvalSamples _ -> outputDataType.SorterSetEvalSamples


        /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

         

    let getOutputDataFolder (workspace:Workspace) (outputDataType: outputDataType) 
                    : string =
        Path.Combine(workspace.WorkspaceFolder, outputDataType |> OutputDataType.toString)

    let getOutputFileName 
                (folder:string) (index:int) (cycle: int<cycleNumber>) 
                (outputDataName: string) 
                : string =
        let fileName = sprintf "%s_%d_%d.msgpack" outputDataName %cycle index 
        Path.Combine(folder, outputDataName, fileName)


    let getRunFileNameForOutputName  (folder:string) (outputName:string) : string =
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
                | SorterTestSet sts ->
                    let dto = SortableTestSetDto.fromDomain sts
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SorterModelSetMaker sms -> 
                    let dto = SorterModelSetMakerDto.fromDomain sms
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SorterTestModelSet sts ->
                    let dto = SorterTestModelSetDto.fromDomain sts
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SorterTestModelSetMaker stsm ->
                    let dto = SorterTestModelSetMakerDto.fromDomain stsm
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SorterSetEvalSamples sse ->
                    let dto = SorterSetEvalBinsDto.fromDomain sse
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask

            with e ->
                printfn "Error saving to file %s: %s" filePath e.Message
                raise e // Re-throw to ensure the caller is aware of the failure
        }