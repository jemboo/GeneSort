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
            |> DataTableFile.addSource "--- ************************ ---"

    
    /// Helper to append a failure section to the data table
    let private appendSourceRunParamsTable (runParams : runParameters list) (dt: dataTableFile) =
        if runParams.IsEmpty then dt
        else
            let tableRows = 
                            [|"--- Source Runs ---"|]
                            |> Array.append
                                (RunParameters.makeIndexAndReplTable runParams
                                |> Array.map (fun row -> String.Join("\t", row)))
                                |> Array.rev
                            |> Array.append [|"--- ********* ---"|]
            dt 
            |> DataTableFile.addSources tableRows



    let binReportExecutor
        (db: IGeneSortDb)
        (projectFolder: string<projectFolder>)
        (minReplNumber: int<replNumber>)
        (maxReplNumber: int<replNumber>)
        (buildQueryParams: runParameters -> outputDataType -> queryParams)
        (allowOverwrite: bool<allowOverwrite>)
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) : Async<Result<unit, string>> =

        asyncResult {
            let reportName = "SorterEval_Bin_Report"
            let timestamp () = MathUtils.getTimestampString()
        
            report progress (sprintf "%s Starting %s for: %s" (timestamp()) reportName %projectFolder)

            // 1. Get Parameters (Handles Error track automatically)
            let! runParamsArray = 
                db.getProjectRunParametersForReplRangeAsync  projectFolder (Some minReplNumber) (Some maxReplNumber) (Some cts.Token) progress

            // 2. Initialize DataTable
            let initialTable = 
                dataTableFile.createFromList reportName 
                    [| "Sorting Width"; "SorterModel"; "ceLength"; "stageLength"; "binCount"; "unsortedReport" |]
                |> DataTableFile.addSource (sprintf "Generated: %s" (timestamp()))

            let mutable dataTable = initialTable
            let mutable failures = []

            // 3. Process each run parameter
            for runParams in runParamsArray do
                let runId = runParams |> RunParameters.getIdString
                let qp = buildQueryParams runParams (outputDataType.SorterSetEval "") 
            
                // Map the naked Async result to the builder's track
                let! result = GeneSortDb.getSorterSetEvalAsync db projectFolder qp |> Async.map Ok

                match result with
                | Ok sse ->
                    let bins = SorterSetEvalBins.create 1 sse
                    let lines = SorterSetEvalBins.getBinCountReport
                                    (runParams.GetSortingWidth()) 
                                    (runParams.GetSorterModelType() |> Option.map SorterModelType.toString |> UmxExt.stringToString )
                                    bins
                    dataTable <- DataTableFile.addRows lines dataTable
                | Error err ->
                    failures <- (runId, err) :: failures
                    report progress (sprintf "%s Warning: Failed run %s: %s" (timestamp()) runId err)

            // 4. Finalize and Save
            let semiFinalDataTable = appendSourceRunParamsTable (List.ofArray runParamsArray) dataTable
            let finalDataTable = appendFailureSummary (List.rev failures)  semiFinalDataTable
            let saveQp = buildQueryParams (runParamsArray.[0].WithRepl None) (outputDataType.TextReport (reportName |> UMX.tag))
            let! _ = db.saveAsync projectFolder saveQp (outputData.TextReport finalDataTable) allowOverwrite |> Async.map Ok

            report progress (sprintf "%s Finished %s for: %s" (timestamp()) reportName %projectFolder)
            return ()
        }


    let ceUseProfileReportExecutor
        (db: IGeneSortDb)
        (projectFolder: string<projectFolder>) 
        (minReplNumber: int<replNumber>)
        (maxReplNumber: int<replNumber>)
        (buildQueryParams: runParameters -> outputDataType -> queryParams)
        (allowOverwrite: bool<allowOverwrite>)
        (cts: CancellationTokenSource) 
        (progress: IProgress<string> option) : Async<Result<unit, string>> =

        asyncResult {
            let reportName = "SorterCeUseProfile_Report"
            let binCount, blockGrowthRate = 20, 1.2
            let timestamp () = MathUtils.getTimestampString()
        
            report progress (sprintf "%s Starting %s for: %s" (timestamp()) reportName %projectFolder)

            // 1. Get Parameters (automatically handles Error exit)
            let! runParamsArray = 
                db.getProjectRunParametersForReplRangeAsync projectFolder (Some minReplNumber) (Some maxReplNumber) (Some cts.Token) progress

            // 2. Setup Initial DataTable
            let initialTable = 
                dataTableFile.createFromList reportName 
                    (Array.append [| 
                            "Id"; "Repl"; "SortingWidth"; "SorterModel"; "DataType"; "MergeFillType"; 
                            "MergeDimension"; "sorterId"; "sorterSetId"; "sorterTestsId"; "UnsortedCount"; 
                            "CeCount"; "StageCount"; "lastCe" 
                            |]
                                  (Array.init 20 (fun i -> i.ToString())))
                |> DataTableFile.addSource (sprintf "Generated: %s" (timestamp()))

            // 3. Process the loop using our new 'For' support
            // We use a fold-like pattern inside the CE to handle the mutable dataTable and failures list
            let mutable dataTable = initialTable
            let mutable failures = []

            for runParams in runParamsArray do
                let runId = runParams |> RunParameters.getIdString
                let qp = buildQueryParams runParams (outputDataType.SorterSetEval "") 
            
                // Try to get the eval data
                let! evalResult = GeneSortDb.getSorterSetEvalAsync db projectFolder qp |> Async.map Ok
            
                match evalResult with
                | Ok sse ->
                    let profile = SorterSetCeUseProfile.makeSorterSetCeUseProfile binCount blockGrowthRate sse
                    let lines = 
                        SorterSetCeUseProfile.makeReportLines 
                            runId 
                            (runParams.GetRepl() |> UmxExt.intToString) 
                            (runParams.GetSortingWidth() |> UmxExt.intToString)
                            (runParams.GetSorterModelType() |> Option.map SorterModelType.toString |> UmxExt.stringToString)
                            (runParams.GetSortableDataType() |> Option.map SortableDataType.toString |> UmxExt.stringToString)
                            (runParams.GetMergeFillType() |> Option.map MergeFillType.toString |> UmxExt.stringToString)
                            (runParams.GetMergeDimension() |> UmxExt.intToString)
                            profile
                    dataTable <- DataTableFile.addRows lines dataTable
                | Error err ->
                    failures <- (runId, err) :: failures
   

            // 4. Finalize and Save
            let semiFinalDataTable = appendSourceRunParamsTable (List.ofArray runParamsArray) dataTable
            let finalDataTable = appendFailureSummary (List.rev failures)  semiFinalDataTable
            let saveQp = buildQueryParams (runParamsArray.[0].WithRepl None) (outputDataType.TextReport (reportName |> UMX.tag))
            let! _ = db.saveAsync projectFolder saveQp (outputData.TextReport finalDataTable) allowOverwrite |> Async.map Ok

            report progress (sprintf "%s Finished %s for: %s" (timestamp()) reportName %projectFolder)
            return ()
        }
