namespace GeneSort.FileDb.V1

open System
open System.IO
open System.Threading
open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage


type private DbMessage =
    | SaveParameterSet of runParameterSet * runParameterWriteMode * IProgress<string> option * AsyncReplyChannel<Result<unit, string>>
    | Save of string<pathToRootFolder> * geneSortDataMode * queryParams * outputData * bool<allowOverwrite> * AsyncReplyChannel<Result<unit, string>>
    | Load of string<pathToRootFolder> * geneSortDataMode * queryParams * AsyncReplyChannel<Result<outputData, string>>
    | GetRunParameters of string<runName> * (int<replNumber> option) * (int<replNumber> option) * CancellationToken option * IProgress<string> option * AsyncReplyChannel<Result<runParameters[], string>>

type GeneSortDbMp(
                rootFolder: string<pathToRootFolder>,
                ?queryCatalogName: string) =

    let dataFolder = DirectoryInfo(%rootFolder)
    let databaseFolder = dataFolder.Parent
    let projectFolder = if isNull databaseFolder then null else databaseFolder.Parent
    let catalogName = defaultArg queryCatalogName (
        if isNull databaseFolder || isNull projectFolder then
            invalidArg (nameof rootFolder) "The data folder must be nested under a project and database folder."
        QueryParamsCatalogRun.nameForDatabase projectFolder.Name databaseFolder.Name)

    let mailbox = MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                let! msg = inbox.Receive()
                match msg with
                | SaveParameterSet (prepared, mode, progress, replyChannel) ->
                    let! res = RunFileStorage.publishAsync rootFolder prepared mode progress |> Async.AwaitTask
                    replyChannel.Reply res
                | Save (projectFolder, dataMode, queryParams, data, allowOverwrite, replyChannel) ->
                    let! res = OutputDataFile.saveToFileAsync projectFolder dataMode queryParams data allowOverwrite
                    replyChannel.Reply res
                | Load (projectFolder, dataMode, queryParams, replyChannel) ->
                    let! res = OutputDataFile.getOutputDataAsync projectFolder dataMode queryParams None
                    replyChannel.Reply res
                | GetRunParameters (runName, replMin, replMax, ct, progress, replyChannel) ->
                    let! res = OutputDataFile.getRunParameters rootFolder runName replMin replMax ct progress
                    replyChannel.Reply res
                return! loop ()
            }
        loop ()
    )

    member _.RootFolder = rootFolder

    interface IRunParameterStore with
        member _.saveRunParameterSetAsync prepared mode progress =
            mailbox.PostAndAsyncReply(fun reply -> SaveParameterSet(prepared, mode, progress, reply))

    interface IGeneSortDb with
        member _.databaseName
            with get (): string<databaseName> = databaseFolder.Name |> UMX.tag

        member _.makeQueryParamsFromRunParams rp odt =
            let projectName = projectFolder.Name |> UMX.tag<projectName>
            let databaseName = databaseFolder.Name |> UMX.tag<databaseName>
            (QueryParamsCatalogRun.get catalogName projectName databaseName) rp odt

        member _.saveAsync dataMode (queryParams: queryParams) (data: outputData) (allowOverwrite: bool<allowOverwrite>) =
            mailbox.PostAndAsyncReply(fun channel -> Save(rootFolder, dataMode, queryParams, data, allowOverwrite, channel))

        member _.loadAsync dataMode (queryParams: queryParams) =
            mailbox.PostAndAsyncReply(fun channel -> Load(rootFolder, dataMode, queryParams, channel))

        member _.doesOutPutDataExist dataMode (queryParams: queryParams) =
                    async {
                        let filePath = OutputDataFile.getFullOutputDataFilePath rootFolder dataMode queryParams
                        return File.Exists %filePath
                    }

        member this.loadIfFoundAsync dataMode (queryParams: queryParams) =
            async {
                let filePath = OutputDataFile.getFullOutputDataFilePath rootFolder dataMode queryParams
                if not (File.Exists %filePath) then
                    return None
                else
                    let! loadResult = (this :> IGeneSortDb).loadAsync dataMode queryParams
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
