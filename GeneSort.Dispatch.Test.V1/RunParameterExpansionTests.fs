module GeneSort.Dispatch.Test.V1.RunParameterExpansionTests

open System
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SortableTests
open GeneSort.Dispatch.V1.SorterEval

[<Fact>]
let ``prefix evaluation combines split spans with reusable setup references`` () =
    for configured in [WorkflowFixtures.prefixEvaluation 24; WorkflowFixtures.prefixEvaluation 32] do
        let host = runHost.Create configured 1
        let setups = host.MakeSetupParameters 0<replNumber> 1<replNumber>
        let runs = host.MakeRunParameters 0<replNumber> 1<replNumber>
        Assert.NotEmpty(setups)
        Assert.Equal(setups.Length, runs.Length)
        for setup, parameters in Array.zip setups runs do
            Assert.True(setup.GetSorterEvalType().IsNone)
            Assert.Equal(Some sorterEvalType.V2, parameters.GetSorterEvalType())
            for key in [ParamKeys.rngTypeKey; ParamKeys.prefixLibIdKey; ParamKeys.simpleSorterModelTypeKey;
                        ParamKeys.sortableDataFormatKey; ParamKeys.sorterCountKey] do
                Assert.Equal(setup.ParamMap.[key], parameters.ParamMap.[key])
            Assert.Equal(Some (setup.GetPrefixLibId().Value.ChannelCount), parameters.GetChannelCount())
            Assert.NotEmpty(SetupFileReferences.fromSetupParameters setup)
            Assert.NotEmpty(SetupFileReferences.fromRunParameters parameters)
            for outputType in [geneSortDataType.SorterModelSet ""; geneSortDataType.SortableTestsWithPrefix ""] do
                let prepared = host.QueryParamsFromSetupParams setup outputType |> Option.get
                let loaded = SetupInputReferences.tryRecordedInput configured.ProjectName configured.DatabaseName parameters outputType |> Option.get
                let alternate = host.QueryParamsFromSetupParams (setup.WithSorterEvalType(Some sorterEvalType.V1)) outputType |> Option.get
                Assert.Equal(prepared.Id, loaded.Id)
                Assert.Equal(loaded.Id, alternate.Id)
                Assert.False(loaded.Properties.ContainsKey ParamKeys.sorterEvalTypeKey)

[<Fact>]
let ``prefix evaluation identities include count and evaluation method but exclude file IDs`` () =
    let host = runHost.Create (WorkflowFixtures.prefixEvaluation 24) 1
    let parameters = host.MakeRunParameters 0<replNumber> 1<replNumber> |> Array.head
    let outputType = geneSortDataType.SorterSetEval ""
    let query rp = host.QueryParamsFromRunParams rp outputType |> Option.get
    let baseline = query parameters
    Assert.NotEqual(baseline.Id, (query (parameters.WithSorterCount(Some 1<sorterCount>))).Id)
    Assert.NotEqual(baseline.Id, (query (parameters.WithSorterEvalType(Some sorterEvalType.V1))).Id)
    let linked = parameters.WithSetupSorterModelSetId(Some (UMX.tag (Guid.NewGuid())))
                           .WithSetupParametersId(Some (UMX.tag (Guid.NewGuid())))
    Assert.Equal(baseline.Id, (query linked).Id)
    let sourceQuery =
        SorterEvalDbs.Prefix.makeQueryParams (parameters.GetRepl().Value) (parameters.GetRngType().Value)
            (parameters.GetPrefixLibId().Value) (parameters.GetSimpleSorterModelType().Value)
            (parameters.GetSortableDataFormat().Value) (parameters.GetSorterCount().Value)
            (parameters.GetSorterEvalType().Value) outputType
    Assert.Equal(baseline.Id, sourceQuery.Id)

