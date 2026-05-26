namespace GeneSort.Dispatch.V1
open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open OpsUtils
open GeneSort.Db.V1
open GeneSort.Project.V1

module ParamOps =

    let saveParametersFiles
            (db: IGeneSortDb)
            (runParameterArray: runParameters[])
            (allowOverwrite: bool<allowOverwrite>) 
            (progress: IProgress<string> option): Async<Result<unit, string>> =
        async {
            try
                report progress (sprintf "%s Saving RunParameter files in %s" (MathUtils.getTimestampString()) %db.databaseName)

                let saveResults = 
                    runParameterArray
                    |> Array.map (fun rp -> 
                        let runName = rp.GetRunName() |> Option.defaultValue ("UnknownRun" |> UMX.tag<runName>)
                        let queryParamsOpt = db.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters runName)
                        match queryParamsOpt with
                        | Some qp -> db.saveAsync qp (outputData.RunParameters rp) allowOverwrite
                        | None -> async { return Error (sprintf "Failed to create query parameters for run parameters: %A" rp) })
                    |> Async.Parallel

                return! saveResults
                    |> Async.map (fun results ->
                        results
                        |> Array.tryFind (function Error _ -> true | Ok _ -> false)
                        |> function
                            | Some (Error err) -> Error err
                            | _ -> Ok ())

            with
            | e ->
                let msg = sprintf "%s Failed to save RunParameter files in %s: %s" 
                                        (MathUtils.getTimestampString()) 
                                        %db.databaseName
                                        e.Message
                report progress msg
                return Error msg
        }


    let initProjectAndRunFiles
        (db: IGeneSortDb)
        (progress: IProgress<string> option)
        (run: run)
        (minReplica: int<replNumber>)
        (maxReplica: int<replNumber>)
        (allowOverwrite: bool<allowOverwrite>)
        (paramRefiner: runParameters seq -> runParameters seq) 
        (parameterSpans: (string * string list) list) : Async<Result<unit, string>> =
        async {
            try
                report progress (sprintf "%s Saving run file: %s" (MathUtils.getTimestampString()) %run.DatabaseName)
                let queryParams = queryParams.createForRun run.DatabaseName run.RunName
                let! saveProjRes = db.saveAsync queryParams (run |> outputData.Run) (true |> UMX.tag<allowOverwrite>)
                match saveProjRes with
                | Error err -> return Error err
                | Ok () ->
                    let runParametersArray = Run.makeRunParameters minReplica maxReplica parameterSpans paramRefiner |> Seq.toArray
                    report progress (sprintf "%s Saving run parameters files: (%d)" (MathUtils.getTimestampString()) runParametersArray.Length)
                    return! saveParametersFiles 
                                db
                                runParametersArray 
                                allowOverwrite progress
            with e ->
                let errorMsg = sprintf "Failed to initialize project files: %s\n" e.Message
                report progress errorMsg
                return Error errorMsg
        }