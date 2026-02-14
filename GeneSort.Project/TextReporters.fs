namespace GeneSort.Project

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Runs
open GeneSort.Db
open GeneSort.SortingResults
open ProjectOps

module TextReporters =

    let private makeErrorTable (failures: (runParameters * string) list) : string [] =
        let mutable modRunParameters = []

        for (rp, err) in failures do
            modRunParameters <- (rp.WithMessage(err)) :: modRunParameters

        let mutable tableRows =     
                            (RunParameters.makeIndexAndReplTable modRunParameters
                            |> Array.map (fun row -> String.Join("\t", row)))

        tableRows <- [|"--- ********* ---"|]
                     |> Array.append  tableRows
                     |> Array.append [|"--- Error Runs ---"|] 

        tableRows

    
    let private makeSourceTable (runParams : runParameters []) : string [] =
        let mutable tableRows =     
                            (RunParameters.makeIndexAndReplTable runParams
                            |> Array.map (fun row -> String.Join("\t", row)))


        tableRows <- [|"--- ********* ---"|]
                     |> Array.append  tableRows
                     |> Array.append [|"--- Successful Runs ---"|] 

        tableRows


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

            // 2. Setup Initial DataTable
            let headers =  [| "Sorting Width"; "SorterModel"; "ceLength"; "stageLength"; "binCount"; "unsortedReport" |]
            let dtReport = DataTableReport.create reportName headers
            let mutable newFailures = []


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
                    dtReport.AppendDataRows lines
                | Error err ->
                    newFailures <- (runParams, err) :: newFailures


            // 4. Finalize and Save
            dtReport.AddSources (makeSourceTable runParamsArray)
            dtReport.AddErrors (makeErrorTable newFailures)

            let saveQp = buildQueryParams (runParamsArray.[0].WithRepl None) (outputDataType.TextReport (reportName |> UMX.tag))
            let! _ = db.saveAsync projectFolder saveQp (outputData.TextReport dtReport) allowOverwrite |> Async.map Ok

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
            let headers = Array.append
                              [| 
                                "Id"; "Repl"; "SortingWidth"; "SorterModel"; "DataType"; "MergeFillType"; 
                                "MergeDimension"; "sorterId"; "sorterSetId"; "sorterTestsId"; "UnsortedCount"; 
                                "Unsorted"; "CeCount"; "StageCount"; "lastCe" 
                              |]
                              (Array.init 20 (fun i -> i.ToString()))

            let dtReport = DataTableReport.create reportName headers
            let mutable newFailures = []

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
                            (runParams.GetSortableDataFormat() |> Option.map SortableDataFormat.toString |> UmxExt.stringToString)
                            (runParams.GetMergeSuffixType() |> Option.map MergeFillType.toString |> UmxExt.stringToString)
                            (runParams.GetMergeDimension() |> UmxExt.intToString)
                            profile
                    dtReport.AppendDataRows lines
                | Error err ->
                    newFailures <- (runParams, err) :: newFailures
   

            // 4. Finalize and Save
            dtReport.AddSources (makeSourceTable runParamsArray)
            dtReport.AddErrors (makeErrorTable newFailures)

            let saveQp = buildQueryParams (runParamsArray.[0].WithRepl None) (outputDataType.TextReport (reportName |> UMX.tag))
            let! _ = db.saveAsync projectFolder saveQp (outputData.TextReport dtReport) allowOverwrite |> Async.map Ok

            report progress (sprintf "%s Finished %s for: %s" (timestamp()) reportName %projectFolder)
            return ()
        }