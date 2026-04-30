namespace GeneSort.Dispatch.V1

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open OpsUtils
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Model.Sortable.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Sorting.Sortable

// --- Core Types ---

/// Data-only specification for a project configuration
type ProjectSpec = {
    ProjectName: string<projectName>
    RunName: string<runName>
    ProjectDesc: string
    DataFolder: string
    Spans: (string * string list) list
    // Logic Callbacks
    GetStageLength: int<sortingWidth> -> int<stageLength>
    Filter: runParameters -> runParameters option
    Enhancer: randomSorterBinsHost -> runParameters -> runParameters
    // Domain Settings
    RngFactory: rngFactory
    CollectNewSortableTests: bool
    AllowOverwrite: bool<allowOverwrite>
    // Factories driven by runParameters
    TestModelFactory: runParameters -> sortableTestModel
    SorterModelGenFactory: runParameters -> sorterModelGen
}

/// Host type managing live environment dependencies and domain logic
and randomSorterBinsHost = 
    private { 
        _projectDb: IGeneSortDb 
        _parameterSpans: (string * string list) list
        _spec: ProjectSpec
        _project: project
    }
    static member Create db spec project =
        { _projectDb = db; _parameterSpans = spec.Spans; _spec = spec; _project = project }

    member this.ProjectDb = this._projectDb
    member this.ParameterSpans = this._parameterSpans
    member this.Spec = this._spec
    member this.Project = this._project

    // --- Query & Refinement Logic ---

    member this.MakeQueryParams (repl: int<replNumber>) (sw: int<sortingWidth>) (smt: simpleSorterModelType option) (odt: outputDataType) =
        let pName = this.Spec.ProjectName
        queryParams.create (Some pName) (Some repl) odt
            [| (runParameters.sortingWidthKey, (Some sw) |> SortingWidth.toString); 
               (runParameters.simpleSorterModelTypeKey, smt |> Option.map SimpleSorterModelType.toString |> UmxExt.stringToString) |]

    member this.MakeQueryParamsFromRunParams (rp: runParameters) (odt: outputDataType) =
        this.MakeQueryParams (rp.GetRepl().Value) (rp.GetSortingWidth().Value) (rp.GetSimpleSorterModelType()) odt

    member this.ParamMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq |> Seq.choose (this.Spec.Filter >> Option.map (this.Spec.Enhancer this))

    member this.ExtractDomainParams (rp: runParameters) =
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let! sl = rp.GetStageLength()
            let! sc = rp.GetSorterCount()
            let! sdt = rp.GetSortableDataFormat()
            let repl = rp.GetRepl() |> Option.defaultValue (0 |> UMX.tag)
            return (smt, sw, sl, sc, sdt, repl)
        }

    member this.MakeSortableTest (rp: runParameters) : Sortable.sortableTest option =
        maybe {
            let! sdt = rp.GetSortableDataFormat()
            let qpTests = this.MakeQueryParamsFromRunParams rp (outputDataType.SortableTest "")
            let testModel = this.Spec.TestModelFactory rp
            return SortableTestModel.makeSortableTest (%qpTests.Id |> UMX.tag) testModel sdt
        }

    /// streamlined: All necessary data is extracted from the single rp argument
    member this.MakeSorterModelSet (rp: runParameters) =
        maybe {
            let! sc = rp.GetSorterCount()
            let repl = rp.GetRepl() |> Option.defaultValue (0 |> UMX.tag)
            let qpFullSet = this.MakeQueryParamsFromRunParams rp (outputDataType.SorterModelSet "")
            let gen = this.Spec.SorterModelGenFactory rp
            let firstIdx = (%repl * %sc) |> UMX.tag<sorterCount>
            return SorterModelGen.makeSorterModelSet (%qpFullSet.Id |> UMX.tag) firstIdx sc gen
        }

// --- Logic Module ---