[<Theory>]
[<InlineData(false)>]
[<InlineData(true)>]
let ``standalone test Results use the same identity for generation and downstream reads`` prefix =
    let configured =
        if prefix then WorkflowFixtures.sortableTestsRun true
        else WorkflowFixtures.sortableTestsRun false
    let rp = RunParameterGeneration.generate configured 0<replNumber> 1<replNumber> |> Array.head
    Assert.True(rp.GetSetupParametersId().IsSome)
    let output = geneSortDataType.SortableTests ""
    let generated =
        QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName rp output
        |> Option.get
    let requested =
        if prefix then
            SortableTestsDbs.Prefix.makePrefixQueryParams (rp.GetRepl().Value) (rp.GetPrefixLibId().Value) (rp.GetSortableDataFormat().Value) output
        else
            SortableTestsDbs.Merge.makeMergeQueryParams (rp.GetRepl().Value) (rp.GetMergeLibId().Value) (rp.GetSortableDataFormat().Value) output
    Assert.Equal(requested.Id, generated.Id)
    Assert.False(generated.Properties.ContainsKey ParamKeys.setupParametersIdKey)
    let runQuery =
        QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName rp (geneSortDataType.Run configured.RunName)
        |> Option.get
    Assert.True(runQuery.Properties.ContainsKey ParamKeys.setupParametersIdKey)

let private configure runValues setupValues outputTypes setupQuery =
    let name = Guid.NewGuid().ToString("N")
    let setupCalls = ref 0
    SetupParametersEnhancerCatalog.register name (fun context sp ->
        setupCalls.Value <- setupCalls.Value + 1
        sp.WithGenerationCurrent(Some (UMX.tag 12))
        |> SetupParametersEnhancers.withFileReferences outputTypes context)
    QueryParamsCatalogSetup.register name setupQuery
    QueryParamsCatalogRun.register (name + ".run") (fun project database rp outputType ->
        // A registered builder owns its full identity; the catalog adds nothing.
        queryParams.create database project (rp.GetRepl()) None outputType [|"R", rp.ParamMap.["R"]|]
        |> QueryParamsBuildersRun.withSetupReferences rp
        |> Some)
    RunParametersEnhancerCatalog.register name (fun context rp ->
        let setup = context.SetupParameters |> Option.defaultWith (fun () -> failwith "Missing associated setup")
        Assert.Equal(setup.GetRepl(), rp.GetRepl())
        Assert.True(rp.GetSetupParametersId().IsSome)
        let enhanced = rp.ParamMap |> Map.add "SeenSetup" setup.ParamMap.["S"] |> runParameters.create
        enhanced.WithRunName(Some context.Run.RunName)) |> ignore
    let configured =
        SimpleRun.create {
            DatabaseName = UMX.tag "db"
            ProjectName = UMX.tag "project"
            RunName = UMX.tag "run"
            Description = "Expansion test"
            AllowOverwrite = false
            Execution = {
                FunctionName = "unused"
                ParameterSpans = ["R", runValues]
                QueryCatalogName = name + ".run"
                ParametersEnhancerName = name
            }
            Setup = {
                FunctionName = "unused"
                ParameterSpans = ["S", setupValues]
                QueryCatalogName = name
                ParametersEnhancerName = name
            }
        } |> Run.ofSimple
    configured, setupCalls

let private query project database (sp: setupParameters) outputType =
    Some (queryParams.create database project (sp.GetRepl()) (sp.GetGenerationCurrent()) outputType [|"S", sp.ParamMap.["S"]|])

