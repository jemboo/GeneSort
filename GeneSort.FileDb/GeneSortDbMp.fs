namespace GeneSort.FileDb
open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Db
open GeneSort.Runs.Params
open GeneSort.Project
open System.IO

type private DbMessage =
    | Save of queryParams * outputData * AsyncReplyChannel<unit>
    | Load of queryParams * outputDataType * AsyncReplyChannel<Result<outputData, OutputError>>
    | GetAllRunParameters of string<projectName> * CancellationToken option * IProgress<string> option * AsyncReplyChannel<runParameters[]>

type GeneSortDbMp(rootFolder: string) =

    let getProjectFolder (queryParams: queryParams) =
        Path.Combine(rootFolder, %queryParams.ProjectName)

    let saveAsync (queryParams: queryParams) (data: outputData) =
        OutputDataFile.saveToFileAsync (getProjectFolder queryParams) queryParams data
    
    let loadAsync (queryParams: queryParams) (dataType: outputDataType) =
        OutputDataFile.getOutputDataAsync (getProjectFolder queryParams) queryParams dataType
    
    let getProjectFolder (projectName: string<projectName>) = 
        Path.Combine(rootFolder, %projectName)
    
    let getAllRunParametersAsync 
                (projectName: string<projectName>) 
                (ct: CancellationToken option) 
                (progress: IProgress<string> option) =
        OutputDataFile.getAllRunParametersAsync (getProjectFolder projectName) ct progress
    

    let mailbox = MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                let! msg = inbox.Receive()
                
                match msg with
                | Save (queryParams, data, replyChannel) ->
                    do! saveAsync queryParams data
                    replyChannel.Reply()
                    
                | Load (queryParams, dataType, replyChannel) ->
                    let! result = loadAsync queryParams dataType
                    replyChannel.Reply(result)
                    
                | GetAllRunParameters (projectName, ct, progress, replyChannel) ->
                    let! result = getAllRunParametersAsync projectName ct progress
                    replyChannel.Reply(result)
                
                return! loop ()
            }
        loop ()
    )
    
    member _.RootFolder = rootFolder
    
    interface IGeneSortDb with
        member _.saveAsync (queryParams: queryParams) (data: outputData) : Async<unit> =
            mailbox.PostAndAsyncReply(fun channel -> Save(queryParams, data, channel))
        
        member _.loadAsync (queryParams: queryParams) (dataType: outputDataType) : Async<Result<outputData, OutputError>> =
            mailbox.PostAndAsyncReply(fun channel -> Load(queryParams, dataType, channel))
        
        member _.getAllRunParametersAsync (projectName: string<projectName>) (ct: CancellationToken option) (progress: IProgress<string> option) : Async<runParameters[]> =
            mailbox.PostAndAsyncReply(fun channel -> GetAllRunParameters(projectName, ct, progress, channel))

        member this.saveAllRunParametersAsync 
                        (projectName: string<projectName>) 
                        (runParamsArray: runParameters[]) 
                        (ct: CancellationToken option) 
                        (progress: IProgress<string> option) : Async<unit> =
            async {
                for runParams in runParamsArray do
                    let queryParamsForRunParams = queryParams.Create(
                                runParams.GetProjectName().Value,
                                runParams.GetIndex(),
                                runParams.GetRepl(), 
                                None, 
                                outputDataType.RunParameters)
                    do! (this :> IGeneSortDb).saveAsync queryParamsForRunParams (runParams |> outputData.RunParameters)
                    match progress with
                    | Some p -> p.Report(sprintf "Saved RunParameters for Run %d" (runParams.GetIndex().Value))
                    | None -> ()
                return ()
            }


