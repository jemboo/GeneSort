namespace GeneSort.Dispatch.V1

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1

module OpsUtils =

    let inline report (progress: IProgress<string> option) msg =
        progress |> Option.iter (fun p -> p.Report msg)


    let makeErrorTable (failures: (runParameters * string) list) : string [] =
        let mutable modRunParameters = []

        for (rp, err) in failures do
            modRunParameters <- (rp.WithMessage(Some err)) :: modRunParameters

        let mutable tableRows =     
                            (RunParameters.makeIndexAndReplTable modRunParameters
                            |> Array.map (fun row -> String.Join("\t", row)))

        tableRows <- [|"\n--- ********* ---"|]
                     |> Array.append  tableRows
                     |> Array.append [|"--- Error Runs ---"|] 

        tableRows

    
    let makeSourceTable (runParams : runParameters []) : string [] =
        let mutable tableRows =     
                            (RunParameters.makeIndexAndReplTable runParams
                            |> Array.map (fun row -> String.Join("\t", row)))


        tableRows <- [|"\n--- ********* ---"|]
                     |> Array.append  tableRows
                     |> Array.append [|"--- Successful Runs ---"|] 

        tableRows


    let printRunParamsTable
            (db: IGeneSortDb)
            (runName: string<runName>)
            (minReplNumber: int<replNumber>)
            (maxReplNumber: int<replNumber>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) =
        asyncResult {
            try
                report progress (sprintf "Displaying Source Table for %s\n" %db.projectName)

                let! (runParametersArray: runParameters array) = 
                    db.getRunParameters runName (Some minReplNumber) (Some maxReplNumber) (Some cts.Token) progress

                if runParametersArray.Length = 0 then
                    report progress "No runs found to display.\n"
                else
                    let sourceTableRows = makeSourceTable runParametersArray
                    sourceTableRows |> Array.iter (report progress)
                    report progress (sprintf "\nFound %d run configurations.\n" runParametersArray.Length)
                return () 
            with e ->
                let msg = sprintf "Error displaying source table: %s" e.Message
                report progress msg
                return! async { return Error msg }
        }