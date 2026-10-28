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
let ``Soss direct builder preserves existing IDs and separates both activator rates`` () =
    let configured, rows = sweep ()
    let direct = QueryParamsBuildersRun.SorterSgd.Msuf.Soss.makeQueryParams configured.ProjectName configured.DatabaseName
    let throughCatalog = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
    // IDs captured before moving identity completion out of the catalog.
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
    let expected = [|
        [|"31071ef8-a441-19eb-ae8f-e942d6872093"; "48a70e50-673a-f6d3-fbea-61f1d617821e"; "f3a2153d-04f2-9655-91d5-c82b50801d69"; "e9084dd9-5f1e-4877-1268-5b155e5b6e9d"; "c5fb32c7-1342-75dd-d4f1-ba6bae688edb"|]
        [|"259ec0c5-db77-d63d-fe95-9cb654c6d473"; "1adf6484-2848-f5c3-74ed-35763b9aa958"; "8da740d0-59b1-8fb8-e34d-fbf132ea3d58"; "2d329b51-a4a0-61bb-50de-76bdd64e6359"; "ed90df9a-fe82-0ebb-e459-27fa84ad10e4"|]
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
            Assert.Equal(Guid.Parse(expected[i][j]), UMX.untag query.Id)
            let path = OutputDataFile.getFullOutputDataFilePath (UMX.tag (Path.Combine(Path.GetTempPath(), "QueryTests", "Data"))) geneSortDataMode.Results query |> UMX.untag
            Assert.True(paths.Add path, "Different evoPaths must have distinct result filenames.")
    // Shared starting inputs are intentionally independent of the execution rate.
    Assert.Equal(rows[0].GetSetupSorterPoolSetId(), rows[1].GetSetupSorterPoolSetId())
