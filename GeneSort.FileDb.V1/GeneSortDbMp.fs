namespace GeneSort.FileDb.V1
open System
open System.IO
open System.Threading
open FSharp.UMX
open GeneSort.Db.V1
open GeneSort.Project.V1

type private DbMessage =
    | Save of string<pathToProjectFolder> * queryParams * outputData * bool<allowOverwrite> * AsyncReplyChannel<Result<unit, string>>
    | Load of string<pathToProjectFolder> * queryParams * AsyncReplyChannel<Result<outputData, string>>
    | GetProjectRunParametersForReplRange of (int<replNumber> option) * (int<replNumber> option) * CancellationToken option * IProgress<string> option * AsyncReplyChannel<Result<runParameters[], string>>

type GeneSortDbMp(projectFolder: string<pathToProjectFolder>) =


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
                | GetProjectRunParametersForReplRange (replMin, replMax, ct, progress, replyChannel) ->
                    let! res = OutputDataFile.getRunParameters projectFolder replMin replMax ct progress
                    replyChannel.Reply res
                return! loop ()
            }
        loop ()
    )

    member _.ProjectFolder = projectFolder

    interface IGeneSortDb with
        member _.projectName
            with get (): string<projectName> = DirectoryInfo(%projectFolder).Name |> UMX.tag

        member _.saveAsync
                    (queryParams: queryParams)
                    (data: outputData)
                    (allowOverwrite: bool<allowOverwrite>) : Async<Result<unit, string>> =
            mailbox.PostAndAsyncReply(fun channel -> Save(projectFolder, queryParams, data, allowOverwrite, channel))

        member _.loadAsync 
                    (queryParams: queryParams) : Async<Result<outputData, OutputError>> =
            mailbox.PostAndAsyncReply(fun channel -> Load(projectFolder, queryParams, channel))

        member _.getRunParameters
                        (minReplNumber: int<replNumber> option)
                        (maxReplNumber: int<replNumber> option)
                        (ct: CancellationToken option)
                        (progress: IProgress<string> option) : Async<Result<runParameters[], string>> =
            mailbox.PostAndAsyncReply(fun channel -> GetProjectRunParametersForReplRange(minReplNumber, maxReplNumber, ct, progress, channel))

        member this.saveRunParameters
                        (runParamsArray: runParameters[])
                        (buildQueryParams: runParameters -> outputDataType -> queryParams)
                        (allowOverwrite: bool<allowOverwrite>)
                        (ct: CancellationToken option)
                        (progress: IProgress<string> option) : Async<Result<unit, string>> =
            async {
                let token = defaultArg ct CancellationToken.None
                let maxDegree = Environment.ProcessorCount / 2  // Configurable based on system.
                use semaphore = new SemaphoreSlim(maxDegree)
                let! results = 
                    runParamsArray 
                    |> Array.map (fun rp -> async {
                        token.ThrowIfCancellationRequested()
                        do! semaphore.WaitAsync(token) |> Async.AwaitTask
                        try
                            let qp = buildQueryParams rp outputDataType.RunParameters
                            let! res = (this :> IGeneSortDb).saveAsync qp (rp |> outputData.RunParameters) allowOverwrite
                            progress |> Option.iter (fun p -> p.Report(sprintf "Saved RunParameters for Run %s Repl %s" (rp |> RunParameters.getIdString) (rp.GetRepl() |> queryParams.ReplString)))
                            return res
                        finally semaphore.Release() |> ignore
                    })
                    |> Async.Parallel
                let errors = results |> Array.choose (function Error e -> Some e | _ -> None)
                if errors.Length > 0 then return Error (String.Join("; ", errors))
                else return Ok ()
            }