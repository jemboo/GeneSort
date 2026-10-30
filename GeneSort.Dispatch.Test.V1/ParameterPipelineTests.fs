module GeneSort.Dispatch.Test.V1.ParameterPipelineTests

open System
open FSharp.UMX
open Xunit
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Dispatch.V1
open GeneSort.Project.Mp.V1

let private configured () = WorkflowFixtures.sossSource [0]
let private setupContext () : setupParametersPipelineContext = {
    Run = configured ()
    QueryParamsFromSetupParams = fun _ _ -> failwith "Unexpected setup identity calculation"
}
let private runContext () =
    runParametersPipelineContext.create (fun _ _ -> failwith "Unexpected run identity calculation") (configured ())

let private issues = function
    | Error issues -> issues
    | Ok _ -> failwith "Expected validation to fail"

// Use the real validators and overwrite the invalid value in the transformation.
// A pre-validation regression would silently accept this input.
let private rejectsBeforeEnhancement definition context invalid repaired =
    let mutable enhanced = false
    let mutable finalized = false
    let definition =
        { definition with
            Enhance = fun _ _ -> enhanced <- true; repaired
            Finalize = fun _ value -> finalized <- true; value }
    let errors = ParameterPipeline.tryApply definition context invalid |> issues
    Assert.Contains(errors, fun issue -> issue.Field.Contains ParamKeys.modificationRateKey)
    Assert.False(enhanced)
    Assert.False(finalized)

[<Fact>]
let ``setup input validation rejects an invalid value before an enhancer can overwrite it`` () =
    let input = setupParameters.create (Map.ofList [ParamKeys.modificationRateKey, "NaN"])
    rejectsBeforeEnhancement SetupParametersPipelines.noOp (setupContext ()) input (setupParameters.create Map.empty)

[<Fact>]
let ``run input validation rejects an invalid value before an enhancer can overwrite it`` () =
    let input = runParameters.create (Map.ofList [ParamKeys.modificationRateKey, "NaN"])
    rejectsBeforeEnhancement RunParametersPipelines.sortableTests (runContext ()) input (runParameters.create Map.empty)

let private rejectsAfterEnhancement definition context input invalid =
    let mutable enhanced = false
    let mutable finalized = false
    let definition =
        { definition with
            Enhance = fun _ _ -> enhanced <- true; invalid
            Finalize = fun _ value -> finalized <- true; value }
    Assert.Empty(definition.ValidateInput context input)
    let errors = ParameterPipeline.tryApply definition context input |> issues
    Assert.Contains(errors, fun issue -> issue.Message.Contains "capacity")
    Assert.True(enhanced)
    Assert.False(finalized)

let private poolValues count = Map.ofList [ParamKeys.sorterCountPerPoolKey, "2"; ParamKeys.sorterPoolCountKey, count]

[<Fact>]
let ``setup effective validation catches a derived capacity overflow before finalization`` () =
    rejectsAfterEnhancement SetupParametersPipelines.sossMsuf32 (setupContext ())
        (setupParameters.create (poolValues "1")) (setupParameters.create (poolValues "2147483647"))

[<Fact>]
let ``run effective validation catches a derived capacity overflow before finalization`` () =
    rejectsAfterEnhancement RunParametersPipelines.Sgd.msrs32MutationRate (runContext ())
        (runParameters.create (poolValues "1")) (runParameters.create (poolValues "2147483647"))

