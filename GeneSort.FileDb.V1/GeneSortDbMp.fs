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

        member _.saveAsync
                    (queryParams: queryParams)
                    (data: outputData)
                    (allowOverwrite: bool<allowOverwrite>) : Async<Result<unit, string>> =
            mailbox.PostAndAsyncReply(fun channel -> Save(rootFolder, queryParams, data, allowOverwrite, channel))

        member _.loadAsync 
                    (queryParams: queryParams) : Async<Result<outputData, OutputError>> =
            mailbox.PostAndAsyncReply(fun channel -> Load(rootFolder, queryParams, channel))

        member _.getRunParameters
                        (runName :string<runName>)
                        (minReplNumber: int<replNumber> option)
                        (maxReplNumber: int<replNumber> option)
                        (ct: CancellationToken option)
                        (progress: IProgress<string> option) : Async<Result<runParameters[], string>> =
            mailbox.PostAndAsyncReply(fun channel -> GetRunParameters(runName, minReplNumber, maxReplNumber, ct, progress, channel))

        member this.saveRunParameters
                        (runParamsArray: runParameters[])
                        (buildQueryParams: runParameters -> outputDataType -> queryParams option)
                        (allowOverwrite: bool<allowOverwrite>)
                        (ct: CancellationToken option)
                        (progress: IProgress<string> option) : Async<Result<unit, string>> =
            async {
                let token = defaultArg ct CancellationToken.None
                let maxDegree = Environment.ProcessorCount / 2 
                use semaphore = new SemaphoreSlim(maxDegree)
                
                let! results = 
                    runParamsArray 
                    |> Array.map (fun rp -> async {
                        try
                            token.ThrowIfCancellationRequested()
            
                            // Wait for the semaphore
                            do! semaphore.WaitAsync(token) |> Async.AwaitTask
            
                            // Use a 'use' binding with a custom disposable to ensure release
                            use _release = { new IDisposable with member _.Dispose() = semaphore.Release() |> ignore }

                            let runNameOpt = rp.GetRunName()
                            match runNameOpt with
                            | None -> return Error (sprintf "Missing RunName for %s" (rp |> RunParameters.getIdString))
                            | Some name ->
                                let qpOpt = buildQueryParams rp (outputDataType.RunParameters %name)
                                match qpOpt with
                                | None -> return Error (sprintf "Could not build QueryParams for %s" (rp |> RunParameters.getIdString))
                                | Some qp ->
                                    return! (this :> IGeneSortDb).saveAsync qp (rp |> outputData.RunParameters) allowOverwrite
                        with
                        | :? OperationCanceledException -> return Error "Operation cancelled"
                        | e -> return Error e.Message
                    })
                    |> Async.Parallel

                let errors = results |> Array.choose (function Error e -> Some e | _ -> None)
                if errors.Length > 0 then 
                    return Error (String.Join("; ", errors |> Array.distinct))
                else 
                    return Ok ()
            }