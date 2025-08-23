
namespace GeneSort.Project

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open System.IO
open GeneSort.Core.Combinatorics
open System.Threading.Tasks
open GeneSort.Model.Sorter
open GeneSort.Model.Sortable
open GeneSort.Model.Mp.Sorter
open GeneSort.Model.Mp.Sortable
open GeneSort.Sorter.Mp


type OutputData =
    | Run of Run
    | SorterSet of SorterSet
    | SorterModelSetMaker of SorterModelSetMaker
    | SorterTestModelSet of SorterTestModelSet
    | SorterTestModelSetMaker of SorterTestModelSetMaker



module OutputData =
        /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)
    
    let toString (outputData: OutputData) : string =
        match outputData with
        | Run _ -> "Run"
        | SorterSet _ -> "SorterSet"
        | SorterModelSetMaker _ -> "SorterModelSet"
        | SorterTestModelSet _ -> "SorterTestModelSet"
        | SorterTestModelSetMaker _ -> "SorterTestModelSetMaker"

    let getOutputDataFolder (workspace:Workspace) (outputDataFolder: string) 
                    : string =
        Path.Combine(workspace.WorkspaceFolder, outputDataFolder)

    let getOutputFileName 
                (folder:string) (index:int) (cycle: int<cycleNumber>) 
                (outputDataName: string) 
                : string =
        let fileName = sprintf "%s_%d_%d.msgpack" outputDataName %cycle index 
        Path.Combine(folder, outputDataName, fileName)


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
                | SorterModelSetMaker sms -> 
                    let dto = SorterModelSetMakerDto.fromDomain sms
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SorterTestModelSet sts ->
                    let dto = SorterTestModelSetDto.fromDomain sts
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask
                | SorterTestModelSetMaker stsm ->
                    let dto = SorterTestModelSetMakerDto.fromDomain stsm
                    do! MessagePackSerializer.SerializeAsync(stream, dto, options) |> Async.AwaitTask

            with e ->
                printfn "Error saving to file %s: %s" filePath e.Message
                raise e // Re-throw to ensure the caller is aware of the failure
        }