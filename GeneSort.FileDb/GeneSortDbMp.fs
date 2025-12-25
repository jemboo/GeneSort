namespace GeneSort.FileDb
open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db
open GeneSort.Runs
open GeneSort.Project
open System.IO

type private DbMessage =
    | Save of queryParams * outputData * AsyncReplyChannel<unit> * bool<allowOverwrite>
    | Load of queryParams * AsyncReplyChannel<Result<outputData, OutputError>>
    | GetAllProjectRunParameters of 
            string<projectName> * CancellationToken option * 
            IProgress<string> option * AsyncReplyChannel<Result<runParameters[], string>>

type GeneSortDbMp(rootFolder: string<pathToRootFolder>) =

    let getPathToProjectFolder (projectName: string) = 
        Path.Combine(%rootFolder, projectName) |> UMX.tag<pathToProjectFolder>

    let saveAsync (queryParams: queryParams) (data: outputData) (allowOverwrite: bool<allowOverwrite>) =
        OutputDataFile.saveToFileAsync (getPathToProjectFolder (queryParams.ProjectName)) queryParams data allowOverwrite
    
    let loadAsync (queryParams: queryParams) =
        OutputDataFile.getOutputDataAsync (getPathToProjectFolder queryParams.ProjectName) queryParams

    let mailbox = MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                let! msg = inbox.Receive()
                
                match msg with
                | Save (queryParams, data, replyChannel, allowOverwrite) ->
                    do! saveAsync queryParams data allowOverwrite
                    replyChannel.Reply()
                    
                | Load (queryParams, replyChannel) ->
                    let! result = loadAsync queryParams
                    replyChannel.Reply(result)
                    
                | GetAllProjectRunParameters (projectName, ct, progress, replyChannel) ->
                    let! result = OutputDataFile.getAllProjectRunParametersAsync (getPathToProjectFolder %projectName) ct progress
                    replyChannel.Reply(result)
                
                return! loop ()
            }
        loop ()
    )
    
    member _.RootFolder = rootFolder
    
    interface IGeneSortDb with

        member this.getAllProjectNamesAsync(): Async<Result<string<projectName> array,string>> =
            async {
                try
                    let root = %rootFolder
                    if not (Directory.Exists(root)) then
                        return Result.Error(sprintf "Root folder '%s' does not exist" root)
                    else
                        let dirs = Directory.GetDirectories(root)
                        let names = dirs |> Array.map (Path.GetFileName >> UMX.tag<projectName>)
                        return Result.Ok names
                with
                | ex -> return Result.Error ex.Message
            }

        member _.saveAsync 
                    (queryParams: queryParams) 
                    (data: outputData) 
                    (allowOverwrite: bool<allowOverwrite>) : Async<unit> =
            mailbox.PostAndAsyncReply(fun channel -> Save(queryParams, data, channel, allowOverwrite))
        
        member _.loadAsync (queryParams: queryParams) : Async<Result<outputData, OutputError>> =
            mailbox.PostAndAsyncReply(fun channel -> Load(queryParams, channel))
        
        member _.getAllProjectRunParametersAsync 
                        (projectName: string<projectName>) 
                        (ct: CancellationToken option) 
                        (progress: IProgress<string> option) : Async<Result<runParameters[], string>> =
            mailbox.PostAndAsyncReply(fun channel -> GetAllProjectRunParameters(projectName, ct, progress, channel))

        member this.saveAllRunParametersAsync 
                        (runParamsArray: runParameters[]) 
                        (yab: runParameters -> outputDataType -> queryParams)
                        (allowOverwrite: bool<allowOverwrite>)
                        (ct: CancellationToken option) 
                        (progress: IProgress<string> option) : Async<unit> =
            async {
                for runParams in runParamsArray do
                    let queryParamsForRunParams = yab runParams outputDataType.RunParameters
                    do! (this :> IGeneSortDb).saveAsync queryParamsForRunParams (runParams |> outputData.RunParameters) allowOverwrite
                    match progress with
                    | Some p -> p.Report(sprintf "Saved RunParameters for Run %s" %(runParams.GetId().Value))
                    | None -> ()
                return ()
            }


