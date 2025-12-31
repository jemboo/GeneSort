namespace GeneSort.Project

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Runs
open GeneSort.Db
open GeneSort.SortingResults
open ProjectOps

module TextReporters =

    /// Helper to append a failure section to the data table
    let private appendFailureSummary (failures: (string * string) list) (dt: dataTableFile) =
        if failures.IsEmpty then dt
        else
            dt 
            |> DataTableFile.addSource "--- ERRORS ENCOUNTERED DURING GENERATION ---"
            |> DataTableFile.addSources (failures |> List.map (fun (id, msg) -> sprintf "Run ID %s: %s" id msg) |> List.toArray)


    let binReportExecutor
        (db: IGeneSortDb)
        (projectName: string<projectName>)
        (yab: runParameters -> outputDataType -> queryParams)
        (allowOverwrite: bool<allowOverwrite>)
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) : Async<Result<unit, string>> =

        asyncResult {
            let reportName = "SorterEval_Bin_Report"
            let timestamp () = MathUtils.getTimestampString()
        
            report progress (sprintf "%s Starting %s for: %s" (timestamp()) reportName %projectName)

            // 1. Get Parameters (Handles Error track automatically)
            let! runParamsArray = 
                db.getAllProjectRunParametersAsync projectName (Some cts.Token) progress

            // 2. Initialize DataTable
            let initialTable = 
                dataTableFile.createFromList reportName 
                    [| "Sorting Width"; "SorterModel"; "ceLength"; "stageLength"; "binCount"; "unsortedReport" |]
                |> DataTableFile.addSource (sprintf "Generated: %s" (timestamp()))

            let mutable dataTable = initialTable
            let mutable failures = []

            // 3. Process each run parameter
            for runParams in runParamsArray do
                let runId = runParams.GetId() |> Option.map (fun x -> %x) |> Option.defaultValue "Unknown"
                let qp = yab runParams (outputDataType.SorterSetEval "") 
            
                // Map the naked Async result to the builder's track
                let! result = GeneSortDb.getSorterSetEvalAsync db qp |> Async.map Ok

                match result with
                | Ok sse ->
                    let bins = SorterSetEvalBins.create 1 sse
                    let lines = SorterSetEvalBins.getBinCountReport (runParams.GetSortingWidth()) (runParams.GetSorterModelType() |> SorterModelType.toString) bins
                    dataTable <- DataTableFile.addRows lines dataTable
                | Error err ->
                    failures <- (runId, err) :: failures
                    report progress (sprintf "%s Warning: Failed run %s: %s" (timestamp()) runId err)

            // 4. Finalize and Save
            let finalDataTable = appendFailureSummary (List.rev failures) dataTable
            let saveQp = queryParams.createForTextReport projectName (reportName |> UMX.tag)
        
            // Ensure the save operation result is handled
            let! _ = db.saveAsync saveQp (outputData.TextReport finalDataTable) allowOverwrite |> Async.map Ok

            report progress (sprintf "%s Finished %s for: %s" (timestamp()) reportName %projectName)
            return ()
        }



    //let binReportExecutor
    //        (db: IGeneSortDb)
    //        (projectName: string<projectName>)
    //        (yab: runParameters -> outputDataType -> queryParams)
    //        (allowOverwrite: bool<allowOverwrite>)
    //        (cts: CancellationTokenSource) 
    //        (progress: IProgress<string> option) : Async<unit> =

    //    async {
    //        let reportName = "SorterEval_Bin_Report"
    //        report progress (sprintf "%s Starting %s for: %s" (MathUtils.getTimestampString()) reportName %projectName)

    //        let! runParamsResult = db.getAllProjectRunParametersAsync projectName (Some cts.Token) progress
    //        match runParamsResult with
    //        | Error msg -> failwithf "%s Critical Error: Could not retrieve project parameters:  %s" (MathUtils.getTimestampString()) msg
    //        | Ok runParamsArray ->

    //            let mutable dataTable = 
    //                dataTableFile.createFromList reportName 
    //                    [| "Sorting Width"; "SorterModel"; "ceLength"; "stageLength"; "binCount"; "unsortedReport" |]
    //                    |> DataTableFile.addSource (sprintf "Generated: %s" (MathUtils.getTimestampString()))

    //            let mutable failures = []

    //            for runParams in runParamsArray do
    //                let runId = runParams.GetId() |> Option.map (fun x -> %x) |> Option.defaultValue "Unknown"
    //                let qp = yab runParams (outputDataType.SorterSetEval "") 
                    
    //                let! result = GeneSortDb.getSorterSetEvalAsync db qp
    //                match result with
    //                | Ok sse ->
    //                    let bins = SorterSetEvalBins.create 1 sse
    //                    let lines = SorterSetEvalBins.getBinCountReport (runParams.GetSortingWidth()) (runParams.GetSorterModelType() |> SorterModelType.toString) bins
    //                    dataTable <- DataTableFile.addRows lines dataTable
    //                | Error err ->
    //                    failures <- (runId, err) :: failures
    //                    progress |> Option.iter (fun p -> p.Report(sprintf "%s Warning: Failed run %s: %s" (MathUtils.getTimestampString()) runId err))

    //            // Finalize report with failure summary
    //            let finalDataTable = appendFailureSummary (List.rev failures) dataTable
    //            let saveQp = queryParams.createForTextReport projectName (reportName |> UMX.tag)
    //            do! db.saveAsync saveQp (outputData.TextReport finalDataTable) allowOverwrite |> Async.Ignore

    //            report progress (sprintf "%s Finished %s for: %s" (MathUtils.getTimestampString()) reportName %projectName)
    //    }


    let ceUseProfileReportExecutor
        (db: IGeneSortDb)
        (projectName: string<projectName>) 
        (yab: runParameters -> outputDataType -> queryParams)
        (allowOverwrite: bool<allowOverwrite>)
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) : Async<Result<unit, string>> =

        asyncResult {
            let reportName = "SorterCeUseProfile_Report"
            let binCount, blockGrowthRate = 20, 1.2
            let timestamp () = MathUtils.getTimestampString()
        
            report progress (sprintf "%s Starting %s for: %s" (timestamp()) reportName %projectName)

            // 1. Get Parameters (automatically handles Error exit)
            let! runParamsArray = 
                db.getAllProjectRunParametersAsync projectName (Some cts.Token) progress

            // 2. Setup Initial DataTable
            let initialTable = 
                dataTableFile.createFromList reportName 
                    (Array.append [| "Id"; "Repl"; "SortingWidth"; "SorterModel"; "DataType"; "MergeFillType"; "MergeDimension"; "sorterId"; "sorterSetId"; "sorterTestsId"; "UnsortedCount"; "CeCount"; "StageCount"; "lastCe" |]
                                  (Array.init 20 (fun i -> i.ToString())))
                |> DataTableFile.addSource (sprintf "Generated: %s" (timestamp()))

            // 3. Process the loop using our new 'For' support
            // We use a fold-like pattern inside the CE to handle the mutable dataTable and failures list
            let mutable dataTable = initialTable
            let mutable failures = []

            for runParams in runParamsArray do
                let runId = runParams.GetId() |> Option.map (fun x -> %x) |> Option.defaultValue (sprintf "Unknown_%s" (Guid.NewGuid().ToString()))
                let qp = yab runParams (outputDataType.SorterSetEval "") 
            
                // Try to get the eval data
                let! evalResult = GeneSortDb.getSorterSetEvalAsync db qp |> Async.map Ok
            
                match evalResult with
                | Ok sse ->
                    let profile = SorterSetCeUseProfile.makeSorterSetCeUseProfile binCount blockGrowthRate sse
                    let lines = 
                        SorterSetCeUseProfile.makeReportLines 
                            runId 
                            (runParams.GetRepl() |> Repl.toString) 
                            (runParams.GetSortingWidth() |> SortingWidth.toString)
                            (runParams.GetSorterModelType() |> SorterModelType.toString)
                            (runParams.GetSortableDataType() |> SortableDataType.toString)
                            (runParams.GetMergeFillType() |> MergeFillType.toString)
                            (runParams.GetMergeDimension() |> MergeDimension.toString)
                            profile
                    dataTable <- DataTableFile.addRows lines dataTable
                | Error err ->
                    failures <- (runId, err) :: failures
   

            // 4. Finalize and Save
            let finalDataTable = appendFailureSummary (List.rev failures) dataTable
            let saveQp = queryParams.createForTextReport projectName (reportName |> UMX.tag)
        
            let! _ = db.saveAsync saveQp (outputData.TextReport finalDataTable) allowOverwrite 

            report progress (sprintf "%s Finished %s for: %s" (timestamp()) reportName %projectName)
            return ()
        }
