module GeneSort.Dispatch.Test.V1.RunQueryBuilderTests

open System
open System.IO
open FSharp.UMX
open Xunit
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.Mp.V1
open GeneSort.FileDb.V1

let private sweep () =
    let dto = WorkflowFixtures.sossSource [0] |> RunDto.fromDomain
    let configured =
        { dto with
            SetupEnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32S
            RunEnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32S
            SetupParameterSpans = Array.append dto.SetupParameterSpans [|{ Key = "ActivationProbability"; Values = [|"0.95"|] }|]
            RunParameterSpans = Array.append dto.RunParameterSpans [|{ Key = "ActivatorMutationRate"; Values = [|"0"; "0.01"|] }|]
        } |> RunDto.toDomain
    configured, RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>

[<Fact>]
let ``every catalog name returns exactly its standalone builder`` () =
    ProjectCatalogs.registerAll ()
    for name, make in QueryParamsBuildersRun.registrations do
        Assert.Same(make, QueryParamsCatalogRun.get name)
    for name, make in QueryParamsBuildersSetup.registrations do
        Assert.Same(make, QueryParamsCatalogSetup.get name)

[<Fact>]
let ``catalog does not add identity fields to a custom builder`` () =
    let name = "query-no-decoration-" + Guid.NewGuid().ToString("N")
    let make project database (rp: runParameters) output =
        Some (queryParams.create database project (rp.GetRepl()) None output [|"OnlyField", "preserved"|])
    QueryParamsCatalogRun.register name make
    let rp = (runParameters.create Map.empty).WithRepl(Some 0<replNumber>).WithActivatorMutationRate(Some 0.01)
    let query = QueryParamsCatalogRun.get name (UMX.tag "P") (UMX.tag "D") rp (geneSortDataType.SorterPoolSet "") |> Option.get
    Assert.Equal(1, query.Properties.Count)
    Assert.False(query.Properties.ContainsKey ParamKeys.activatorMutationRateKey)

[<Fact>]
let ``setup catalog returns custom builder output without stripping or adding properties`` () =
    let name = "setup-query-no-decoration-" + Guid.NewGuid().ToString("N")
    let make project database (sp: setupParameters) output =
        Some (queryParams.create database project (sp.GetRepl()) None output (Map.toArray sp.ParamMap))
    QueryParamsCatalogSetup.register name make
    let sp = setupParameters.create (Map.ofList [ParamKeys.setupSorterModelSetIdKey, string (Guid.NewGuid()); "OnlyField", "preserved"])
    let direct = make (UMX.tag "P") (UMX.tag "D") sp (geneSortDataType.SorterModelSet "")
    Assert.Equal(direct, QueryParamsCatalogSetup.get name (UMX.tag "P") (UMX.tag "D") sp (geneSortDataType.SorterModelSet ""))

[<Fact>]
let ``Soss Setup constructors share inputs while Run constructors separate activator rates`` () =
    let configured, rows = sweep ()
    let make = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
    for output in [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""] do
        let requests = rows |> Array.map (fun rp -> make rp output |> Option.get)
        Assert.NotEqual(requests[0].Id, requests[1].Id)
        let stored =
            Array.map2 (fun (rp: runParameters) request ->
                let setup = QueryParamsCatalogSetup.get configured.SetupQueryCatalogName configured.ProjectName configured.DatabaseName
                let resolved = setup (setupParameters.create rp.ParamMap) output |> Option.get
                Assert.Equal(rp.ParamMap[SetupFileReferences.keyForOutputType output], string resolved.Id)
                resolved.Id) rows requests
        Assert.Equal(stored[0], stored[1])

