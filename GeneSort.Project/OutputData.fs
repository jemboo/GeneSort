
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


type OutputData =
    | Run of Run
    | SorterSet of sorterSet
    | SorterTestSet of sorterTestSet
    | SorterModelSetMaker of sorterModelSetMaker
    | SorterTestModelSet of sorterTestModelSet
    | SorterTestModelSetMaker of sorterTestModelSetMaker
    | SorterSetEvalSamples of sorterSetEvalSamples


     
module OutputData =
        /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)
    
    let toString (outputData: OutputData) : string =
        match outputData with
        | Run _ -> "Run"
        | SorterSet _ -> "SorterSet"
        | SorterTestSet _ -> "SorterTestSet"
        | SorterModelSetMaker _ -> "SorterModelSet"
        | SorterTestModelSet _ -> "SorterTestModelSet"
        | SorterTestModelSetMaker _ -> "SorterTestModelSetMaker"
        | SorterSetEvalSamples _ -> "SorterSetEvalSamples"
        | _ -> failwith "Unknown OutputData type"
         

    let getOutputDataFolder (workspace:Workspace) (outputDataFolder: string) 
                    : string =
        Path.Combine(workspace.WorkspaceFolder, outputDataFolder)

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
            (outputData: OutputData) : Async<unit> =
        async {
            let filePath = getOutputFileName workspaceFolder index cycle (toString outputData)
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
                    let dto = SorterTestSetDto.fromDomain sts
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
                    let dto = SorterSetEvalSamplesDto.fromDomain sse
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask

            with e ->
                printfn "Error saving to file %s: %s" filePath e.Message
                raise e // Re-throw to ensure the caller is aware of the failure
        }