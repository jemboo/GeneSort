namespace GeneSort.Dispatch.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.FileDb.V1


// --- 4. Host Implementation ---

type runHost = 
    private { 
        _projectDb: IGeneSortDb 
        _parameterSpans: (string * string list) list
        _spec: runHostSpec
        _run: run
    }
    
    static member Create db spec run =
        { _projectDb = db; _parameterSpans = spec.Spans; _spec = spec; _run = run }

    member this.Spec = this._spec

    member this.MakeQueryParams 
                    (repl: int<replNumber>) 
                    (sw: int<sortingWidth>) 
                    (smt: simpleSorterModelType option) 
                    (rng: rngType option)
                    (odt: outputDataType) : queryParams =
        queryParams.create 
                    this._spec.ProjectName
                    (Some repl) 
                    odt
            [| (runParameters.sortingWidthKey, (Some sw) |> SortingWidth.toString); 
               (runParameters.simpleSorterModelTypeKey, smt |> Option.map SimpleSorterModelType.toString |> UmxExt.stringOptionToString) 
               (runParameters.rngTypeKey, rng |> Option.map RngType.toString |> UmxExt.stringOptionToString) |]

    member this.MakeQueryParamsFromRunParams (rp: runParameters) (odt: outputDataType) : queryParams =
        this.MakeQueryParams (rp.GetRepl().Value) (rp.GetSortingWidth().Value) (rp.GetSimpleSorterModelType()) (rp.GetRngType()) odt

    member this.ParamMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq 
        |> Seq.choose (this._spec.Filter >> Option.map (this._spec.Enhancer (this :> IRunHost)))

    interface IRunHost with
        member this.ProjectDb = this._projectDb
        member this.Run = this._run
        member this.ParameterSpans = this._parameterSpans
        member this.AllowOverwrite = this._spec.AllowOverwrite
        member this.MakeQueryParamsFromRunParams rp odt = this.MakeQueryParamsFromRunParams rp odt
        member this.ParamMapRefiner rps = this.ParamMapRefiner rps


module RunHost =
    let createRunHost (spec: runHostSpec) : IRunHost =
        let folder = spec.DataFolder |> UMX.tag
        let db = new GeneSortDbMp(folder) :> IGeneSortDb
        let run = run.create spec.ProjectName spec.RunName spec.RunDescription
        runHost.Create db spec run :> IRunHost

