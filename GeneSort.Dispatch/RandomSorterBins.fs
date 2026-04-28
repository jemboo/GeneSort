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

// --- Core Types ---

/// Data-only specification for a project configuration
type ProjectSpec = {
    ProjectName: string
    ProjectDesc: string
    DefaultFolder: string
    Spans: (string * string list) list
    CustomFolder: string option
    // Logic Callbacks
    GetStageLength: int<sortingWidth> -> int<stageLength>
    Filter: runParameters -> runParameters option
    Enhancer: randomSorterBinsHost -> runParameters -> runParameters
    // Domain Settings
    RngFactory: rngFactory
    CollectNewSortableTests: bool
    AllowOverwrite: bool<allowOverwrite>
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

    // --- Encapsulated Query & Refinement Logic ---

    member this.MakeQueryParams (repl: int<replNumber>) (sw: int<sortingWidth>) (smt: simpleSorterModelType option) (odt: outputDataType) =
        let pName = this.Spec.ProjectName |> UMX.tag<projectName>
        queryParams.create (Some pName) (Some repl) odt
            [| (runParameters.sortingWidthKey, (Some sw) |> SortingWidth.toString); 
               (runParameters.simpleSorterModelTypeKey, smt |> Option.map SimpleSorterModelType.toString |> UmxExt.stringToString) |]

    member this.MakeQueryParamsFromRunParams (rp: runParameters) (odt: outputDataType) =
        this.MakeQueryParams (rp.GetRepl().Value) (rp.GetSortingWidth().Value) (rp.GetSimpleSorterModelType()) odt

    member this.ParamMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq |> Seq.choose (this.Spec.Filter >> Option.map (this.Spec.Enhancer this))

    member this.ExtractDomainParams (rp: runParameters) =
        maybe {
            let! smk = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let! sl = rp.GetStageLength()
            let! sc = rp.GetSorterCount()
            let! sdt = rp.GetSortableDataFormat()
            return (smk, sw, sl, sc, sdt)
        }

// --- Logic Module ---

module RandomSorterBins =

    // --- Core Logic Implementations ---

    let private standardEnhancer (host: randomSorterBinsHost) (rp : runParameters) : runParameters =
        let sw = rp.GetSortingWidth().Value
        let qp = host.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters)
        let sl = host.Spec.GetStageLength sw
        let cl = sl |> StageLength.toCeLength sw
        let pName = host.Spec.ProjectName |> UMX.tag<projectName>

        rp.WithProjectName(Some pName)
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
        | _ -> failwithf "Unsupported sorting width: %d" %sw
        |> UMX.tag

    // --- Specification Registry ---

    module Specs =
        let P1 = {
            ProjectName = "RandomSorterBins_Standard"
            ProjectDesc = "Standard binning for Msce/Mssi/Msrs"
            DefaultFolder = "c:\\ProjectsV1\\RandomSorterBins\\Data"
            Spans = [
                (runParameters.sortingWidthKey, ["12"])
                (runParameters.simpleSorterModelTypeKey, [simpleSorterModelType.Msce] |> List.map SimpleSorterModelType.toString)
                (runParameters.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (runParameters.sorterCountKey, ["1000"])
            ]
            CustomFolder = None
            GetStageLength = standardStageLength
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            RngFactory = rngFactory.LcgFactory
            CollectNewSortableTests = true
            AllowOverwrite = false |> UMX.tag
        }

        let P2 = {
            ProjectName = "RandomSorterBins_Heavy"
            ProjectDesc = "High-width stress tests for 9950X3D"
            DefaultFolder = "c:\\ProjectsV1\\RandomSorterBins_Heavy\\Data"
            Spans = [
                (runParameters.sortingWidthKey, [18; 20] |> List.map string)
                (runParameters.simpleSorterModelTypeKey, 
                    [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4; simpleSorterModelType.Msuf6] 
                    |> List.map SimpleSorterModelType.toString)
                (runParameters.sortableDataFormatKey, [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (runParameters.sorterCountKey, ["10000"])
            ]
            CustomFolder = None
            GetStageLength = standardStageLength
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            RngFactory = rngFactory.LcgFactory
            CollectNewSortableTests = true
            AllowOverwrite = false |> UMX.tag
        }

    let Configs = Map.ofList [ ("P1", Specs.P1); ("P2", Specs.P2) ]

    /// Factory to create a live Host from a Spec
    let CreateHost (spec: ProjectSpec) =
        let folder = spec.CustomFolder |> Option.defaultValue spec.DefaultFolder |> UMX.tag
        let db = new GeneSortDbMp(folder) :> IGeneSortDb
        let pName = spec.ProjectName |> UMX.tag<projectName>
        
        let proj = project.create pName spec.ProjectDesc 
                                  [| outputDataType.RunParameters; outputDataType.SorterEvalBins ""; |]
        
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
                let repl = runParameters.GetRepl() |> Option.defaultValue (0 |> UMX.tag)
                report progress (sprintf "%s Starting Run %s" (MathUtils.getTimestampString()) %runId)

                let! (smt, sw, sl, sc, sdt) = host.ExtractDomainParams runParameters |> Result.ofOption "Missing parameters"

                let sorterModelGen = SimpleSorterModelGen.makeUniform host.Spec.RngFactory sw sl smt |> sorterModelGen.Simple
                let firstIdx = (%repl * %sc) |> UMX.tag<sorterCount>

                let qpFullSet = host.MakeQueryParamsFromRunParams runParameters (outputDataType.SorterModelSet "") 
                let sorterModelSet = SorterModelGen.makeSorterModelSet (%qpFullSet.Id |> UMX.tag) firstIdx sc sorterModelGen
                let fullSorterSet = SorterModelSet.makeSorterSet (%qpFullSet.Id |> UMX.tag) sorterModelSet

                let! (_: unit) = checkCancellation cts.Token
                let testModel = msasF.create sw |> sortableTestModel.MsasF
                let qpTests = host.MakeQueryParamsFromRunParams runParameters (outputDataType.SortableTest "")
                let tests = SortableTestModel.makeSortableTest (%qpTests.Id |> UMX.tag) testModel sdt

                let qpEval = host.MakeQueryParamsFromRunParams runParameters (outputDataType.SorterSetEval "")
                let sorterSetEval = SorterSetEval.makeSorterSetEval (%qpEval.Id |> UMX.tag) fullSorterSet tests host.Spec.CollectNewSortableTests

                let qpBins = host.MakeQueryParamsFromRunParams runParameters (outputDataType.SorterEvalBins "")
                let sorterEvalBins = sorterEvalBinsV1.createFromEvals (%qpBins.Id |> UMX.tag) (%qpTests.Id |> UMX.tag) sorterSetEval.SorterEvals |> sorterEvalBins.V1

                let! (_: unit) = host.ProjectDb.saveAsync qpBins (sorterEvalBins |> outputData.SorterEvalBins) host.Spec.AllowOverwrite

                return runParameters.WithRunFinished (Some true)

            with e -> 
                return! Error (sprintf "Error in %s: %s" (runParameters |> RunParameters.getIdString) e.Message) |> async.Return
        }