[<Fact>]
let ``Cartesian expansion links every output to the matching enhanced setup within each replica`` () =
    let outputs = [geneSortDataType.SorterModelSet "models"; geneSortDataType.SortableTestsWithPrefix "tests"]
    let configured, calls = configure ["a"; "b"; "c"] ["one"; "two"] outputs query
    let generated = RunParameterGeneration.generate configured 2<replNumber> 4<replNumber>
    Assert.Equal(12, generated.Length)
    Assert.Equal(4, calls.Value)
    Assert.Equal(12, generated |> Array.map (fun rp -> rp.GetId().Value) |> Array.distinct |> Array.length)
    for rp in generated do
        let sp = (setupParameters.create (Map.ofList ["S", rp.ParamMap.["SeenSetup"]])).WithRepl(rp.GetRepl()).WithGenerationCurrent(Some (UMX.tag 12))
        let expected outputType = query configured.ProjectName configured.DatabaseName sp outputType |> Option.get |> fun q -> q.Id
        Assert.Equal(Some (expected outputs.[0]), rp.GetSetupSorterModelSetId())
        Assert.Equal(Some (expected outputs.[1]), rp.GetSetupSortableTestsWithPrefixId())
        Assert.False(rp.ParamMap.ContainsKey "S")
    for _, group in generated |> Array.groupBy (fun rp -> rp.GetRepl(), rp.ParamMap.["SeenSetup"]) do
        Assert.Equal<string>([|"a"; "b"; "c"|], group |> Array.map (fun rp -> rp.ParamMap.["R"]))
        Assert.Single(group |> Array.map (fun rp -> rp.GetSetupParametersId()) |> Array.distinct) |> ignore
    let builder = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
    let savedIds = generated |> Array.map (fun rp -> (builder rp (geneSortDataType.RunParameters configured.RunName) |> Option.get).Id)
    Assert.Equal(12, savedIds |> Array.distinct |> Array.length)

[<Fact>]
let ``setup association keeps runs distinct even when there are no output files`` () =
    let configured, _ = configure ["a"] ["one"; "two"] [] (fun _ _ _ _ -> failwith "No output queries expected")
    let generated = RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>
    Assert.Equal(2, generated.Length)
    Assert.NotEqual(generated.[0].GetId(), generated.[1].GetId())
    Assert.All(generated, fun rp -> Assert.True(rp.GetSetupSorterModelSetId().IsNone))

[<Fact>]
let ``an empty span produces an empty product`` () =
    for runs, setups in [[], ["one"]; ["a"], []] do
        let configured, _ = configure runs setups [] query
        Assert.Empty(RunParameterGeneration.generate configured 0<replNumber> 1<replNumber>)

[<Fact>]
let ``missing setup queries and conflicting output keys fail explicitly`` () =
    let configured, _ = configure ["a"] ["one"] [geneSortDataType.SorterModelSet ""] (fun _ _ _ _ -> None)
    Assert.Throws<InvalidOperationException>(fun () -> RunParameterGeneration.generate configured 0<replNumber> 1<replNumber> |> ignore) |> ignore
    let conflicting, _ = configure ["a"] ["one"] [geneSortDataType.SorterModelSet "a"; geneSortDataType.SorterModelSet "b"] query
    Assert.Throws<ArgumentException>(fun () -> RunParameterGeneration.generate conflicting 0<replNumber> 1<replNumber> |> ignore) |> ignore

[<Fact>]
let ``source file IDs affect run identities but not setup identities`` () =
    let configured, _ = configure ["a"] ["one"] [] (fun project database sp outputType ->
        let settings = SetupFileReferences.withoutReferences sp
        Some (queryParams.create database project (sp.GetRepl()) None outputType (Map.toArray settings.ParamMap)))
    let setupQuery = QueryParamsCatalogSetup.get configured.SetupQueryCatalogName configured.ProjectName configured.DatabaseName
    let runQuery = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
    let first = Guid.NewGuid() |> UMX.tag<queryParamsId>
    let second = Guid.NewGuid() |> UMX.tag<queryParamsId>
    let input = (setupParameters.create (Map.ofList ["S", "one"])).WithRepl(Some 0<replNumber>)
    let outputType = geneSortDataType.SorterModelSet ""
    let baseline = setupQuery input outputType |> Option.get
    for source in [first; second] do
        let linked = input.WithSetupSorterModelSetId(Some source)
        let actual = setupQuery linked outputType |> Option.get
        Assert.Equal(baseline.Id, actual.Id)
        Assert.False(actual.Properties.ContainsKey ParamKeys.setupSorterModelSetIdKey)
        Assert.Equal(Some source, linked.GetSetupSorterModelSetId())
    let generate source =
        RunParameterGeneration.generateWithBindings runQuery setupQuery (fun _ rp -> rp)
            (fun _ sp -> sp.WithSetupSorterModelSetId(Some source)) configured 0<replNumber> 1<replNumber>
        |> Array.exactlyOne
    let a, b = generate first, generate second
    Assert.Equal(a.GetSetupParametersId(), b.GetSetupParametersId())
    Assert.Equal(Some first, a.GetSetupSorterModelSetId())
    Assert.Equal(Some second, b.GetSetupSorterModelSetId())
    Assert.NotEqual(a.GetId(), b.GetId())