[<Fact>]
let ``Soss direct builder matches its catalog and separates both activator rates`` () =
    let configured, rows = sweep ()
    let direct = QueryParamsBuildersRun.SorterSgd.Msuf.Soss.makeQueryParams configured.ProjectName configured.DatabaseName
    let throughCatalog = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
    let outputs = [|
        geneSortDataType.RunParameters configured.RunName
        geneSortDataType.SorterPoolSet ""
        geneSortDataType.SorterPoolSetSummarySet ""
        geneSortDataType.SorterPoolBinsSetSeries ""
        geneSortDataType.TextReport (UMX.tag "SummaryReport")
    |]
    // Each file recipe can be called and edited independently of the catalog adapter.
    let constructors = [|
        fun rp -> QueryParamsBuildersRun.SorterSgd.Msuf.Soss.makeRunParametersQueryParams configured.ProjectName configured.DatabaseName rp configured.RunName
        fun rp -> QueryParamsBuildersRun.SorterSgd.Msuf.Soss.makeSorterPoolSetQueryParams configured.ProjectName configured.DatabaseName rp ""
        fun rp -> QueryParamsBuildersRun.SorterSgd.Msuf.Soss.makeSorterPoolSetSummarySetQueryParams configured.ProjectName configured.DatabaseName rp ""
        fun rp -> QueryParamsBuildersRun.SorterSgd.Msuf.Soss.makeSorterPoolBinsSetSeriesQueryParams configured.ProjectName configured.DatabaseName rp ""
        fun rp -> QueryParamsBuildersRun.SorterSgd.Msuf.Soss.makeTextReportQueryParams configured.ProjectName configured.DatabaseName rp (UMX.tag "SummaryReport")
    |]
    Assert.Equal(2, rows.Length)
    let paths = Collections.Generic.HashSet<string>()
    for i in 0 .. rows.Length - 1 do
        let row = rows[i].WithGenerationCurrent(Some 100<generationNumber>)
        for j in 0 .. outputs.Length - 1 do
            let query = constructors[j] row |> Option.get
            Assert.Equal(direct row outputs[j] |> Option.get, query)
            Assert.Equal(throughCatalog row outputs[j] |> Option.get, query)
            Assert.Equal(row.ParamMap[ParamKeys.activatorMutationRateKey], query.Properties[ParamKeys.activatorMutationRateKey])
            let path = OutputDataFile.getFullOutputDataFilePath (UMX.tag (Path.Combine(Path.GetTempPath(), "QueryTests", "Data"))) geneSortDataMode.Results query |> UMX.untag
            Assert.True(paths.Add path, "Different evoPaths must have distinct result filenames.")
    // Shared starting inputs are intentionally independent of the execution rate.
    Assert.Equal(rows[0].GetSetupSorterPoolSetId(), rows[1].GetSetupSorterPoolSetId())

// Exercise inheritance independently of query recipes so every input field affects identity.
let private enhancerContext setup =
    let configured = WorkflowFixtures.sossSource [0]
    let query (rp: runParameters) output =
        Some (queryParams.create configured.DatabaseName configured.ProjectName (rp.GetRepl()) None output (Map.toArray rp.ParamMap))
    runParamEnhancerContext.createForSetup query configured (setupParameters.create setup)

[<Fact>]
let ``equivalent prepared enhancers preserve execution overrides and exclude setup references`` () =
    let context = enhancerContext (Map.ofList ["Inherited", "setup"; "Override", "setup"; ParamKeys.setupSorterModelSetIdKey, string (Guid.NewGuid())])
    let parameters = runParameters.create (Map.ofList ["Override", "execution"])
    let results =
        [RunParamEnhancerBuilders.sorterEvalStandard; RunParamEnhancerBuilders.sorterEvalMerge;
         RunParamEnhancerBuilders.sorterMutateStandard; RunParamEnhancerBuilders.sorterMutateStandardFormat]
        |> List.map (fun enhance -> (enhance context parameters).ParamMap)
    for result in results do
        Assert.Equal<Map<string, string>>(results.Head, result)
        Assert.Equal("setup", result["Inherited"])
        Assert.Equal("execution", result["Override"])
        Assert.False(result.ContainsKey ParamKeys.setupSorterModelSetIdKey)
    let direct = RunParamEnhancerBuilders.sortableTests context parameters
    Assert.False(direct.ParamMap.ContainsKey "Inherited")

[<Fact>]
let ``prefix enhancer retains strict setup precedence and missing field validation`` () =
    let keys = [ParamKeys.rngTypeKey; ParamKeys.prefixLibIdKey; ParamKeys.simpleSorterModelTypeKey;
                ParamKeys.sortableDataFormatKey; ParamKeys.sorterCountKey; ParamKeys.aMaxKey]
    let setup = keys |> List.map (fun key -> key, "setup") |> Map.ofList
    let parameters = keys |> List.map (fun key -> key, "execution") |> Map.ofList |> runParameters.create
    let result = RunParamEnhancerBuilders.sorterEvalPrefix (enhancerContext setup) parameters
    for key in keys do Assert.Equal("setup", result.ParamMap[key])
    let error = Assert.Throws<InvalidOperationException>(fun () ->
        RunParamEnhancerBuilders.sorterEvalPrefix (enhancerContext (Map.remove ParamKeys.rngTypeKey setup)) parameters |> ignore)
    Assert.Contains(ParamKeys.rngTypeKey, error.Message)

