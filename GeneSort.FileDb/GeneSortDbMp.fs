namespace GeneSort.FileDb
open System
open System.IO
open System.Threading
open FSharp.UMX
open GeneSort.Db
open GeneSort.Runs
open GeneSort.Project

type private DbMessage =
    | Save of queryParams * outputData * bool<allowOverwrite> * AsyncReplyChannel<Result<unit, string>>
    | Load of queryParams * AsyncReplyChannel<Result<outputData, string>>
    | GetAllProjectRunParameters of string<projectName> * CancellationToken option * IProgress<string> option * AsyncReplyChannel<Result<runParameters[], string>>

type GeneSortDbMp(rootFolder: string<pathToRootFolder>) =
    let getPathToProjectFolder (projectName: string) =
        Path.Combine(%rootFolder, projectName) |> UMX.tag<pathToProjectFolder>

    let mailbox = MailboxProcessor.Start(fun inbox ->
        let rec loop () =
            async {
                let! msg = inbox.Receive()
                match msg with
                | Save (queryParams, data, allowOverwrite, replyChannel) ->
                    let! res = OutputDataFile.saveToFileAsync (getPathToProjectFolder queryParams.ProjectName) queryParams data allowOverwrite
                    replyChannel.Reply res
                | Load (queryParams, replyChannel) ->
                    let! res = OutputDataFile.getOutputDataAsync (getPathToProjectFolder queryParams.ProjectName) queryParams None
                    replyChannel.Reply res
                | GetAllProjectRunParameters (projectName, ct, progress, replyChannel) ->
                    let! res = OutputDataFile.getAllProjectRunParametersAsync (getPathToProjectFolder %projectName) ct progress
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
                    (queryParams: queryParams)
                    (data: outputData)
                    (allowOverwrite: bool<allowOverwrite>) : Async<Result<unit, string>> =
            mailbox.PostAndAsyncReply(fun channel -> Save(queryParams, data, allowOverwrite, channel))

        member _.loadAsync (queryParams: queryParams) : Async<Result<outputData, OutputError>> =
            mailbox.PostAndAsyncReply(fun channel -> Load(queryParams, channel))

        member _.getAllProjectRunParametersAsync
                        (projectName: string<projectName>)
                        (ct: CancellationToken option)
                        (progress: IProgress<string> option) : Async<Result<runParameters[], string>> =
            mailbox.PostAndAsyncReply(fun channel -> GetAllProjectRunParameters(projectName, ct, progress, channel))

        member this.saveAllRunParametersAsync
                        (runParamsArray: runParameters[])
                        (buildQueryParams: runParameters -> outputDataType -> queryParams)  // Renamed yab.
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
                            progress |> Option.iter (fun p -> p.Report(sprintf "Saved RunParameters for Run %s" %(rp.GetId().Value) ))
                            return res
                        finally semaphore.Release() |> ignore
                    })
                    |> Async.Parallel
                let errors = results |> Array.choose (function Error e -> Some e | _ -> None)
                if errors.Length > 0 then return Error (String.Join("; ", errors))
                else return Ok ()
            }