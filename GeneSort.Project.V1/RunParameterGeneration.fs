namespace GeneSort.Project.V1

open FSharp.UMX
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters

module RunParameterGeneration =
    /// Pair every run span combination with every enhanced setup combination within the same replica.
    let generateWithBindings
            (runQuery: runQueryParamsBuilder)
            (setupQuery: setupQueryParamsBuilder)
            (enhanceRun: runParamEnhancerBuilder)
            (enhanceSetup: setupEnhancerFunction)
            (configured: run) (minReplica: int<replNumber>) (maxReplica: int<replNumber>) : runParameters[] =
        let setupContext = { Run = configured; QueryParamsFromSetupParams = setupQuery }
        [| for replica in int minReplica .. int maxReplica - 1 do
               let repl = UMX.tag<replNumber> replica
               let next = UMX.tag<replNumber> (replica + 1)
               let runs = SimpleRun.makeRunParameters repl next id configured.BaseRun
               let setups = SimpleRun.makeSetupParameters repl next (Seq.map (enhanceSetup setupContext)) configured.BaseRun
               for setup in setups do
                   let references =
                       if configured.RunQueryCatalogName = QueryCatalogNamesRun.sorterEvalPrefix
                          || configured.RunEnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32 then []
                       else SetupOutputs.referencesFor configured setup
                   let context = runParamEnhancerContext.createForSetup runQuery configured setup
                   for parameters in runs do
                       let enhanced =
                           parameters
                           |> SetupFileReferences.attach references
                           |> enhanceRun context
                           |> SetupFileReferences.attach references
                       let query =
                           runQuery enhanced (geneSortDataType.Run configured.RunName)
                           |> Option.defaultWith (fun () -> invalidOp (sprintf "Run query '%s' could not build an identity for the enhanced parameters.\n%s"
                                                                           configured.RunQueryCatalogName (RunParameters.reportKvps enhanced)))
                       yield enhanced.WithId(Some query.Id) |]

    /// Resolve catalog functions for callers that do not have a run host.
    let generate (configured: run) (minReplica: int<replNumber>) (maxReplica: int<replNumber>) : runParameters[] =
        ProjectCatalogs.registerAll ()
        let runQuery = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
        let setupQuery = QueryParamsCatalogSetup.get configured.SetupQueryCatalogName configured.ProjectName configured.DatabaseName
        let enhanceRun = RunParamEnhancerBuilders.get configured.RunEnhancerFunctionName
        let enhanceSetup = SetupEnhancerCatalog.get configured.SetupEnhancerFunctionName
        generateWithBindings runQuery setupQuery enhanceRun enhanceSetup configured minReplica maxReplica