[<Fact>]
let ``built-in setup enhancers populate their required file IDs`` () =
    let configured, _ = configure ["a"] ["one"] [] query
    let setupQuery = QueryParamsCatalogSetup.get configured.SetupQueryCatalogName configured.ProjectName configured.DatabaseName
    let context = { Run = configured; QueryParamsFromSetupParams = setupQuery }
    let input = (setupParameters.create (Map.ofList ["S", "one"])).WithRepl(Some 0<replNumber>)
    for enhancer, pool in [SetupParametersEnhancers.sorterEval, false; SetupParametersEnhancers.sorterMutate, false; SetupParametersEnhancers.sorterSgd, true] do
        let enhanced = enhancer context input
        let expected outputType = setupQuery enhanced outputType |> Option.get |> fun q -> Some q.Id
        Assert.Equal(expected (geneSortDataType.SortableTestsWithPrefix ""), enhanced.GetSetupSortableTestsWithPrefixId())
        if pool then Assert.Equal(expected (geneSortDataType.SorterPoolSet ""), enhanced.GetSetupSorterPoolSetId())
        else Assert.Equal(expected (geneSortDataType.SorterModelSet ""), enhanced.GetSetupSorterModelSetId())
        Assert.Equal<Map<string, string>>(input.ParamMap, (SetupFileReferences.withoutReferences enhanced).ParamMap)

[<Fact>]
let ``initialization persists the entire expanded product with distinct file queries`` () =
    let configured, _ = configure ["a"; "b"; "c"] ["one"; "two"] [geneSortDataType.SorterModelSet ""] query
    let writes = System.Collections.Concurrent.ConcurrentBag<queryParams * runParameters>()
    let db = { new IGeneSortDb with
        member _.databaseName = configured.DatabaseName
        member _.saveAsync _ qp data _ = async {
            match data with outputData.RunParameters rp -> writes.Add(qp, rp) | _ -> ()
            return Ok () }
        member _.loadAsync _ _ = failwith "Unexpected read"
        member _.loadIfFoundAsync _ _ = failwith "Unexpected read"
        member _.doesOutPutDataExist _ _ = failwith "Unexpected read"
        member _.getRunParameters _ _ _ _ _ = failwith "Unexpected read"
        member _.makeQueryParamsFromRunParams _ _ = failwith "Unexpected query"
      interface IRunParameterStore with
        member _.saveRunParameterSetAsync prepared _ _ = async {
            for entry in prepared.Entries do writes.Add entry
            return Ok () } }
    let result = ParamOps.initRunAndParamFiles db None configured 0<replNumber> 1<replNumber> (UMX.tag false)
                     (RunParameterGeneration.generate configured) |> Async.RunSynchronously
    Assert.True(Result.isOk result, sprintf "%A" result)
    Assert.Equal(6, writes.Count)
    Assert.Equal(6, writes |> Seq.map (fun (q, _) -> q.Id) |> Seq.distinct |> Seq.length)
    Assert.All(writes, fun (_, parameters) -> Assert.Equal(Some configured.RunName, parameters.GetRunName()))
