namespace GeneSort.Dispatch.V1.SorterEvalBins

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.FileDb.V1
open Common
open SimpleSorterModelDbs


// --- 4. Host Implementation ---

type runHostEvalBins = 
    private { 
        _projectDb: IGeneSortDb 
        _parameterSpans: (string * string list) list
        _spec: runHostSpec
        _run: run
    }
    
    static member Create db spec run =
        { _projectDb = db; _parameterSpans = spec.Spans; _spec = spec; _run = run }

    member this.Spec = this._spec

    member this.ParamMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq 
        |> Seq.choose (this._spec.Filter >> Option.map (this._spec.Enhancer (this :> IRunHost)))

    interface IRunHost with
        member this.ProjectDb = this._projectDb
        member this.Run = this._run
        member this.ParameterSpans = this._parameterSpans
        member this.AllowOverwrite = this._spec.AllowOverwrite
        member this.ParamMapRefiner rps = this.ParamMapRefiner rps


module RunHostEvalBins =

    let createRunHost (spec: runHostSpec) : IRunHost =
        let db = getDatabaseByName spec.DatabaseName
        let run = run.create spec.ProjectName spec.RunName spec.RunDescription
        runHostEvalBins.Create db spec run :> IRunHost

