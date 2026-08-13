
namespace GeneSort.FileDb.V1

open System
open System.IO
open System.Threading
open FSharp.UMX
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Core

type private DbMessage =
    | Save of string<pathToRootFolder> * queryParams * outputData * bool<allowOverwrite> * AsyncReplyChannel<Result<unit, string>>
    | Load of string<pathToRootFolder> * queryParams * AsyncReplyChannel<Result<outputData, string>>
    | GetRunParameters of string<runName> * (int<replNumber> option) * (int<replNumber> option) * CancellationToken option * IProgress<string> option * AsyncReplyChannel<Result<runParameters[], string>>

type GeneSortGenDbMp(
                rootFolder: string<pathToRootFolder>,
                queryParamsMaker: runParameters -> outputDataType -> queryParams option,
                genSaveIntervals: samplingConfig,
                genSaveSubIntervals: samplingConfig) =

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
    member _.GenSaveIntervals = genSaveIntervals
    member _.GenSaveSubIntervals = genSaveSubIntervals

    interface IGeneSortDb with
        member _.databaseName
            with get (): string<databaseName> = DirectoryInfo(%rootFolder).Name |> UMX.tag

        member _.MakeQueryParamsFromRunParams rp odt =
            queryParamsMaker rp odt

        member _.saveAsync (queryParams: queryParams) (data: outputData) (allowOverwrite: bool<allowOverwrite>) =
            mailbox.PostAndAsyncReply(fun channel -> Save(rootFolder, queryParams, data, allowOverwrite, channel))

        member _.loadAsync (queryParams: queryParams) =
            mailbox.PostAndAsyncReply(fun channel -> Load(rootFolder, queryParams, channel))

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

    interface IGeneSortGenDb with
        member _.getGenSaveIntervals () = genSaveIntervals
        member _.getGenSaveSubIntervals () = genSaveSubIntervals

        member this.getNextGenItemAsync 
                    (baseQueryParams: queryParams) 
                    (generationMutator: queryParams -> int<generationNumber> -> queryParams) : Async<outputData option> =
            async {
                // Generate up to 200 linear checkpoints
                let targetCount = defaultArg genSaveIntervals.MaxCount 200
                let intervals: int<generationNumber>[] = 
                    IntSampleMethod.generate genSaveIntervals.Method genSaveIntervals.Min targetCount
                    |> Seq.map (fun gen -> int (ceil (float gen * genSaveIntervals.Scale)))
                    |> Seq.toArray
                    |> Array.map (fun gen -> %gen : int<generationNumber>)

                if intervals.Length = 0 then
                    return None
                else
                    let db = this :> IGeneSortDb

                    // Binary search for the highest existing checkpoint index
                    let rec binarySearch low high bestMatch =
                        async {
                            if low > high then
                                return bestMatch
                            else
                                let mid = low + (high - low) / 2
                                let currentGen = intervals.[mid]
                                let targetQueryParams = generationMutator baseQueryParams currentGen
                            
                                let! itemOpt = db.loadIfFoundAsync targetQueryParams
                            
                                match itemOpt with
                                | Some data -> 
                                    // Found at mid! Search right half for potentially higher saved generations
                                    return! binarySearch (mid + 1) high (Some data)
                                | None -> 
                                    // Missing at mid; highest saved must be in the left half
                                    return! binarySearch low (mid - 1) bestMatch
                        }

                    return! binarySearch 0 (intervals.Length - 1) None
            }