namespace GeneSort.Project

open System

open FSharp.UMX
open System.Threading

open GeneSort.Core
open GeneSort.Runs.Params
open GeneSort.Db
open GeneSort.SortingResults


module Reporters =

    let binReportExecutor
            (db: IGeneSortDb)
            (projectName: string<projectName>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<unit> =

        async {
            match progress with
            | Some p -> p.Report(sprintf "Making performace bin report for project: %s" %projectName)
            | None -> ()

            let! runParamsResult = db.getAllProjectRunParametersAsync projectName (Some cts.Token) progress
            let runParamsArray =
                match runParamsResult with
                | Ok rpArray -> rpArray
                | Error msg -> failwithf "Error retrieving RunParameters for project %s: %s" %projectName msg

            let mutable dataTableFile = 
                        DataTableFile.create "SorterEval_Bin_Report" 
                                [| "Sorting Width"; "SorterModel"; "ceLength"; "stageLength"; "binCount"; "unsortedReport" |]
                                |> DataTableFile.addSource (sprintf "Generated: %s" (DateTime.Now.ToLongTimeString()))
                                |> DataTableFile.addSource (sprintf "Sources (%d):" runParamsArray.Length)

            let csvRows = (RunParameters.makeIndexAndReplTable runParamsArray) |> RunParameters.tableToTabDelimited
            dataTableFile <- DataTableFile.addSources csvRows dataTableFile

            do! runParamsArray
                |> Array.map (fun runParams -> async {
                    let queryParamsForSorterSetEval = queryParams.createFromRunParams outputDataType.SorterSetEval None runParams
                    let sortingWidth = runParams.GetSortingWidth()
                    let sorterModelKey =  runParams.GetSorterModelKey() |> SorterModelKey.toString
        
                    let! sorterSetEvalResult = db.loadAsync queryParamsForSorterSetEval outputDataType.SorterSetEval
                    let sorterSetEval = 
                        match sorterSetEvalResult with
                        | Ok (SorterSetEval sse) -> sse
                        | Ok _ -> failwith (sprintf "Unexpected output data type for SorterSetEval")
                        | Error err -> failwith (sprintf "Error loading SorterSetEval: %s" err)
        
                    let sorterSetEvalBins = SorterSetEvalBins.create 1 sorterSetEval
                    let reportLines = SorterSetEvalBins.getBinCountReport sortingWidth sorterModelKey sorterSetEvalBins
                    dataTableFile <- DataTableFile.addRows reportLines dataTableFile
                    ()
                })
                |> Async.Sequential
                |> Async.Ignore  // Convert Async<unit[]> to Async<unit>


            let queryParams = queryParams.createForTextReport projectName ("SorterEval_Bin_Report" |> UMX.tag<textReportName> )
            let outputData =  dataTableFile |> outputData.TextReport
            do! db.saveAsync queryParams outputData
            return ()
        }


    let ceUseProfileReportExecutor
            (db: IGeneSortDb)
            (projectName: string<projectName>) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<unit> =

        async {
            match progress with
            | Some p -> p.Report(sprintf "Making CE use profile report for project: %s" %projectName)
            | None -> ()

            let binCount = 20
            let blockGrowthRate = 1.2

            let! runParamsResult = db.getAllProjectRunParametersAsync projectName (Some cts.Token) progress
            let runParamsArray =
                match runParamsResult with
                | Ok rpArray -> rpArray
                | Error msg -> failwithf "Error retrieving RunParameters for project %s: %s" %projectName msg

            let mutable dataTableFile = 
                        DataTableFile.create "SorterCeUseProfile_Report" 
                              (  (Array.init 20 (fun i -> i.ToString()))
                                    |> Array.append 
                                  [| "Sorting Width"; "SorterModel"; "sorterId"; "sorterSetId"; "sorterTestsId"; "lastCe" |]
                              )
                              |> DataTableFile.addSource (sprintf "Generated: %s" (DateTime.Now.ToLongTimeString()))
                              |> DataTableFile.addSource (sprintf "Sources (%d):" runParamsArray.Length)

            let csvRows = (RunParameters.makeIndexAndReplTable runParamsArray) |> RunParameters.tableToTabDelimited
            dataTableFile <- DataTableFile.addSources csvRows dataTableFile

            do! runParamsArray
                |> Array.map (fun runParams -> async {
                    let queryParamsForSorterSetEval = queryParams.createFromRunParams outputDataType.SorterSetEval None runParams
                    let sortingWidth = runParams.GetSortingWidth()
                    let sorterModelKey = runParams.GetSorterModelKey() |> SorterModelKey.toString

                    match progress with
                    | Some p -> p.Report(sprintf "Processing SorterSetEval for %s %s" (%sortingWidth.ToString()) sorterModelKey)
                    | None -> ()

                    let! sorterSetEvalResult = db.loadAsync queryParamsForSorterSetEval outputDataType.SorterSetEval
                    let sorterSetEval = 
                        match sorterSetEvalResult with
                        | Ok (SorterSetEval sse) -> sse
                        | Ok _ -> failwith (sprintf "Unexpected output data type for SorterSetEval")
                        | Error err -> failwith (sprintf "Error loading SorterSetEval: %s" err)

                    let sorterSetCeUseProfile = SorterSetCeUseProfile.makeSorterSetCeUseProfile binCount blockGrowthRate sorterSetEval
                    let reportLines = SorterSetCeUseProfile.makeReportLines sortingWidth sorterModelKey sorterSetCeUseProfile
                
                    dataTableFile <- DataTableFile.addRows reportLines dataTableFile
                    ()
                })
                |> Async.Sequential
                |> Async.Ignore

            let queryParams = queryParams.createForTextReport projectName ("SorterCeUseProfile_Report" |> UMX.tag<textReportName>)
            let outputData = dataTableFile |> outputData.TextReport
            do! db.saveAsync queryParams outputData
        
            match progress with
            | Some p -> p.Report(sprintf "CE use profile report saved for project: %s" %projectName)
            | None -> ()

            return ()
        }