[<Fact>]
let ``required values may be inherited and symbolic choices resolve only during enhancement`` () =
    let configured = configured ()
    let setup = setupParameters.create (poolValues "3")
    let input = runParameters.create (Map.ofList [ParamKeys.selectedSorterCountPerPoolKey, "All"])
    let mutable queried = false
    let query (parameters: runParameters) output =
        queried <- true
        Assert.Equal("2", parameters.ParamMap[ParamKeys.selectedSorterCountPerPoolKey])
        Some (queryParams.create configured.DatabaseName configured.ProjectName None None output [||])
    let context = runParametersPipelineContext.createForSetup query configured setup
    let definition = RunParametersPipelines.Sgd.msrs32MutationRate
    Assert.Empty(definition.ValidateInput context input)
    Assert.NotEmpty(definition.ValidateEffective context input)
    Assert.Equal("All", input.ParamMap[ParamKeys.selectedSorterCountPerPoolKey])
    let result = ParameterPipeline.apply definition context input
    Assert.Equal("2", result.ParamMap[ParamKeys.sorterCountPerPoolKey])
    Assert.Equal("3", result.ParamMap[ParamKeys.sorterPoolCountKey])
    Assert.True(queried)
    Assert.True(result.GetId().IsSome)

[<Theory>]
[<InlineData("SelectedSorterCountPerPool", "All")>]
[<InlineData("SorterEvalMeasureInitial", "SameAsEvaluation")>]
let ``unresolved choices produce effective validation issues for both collections`` key value =
    let map = Map.ofList [key, value]
    let setup, run = setupParameters.create map, runParameters.create map
    let setupDefinition, runDefinition = SetupParametersPipelines.noOp, RunParametersPipelines.sortableTests
    Assert.Empty(setupDefinition.ValidateInput (setupContext ()) setup)
    Assert.Empty(runDefinition.ValidateInput (runContext ()) run)
    for errors in [ParameterPipeline.tryApply setupDefinition (setupContext ()) setup |> issues;
                   ParameterPipeline.tryApply runDefinition (runContext ()) run |> issues] do
        Assert.Contains(errors, fun issue -> issue.Field.EndsWith key && issue.Message.Contains "requires")

[<Fact>]
let ``host setup generation and output prediction validate before calculating references`` () =
    let name = "tests.validation-order." + Guid.NewGuid().ToString("N")
    let mutable queries = 0
    QueryParamsCatalogSetup.register name (fun _ _ _ _ -> queries <- queries + 1; None)
    SetupParametersPipelineCatalog.register name SetupParametersPipelines.standardMsuf32
    let dto = RunDto.fromDomain (WorkflowFixtures.standardSource ())
    let invalid =
        { dto with
            SetupQueryCatalogName = name
            SetupParametersPipelineName = name
            SetupParameterSpans = dto.SetupParameterSpans |> Array.map (fun span ->
                if span.Key = ParamKeys.sorterPoolCountKey then { span with Values = [|"0"|] } else span) }
        |> RunDto.toDomain
    let host = runHost.Create invalid 1
    Assert.Throws<ArgumentException>(fun () -> host.MakeSetupParameters 0<replNumber> 1<replNumber> |> ignore) |> ignore
    Assert.Throws<ArgumentException>(fun () -> host.MakeRunParameters 0<replNumber> 1<replNumber> |> ignore) |> ignore
    Assert.True(SetupOutputs.getIds invalid.BaseRun 0<replNumber> (geneSortDataType.SorterPoolSet "") |> Result.isError)
    Assert.Equal(0, queries)

[<Fact>]
let ``direct run generation validates before invoking a query builder`` () =
    let name = "tests.direct-validation." + Guid.NewGuid().ToString("N")
    let mutable queries = 0
    QueryParamsCatalogRun.register name (fun _ _ _ _ -> queries <- queries + 1; None)
    let dto = GeneSort.Dispatch.V1.SortableTests.Prefix.Channels32.FourStage_32 () |> RunDto.fromDomain
    let invalid =
        { dto with
            RunQueryCatalogName = name
            RunParameterSpans = Array.append dto.RunParameterSpans [|{ Key = ParamKeys.modificationRateKey; Values = [|"NaN"|] }|] }
        |> RunDto.toDomain
    Assert.Throws<ArgumentException>(fun () -> RunParameterGeneration.generate invalid 0<replNumber> 1<replNumber> |> ignore) |> ignore
    Assert.Equal(0, queries)

