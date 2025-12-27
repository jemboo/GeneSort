namespace GeneSort.Project

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Runs
open GeneSort.Db
open GeneSort.SortingResults

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
            (progress: IProgress<string> option) : Async<unit> =

        async {
            let reportName = "SorterEval_Bin_Report"
            progress |> Option.iter (fun p -> p.Report(sprintf "Starting %s for: %s" reportName %projectName))

            let! runParamsResult = db.getAllProjectRunParametersAsync projectName (Some cts.Token) progress
            match runParamsResult with
            | Error msg -> failwithf "Critical Error: Could not retrieve project parameters: %s" msg
            | Ok runParamsArray ->

                let mutable dataTable = 
                    dataTableFile.createFromList reportName 
                        [| "Sorting Width"; "SorterModel"; "ceLength"; "stageLength"; "binCount"; "unsortedReport" |]
                        |> DataTableFile.addSource (sprintf "Generated: %s" (DateTime.Now.ToString("G")))

                let mutable failures = []

                for runParams in runParamsArray do
                    let runId = runParams.GetId() |> Option.map (fun x -> %x) |> Option.defaultValue "Unknown"
                    let qp = yab runParams (outputDataType.SorterSetEval "") 
                    
                    let! result = GeneSortDb.getSorterSetEvalAsync db qp
                    match result with
                    | Ok sse ->
                        let bins = SorterSetEvalBins.create 1 sse
                        let lines = SorterSetEvalBins.getBinCountReport (runParams.GetSortingWidth()) (runParams.GetSorterModelType() |> SorterModelType.toString) bins
                        dataTable <- DataTableFile.addRows lines dataTable
                    | Error err ->
                        failures <- (runId, err) :: failures
                        progress |> Option.iter (fun p -> p.Report(sprintf "Warning: Failed run %s: %s" runId err))

                // Finalize report with failure summary
                let finalDataTable = appendFailureSummary (List.rev failures) dataTable
                let saveQp = queryParams.createForTextReport projectName (reportName |> UMX.tag)
                do! db.saveAsync saveQp (outputData.TextReport finalDataTable) allowOverwrite |> Async.Ignore
        }

    let ceUseProfileReportExecutor
            (db: IGeneSortDb)
            (projectName: string<projectName>) 
            (yab: runParameters -> outputDataType -> queryParams)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<unit> =

        async {
            let reportName = "SorterCeUseProfile_Report"
            let binCount, blockGrowthRate = 20, 1.2
            progress |> Option.iter (fun p -> p.Report(sprintf "Starting %s for: %s" reportName %projectName))

            let! runParamsResult = db.getAllProjectRunParametersAsync projectName (Some cts.Token) progress
            match runParamsResult with
            | Error msg -> failwithf "Critical Error: Could not retrieve project parameters: %s" msg
            | Ok runParamsArray ->

                let mutable dataTable = 
                    dataTableFile.createFromList reportName 
                        (Array.append [| "Id"; "Repl"; "SortingWidth"; "SorterModel"; "DataType"; "MergeFillType"; "MergeDimension"; "sorterId"; "sorterSetId"; "sorterTestsId"; "UnsortedCount"; "CeCount"; "StageCount"; "lastCe" |]
                                      (Array.init 20 (fun i -> i.ToString())))
                        |> DataTableFile.addSource (sprintf "Generated: %s" (DateTime.Now.ToString("G")))

                let mutable failures = []

                for runParams in runParamsArray do
                    let runId = runParams.GetId() |> Option.map (fun x -> %x) |> Option.defaultValue "Unknown"
                    let qp = yab runParams (outputDataType.SorterSetEval "") 
                    
                    let! result = GeneSortDb.getSorterSetEvalAsync db qp
                    match result with
                    | Ok sse ->
                        let profile = SorterSetCeUseProfile.makeSorterSetCeUseProfile binCount blockGrowthRate sse
                        let lines = SorterSetCeUseProfile.makeReportLines 
                                        runId (runParams.GetRepl() |> Repl.toString) 
                                        (runParams.GetSortingWidth() |> SortingWidth.toString)
                                        (runParams.GetSorterModelType() |> SorterModelType.toString)
                                        (runParams.GetSortableDataType() |> SortableDataType.toString)
                                        (runParams.GetMergeFillType() |> MergeFillType.toString)
                                        (runParams.GetMergeDimension() |> MergeDimension.toString)
                                        profile
                        dataTable <- DataTableFile.addRows lines dataTable
                    | Error err ->
                        failures <- (runId, err) :: failures

                let finalDataTable = appendFailureSummary (List.rev failures) dataTable
                let saveQp = queryParams.createForTextReport projectName (reportName |> UMX.tag)
                do! db.saveAsync saveQp (outputData.TextReport finalDataTable) allowOverwrite |> Async.Ignore
        }