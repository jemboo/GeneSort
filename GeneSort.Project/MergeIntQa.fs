namespace GeneSort.Project

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Runs
open GeneSort.Db
open GeneSort.SortingOps
open ProjectOps
open GeneSort.Model.Sorting
open GeneSort.FileDb
open OpsUtils

/// Host type for MergeIntQa supporting P1/P2 transitions
type mergeIntQaHost = 
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

    member this.GetSorterCount (sw: int<sortingWidth>) (repl: int<replNumber>) : int<sorterCount> =
        let baseCount = 
            match %sw with
            | 384 -> 4 
            | _ -> 10
            |> UMX.tag<sorterCount>
        let factor = if %repl = 0 then 1 else 10
        baseCount * factor

    member this.GetStageLength (sw: int<sortingWidth>) : int<stageLength> =
        match %sw with
        | 4 -> 50 | 6 -> 75 | 8 -> 100 | 9 -> 120 | 12 -> 150 
        | 16 -> 200 | 18 -> 230 | 24 -> 300 | 32 -> 450 | 36 -> 500 
        | 48 -> 800 | 64 -> 1000 
        | _ -> failwithf "Unsupported width for stage length: %d" %sw
        |> UMX.tag

    member this.ExtractDomainParams (rp: runParameters) =
        maybe {
            let! r = rp.GetRepl()
            let! w = rp.GetSortingWidth()
            let! dt = rp.GetSortableDataFormat()
            let! md = rp.GetMergeDimension()
            let! mf = rp.GetMergeSuffixType()
            let! sm = rp.GetSorterModelType()
            let! sl = rp.GetStageLength()
            let! ce = rp.GetCeLength()
            let! sc = rp.GetSorterCount()
            return (r, w, md, mf, dt, sm, sl, ce, sc)
        }