[<Fact>]
let ``generation presents raw choices to input validators and effective choices to final validators`` () =
    let configured = WorkflowFixtures.standardSource ()
    let host = runHost.Create configured 1
    let events = ResizeArray<string>()
    let setupDefinition =
        { host.SetupParametersPipeline with
            ValidateInput = fun context parameters ->
                events.Add "setup-input"
                Assert.Equal("All", parameters.ParamMap[ParamKeys.selectedSorterCountPerPoolKey])
                host.SetupParametersPipeline.ValidateInput context parameters
            ValidateEffective = fun context parameters ->
                events.Add "setup-effective"
                Assert.Equal("32", parameters.ParamMap[ParamKeys.selectedSorterCountPerPoolKey])
                host.SetupParametersPipeline.ValidateEffective context parameters }
    let runDefinition =
        { host.RunParametersPipeline with
            ValidateInput = fun context parameters ->
                events.Add "run-input"
                Assert.False(parameters.ParamMap.ContainsKey ParamKeys.sorterPoolCountKey)
                host.RunParametersPipeline.ValidateInput context parameters
            ValidateEffective = fun context parameters ->
                events.Add "run-effective"
                Assert.Equal("32", parameters.ParamMap[ParamKeys.selectedSorterCountPerPoolKey])
                host.RunParametersPipeline.ValidateEffective context parameters }
    let generated = RunParameterGeneration.generateWithBindings host.QueryParamsFromRunParams host.QueryParamsFromSetupParams
                        runDefinition setupDefinition configured 0<replNumber> 1<replNumber>
    Assert.Single(generated) |> ignore
    Assert.Equal<string>([|"setup-input"; "setup-effective"; "run-input"; "run-effective"|], events.ToArray())

[<Fact>]
let ``effective validation rejects a custom enhancer that changes fixed setup settings`` () =
    let configured = WorkflowFixtures.standardSource ()
    let host = runHost.Create configured 1
    let corrupt =
        { host.RunParametersPipeline with
            Enhance = fun context parameters ->
                let enhanced = host.RunParametersPipeline.Enhance context parameters
                runParameters.create (Map.add ParamKeys.channelCountKey "16" enhanced.ParamMap) }
    let mutable queries = 0
    let query parameters output = queries <- queries + 1; host.QueryParamsFromRunParams parameters output
    let error = Assert.Throws<ArgumentException>(fun () ->
        RunParameterGeneration.generateWithBindings query host.QueryParamsFromSetupParams corrupt host.SetupParametersPipeline
            configured 0<replNumber> 1<replNumber> |> ignore)
    Assert.Contains(ParamKeys.channelCountKey, error.Message)
    Assert.Equal(0, queries)

[<Fact>]
let ``continuation overrides are applied before effective validation`` () =
    let source = configured ()
    let dto = RunDto.fromDomain source
    let continuation = { dto with RunParameterSpans = [|{ Key = ParamKeys.modificationRateKey; Values = [|"0.25"|] }|] }
                       |> RunDto.toDomain |> Run.withParameterSource source []
    let host = runHost.Create continuation 1
    let mutable validated = false
    let definition =
        { host.RunParametersPipeline with
            Enhance = fun context parameters ->
                let enhanced = host.RunParametersPipeline.Enhance context parameters
                runParameters.create (Map.add ParamKeys.modificationRateKey "0.1" enhanced.ParamMap)
            ValidateEffective = fun _ parameters ->
                validated <- true
                Assert.Equal("0.25", parameters.ParamMap[ParamKeys.modificationRateKey])
                [|validationIssue.create "Execution.ModificationRate" "Rejected by effective validator"|]
            Finalize = fun _ _ -> failwith "Invalid continuation was finalized" }
    let error = Assert.Throws<ArgumentException>(fun () ->
        RunParameterGeneration.generateWithBindings host.QueryParamsFromRunParams host.QueryParamsFromSetupParams definition host.SetupParametersPipeline
            continuation 0<replNumber> 1<replNumber> |> ignore)
    Assert.True(validated)
    Assert.Contains("Rejected by effective validator", error.Message)
