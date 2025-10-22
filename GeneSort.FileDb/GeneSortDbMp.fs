namespace GeneSort.FileDb

open System
open System.Threading
open GeneSort.Core
open GeneSort.Db
open GeneSort.Runs.Params
open GeneSort.Project

type private DbMessage =
    | Save of queryParams * outputData * AsyncReplyChannel<unit>
    | Load of queryParams * outputDataType * AsyncReplyChannel<Result<outputData, OutputError>>
    | GetAllRunParameters of CancellationToken option * IProgress<string> option * AsyncReplyChannel<runParameters[]>


type GeneSortDbMp(projectFolder: string) =
    
    let saveAsync (queryParams: queryParams) (data: outputData) =
        OutputDataFile.saveToFileAsync projectFolder queryParams data
    
    let loadAsync (queryParams: queryParams) (dataType: outputDataType) =
        OutputDataFile.getOutputDataAsync projectFolder queryParams dataType
    
    let getAllRunParametersAsync (ct: CancellationToken option) (progress: IProgress<string> option) =
        OutputDataFile.getAllRunParametersAsync projectFolder ct progress
    
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
                    
                | GetAllRunParameters (ct, progress, replyChannel) ->
                    let! result = getAllRunParametersAsync ct progress
                    replyChannel.Reply(result)
                
                return! loop ()
            }
        loop ()
    )
    
    member _.ProjectFolder = projectFolder
    
    interface IGeneSortDb with
        member _.saveAsync (queryParams: queryParams) (data: outputData) : Async<unit> =
            mailbox.PostAndAsyncReply(fun channel -> Save(queryParams, data, channel))
        
        member _.loadAsync (queryParams: queryParams) (dataType: outputDataType) : Async<Result<outputData, OutputError>> =
            mailbox.PostAndAsyncReply(fun channel -> Load(queryParams, dataType, channel))
        
        member _.getAllRunParametersAsync (ct: CancellationToken option) (progress: IProgress<string> option) : Async<runParameters[]> =
            mailbox.PostAndAsyncReply(fun channel -> GetAllRunParameters(ct, progress, channel))

