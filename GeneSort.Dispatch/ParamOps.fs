namespace GeneSort.Dispatch.V1
open System
open System.Threading
open FSharp.UMX
open GeneSort.Db
open GeneSort.Core
open OpsUtils
open GeneSort.Db.V1
open GeneSort.Project.V1

module ParamOps =

    let saveParametersFiles
            (db: IGeneSortDb)
            (runParameterArray: runParameters[])
            (buildQueryParams: runParameters -> outputDataType -> queryParams)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<unit, string>> =
        async {
            try
                report progress (sprintf "%s Saving RunParameter files in %s" (MathUtils.getTimestampString()) %db.projectName)
                cts.Token.ThrowIfCancellationRequested()
                let! res = db.saveRunParameters runParameterArray buildQueryParams allowOverwrite (Some cts.Token) progress
                match res with
                | Ok () -> 
                    report progress (sprintf "%s Successfully saved %d RunParameter files" 
                                        (MathUtils.getTimestampString()) runParameterArray.Length)
                    return Ok ()
                | Error msg -> return Error msg
            with
            | :? OperationCanceledException ->
                let msg = "Saving RunParameter files was cancelled"
                report progress msg
                return Error msg
            | e ->
                let msg = sprintf "%s Failed to save RunParameter files in %s: %s" 
                                        (MathUtils.getTimestampString()) 
                                        %db.projectName
                                        e.Message
                report progress msg
                return Error msg
        }


    let initProjectAndRunFiles
        (db: IGeneSortDb)
        (buildQueryParams: runParameters -> outputDataType -> queryParams)
        (cts: CancellationTokenSource)
        (progress: IProgress<string> option)
        (run: run)
        (minReplica: int<replNumber>)
        (maxReplica: int<replNumber>)
        (allowOverwrite: bool<allowOverwrite>)
        (paramRefiner: runParameters seq -> runParameters seq) 
        (parameterSpans: (string * string list) list) : Async<Result<unit, string>> =
        async {
            try
                report progress (sprintf "%s Saving project file: %s" (MathUtils.getTimestampString()) %run.ProjectName)
                let queryParams = queryParams.createForRun run.ProjectName run.RunName
                let! saveProjRes = db.saveAsync queryParams (run |> outputData.Run) (true |> UMX.tag<allowOverwrite>)
                match saveProjRes with
                | Error err -> return Error err
                | Ok () ->
                    let runParametersArray = Project.makeRunParameters minReplica maxReplica parameterSpans paramRefiner |> Seq.toArray
                    report progress (sprintf "%s Saving run parameters files: (%d)" (MathUtils.getTimestampString()) runParametersArray.Length)
                    return! saveParametersFiles 
                                db
                                runParametersArray buildQueryParams 
                                allowOverwrite cts progress
            with e ->
                let errorMsg = sprintf "Failed to initialize project files: %s\n" e.Message
                report progress errorMsg
                return Error errorMsg
        }