namespace GeneSort.Dispatch.V1.SorterEvalBins

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
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

    member this.MakeQueryParams 
                    (repl: int<replNumber>) 
                    (sw: int<sortingWidth>) 
                    (smt: simpleSorterModelType option) 
                    (odt: outputDataType) : queryParams =
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
        member this.Executor runParameters allowOverwrite cts progress = 
            Executor.makeSorterEvalBinsStandard
                this 
                this._spec.CollectNewSortableTests 
                runParameters 
                allowOverwrite 
                cts 
                progress

