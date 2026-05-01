namespace GeneSort.Dispatch.V1.SorterEvalBins

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Model.Sortable.V1
open GeneSort.Dispatch.V1



type runHostSpec = {
    ProjectName: string<projectName>
    RunName: string<runName>
    ProjectDesc: string
    DataFolder: string
    Spans: (string * string list) list
    // Logic Callbacks
    GetStageLength: int<sortingWidth> -> int<stageLength>
    Filter: runParameters -> runParameters option
    Enhancer: IRunHost -> runParameters -> runParameters
    // Domain Settings
    RngFactory: rngFactory
    CollectNewSortableTests: bool
    AllowOverwrite: bool<allowOverwrite>
    // Factories
    TestModelFactory: runParameters -> sortableTestModel
    SorterModelGenFactory: runParameters -> sorterModelGen
}



// --- 4. Host Implementation ---

type runHost = 
    private { 
        _projectDb: IGeneSortDb 
        _parameterSpans: (string * string list) list
        _spec: runHostSpec
        _project: run
    }
    
    static member Create db spec project =
        { _projectDb = db; _parameterSpans = spec.Spans; _spec = spec; _project = project }

    member this.Spec = this._spec

    // Internal Factories
    member this.MakeSortableTest (rp: runParameters) : Sortable.sortableTest option =
        maybe {
            let! sdt = rp.GetSortableDataFormat()
            let qpTests = this.MakeQueryParamsFromRunParams rp (outputDataType.SortableTest "")
            let testModel = this._spec.TestModelFactory rp
            return SortableTestModel.makeSortableTest (%qpTests.Id |> UMX.tag) testModel sdt
        }

    member this.MakeSorterModelSet (rp: runParameters) : sorterModelSet option =
        maybe {
            let! sc = rp.GetSorterCount()
            let repl = rp.GetRepl() |> Option.defaultValue (0 |> UMX.tag)
            let qpFullSet = this.MakeQueryParamsFromRunParams rp (outputDataType.SorterModelSet "")
            let gen = this._spec.SorterModelGenFactory rp
            let firstIdx = (%repl * %sc) |> UMX.tag<sorterCount>
            return SorterModelGen.makeSorterModelSet (%qpFullSet.Id |> UMX.tag) firstIdx sc gen
        }

    member this.MakeQueryParams (repl: int<replNumber>) (sw: int<sortingWidth>) (smt: simpleSorterModelType option) (odt: outputDataType) : queryParams =
        let pName = this._spec.ProjectName
        queryParams.create (Some pName) (Some repl) odt
            [| (runParameters.sortingWidthKey, (Some sw) |> SortingWidth.toString); 
               (runParameters.simpleSorterModelTypeKey, smt |> Option.map SimpleSorterModelType.toString |> UmxExt.stringToString) |]

    member this.MakeQueryParamsFromRunParams (rp: runParameters) (odt: outputDataType) : queryParams =
        this.MakeQueryParams (rp.GetRepl().Value) (rp.GetSortingWidth().Value) (rp.GetSimpleSorterModelType()) odt

    member this.ParamMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq 
        |> Seq.choose (this._spec.Filter >> Option.map (this._spec.Enhancer (this :> IRunHost)))

    interface IRunHost with
        member this.ProjectDb = this._projectDb
        member this.Project = this._project
        member this.ParameterSpans = this._parameterSpans
        member this.AllowOverwrite = this._spec.AllowOverwrite
        member this.MakeQueryParamsFromRunParams rp odt = this.MakeQueryParamsFromRunParams rp odt
        member this.ParamMapRefiner rps = this.ParamMapRefiner rps
        member this.Executor rp ow cts prog = 
            Executor.makeSorterEvalBins 
                this this.MakeSorterModelSet this.MakeSortableTest 
                this._spec.CollectNewSortableTests rp ow cts prog

