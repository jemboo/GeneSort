namespace GeneSort.FileDb.V1

open System
open System.IO
open System.Threading
open FSharp.UMX
open GeneSort.Db.V1
open GeneSort.Project.V1


type private DbMessage =
    | Save of string<pathToRootFolder> * queryParams * outputData * bool<allowOverwrite> * AsyncReplyChannel<Result<unit, string>>
    | Load of string<pathToRootFolder> * queryParams * AsyncReplyChannel<Result<outputData, string>>
    | GetRunParameters of string<runName> * (int<replNumber> option) * (int<replNumber> option) * CancellationToken option * IProgress<string> option * AsyncReplyChannel<Result<runParameters[], string>>

type GeneSortDbMp(
                rootFolder: string<pathToRootFolder>,
                queryParamsMaker: runParameters -> outputDataType -> queryParams option) =

    let mailbox = MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                let! msg = inbox.Receive()
                match msg with
                | Save (projectFolder, queryParams, data, allowOverwrite, replyChannel) ->
                    let! res = OutputDataFile.saveToFileAsync projectFolder queryParams data allowOverwrite
                    replyChannel.Reply res
                | Load (projectFolder, queryParams, replyChannel) ->
                    let! res = OutputDataFile.getOutputDataAsync projectFolder queryParams None
                    replyChannel.Reply res
                | GetRunParameters (runName, replMin, replMax, ct, progress, replyChannel) ->
                    let! res = OutputDataFile.getRunParameters rootFolder runName replMin replMax ct progress
                    replyChannel.Reply res
                return! loop ()
            }
        loop ()
    )

    member _.RootFolder = rootFolder
    member _.QueryParamsMaker = queryParamsMaker

    interface IGeneSortDb with
        member _.databaseName
            with get (): string<databaseName> = DirectoryInfo(%rootFolder).Name |> UMX.tag

        member _.MakeQueryParamsFromRunParams rp odt =
            queryParamsMaker rp odt

        member _.saveAsync (queryParams: queryParams) (data: outputData) (allowOverwrite: bool<allowOverwrite>) =
            mailbox.PostAndAsyncReply(fun channel -> Save(rootFolder, queryParams, data, allowOverwrite, channel))

        member _.loadAsync (queryParams: queryParams) =
            mailbox.PostAndAsyncReply(fun channel -> Load(rootFolder, queryParams, channel))

        member _.doesOutPutDataExist (queryParams: queryParams) =
                    async {
                        let filePath = OutputDataFile.getFullOutputDataFilePath rootFolder queryParams
                        return File.Exists %filePath
                    }

        member this.loadIfFoundAsync(queryParams: queryParams) =
            async {
                let filePath = OutputDataFile.getFullOutputDataFilePath rootFolder queryParams
                if not (File.Exists %filePath) then
                    return None
                else
                    let! loadResult = (this :> IGeneSortDb).loadAsync queryParams
                    match loadResult with
                    | Ok data -> return Some data
                    | Error _ -> return None
            }

        member _.getRunParameters 
                        (runName: string<runName>) 
                        (minReplNumber: int<replNumber> option) 
                        (maxReplNumber: int<replNumber> option) 
                        (ct: CancellationToken option) 
                        (progress: IProgress<string> option) =
                mailbox.PostAndAsyncReply(fun channel -> 
                    GetRunParameters(runName, minReplNumber, maxReplNumber, ct, progress, channel))