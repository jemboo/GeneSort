namespace GeneSort.Dispatch.V1

open FSharp.UMX
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Project.V1
open GeneSort.Core

module RunHost =
    let makeDataFolder (run: run) =
        @$"c:\Projects\{%run.ProjectName}\{%run.DatabaseName}\Data"
        |> UMX.tag<pathToRootFolder>

    let makeDatabase (run: run) : IGeneSortDb =
        new GeneSortDbMp(makeDataFolder run, run.QueryCatalogName)

type simpleRunHost = private {
    projectDb: IGeneSortDb
    run: simpleRun
    queryParamsFromRunParams: queryParamsBuilder
    maxParallel: int
}

type sgdRunHost = private {
    simpleHost: simpleRunHost
    run: sgdRun
    genSaveIntervals: genIntervalConfig
    genSaveSubIntervals: genIntervalConfig
} with
    member this.GenSaveIntervals = this.genSaveIntervals
    member this.GenSaveSubIntervals = this.genSaveSubIntervals

type runHost =
    | SimpleRunHost of simpleRunHost
    | SgdRunHost of sgdRunHost

    static member Create (run: run) (maxParallel: int) =
        let db = RunHost.makeDatabase run

        let makeSimpleHost (simple: simpleRun) =
            { projectDb = db
              run = simple
              queryParamsFromRunParams =
                  QueryParamsCatalog.get
                      run.QueryCatalogName
                      run.ProjectName
                      run.DatabaseName
              maxParallel = maxParallel }

        let lookupInterval name =
            match GenIntervalRegistry.genIntervalConfigsDict.TryGetValue name with
            | true, interval -> interval
            | _ -> failwithf "Sampling interval '%s' is not registered for run '%s'." name (%run.RunName)

        match run with
        | run.SimpleRun value -> SimpleRunHost (makeSimpleHost value)
        | run.SgdRun value ->
            let simpleHost = makeSimpleHost (SgdRun.baseRun value)
            SgdRunHost {
                simpleHost = simpleHost
                run = value
                genSaveIntervals = SgdRun.genSaveIntervalsName value |> lookupInterval
                genSaveSubIntervals = SgdRun.genSaveSubIntervalsName value |> lookupInterval
            }

    member this.RunDb =
        match this with
        | SimpleRunHost host -> host.projectDb
        | SgdRunHost host -> host.simpleHost.projectDb

    member this.Run =
        match this with
        | SimpleRunHost host -> run.SimpleRun host.run
        | SgdRunHost host -> run.SgdRun host.run

    member this.QueryParamsFromRunParams =
        match this with
        | SimpleRunHost host -> host.queryParamsFromRunParams
        | SgdRunHost host -> host.simpleHost.queryParamsFromRunParams

    member this.GenSaveIntervals =
        match this with
        | SimpleRunHost _ -> None
        | SgdRunHost host -> Some host.GenSaveIntervals

    member this.GenSaveSubIntervals =
        match this with
        | SimpleRunHost _ -> None
        | SgdRunHost host -> Some host.GenSaveSubIntervals

    member this.AllowOverwrite = this.Run.AllowOverwrite |> UMX.tag<allowOverwrite>

    member this.MaxParallel =
        match this with
        | SimpleRunHost host -> host.maxParallel
        | SgdRunHost host -> host.simpleHost.maxParallel

    member this.ParamMapRefiner (runParametersSeq: runParameters seq) : runParameters seq =
        let filter = RunParamFilterBuilders.get this.Run.FilterCatalogName
        let enhancer = RunParamEnhancerBuilders.get this.Run.EnhancerCatalogName
        let context = { QueryParamsFromRunParams = this.QueryParamsFromRunParams; Run = this.Run }
        runParametersSeq
        |> Seq.choose (filter >> Option.map (enhancer context))
