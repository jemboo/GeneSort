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
                rootFolder: string<pathToRootFolder>) =

    let dataFolder = DirectoryInfo(%rootFolder)
    let databaseFolder = dataFolder.Parent
    let projectFolder = if isNull databaseFolder then null else databaseFolder.Parent
    let catalogName =
        if isNull databaseFolder || isNull projectFolder then
            invalidArg (nameof rootFolder) "The data folder must be nested under a project and database folder."
        QueryParamsCatalog.nameForDatabase projectFolder.Name databaseFolder.Name

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

    interface IGeneSortDb with
        member _.databaseName
            with get (): string<databaseName> = databaseFolder.Name |> UMX.tag

        member _.MakeQueryParamsFromRunParams rp odt =
            (QueryParamsCatalog.get catalogName) rp odt

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
