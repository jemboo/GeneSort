module GeneSort.Dispatch.Test.V1.SetupInputReferenceTests

open System
open System.IO
open FSharp.UMX
open Xunit
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Runtime
open GeneSort.FileDb.V1

let private project = UMX.tag<projectName> "PlanTests"
let private database = UMX.tag<databaseName> "Shared"
let private models = geneSortDataType.SorterModelSet ""
let private tests = geneSortDataType.SortableTestsWithPrefix ""
let private parameters = runParameters.create (Map.ofList ["SortingWidth", "4"; "RngType", "LCG"; "SimpleSorterModelType", "Msce"; "SorterCount", "2"; "Repl", "0"])
let private legacy value output = queryParams.create database project None None output [|"Execution", value|]
let private setupQuery = QueryParamsBuildersSetup.SorterEval.Standard.makeQueryParams project database
let private stored output = setupQuery (setupParameters.create parameters.ParamMap) output |> Option.get

[<Fact>]
let ``Setup constructors directly preserve stored IDs and physical paths`` () =
    let expected = [models, "53bb8332-79a5-4b83-16c1-1cc687fc6515"; tests, "13408a82-615d-ff16-32dc-24dc457adc01"]
    let root = Path.Combine(Path.GetTempPath(), "PlanTests", "Shared", "Data")
    for output, id in expected do
        let query = stored output
        Assert.Equal(Guid.Parse id, UMX.untag query.Id)
        let path = OutputDataFile.getFullOutputDataFilePath (UMX.tag root) geneSortDataMode.Setup query |> UMX.untag
        Assert.Equal(Path.Combine(root, "Setup", GeneSortDataType.toFolderName output, id + ".msgpack"), path)

[<Fact>]
let ``saved input references reopen without generator settings`` () =
    let query = stored models
    let saved = SetupInputReferences.recordFiles [query] (runParameters.create Map.empty)
    let reopened = SetupInputReferences.tryRecordedInput project database saved models |> Option.get
    Assert.Equal(SetupInputReferences.fileIdentity query, SetupInputReferences.fileIdentity reopened)
    let other = SetupInputReferences.tryRecordedInput project (UMX.tag "OtherDb") saved models |> Option.get
    Assert.NotEqual(SetupInputReferences.fileIdentity query, SetupInputReferences.fileIdentity other)

[<Fact>]
let ``conflicting input references fail explicitly`` () =
    Assert.Throws<ArgumentException>(fun () ->
        SetupInputReferences.recordFiles [stored models; legacy "Other" models] parameters |> ignore) |> ignore

[<Fact>]
let ``legacy pools retain original addresses while tests use Setup constructors`` () =
    let pool = legacy "LegacyPool" (geneSortDataType.SorterPoolSet "")
    let testRequest = legacy "LegacyTests" tests
    Assert.Equal(pool, SetupInputReferences.resolveSavedInput setupQuery parameters pool)
    Assert.Equal((stored tests).Id, (SetupInputReferences.resolveSavedInput setupQuery parameters testRequest).Id)
    let importedId = Guid.Parse "30000000-0000-0000-0000-000000000003" |> UMX.tag<queryParamsId>
    let saved = parameters.WithSetupSortableTestsWithPrefixId(Some importedId)
    Assert.Equal(importedId, (SetupInputReferences.resolveSavedInput setupQuery saved testRequest).Id)

[<Fact>]
let ``imports use explicit Setup references and ignore obsolete source file aliases`` () =
    let output = geneSortDataType.SorterPoolSet ""
    let request = legacy "Append" output
    let sourceId = Guid.Parse "10000000-0000-0000-0000-000000000001"
    let convertedId = Guid.Parse "20000000-0000-0000-0000-000000000002"
    let saved = runParameters.create (Map.ofList ["SourcePoolFileId", string sourceId; ParamKeys.setupSorterPoolSetIdKey, string convertedId])
    Assert.Equal(convertedId, UMX.untag (SetupInputReferences.resolveRecorded saved request).Id)
    let oldParameters = runParameters.create (saved.ParamMap.Remove ParamKeys.setupSorterPoolSetIdKey)
    Assert.Equal(request, SetupInputReferences.resolveRecorded oldParameters request)
    Assert.Equal(request, SetupInputReferences.resolveRecorded (runParameters.create Map.empty) request)

[<Fact>]
let ``imports preserve copy IDs and the existing conversion identity`` () =
    let pool = legacy "SourcePool" (geneSortDataType.SorterPoolSet "")
    let sourceTests = legacy "SourceTests" tests
    Assert.Equal(pool.Id, (SetupInputReferences.copyPool project database pool).Id)
    let copiedTests = SetupInputReferences.copyTests project database sourceTests
    Assert.Equal(sourceTests.Id, copiedTests.Id)
    let converted = SetupInputReferences.convertedPool project database ExecuteFunctionNames.soss pool sourceTests
    let expected = queryParams.create database project None None (geneSortDataType.SorterPoolSet "")
                       [|"SourceFileId", string pool.Id; "SourceTestsFileId", string sourceTests.Id; "ExecutionFunction", ExecuteFunctionNames.soss|]
    Assert.Equal(expected.Id, converted.Id)
    Assert.NotEqual(pool.Id, converted.Id)
    let saved = SetupInputReferences.recordFiles [converted; copiedTests] (runParameters.create Map.empty)
    Assert.Equal(converted.Id, (SetupInputReferences.resolveRecorded saved pool).Id)
    Assert.Equal(copiedTests.Id, (SetupInputReferences.resolveRecorded saved sourceTests).Id)

[<Fact>]
let ``preparation inventories five directly addressed setup files without IO`` () =
    let configured =
        SimpleRun.create {
            ProjectName = project; DatabaseName = database; RunName = UMX.tag "Evaluation"
            Description = "Setup inputs"; AllowOverwrite = false
            Setup = {
                FunctionName = SetupCatalogNames.standard
                QueryCatalogName = QueryCatalogNamesSetup.sorterEvalStandard
                EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
                ParameterSpans = ["SortingWidth", ["4"]; "RngType", ["LCG"]; "SimpleSorterModelType", ["Msce"; "Msrs"]; "SorterCount", ["2"]]
            }
            Execution = {
                FunctionName = ExecuteFunctionNames.sorterEval
                QueryCatalogName = QueryCatalogNamesRun.sorterEvalStandard
                EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalStandard
                ParameterSpans = [ParamKeys.sorterEvalTypeKey, ["V1"; "V2"]]
            }
        } |> run.SimpleRun
    let prepared = RunPreparation.prepare { Run = configured; FirstReplica = 0; ReplicaCount = 2 }
    Assert.Equal(8, prepared.FileCount)
    Assert.Equal(5, prepared.SetupFiles.Length)
    let storedFiles = prepared.SetupFiles |> Array.map SetupInputReferences.fileIdentity |> Set.ofArray
    let make = QueryParamsCatalogSetup.get configured.SetupQueryCatalogName project database
    for _, rp in prepared.Entries do
        for output in [models; tests] do
            let query = make (setupParameters.create rp.ParamMap) output |> Option.get
            Assert.True(storedFiles.Contains(SetupInputReferences.fileIdentity query))
            Assert.Equal(rp.ParamMap[SetupFileReferences.keyForOutputType output], string query.Id)


