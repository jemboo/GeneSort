namespace GeneSort.FileDb

open System
open System.IO
open System.Threading
open GeneSort.Core
open GeneSort.Db
open GeneSort.Runs.Params
open GeneSort.Project

type private DbMessage2 =
    | Save of runParameters option * outputData * AsyncReplyChannel<unit>
    | Load of runParameters option * outputDataType * AsyncReplyChannel<Result<outputData, OutputError>>
    | GetAllRunParameters of CancellationToken option * IProgress<string> option * AsyncReplyChannel<runParameters[]>


type GeneSortDbMp2(projectFolder: string) =
    
    let saveAsync (runParams: runParameters option) (data: outputData) =
        OutputDataFile2.saveToFileAsync projectFolder runParams data
    
    let loadAsync (runParams: runParameters option) (dataType: outputDataType) =
        OutputDataFile2.getOutputDataAsync projectFolder runParams dataType
    
    let getAllRunParametersAsync (ct: CancellationToken option) (progress: IProgress<string> option) =
        OutputDataFile2.getAllRunParametersAsync projectFolder ct progress
    
    let mailbox = MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                let! msg = inbox.Receive()
                
                match msg with
                | Save (runParams, data, replyChannel) ->
                    do! saveAsync runParams data
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
    
    interface IGeneSortDb2 with
        member _.saveAsync (runParams: runParameters option) (data: outputData) : Async<unit> =
            mailbox.PostAndAsyncReply(fun channel -> Save(runParams, data, channel))
        
        member _.loadAsync (runParams: runParameters option) (dataType: outputDataType) : Async<Result<outputData, OutputError>> =
            mailbox.PostAndAsyncReply(fun channel -> Load(runParams, dataType, channel))
        
        member _.getAllRunParametersAsync (ct: CancellationToken option) (progress: IProgress<string> option) : Async<runParameters[]> =
            mailbox.PostAndAsyncReply(fun channel -> GetAllRunParameters(ct, progress, channel))