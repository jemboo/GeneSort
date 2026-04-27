namespace GeneSort.Project

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Runs
open GeneSort.Db
open GeneSort.FileDb
open GeneSort.Model.Sortable
open OpsUtils

/// Host type to toggle between P1 (Testing) and P2 (Production) configurations
type sortableMergeHost = 
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

    /// Centralized parameter extraction for the executor
    member this.ExtractDomainParams (rp: runParameters) =
        maybe {
            let! width = rp.GetSortingWidth()
            let! dim = rp.GetMergeDimension()
            let! fill = rp.GetMergeSuffixType()
            let! dataType = rp.GetSortableDataFormat()
            return (width, dim, fill, dataType)
        }

module SortableMergeTests =

    let projectName = "SortableMergeTests" |> UMX.tag<projectName>
    let projectFolder = "c:\\Projects\\SortableMergeTests\\Data" |> UMX.tag<pathToProjectFolder>
    let projectDesc = "Calc and save large SortableMergeTests"

    // --- Shared Filters ---

    let private mergeDimensionDividesSortingWidth (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let md = rp.GetMergeDimension().Value
        if (%sw % %md = 0) then Some rp else None

    let private limitForMergeFillType (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let ft = rp.GetMergeSuffixType().Value
        if (ft.IsNoSuffix && %sw > 64) then None else Some rp

    let private limitForMergeDimension (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let md = rp.GetMergeDimension().Value
        if (%md > 6 && %sw > 144) then None else Some rp

    let paramMapFilter (rp: runParameters) = 
        Some rp
        |> Option.bind mergeDimensionDividesSortingWidth
        |> Option.bind limitForMergeFillType
        |> Option.bind limitForMergeDimension

    // --- Shared Query Logic ---

    let makeQueryParams (repl: int<replNumber> option) (sortingWidth: int<sortingWidth> option)
                        (mergeDimension: int<mergeDimension> option) (mergeFillType: mergeSuffixType option)
                        (sortableDataFormat: sortableDataFormat option) (outputDataType: outputDataType) : queryParams =
        queryParams.create (Some projectName) repl outputDataType
            [| (runParameters.sortingWidthKey, sortingWidth |> SortingWidth.toString); 
               (runParameters.mergeDimensionKey, mergeDimension |> MergeDimension.toString);
               (runParameters.mergeSuffixTypeKey, mergeFillType |> Option.map MergeSuffixType.toString |> UmxExt.stringToString );
               (runParameters.sortableDataFormatKey, sortableDataFormat |> Option.map SortableDataFormat.toString |> UmxExt.stringToString ); |]

    let makeQueryParamsFromRunParams (runParams: runParameters) (outputDataType: outputDataType) =
        makeQueryParams (runParams.GetRepl()) (runParams.GetSortingWidth()) (runParams.GetMergeDimension())
                        (runParams.GetMergeSuffixType()) (runParams.GetSortableDataFormat()) outputDataType

    // --- Configuration P1 (Testing) ---

    module P1 =
        let spans = [
            (runParameters.sortingWidthKey, [16;] |> List.map string)
            (runParameters.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            (runParameters.mergeDimensionKey, [2;] |> List.map string)
            (runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix; mergeSuffixType.VV_1] |> List.map MergeSuffixType.toString)
        ]
        let host = sortableMergeHost.Create (new GeneSortDbMp(projectFolder) :> IGeneSortDb) spans paramMapFilter

    // --- Configuration P2 (Production) ---

    module P2 =
        let spans = [
            (runParameters.sortingWidthKey, [16; 18; 24; 32; 36; 48; 64; 96; 128; 192; 256] |> List.map string)
            (runParameters.sortableDataFormatKey, [sortableDataFormat.Int8Vector512] |> List.map SortableDataFormat.toString)
            (runParameters.mergeDimensionKey, [2; 3; 4; 6; 8] |> List.map string)
            (runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix; mergeSuffixType.VV_1] |> List.map MergeSuffixType.toString)
        ]
        let host = sortableMergeHost.Create (new GeneSortDbMp(projectFolder) :> IGeneSortDb) spans paramMapFilter

    // --- Project Refinement ---

    let enhancer (rp : runParameters) =
        let qp = makeQueryParamsFromRunParams rp (outputDataType.RunParameters)
        rp.WithProjectName(Some projectName).WithRunFinished(Some false).WithId (Some qp.Id)

    /// Note: You can now pass either P1.host or P2.host to drive the refiner
    let paramMapRefiner (host: sortableMergeHost) (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq 
        |> Seq.choose (host.Filter >> Option.map enhancer)
        |> Seq.toArray
        |> Array.toSeq

    let outputDataTypes = [| outputDataType.SortableTestSet ""; outputDataType.RunParameters |]

    let project = project.create  projectName projectDesc outputDataTypes

    // --- Executor ---

    let executor (host: sortableMergeHost) (runParams: runParameters) 
                 (allowOverwrite: bool<allowOverwrite>) (cts: CancellationTokenSource) 
                 (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        asyncResult {
            try
                let! (_: unit) = checkCancellation cts.Token
                let runId = runParams |> RunParameters.getIdString
                report progress (sprintf "Starting Merge Generation Run %s" %runId)

                // 2. Safe extraction (Via Host)
                let! (sw, md, mft, sdf) = 
                    host.ExtractDomainParams runParams 
                    |> Result.ofOption "Missing domain parameters required for generation"

                // 3. Create SortableTestModel
                let sortableTestModel = msasM.create sw md mft |> sortableTestModel.MsasMi
            
                let qpForSortableTest = makeQueryParamsFromRunParams runParams (outputDataType.SortableTest "") 
                let sortableTests = SortableTestModel.makeSortableTest (%qpForSortableTest.Id |> UMX.tag) sortableTestModel sdf

                // 4. Save (Using Host DB)
                let! (_: unit) = checkCancellation cts.Token
                let! (_: unit) = host.ProjectDb.saveAsync qpForSortableTest (sortableTests |> outputData.SortableTest) allowOverwrite

                return runParams.WithRunFinished (Some true)

            with (e: exn) ->
                let runId = runParams |> RunParameters.getIdString
                return! Error (sprintf "Fatal error in Generation Run %s: %s" runId e.Message)
        }