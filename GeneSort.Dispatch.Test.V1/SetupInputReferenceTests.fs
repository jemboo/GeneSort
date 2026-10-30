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
let private parameters = runParameters.create (Map.ofList ["ChannelCount", "4"; "RngType", "LCG"; "SimpleSorterModelType", "Msce"; "SorterCount", "2"; "Repl", "0"])
let private legacy value output = queryParams.create database project None None output [|"Execution", value|]
let private setupQuery = QueryParamsBuildersSetup.SorterEval.Standard.makeQueryParams project database
let private stored output = setupQuery (setupParameters.create parameters.ParamMap) output |> Option.get

[<Fact>]
let ``Setup constructors directly preserve stored IDs and physical paths`` () =
    let expected = [models, "cfc9d0d9-ef27-817d-5ea5-317115b05c85"; tests, "5b1ee568-d5a4-e41e-5971-87fb7518d085"]
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
                ParametersEnhancerName = SetupParametersEnhancerNames.sorterEval
                ParameterSpans = ["ChannelCount", ["4"]; "RngType", ["LCG"]; "SimpleSorterModelType", ["Msce"; "Msrs"]; "SorterCount", ["2"]]
            }
            Execution = {
                FunctionName = ExecuteFunctionNames.sorterEval
                QueryCatalogName = QueryCatalogNamesRun.sorterEvalStandard
                ParametersEnhancerName = RunParametersEnhancerNames.sorterEvalStandard
                ParameterSpans = [ParamKeys.sorterEvalTypeKey, ["V1"; "V2"]]
            }
        } |> Run.ofSimple
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



[<Fact>]
let ``Evaluation source database changes setup identity but is absent from processing queries`` () =
    ProjectCatalogs.registerAll ()
    for configured in [GeneSort.Dispatch.V1.SorterEval.Merge.Channels32.Test (); GeneSort.Dispatch.V1.SorterEval.Prefix.Channels32 ()] do
        let original = RunParameterGeneration.generate configured 0<replNumber> 1<replNumber> |> Array.head
        let path = original.ParamMap[ParamKeys.sortableTestsModuleKey]
        let alternate = runParameters.create (original.ParamMap |> Map.add ParamKeys.sortableTestsModuleKey "OtherProject.OtherModule")
        let setup = QueryParamsCatalogSetup.get configured.SetupQueryCatalogName configured.ProjectName configured.DatabaseName
        let first = setup (setupParameters.create original.ParamMap) tests |> Option.get
        let second = setup (setupParameters.create alternate.ParamMap) tests |> Option.get
        Assert.Equal(path, first.Properties[ParamKeys.sortableTestsModuleKey])
        Assert.NotEqual(first.Id, second.Id)
        let processing = QueryParamsCatalogRun.get configured.RunQueryCatalogName configured.ProjectName configured.DatabaseName
        for output in [geneSortDataType.Run configured.RunName; geneSortDataType.RunParameters configured.RunName; geneSortDataType.SorterSetEval ""] do
            let query = processing original output |> Option.get
            let changed = processing alternate output |> Option.get
            Assert.False(query.Properties.ContainsKey ParamKeys.sortableTestsModuleKey)
            Assert.Equal(query.Id, changed.Id)
        Assert.Equal(parameterRole.SetupOnly, Run.parameterRole configured ParamKeys.sortableTestsModuleKey)

[<Theory>]
[<InlineData("")>]
[<InlineData("Channels32")>]
[<InlineData("../Channels32")>]
[<InlineData("Project/Database")>]
[<InlineData("Project..Database")>]
let ``SortableTestsModule requires a qualified module name`` value =
    Assert.True(ParameterValidation.validateValue ParamKeys.sortableTestsModuleKey value |> Result.isError)

[<Fact>]
let ``Run modules expose the catalog and database of every example`` () =
    for kind in GeneSort.Dispatch.V1.RunTypeCatalog.all do
        let catalogName = kind.ExecutionQueryCatalogName
        let source = RunModuleCatalog.resolve kind.Name |> function Ok value -> value | Error error -> failwith error
        for example in kind.Examples do
            Assert.Equal(string example.Run.ProjectName, string source.ProjectName)
            Assert.Equal(string example.Run.DatabaseName, string source.DatabaseName)
            Assert.Equal(example.Run.RunQueryCatalogName, source.ExecutionQueryCatalogName)
        let moduleTypeName = "GeneSort.Dispatch.V1." + kind.Name
        let moduleType = typeof<GeneSort.Dispatch.V1.runHost>.Assembly.GetTypes() |> Array.find (fun t -> t.FullName.Replace('+', '.') = moduleTypeName)
        let catalog = moduleType.GetProperty("executionQueryCatalogName", System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.Static ||| System.Reflection.BindingFlags.DeclaredOnly)
        Assert.NotNull(catalog)
        Assert.Equal(catalogName, catalog.GetValue(null) :?> string)