module MergeIntQa =

    let projectName = "MergeIntQa" |> UMX.tag<projectName>
    let projectFolder = "c:\\Projects\\MergeIntQa\\Data" |> UMX.tag<pathToProjectFolder>
    let projectDesc = "MergeIntQa on RandomSorters (Msce model)"

    // --- Shared Filtering ---
    let paramMapFilter (rp: runParameters) = Some rp

    // --- Configuration P1 (Testing) ---
    module P1 =
        let spans = [
            (runParameters.sortingWidthKey, ["16"])
            (runParameters.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            (runParameters.mergeDimensionKey, ["2"])
            (runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)
        ]
        let host = mergeIntQaHost.Create (new GeneSortDbMp(projectFolder) :> IGeneSortDb) spans paramMapFilter

    // --- Configuration P2 (Production) ---
    module P2 =
        let spans = [
            (runParameters.sortingWidthKey, [16; 18; 24; 32; 36; 48; 64] |> List.map string)
            (runParameters.sortableDataFormatKey, [sortableDataFormat.IntArray; sortableDataFormat.BoolArray; sortableDataFormat.Int8Vector256; sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            (runParameters.mergeDimensionKey, ["2"; "3"; "4"])
            (runParameters.mergeSuffixTypeKey, [mergeSuffixType.VV_1; mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)
        ]
        let host = mergeIntQaHost.Create (new GeneSortDbMp(projectFolder) :> IGeneSortDb) spans paramMapFilter

    // --- Helpers ---
    let makeQueryParams repl sw smt sdf md mft outputDataType =
        queryParams.create (Some projectName) repl outputDataType
            [| (runParameters.sortingWidthKey, sw |> UmxExt.intToString); 
               (runParameters.sorterModelTypeKey, smt |> Option.map SorterModelType.toString |> UmxExt.stringToString);
               (runParameters.sortableDataFormatKey, sdf |> Option.map SortableDataFormat.toString |> UmxExt.stringToString);
               (runParameters.mergeDimensionKey, md |> UmxExt.intToString);
               (runParameters.mergeSuffixTypeKey, mft |> Option.map MergeSuffixType.toString |> UmxExt.stringToString) |]

    let makeQueryParamsFromRunParams (rp: runParameters) (odt: outputDataType) =
        makeQueryParams (rp.GetRepl()) (rp.GetSortingWidth()) (rp.GetSorterModelType()) 
                        (rp.GetSortableDataFormat()) (rp.GetMergeDimension()) (rp.GetMergeSuffixType()) odt

    let enhancer (host: mergeIntQaHost) (rp : runParameters) : runParameters =
        let repl = rp.GetRepl().Value
        let sw = rp.GetSortingWidth().Value
        let sl = host.GetStageLength sw
        let ce = (((float %sl) * (float %sw) * 0.6) |> int) |> UMX.tag<ceLength>
        let sc = host.GetSorterCount sw repl
        let qp = makeQueryParamsFromRunParams rp (outputDataType.RunParameters)

        rp.WithProjectName(Some projectName)
          .WithRunFinished(Some false)
          .WithCeLength(Some ce)
          .WithStageLength(Some sl)
          .WithSorterCount(Some sc)
          .WithSorterModelType(Some sorterModelType.Msce)
          .WithId (Some qp.Id)

    let paramMapRefiner (host: mergeIntQaHost) (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq |> Seq.choose (host.Filter >> Option.map (enhancer host))

    let outputDataTypes = 
        [| outputDataType.RunParameters; outputDataType.SorterSetEval ""; outputDataType.TextReport ("Profiles" |> UMX.tag) |]


    let project = project.create  projectName projectDesc outputDataTypes

    // --- The Executor ---
    let executor (host: mergeIntQaHost) (runParameters: runParameters) 
                 (allowOverwrite: bool<allowOverwrite>) (cts: CancellationTokenSource) 
                 (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        asyncResult {
            try
                let! _ = checkCancellation cts.Token
                let runId = runParameters |> RunParameters.getIdString
                let! (repl, sw, md, mft, sdf, sm, sl, ce, sc) = 
                    host.ExtractDomainParams runParameters |> Result.ofOption "Missing domain parameters"
                
                report progress (sprintf "%s Starting Run %s repl %d" (MathUtils.getTimestampString()) runId %repl)

                // 3. Load Sortable Tests (Cross-project)
                let qpTests = SortableMergeTests.makeQueryParams (Some (0|>UMX.tag)) (Some sw) (Some md) (Some mft) (Some sdf) (outputDataType.SortableTest "")
                let dbTests = new GeneSortDbMp(SortableMergeTests.projectFolder) :> IGeneSortDb
                let! rawTestData = dbTests.loadAsync qpTests 
                let! sortableTest = rawTestData |> OutputData.asSortableTest

                // 4. Load Sorter Set (Cross-project)
                let qpSorters = RandomSorters.makeQueryParams (Some repl) (Some sw) (Some sm) (outputDataType.SortingSet "")
                let dbSorters = new GeneSortDbMp(RandomSorters.projectFolder) :> IGeneSortDb
                let! sortingSet = dbSorters.loadAsync qpSorters |> AsyncResult.bindResult OutputData.asSortingSet
                let sorterSet = SortingSet.makeSorterSet sortingSet

                // 5. Computation
                let qpEval = makeQueryParamsFromRunParams runParameters (outputDataType.SorterSetEval "")
                let sorterSetEval = SorterSetEval.makeSorterSetEval (%qpEval.Id |> UMX.tag) sorterSet sortableTest false
                
                report progress (sprintf "%s Saving test results %s" (MathUtils.getTimestampString()) runId)
                let! _ = checkCancellation cts.Token

                // 6. Save
                let! _ = host.ProjectDb.saveAsync qpEval (sorterSetEval |> outputData.SorterSetEval) allowOverwrite

                return runParameters.WithRunFinished (Some true)

            with e ->
                return! Error (sprintf "Fatal error in Run %s: %s" (runParameters |> RunParameters.getIdString) e.Message) |> async.Return
        }