namespace GeneSort.FileDb

open System
open System.IO
open System.Threading
open GeneSort.Core
open GeneSort.Db
open GeneSort.Runs.Params

type private DbMessage =
    | Save of runParameters option * outputData * outputDataType * AsyncReplyChannel<unit>
    | Load of runParameters option * outputDataType * AsyncReplyChannel<Result<outputData, OutputError>>
    | GetAllRunParameters of CancellationToken option * IProgress<string> option * AsyncReplyChannel<runParameters[]>

type GeneSortDbMp(projectFolder: string) =
    
    let saveAsync (runParams: runParameters option) (data: outputData) (dataType: outputDataType) =
        OutputDataFile.saveToFileAsync projectFolder runParams data
    
    let loadAsync (runParams: runParameters option) (dataType: outputDataType) =
        OutputDataFile.getOutputDataAsync projectFolder runParams dataType
    
    let getAllRunParametersAsync (ct: CancellationToken option) (progress: IProgress<string> option) =
        OutputDataFile.getAllRunParametersAsync projectFolder ct progress
    
    let mailbox = MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                let! msg = inbox.Receive()
                
                match msg with
                | Save (runParams, data, dataType, replyChannel) ->
                    do! saveAsync runParams data dataType
                    replyChannel.Reply()
                    
                | Load (runParams, dataType, replyChannel) ->
                    let! result = loadAsync runParams dataType
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
        member _.saveAsync (runParams: runParameters option) (data: outputData) (dataType: outputDataType) : Async<unit> =
            mailbox.PostAndAsyncReply(fun channel -> Save(runParams, data, dataType, channel))
        
        member _.loadAsync (runParams: runParameters option) (dataType: outputDataType) : Async<Result<outputData, OutputError>> =
            mailbox.PostAndAsyncReply(fun channel -> Load(runParams, dataType, channel))
        
        member _.getAllRunParametersAsync (ct: CancellationToken option) (progress: IProgress<string> option) : Async<runParameters[]> =
            mailbox.PostAndAsyncReply(fun channel -> GetAllRunParameters(ct, progress, channel))