namespace GeneSort.Dispatch.V1

open System
open System.Threading
open FSharp.UMX
open GeneSort.Db.V1
open GeneSort.Project.V1


type runHost = 
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
          _parameterSpans = spec.spans; 
          _spec = spec;
          _run = run;
          _maxParallel = spec.maxParallel }

    member this.Spec = this._spec

    member this.ParamMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq 
        |> Seq.choose (this._spec.filter >> Option.map (this._spec.enhancer (this :> IRunHost)))

    interface IRunHost with
        member this.RunDb = this._projectDb
        member this.Run = this._run
        member this.ParameterSpans = this._parameterSpans
        member this.AllowOverwrite = this._spec.allowOverwrite
        member this.ParamMapRefiner rps = this.ParamMapRefiner rps
        member this.MaxParallel with get (): int = this._maxParallel



