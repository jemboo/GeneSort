namespace GeneSort.Dispatch.V1.SortableTest

open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open SortableTestDb


type runHostSortableTest = 
    private { 
        _projectDb: IGeneSortDb 
        _parameterSpans: (string * string list) list
        _spec: runHostSpec
        _run: run
        _maxParallel: int
    }
    
    static member Create db spec run =
        { 
            _projectDb = db; 
            _parameterSpans = spec.Spans; 
            _spec = spec; 
            _run = run;
            _maxParallel = spec.MaxParallel
        }

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
        member this.MaxParallel with get (): int = this._maxParallel


module RunHostSortableTest =

    let createRunHost (spec: runHostSpec) : IRunHost =
        let db = getDatabaseByName spec.DatabaseName
        let run = run.create spec.QueryName spec.RunName spec.RunDescription
        runHostSortableTest.Create db spec run :> IRunHost