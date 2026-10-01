namespace GeneSort.Dispatch.V1

open System
open System.Threading
open FSharp.UMX
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Core


type runHost = 
    private { 
        _projectDb: IGeneSortDb 
        _spec: runHostSpec
        _run: run
        _genSaveIntervals: genIntervalConfig
        _genSaveSubIntervals: genIntervalConfig
        _queryParamsFromRunParams: queryParamsBuilder
        _maxParallel: int
    }
    
    static member Create (db: IGeneSortDb) (spec: runHostSpec) (run: run) =
        let lookupInterval name =
            match GenIntervalRegistry.genIntervalConfigsDict.TryGetValue name with
            | true, interval -> interval
            | _ -> failwithf "Sampling interval '%s' is not registered for run '%s'." name (%run.RunName)
        { 
          _projectDb = db; 
          _spec = spec;
          _run = run;
          _genSaveIntervals = lookupInterval run.GenSaveIntervalsName;
          _genSaveSubIntervals = lookupInterval run.GenSaveSubIntervalsName;
          _queryParamsFromRunParams = QueryParamsCatalog.get run.QueryCatalogName;
          _maxParallel = spec.maxParallel }

    member this.Spec = this._spec

    member this.ParamMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 
        runParametersSeq 
        |> Seq.choose (this._spec.filter >> Option.map (this._spec.enhancer (this :> IRunHost)))

    interface IRunHost with
        member this.RunDb = this._projectDb
        member this.Run = this._run
        member this.GenSaveIntervals = this._genSaveIntervals
        member this.GenSaveSubIntervals = this._genSaveSubIntervals
        member this.QueryParamsFromRunParams = this._queryParamsFromRunParams
        member this.AllowOverwrite = this._spec.allowOverwrite
        member this.ParamMapRefiner rps = this.ParamMapRefiner rps
        member this.MaxParallel with get (): int = this._maxParallel



