
namespace GeneSort.Project

open System
open FSharp.UMX
open GeneSort.Sorter
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open System.IO
open GeneSort.Core.Combinatorics
open System.Threading.Tasks
open GeneSort.Model.Sorter



//type OutputDataType =
//    | Run
//    | SorterSet
//    | SorterModelSet


//module OutputDataType =
//    let toString = function
//        | Run -> "Run"
//        | SorterSet -> "SorterSet"
//        | SorterModelSet -> "SorterModelSet"
//    let fromString (s: string) =
//        match s with
//        | "Run" -> Run
//        | "SorterSet" -> SorterSet
//        | "SorterModelSet" -> SorterModelSet
//        | _ -> failwithf "Unknown OutputFileType: %s" s


type OutputData =
    | Run of Run
    | SorterSet of SorterSet
    | SorterModelSet of SorterModelSetMaker


module OutputData =
        /// Options for MessagePack serialization, using FSharpResolver and StandardResolver.
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)
    
    let toString (outputData: OutputData) : string =
        match outputData with
        | Run _ -> "Run"
        | SorterSet _ -> "SorterSet"
        | SorterModelSet _ -> "SorterModelSet"

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
            (workspaceFolder:string) 
            (index:int) 
            (cycle: int<cycleNumber>) 
            (outputData: OutputData) : System.Threading.Tasks.Task =
    
        let filePath = getOutputFileName workspaceFolder index cycle (toString outputData)
        let directory = Path.GetDirectoryName filePath
        Directory.CreateDirectory directory |> ignore
        use stream = new FileStream(filePath, FileMode.Create, FileAccess.Write)
        match outputData with
        | Run r -> 
            let dto = RunDto.toRunDto r
            MessagePackSerializer.SerializeAsync(stream, dto, options)// |> Async.AwaitTask |> Async.RunSynchronously
        | SorterSet ss -> 
            failwith "SorterSet serialization not implemented yet"
            //let dto = SorterSetDto.toSorterSetDto ss
            //MessagePackSerializer.SerializeAsync(stream, dto, options)// |> Async.AwaitTask |> Async.RunSynchronously    
        | SorterModelSet sms -> 
            failwith "SorterModelSet serialization not implemented yet"
            //let dto = SorterModelSetDto.toSorterModelSetDto sms
            //MessagePackSerializer.SerializeAsync(stream, dto, options)// |> Async.AwaitTask |> Async.RunSynchronously
