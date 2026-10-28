namespace GeneSort.Dispatch.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Execution
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Catalogs.Setup.SorterSgd
open GeneSort.FileDb.V1

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

    let openDatabase project database queryCatalog : IGeneSortDb =
        let root = System.IO.Path.Combine(projectsRoot (), %project, %database, "Data") |> UMX.tag<pathToRootFolder>
        new GeneSortDbMp(root, queryCatalog)

    let makeDatabase (run: run) = openDatabase run.ProjectName run.DatabaseName run.RunQueryCatalogName

type simpleRunHost = private {
    storage: runStorage
    run: simpleRun
    queryParamsFromSetupParams: setupQueryParamsBuilder
    maxParallel: int
    setup: setupFunction option
    bindExecution: runStorage -> runFunction
    executeFunction: runFunction
    runEnhancer: runParamEnhancerBuilder
    setupEnhancer: setupEnhancerFunction
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
    member this.ExecuteFunction = this.simpleHost.executeFunction

type runHost =
    | SimpleRunHost of simpleRunHost
    | SgdRunHost of sgdRunHost

    static member Create (run: run) (maxParallel: int) =
        ProjectCatalogs.registerAll ()
        let db = RunHost.makeDatabase run
        let execution = ExecuteFunctionCatalog.get run.ExecuteFunctionName

        let makeSimpleHost (simple: simpleRun) bindExecution =
            let storage = runStorage.create {
                RunDb = db
                QueryParamsFromRunParams = QueryParamsCatalogRun.get run.RunQueryCatalogName run.ProjectName run.DatabaseName
            }
            { storage = storage
              run = simple
              queryParamsFromSetupParams =
                  if not run.HasSetup then fun _ _ -> None
                  else QueryParamsCatalogSetup.get
                        run.SetupQueryCatalogName 
                        run.ProjectName 
                        run.DatabaseName

              maxParallel = maxParallel
              setup = None
              bindExecution = bindExecution
              executeFunction = bindExecution storage
              runEnhancer = RunParamEnhancerBuilders.get run.RunEnhancerFunctionName
              setupEnhancer = if run.HasSetup then SetupEnhancerCatalog.get run.SetupEnhancerFunctionName else fun _ parameters -> parameters }

        let lookupInterval = RunIntervals.resolve run.RunName

        match run with
        | run.DirectRun value | run.PreparedRun value ->
            match execution with
            | SimpleExecution bind -> SimpleRunHost (makeSimpleHost value bind)
            | SgdExecution _ -> invalidArg "run" (sprintf "Execute function '%s' requires an SgdRun with a mutator and generation intervals." run.ExecuteFunctionName)
        | run.EvolutionRun value | run.ContinuationRun value ->
            let names = SgdRun.makerNames value
            let makeTests = SortableTestsMakerCatalog.get names.SortableTests
            let makeSeedPool = SeedSorterPoolSetMakerCatalog.get names.SeedPoolSet
            let makeMutator = MutatorMakerCatalog.get names.Mutator
            let dependencies = sgdExecution.create {
                MakeMutator = makeMutator
                GenSaveIntervals = SgdRun.genSaveIntervalsName value |> lookupInterval
                GenSaveSubIntervals = SgdRun.genSaveSubIntervalsName value |> lookupInterval
            }
            let bind =
                match execution with
                | SimpleExecution bind -> bind
                | SgdExecution bind -> fun storage ->
                    let execute = bind storage dependencies
                    fun parameters overwrite cts progress -> async {
                        try
                            let path, state = EvoPath.fromParameters run parameters
                            let! result = execute path state overwrite cts progress
                            return result |> Result.map (EvoPath.toParameters path)
                        with error -> return Error error.Message
                    }
            let simpleHost = makeSimpleHost (SgdRun.baseRun value) bind
            SgdRunHost {
                simpleHost = simpleHost
                run = value
                genSaveIntervals = dependencies.GenSaveIntervals
                genSaveSubIntervals = dependencies.GenSaveSubIntervals
                makeSortableTests = makeTests
                makeSeedPoolSet = makeSeedPool
                makeMutator = makeMutator
            }

    member this.Storage =
        match this with
        | SimpleRunHost host -> host.storage
        | SgdRunHost host -> host.simpleHost.storage

    member this.RunDb = this.Storage.RunDb

    /// Uses an alternate storage implementation while preserving the resolved run configuration.
    member this.WithRunDb(db: IGeneSortDb) =
        let replace (host: simpleRunHost) =
            let storage = runStorage.create { RunDb = db; QueryParamsFromRunParams = host.storage.QueryParamsFromRunParams }
            { host with storage = storage; executeFunction = host.bindExecution storage }
        match this with
        | SimpleRunHost host -> SimpleRunHost (replace host)
        | SgdRunHost host -> SgdRunHost { host with simpleHost = replace host.simpleHost }

    member this.Run =
        match this with
        | SimpleRunHost host -> Run.ofSimple host.run
        | SgdRunHost host -> Run.ofSgd host.run

    member this.QueryParamsFromRunParams =
        this.Storage.QueryParamsFromRunParams

    member this.QueryParamsFromSetupParams =
        match this with
        | SimpleRunHost host -> host.queryParamsFromSetupParams
        | SgdRunHost host -> host.simpleHost.queryParamsFromSetupParams

    member this.GenSaveIntervals =
        match this with
        | SimpleRunHost _ -> None
        | SgdRunHost host -> Some host.GenSaveIntervals

    member this.GenSaveSubIntervals =
        match this with
        | SimpleRunHost _ -> None
        | SgdRunHost host -> Some host.GenSaveSubIntervals

    member this.AllowOverwrite = this.Run.AllowOverwrite |> UMX.tag<allowOverwrite>

    member this.ExecuteFunction : runFunction =
        match this with
        | SimpleRunHost host -> host.executeFunction
        | SgdRunHost host -> host.simpleHost.executeFunction

    /// Bind storage and cancellation to the complete setup selected by the run.
    member this.WithSetupContext(context: setupContext) =
        let setup = SetupCatalog.get (Run.requireSetupFunctionName this.Run) context
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

    member this.RunEnhancer : runParamEnhancerBuilder =
        match this with
        | SimpleRunHost host -> host.runEnhancer
        | SgdRunHost host -> host.simpleHost.runEnhancer

    member this.SetupEnhancer : setupEnhancerFunction =
        match this with
        | SimpleRunHost host -> host.setupEnhancer
        | SgdRunHost host -> host.simpleHost.setupEnhancer

    member this.ParamMapRefiner (runParametersSeq: runParameters seq) : runParameters seq =
        let context = runParamEnhancerContext.create this.QueryParamsFromRunParams this.Run
        runParametersSeq
        |> Seq.map (this.RunEnhancer context)

    member this.SetupParamMapRefiner (parameters: setupParameters seq) : setupParameters seq =
        let context = { Run = this.Run; QueryParamsFromSetupParams = this.QueryParamsFromSetupParams }
        parameters |> Seq.map (this.SetupEnhancer context)

    member this.MakeRunParameters minReplica maxReplica =
        RunParameterGeneration.generateWithBindingsAndSourceSelection
            (EvoCondenseSelection.select RunHost.makeDatabase)
            this.QueryParamsFromRunParams this.QueryParamsFromSetupParams
            this.RunEnhancer this.SetupEnhancer this.Run minReplica maxReplica

    member this.MakeSetupParameters minReplica maxReplica =
        if not this.Run.HasSetup then [||]
        else SimpleRun.makeSetupParameters minReplica maxReplica this.SetupParamMapRefiner this.Run.BaseRun
