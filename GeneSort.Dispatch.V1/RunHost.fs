namespace GeneSort.Dispatch.V1

open FSharp.UMX
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Core

type runHostSpec = {
    queryCatalogName: string
    databaseName: string<databaseName>
    runName: string<runName>
    runDescription: string
    spans: (string * string list) list
    filterCatalogName: string
    enhancerCatalogName: string
    allowOverwrite: bool<allowOverwrite>
    maxParallel: int
}

and simpleRunHost = private {
    projectDb: IGeneSortDb
    spec: runHostSpec
    run: simpleRun
    queryParamsFromRunParams: queryParamsBuilder
    maxParallel: int
}

and sgdRunHost = private {
    simpleHost: simpleRunHost
    run: sgdRun
    genSaveIntervals: genIntervalConfig
    genSaveSubIntervals: genIntervalConfig
} with
    member this.GenSaveIntervals = this.genSaveIntervals
    member this.GenSaveSubIntervals = this.genSaveSubIntervals

and runHost =
    | SimpleRunHost of simpleRunHost
    | SgdRunHost of sgdRunHost

    static member Create (db: IGeneSortDb) (spec: runHostSpec) (run: run) =
        let makeSimpleHost (simple: simpleRun) =
            { projectDb = db
              spec = spec
              run = simple
              queryParamsFromRunParams = QueryParamsCatalog.get spec.queryCatalogName (SimpleRun.projectName simple) (SimpleRun.databaseName simple)
              maxParallel = spec.maxParallel }

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

    member this.Spec =
        match this with
        | SimpleRunHost host -> host.spec
        | SgdRunHost host -> host.simpleHost.spec

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

    member this.AllowOverwrite = this.Spec.allowOverwrite

    member this.MaxParallel =
        match this with
        | SimpleRunHost host -> host.maxParallel
        | SgdRunHost host -> host.simpleHost.maxParallel

    member this.ParamMapRefiner (runParametersSeq: runParameters seq) : runParameters seq =
        let filter = RunParamFilterBuilders.get this.Spec.filterCatalogName
        let enhancer = RunParamEnhancerBuilders.get this.Spec.enhancerCatalogName
        let context = { QueryParamsFromRunParams = this.QueryParamsFromRunParams; Run = this.Run }
        runParametersSeq
        |> Seq.choose (filter >> Option.map (enhancer context))

