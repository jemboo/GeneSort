namespace GeneSort.Project

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.Runs
open GeneSort.Db
open ProjectOps
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.ModelParams
open GeneSort.FileDb
open ParamOps
open OpsUtils

/// Host type for FullBoolEvals to manage environment configurations
type fullBoolEvalsHost = 
    private { 
        _projectDb: IGeneSortDb 
        _parameterSpans: (string * string list) list
        _filter: runParameters -> runParameters option
    }
    static member Create db spans filter =
        { _projectDb = db; _parameterSpans = spans; _filter = filter }

    member this.ProjectDb = this._projectDb
    member this.ParameterSpans = this._parameterSpans
    member this.Filter = this._filter

    member this.ExtractDomainParams (rp: runParameters) =
        maybe {
            let! smt = rp.GetSorterModelType()
            let! sw = rp.GetSortingWidth()
            let! sdt = rp.GetSortableDataFormat()
            return (smt, sw, sdt)
        }

module FullBoolEvals =

    let projectName = "FullBoolEvals" |> UMX.tag<projectName>
    let projectFolder = "c:\\Projects\\FullBoolEvals\\Data" |> UMX.tag<pathToProjectFolder>
    let projectDesc = "FullBoolEvals on RandomSorters with SortingWidth from 4 to 24"

    // --- Shared Filtering Logic ---

    let private sorterModelTypeForSortingWidth (rp: runParameters) =
        let sorterModelKey = rp.GetSorterModelType().Value
        let sortingWidth = rp.GetSortingWidth().Value
        let has2factor = (%sortingWidth % 2 = 0)
        let isMuf4able = (MathUtils.isAPowerOfTwo %sortingWidth)
        let isMuf6able = (%sortingWidth % 3 = 0) && (MathUtils.isAPowerOfTwo (%sortingWidth / 3))

        match sorterModelKey with
        | sorterModelType.Msce -> Some rp
        | sorterModelType.Mssi | sorterModelType.Msrs -> if has2factor then Some rp else None
        | sorterModelType.Msuf4 -> if isMuf4able then Some rp else None
        | sorterModelType.Msuf6 -> if isMuf6able then Some rp else None
        | _ -> None

    let paramMapFilter (rp: runParameters) = 
        Some rp |> Option.bind sorterModelTypeForSortingWidth

    // --- Configuration P1 (Testing) ---

    module P1 =
        let spans = [
            (runParameters.sortingWidthKey, ["16"])
            (runParameters.sorterModelTypeKey, [sorterModelType.Msce] |> List.map SorterModelType.toString)
            (runParameters.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
        ]
        let host = fullBoolEvalsHost.Create (new GeneSortDbMp(projectFolder) :> IGeneSortDb) spans paramMapFilter

    // --- Configuration P2 (Production) ---

    module P2 =
        let spans = [
            (runParameters.sortingWidthKey, [4; 6; 8; 12; 16; 18; 20; 22; 24] |> List.map string)
            (runParameters.sorterModelTypeKey, [sorterModelType.Msce; sorterModelType.Mssi; sorterModelType.Msrs; sorterModelType.Msuf4] |> List.map SorterModelType.toString)
            (runParameters.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
        ]
        let host = fullBoolEvalsHost.Create (new GeneSortDbMp(projectFolder) :> IGeneSortDb) spans paramMapFilter

    // --- Helpers ---

    let makeQueryParams repl sortingWidth sorterModelType sortableDataFormat outputDataType =
        queryParams.create (Some projectName) repl outputDataType
            [| (runParameters.sortingWidthKey, sortingWidth |> UmxExt.intToString); 
               (runParameters.sorterModelTypeKey, sorterModelType |> Option.map SorterModelType.toString |> UmxExt.stringToString);
               (runParameters.sortableDataFormatKey, sortableDataFormat |> Option.map SortableDataFormat.toString |> UmxExt.stringToString) |]

    let makeQueryParamsFromRunParams (rp: runParameters) (odt: outputDataType) =
        makeQueryParams (rp.GetRepl()) (rp.GetSortingWidth()) (rp.GetSorterModelType()) (rp.GetSortableDataFormat()) odt

    let enhancer (rp : runParameters) : runParameters =
        let qp = makeQueryParamsFromRunParams rp (outputDataType.RunParameters)
        rp.WithProjectName(Some projectName).WithRunFinished(Some false).WithId (Some qp.Id)

    let paramMapRefiner (host: fullBoolEvalsHost) (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq |> Seq.choose (host.Filter >> Option.map enhancer) |> Seq.toArray |> Array.toSeq

    let outputDataTypes = 
        [| outputDataType.RunParameters; outputDataType.SorterSetEval "";
           outputDataType.TextReport ("Bins" |> UMX.tag); outputDataType.TextReport ("Profiles" |> UMX.tag) |]

    let project = project.create  projectName projectDesc outputDataTypes

    // --- The Executor ---

    let executor (host: fullBoolEvalsHost) (runParameters: runParameters) 
                 (allowOverwrite: bool<allowOverwrite>) (cts: CancellationTokenSource) 
                 (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        asyncResult {
            try
                // 1. Setup
                let! _ = checkCancellation cts.Token
                let runId = runParameters |> RunParameters.getIdString
                let repl = runParameters.GetRepl() |> Option.defaultValue (-1 |> UMX.tag)
                report progress (sprintf "%s Starting Run %s repl %d" (MathUtils.getTimestampString()) %runId %repl)

                // 2. Safe Domain Parameter Extraction
                let! (sorterModelType, sortingWidth, sortableDataFormat) = 
                    host.ExtractDomainParams runParameters 
                    |> Result.ofOption (sprintf "Run %s, Repl %d: Missing required parameters" %runId %repl)

                // 3. Load SortingSet (Cross-project)
                let qpSortingSet = RandomSorters.makeQueryParams (Some repl) (Some sortingWidth) (Some sorterModelType) (outputDataType.SortingSet "")
                let dbRandomSorters = new GeneSortDbMp(RandomSorters.projectFolder) :> IGeneSortDb
                
                // Using bindResult to unwrap the Result returned by OutputData.asSortingSet
                let! sortingSet = dbRandomSorters.loadAsync qpSortingSet |> AsyncResult.bindResult OutputData.asSortingSet

                // 4. Perform Computation
                let! _ = checkCancellation cts.Token
                let sortableTestModel = msasF.create sortingWidth |> sortableTestModel.MsasF
                let qpSortableTests = makeQueryParamsFromRunParams runParameters (outputDataType.SortableTest "")
                let sortableTests = SortableTestModel.makeSortableTest (%qpSortableTests.Id |> UMX.tag) sortableTestModel sortableDataFormat

                let sorterSet = sortingSet |> SortingSet.makeSorterSet
                let qpEval = makeQueryParamsFromRunParams runParameters (outputDataType.SorterSetEval "")
                let sorterSetEval = SorterSetEval.makeSorterSetEval (%qpEval.Id |> UMX.tag) sorterSet sortableTests false

                // 5. Save Eval Results
                let! _ = host.ProjectDb.saveAsync qpEval (sorterSetEval |> outputData.SorterSetEval) allowOverwrite

                // 6. Make passing sorterSet
                let qpPass = makeQueryParamsFromRunParams runParameters (outputDataType.SortingSet "Pass")
                let passingSet = SorterSetEval.makePassingSortingSet (%qpPass.Id |> UMX.tag) sortingSet sorterSetEval
                let! _ = host.ProjectDb.saveAsync qpPass (passingSet |> outputData.SortingSet) allowOverwrite

                // 7. Success
                return runParameters.WithRunFinished (Some true)

            with e ->
                let rawId = runParameters |> RunParameters.getIdString
                return! Error (sprintf "Execution exception in Run %s: %s" rawId e.Message) |> async.Return
        }