module RandomSorterBins =

    // --- Logic Implementations ---

    let private standardEnhancer (host :randomSorterBinsHost) (rp :runParameters) : runParameters =
        let sw = rp.GetSortingWidth().Value
        let qp = host.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters %host.Project.ProjectName)
        let sl = host.Spec.GetStageLength sw
        let cl = sl |> StageLength.toCeLength sw
        let pName = host.Spec.ProjectName
        let rName = host.Spec.RunName

        rp.WithProjectName(Some pName)
          .WithRunName(Some rName)
          .WithRunFinished(Some false)
          .WithCeLength(Some cl)
          .WithStageLength(Some sl)
          .WithId (Some qp.Id)

    let private standardSorterModelTypeFilter (rp: runParameters) =
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let has2factor = (%sw % 2 = 0)
            let isMuf4able = (MathUtils.isAPowerOfTwo %sw)
            let isMuf6able = (%sw % 3 = 0) && (MathUtils.isAPowerOfTwo (%sw / 3))

            return! 
                match smt with
                | simpleSorterModelType.Msce -> Some rp
                | simpleSorterModelType.Mssi | simpleSorterModelType.Msrs -> if has2factor then Some rp else None
                | simpleSorterModelType.Msuf4 -> if isMuf4able then Some rp else None
                | simpleSorterModelType.Msuf6 -> if isMuf6able then Some rp else None
                | _ -> None
        }

    let private standardStageLength (sw: int<sortingWidth>) : int<stageLength> =
        match %sw with
        | 4 -> 5 | 6 -> 20 | 8 -> 40 | 12 -> 40 | 16 -> 50
        | 18 -> 120 | 20 -> 130 | 22 -> 140 | 24 -> 200 
        | _ -> failwithf "Unsupported width: %d" %sw
        |> UMX.tag

    // --- Specs ---

    module Specs =

        let P1 = {
            ProjectName = "Standard" |>UMX.tag<projectName>
            RunName = "Standard" |> UMX.tag<runName>
            ProjectDesc = "Standard binning for Msce/Mssi/Msrs"
            DataFolder = "c:\\ProjectsV1\\RandomSorterBins\\Data"
            Spans = [
                (runParameters.sortingWidthKey, ["12"])
                (runParameters.simpleSorterModelTypeKey, [simpleSorterModelType.Msce] |> List.map SimpleSorterModelType.toString)
                (runParameters.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (runParameters.sorterCountKey, ["1000"])
            ]
            GetStageLength = standardStageLength
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            RngFactory = rngFactory.LcgFactory
            CollectNewSortableTests = true
            AllowOverwrite = false |> UMX.tag
            TestModelFactory = fun rp -> 
                let sw = rp.GetSortingWidth().Value
                msasF.create sw |> sortableTestModel.MsasF
            SorterModelGenFactory = fun rp ->
                let smt = rp.GetSimpleSorterModelType().Value
                let sw = rp.GetSortingWidth().Value
                let sl = rp.GetStageLength().Value
                SimpleSorterModelGen.makeUniform rngFactory.LcgFactory sw sl smt |> sorterModelGen.Simple
        }

    let Configs = Map.ofList [ ("P1", Specs.P1) ]

    let CreateHost (spec: ProjectSpec) =
        let folder = spec.DataFolder |> UMX.tag
        let db = new GeneSortDbMp(folder) :> IGeneSortDb
        let proj = project.create 
                        spec.ProjectName
                        spec.RunName
                        spec.ProjectDesc 
                        [| outputDataType.RunParameters %spec.ProjectName; outputDataType.SorterEvalBins ""; |]
        randomSorterBinsHost.Create db spec proj

    // --- Execution Logic ---

    let executor (host: randomSorterBinsHost) (runParameters: runParameters) 
                 (_: bool<allowOverwrite>) 
                 (cts: CancellationTokenSource) 
                 (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        asyncResult {
            try
                let! (_: unit) = checkCancellation cts.Token
                let runId = runParameters |> RunParameters.getIdString
                report progress (sprintf "%s Starting Run %s" (MathUtils.getTimestampString()) %runId)

                // 1. Sorter Models
                let! sorterModelSet = host.MakeSorterModelSet runParameters |> Result.ofOption "Failed to create SorterModelSet"
                let fullSorterSet = SorterModelSet.makeSorterSet (%sorterModelSet.Id |> UMX.tag) sorterModelSet

                // 2. Sortable Tests
                let! (_: unit) = checkCancellation cts.Token
                let! tests = host.MakeSortableTest runParameters |> Result.ofOption "Failed to create SortableTests"

                // 3. Evaluation
                let qpEval = host.MakeQueryParamsFromRunParams runParameters (outputDataType.SorterSetEval "")
                let sorterSetEval = SorterSetEval.makeSorterSetEval 
                                                    (%qpEval.Id |> UMX.tag) 
                                                    fullSorterSet 
                                                    tests 
                                                    host.Spec.CollectNewSortableTests

                // 4. Bin and Save
                let qpBins = host.MakeQueryParamsFromRunParams runParameters (outputDataType.SorterEvalBins "")
                let sorterEvalBins = sorterEvalBinsV1.createFromEvals 
                                            (%qpBins.Id |> UMX.tag) 
                                            (tests |> SortableTests.getId) 
                                            sorterSetEval.SorterEvals 
                                     |> sorterEvalBins.V1

                let! (_: unit) = host.ProjectDb.saveAsync qpBins (sorterEvalBins |> outputData.SorterEvalBins) host.Spec.AllowOverwrite

                return runParameters.WithRunFinished (Some true)

            with e -> 
                return! Error (sprintf "Error in %s: %s" (runParameters |> RunParameters.getIdString) e.Message) |> async.Return
        }