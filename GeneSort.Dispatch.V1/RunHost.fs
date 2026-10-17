namespace GeneSort.Dispatch.V1

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Setup
open FSharp.UMX
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Project.V1
open GeneSort.Core
open GeneSort.Dispatch.V1.SorterSgd

module RunHost =
    /// Set independently on each machine; project/database identities remain unchanged.
    let projectsRoot () =
        match System.Environment.GetEnvironmentVariable("GENESORT_PROJECTS_ROOT") with
        | value when not (System.String.IsNullOrWhiteSpace value) -> System.IO.Path.GetFullPath value
        | _ when System.OperatingSystem.IsWindows() -> @"c:\Projects"
        | _ -> System.IO.Path.Combine(System.Environment.CurrentDirectory, "Projects")

    let makeDataFolder (run: run) =
        System.IO.Path.Combine(projectsRoot (), %run.ProjectName, %run.DatabaseName, "Data")
        |> UMX.tag<pathToRootFolder>

    let makeDatabase (run: run) : IGeneSortDb =
        new GeneSortDbMp(makeDataFolder run, run.QueryCatalogName)

type simpleRunHost = private {
    projectDb: IGeneSortDb
    run: simpleRun
    queryParamsFromRunParams: queryParamsBuilder
    maxParallel: int
    setup: setupFunction option
    executeFunction: executeFunction option
}

type sgdRunHost = private {
    simpleHost: simpleRunHost
    run: sgdRun
    genSaveIntervals: genIntervalConfig
    genSaveSubIntervals: genIntervalConfig
    makeSortableTests: sortableTestsMaker
    makeSeedPoolSet: seedSorterPoolSetMaker
    makeMutator: sorterModelMutatorMaker
} with
    member this.GenSaveIntervals = this.genSaveIntervals
    member this.GenSaveSubIntervals = this.genSaveSubIntervals
    member this.MakerNames = SgdRun.makerNames this.run
    member this.MakeSortableTests = this.makeSortableTests
    member this.MakeSeedPoolSet = this.makeSeedPoolSet
    member this.MakeMutator = this.makeMutator
    member this.ExecuteFunction = this.simpleHost.executeFunction.Value

type runHost =
    | SimpleRunHost of simpleRunHost
    | SgdRunHost of sgdRunHost

    static member Create (run: run) (maxParallel: int) =
        ProjectCatalogs.registerAll ()
        let db = RunHost.makeDatabase run

        let makeSimpleHost (simple: simpleRun) =
            { projectDb = db
              run = simple
              queryParamsFromRunParams =
                  QueryParamsCatalog.get
                      run.QueryCatalogName
                      run.ProjectName
                      run.DatabaseName
              maxParallel = maxParallel
              setup = None
              executeFunction = run.ExecuteFunctionName |> Option.map ExecuteFunctionCatalog.get }

        let lookupInterval name =
            match GenIntervalRegistry.genIntervalConfigsDict.TryGetValue name with
            | true, interval -> interval
            | _ -> failwithf "Sampling interval '%s' is not registered for run '%s'." name (%run.RunName)

        match run with
        | run.SimpleRun value -> SimpleRunHost (makeSimpleHost value)
        | run.SgdRun value ->
            let names = SgdRun.makerNames value
            let makeTests = SortableTestsMakerCatalog.get names.SortableTests
            let makeSeedPool = SeedSorterPoolSetMakerCatalog.get names.SeedPoolSet
            let makeMutator = MutatorMakerCatalog.get names.Mutator
            let simpleHost = makeSimpleHost (SgdRun.baseRun value)
            SgdRunHost {
                simpleHost = simpleHost
                run = value
                genSaveIntervals = SgdRun.genSaveIntervalsName value |> lookupInterval
                genSaveSubIntervals = SgdRun.genSaveSubIntervalsName value |> lookupInterval
                makeSortableTests = makeTests
                makeSeedPoolSet = makeSeedPool
                makeMutator = makeMutator
            }

    member this.RunDb =
        match this with
        | SimpleRunHost host -> host.projectDb
        | SgdRunHost host -> host.simpleHost.projectDb

    /// Uses an alternate storage implementation while preserving the resolved run configuration.
    member this.WithRunDb(db: IGeneSortDb) =
        match this with
        | SimpleRunHost host -> SimpleRunHost { host with projectDb = db }
        | SgdRunHost host -> SgdRunHost { host with simpleHost = { host.simpleHost with projectDb = db } }

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

    member this.ExecutionContext : executeContext = {
        RunDb = this.RunDb
        QueryParamsFromRunParams = this.QueryParamsFromRunParams
        MakeMutator = match this with SgdRunHost host -> Some host.MakeMutator | _ -> None
        GenSaveIntervals = this.GenSaveIntervals
        GenSaveSubIntervals = this.GenSaveSubIntervals
    }

    member this.ExecuteFunction : executeFunction =
        let configured =
            match this with
            | SimpleRunHost host -> host.executeFunction
            | SgdRunHost host -> host.simpleHost.executeFunction
        match configured with
        | Some execute -> execute
        | None -> fun _ _ _ _ _ -> async { return Error "Run must specify an ExecuteFunctionName before execution." }

    /// Bind storage and cancellation to the complete setup selected by the run.
    member this.WithSetupContext(context: setupContext) =
        let setup = SetupCatalog.get this.Run.SetupCatalogName context
        match this with
        | SimpleRunHost host -> SimpleRunHost { host with setup = Some setup }
        | SgdRunHost host -> SgdRunHost { host with simpleHost = { host.simpleHost with setup = Some setup } }

    member this.Setup with get() : setupFunction =
        let configured =
            match this with
            | SimpleRunHost host -> host.setup
            | SgdRunHost host -> host.simpleHost.setup
        match configured with
        | Some setup -> setup
        | None -> fun _ -> async { return Error "Run host setup is not bound. Call WithSetupContext before executing setup." }

    member this.MaxParallel =
        match this with
        | SimpleRunHost host -> host.maxParallel
        | SgdRunHost host -> host.simpleHost.maxParallel

    member this.ParamMapRefiner (runParametersSeq: runParameters seq) : runParameters seq =
        let filter = RunParamFilterBuilders.get this.Run.FilterCatalogName
        let enhancer = RunParamEnhancerBuilders.get this.Run.EnhancerCatalogName
        let context = runParamEnhancerContext.create this.QueryParamsFromRunParams this.Run
        runParametersSeq
        |> Seq.choose (filter >> Option.map (enhancer context))