[<Fact>]
let ``equivalent SGD continuation enhancers preserve generation and validate pool sizes`` () =
    let configured = WorkflowFixtures.sossSource [0]
    let parameters = (RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>)[0]
    let parameters = parameters.WithGenerationCurrent(Some 42<generationNumber>)
    let context = enhancerContext Map.empty
    let enhancers =
        [RunParamEnhancerBuilders.Sgd.msrs32MutationRate; RunParamEnhancerBuilders.Sgd.msrs32MutationRateMax;
         RunParamEnhancerBuilders.Sgd.msuf624p3bMutationRate; RunParamEnhancerBuilders.Sgd.msrs24p3aOrthoPara;
         RunParamEnhancerBuilders.Sgd.msrs24p3aPoolModComp; RunParamEnhancerBuilders.Sgd.msrs24p3bOrthoPara;
         RunParamEnhancerBuilders.Sgd.mssi24p3bOrthoPara]
    let expected = (enhancers.Head context parameters).ParamMap
    for enhance in enhancers do
        let result = enhance context parameters
        Assert.Equal<Map<string, string>>(expected, result.ParamMap)
        Assert.Equal(Some 42<generationNumber>, result.GetGenerationCurrent())
        let invalid = parameters.ParamMap |> Map.add ParamKeys.sorterCountPerPoolKey "2" |> Map.add ParamKeys.sorterCountPerPoolSetKey "3" |> runParameters.create
        Assert.Throws<ArgumentException>(fun () -> enhance context invalid |> ignore) |> ignore
        for key in [ParamKeys.sorterCountPerPoolKey; ParamKeys.sorterCountPerPoolSetKey] do
            for value in ["0"; "-1"; "1.5"; "not-a-number"; "2147483648"] do
                let invalid = parameters.ParamMap |> Map.add key value |> runParameters.create
                let error = Assert.Throws<ArgumentException>(fun () -> enhance context invalid |> ignore)
                Assert.Contains(key, error.Message)
                Assert.Contains("Positive", error.Message)
            let missing = parameters.ParamMap |> Map.remove key |> runParameters.create
            let error = Assert.Throws<ArgumentException>(fun () -> enhance context missing |> ignore)
            Assert.Contains(key, error.Message)
            Assert.Contains("required", error.Message)
    let reset = RunParamEnhancerBuilders.Sgd.msuf32FromSetup context parameters
    Assert.Equal(Some 0<generationNumber>, reset.GetGenerationCurrent())



[<Fact>]
let ``replica alone changes parameter and result filename hashes`` () =
    ProjectCatalogs.registerAll ()
    let configurations = [
        WorkflowFixtures.standardSource ()
        WorkflowFixtures.sossSource [0]
        WorkflowFixtures.prefixEvaluation 16
        GeneSort.Dispatch.V1.SorterEval.Standard.Channels16.Test ()
    ]
    for configured in configurations do
        let original = (RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>)[0]
        // Retain every property, including setup references: only the replica changes.
        let replica = original.WithRepl(Some 1<replNumber>)
        let make = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
        let outputs = [
            geneSortDataType.RunParameters configured.RunName
            geneSortDataType.SorterPoolSet ""
            geneSortDataType.SorterSetEval ""
            geneSortDataType.TextReport (UMX.tag "SummaryReport")
        ]
        Assert.True((make original outputs.Head).IsSome)
        for output in outputs do
            match make original output with
            | None -> () // Not every workflow produces every result type.
            | Some first ->
                let second = make replica output |> Option.get
                Assert.Equal<Map<string, string>>(first.Properties, second.Properties)
                Assert.Equal(Some 0<replNumber>, first.Repl)
                Assert.Equal(Some 1<replNumber>, second.Repl)
                Assert.NotEqual(first.Id, second.Id)
                Assert.Equal(first.Id, (make original output |> Option.get).Id)
                let filename query =
                    OutputDataFile.getFullOutputDataFilePath (UMX.tag "ReplicaIdentityTests") geneSortDataMode.Results query
                    |> UMX.untag |> Path.GetFileName
                Assert.NotEqual<string>(filename first, filename second)
