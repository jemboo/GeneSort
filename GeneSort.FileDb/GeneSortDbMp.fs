namespace GeneSort.FileDb
open System
open System.IO
open System.Threading
open FSharp.UMX
open GeneSort.Db
open GeneSort.Runs
open GeneSort.Project

type private DbMessage =
    | Save of string<projectFolder> * queryParams * outputData * bool<allowOverwrite> * AsyncReplyChannel<Result<unit, string>>
    | Load of string<projectFolder> * queryParams * AsyncReplyChannel<Result<outputData, string>>
    | GetProjectRunParametersForReplRange of string<projectFolder> * (int<replNumber> option) * (int<replNumber> option) * CancellationToken option * IProgress<string> option * AsyncReplyChannel<Result<runParameters[], string>>

type GeneSortDbMp(rootFolder: string<pathToRootFolder>) =

    let getPathToProjectFolder (projectFolder: string<projectFolder>) =
        Path.Combine(%rootFolder, %projectFolder) |> UMX.tag<pathToProjectFolder>

    let mailbox = MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                let! msg = inbox.Receive()
                match msg with
                | Save (projectFolder, queryParams, data, allowOverwrite, replyChannel) ->
                    let! res = OutputDataFile.saveToFileAsync (getPathToProjectFolder projectFolder) queryParams data allowOverwrite
                    replyChannel.Reply res
                | Load (projectFolder, queryParams, replyChannel) ->
                    let! res = OutputDataFile.getOutputDataAsync (getPathToProjectFolder projectFolder) queryParams None
                    replyChannel.Reply res
                | GetProjectRunParametersForReplRange (projectFolder, replMin, replMax, ct, progress, replyChannel) ->
                    let! res = OutputDataFile.getProjectRunParametersForReplRangeAsync  (getPathToProjectFolder %projectFolder) replMin replMax ct progress
                    replyChannel.Reply res
                return! loop ()
            }
        loop ()
    )

    member _.RootFolder = rootFolder

    interface IGeneSortDb with
        member _.getAllProjectNamesAsync(): Async<Result<string<projectName> array,string>> =
            async {
                try
                    let root = %rootFolder
                    if not (Directory.Exists(root)) then
                        return Error (sprintf "Root folder '%s' does not exist" root)
                    else
                        let dirs = Directory.GetDirectories(root)
                        let names = dirs |> Array.map (Path.GetFileName >> UMX.tag<projectName>)
                        return Ok names
                with ex -> return Error ex.Message
            }

        member _.saveAsync
                    (projectFolder: string<projectFolder>)
                    (queryParams: queryParams)
                    (data: outputData)
                    (allowOverwrite: bool<allowOverwrite>) : Async<Result<unit, string>> =
            mailbox.PostAndAsyncReply(fun channel -> Save(projectFolder, queryParams, data, allowOverwrite, channel))

        member _.loadAsync 
                    (projectFolder: string<projectFolder>)
                    (queryParams: queryParams) : Async<Result<outputData, OutputError>> =
            mailbox.PostAndAsyncReply(fun channel -> Load(projectFolder, queryParams, channel))

        member _.getProjectRunParametersForReplRangeAsync
                        (projectFolder: string<projectFolder>)
                        (minReplNumber: int<replNumber> option)
                        (maxReplNumber: int<replNumber> option)
                        (ct: CancellationToken option)
                        (progress: IProgress<string> option) : Async<Result<runParameters[], string>> =
            mailbox.PostAndAsyncReply(fun channel -> GetProjectRunParametersForReplRange(projectFolder, minReplNumber, maxReplNumber, ct, progress, channel))

        member this.saveAllRunParametersAsync
                        (projectFolder: string<projectFolder>)
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
                            let! res = (this :> IGeneSortDb).saveAsync projectFolder qp (rp |> outputData.RunParameters) allowOverwrite
                            progress |> Option.iter (fun p -> p.Report(sprintf "Saved RunParameters for Run %s Repl %s" (rp |> RunParameters.getIdString) (rp.GetRepl() |> queryParams.ReplString)))
                            return res
                        finally semaphore.Release() |> ignore
                    })
                    |> Async.Parallel
                let errors = results |> Array.choose (function Error e -> Some e | _ -> None)
                if errors.Length > 0 then return Error (String.Join("; ", errors))
                else return Ok ()
            }