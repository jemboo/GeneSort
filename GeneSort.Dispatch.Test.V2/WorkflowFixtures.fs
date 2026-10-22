module GeneSort.Dispatch.Test.V2.WorkflowFixtures

open FSharp.UMX
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Sorting
open GeneSort.SortingLib.Sorter
open GeneSort.Dispatch.V1.SortableTests
open GeneSort.Dispatch.V1.SorterEval

let private simpleFixture project database (name: string) setup execution =
    SimpleRun.create {
        ProjectName = project
        DatabaseName = database
        RunName = UMX.tag name
        Description = "Fixed workflow test fixture"
        AllowOverwrite = false
        Setup = setup
        Execution = execution
    } |> run.SimpleRun

let private prefixId (width: int) =
    prefixLibId.create (UMX.tag width) 4<stageLength> prefixLibVariant.PrefixA
    |> PrefixLibId.toString

let sortableTestsRun prefix =
    let spans = [
        ParamKeys.sortableDataFormatKey, ["BitVector512"]
        if prefix then ParamKeys.prefixLibIdKey, [prefixId 32]
        else ParamKeys.mergeLibIdKey, [mergeLibId.create 4<sortingWidth> 2<mergeDimension> sorterLibVariant.VariantA |> MergeLibId.toString]
    ]
    simpleFixture (if prefix then SortableTestsDbs.Prefix.projectName else SortableTestsDbs.Merge.projectName)
        (if prefix then SortableTestsDbs.Prefix.dbName else SortableTestsDbs.Merge.dbName)
        "SortableTestsFixture"
        {
            FunctionName = if prefix then SetupCatalogNames.prefix else SetupCatalogNames.merge
            ParameterSpans = spans
            QueryCatalogName = if prefix then QueryCatalogNamesSetup.sortableTestPrefix else QueryCatalogNamesSetup.sortableTestMerge
            EnhancerFunctionName = SetupEnhancerFunctionNames.sortableTests
        }
        {
            FunctionName = ExecuteFunctionNames.sortableTests
            ParameterSpans = spans
            QueryCatalogName = if prefix then QueryCatalogNamesRun.sortableTestPrefix else QueryCatalogNamesRun.sortableTestMerge
            EnhancerFunctionName = RunParamBuilderNames.Enhancer.sortableTests
        }

let prefixEvaluation width =
    simpleFixture SorterEvalDbs.Prefix.projectName SorterEvalDbs.Prefix.dbName "PrefixEvaluationFixture"
        {
            FunctionName = SetupCatalogNames.prefix
            ParameterSpans = [
                ParamKeys.rngTypeKey, ["LCG"]
                ParamKeys.prefixLibIdKey, [prefixId width]
                ParamKeys.simpleSorterModelTypeKey, ["Msuf4"]
                ParamKeys.sortableDataFormatKey, ["BitVector512"]
                ParamKeys.sorterCountKey, ["8"; "16"]
            ]
            QueryCatalogName = QueryCatalogNamesSetup.sorterEvalPrefix
            EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEvalPrefix
        }
        {
            FunctionName = ExecuteFunctionNames.sorterEval
            ParameterSpans = [ParamKeys.sorterEvalTypeKey, ["V2"]]
            QueryCatalogName = QueryCatalogNamesRun.sorterEvalPrefix
            EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalPrefix
        }

// Catalog-resolution fixtures only: no parameters are generated or executed.
let standardEvaluationHost () =
    simpleFixture (UMX.tag "Tests.Evaluation") (UMX.tag "Fixture") "StandardEvaluationHost"
        {
            FunctionName = SetupCatalogNames.standard
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesSetup.sorterEvalStandard
            EnhancerFunctionName = SetupEnhancerFunctionNames.sorterEval
        }
        {
            FunctionName = ExecuteFunctionNames.sorterEval
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesRun.sorterEvalStandard
            EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterEvalStandard
        }

let mutationHost () =
    simpleFixture (UMX.tag "Tests.Mutation") (UMX.tag "Fixture") "MutationHost"
        {
            FunctionName = SetupCatalogNames.mutantPrefix
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesSetup.sorterMutatePrefix
            EnhancerFunctionName = SetupEnhancerFunctionNames.sorterMutate
        }
        {
            FunctionName = ExecuteFunctionNames.sorterMutate
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesRun.sorterMutatePrefix
            EnhancerFunctionName = RunParamBuilderNames.Enhancer.sorterMutateStandard
        }

// Fixed source for behavioral tests; independent of editable Dispatch presets.
let standardSource () =
    SgdRun.create {
        DatabaseName = UMX.tag "StandardFixture"
        ProjectName = UMX.tag "Tests.Standard"
        RunName = UMX.tag "StandardSource"
        Description = "Single-execution Standard test fixture"
        AllowOverwrite = false
        Setup = {
            FunctionName = SetupCatalogNames.standard
            ParameterSpans = [
                ParamKeys.sorterCountPerPoolKey, ["32"]
                ParamKeys.sorterCountPerPoolSetKey, ["128"]
                ParamKeys.codeModKey, ["NoMods"]
                ParamKeys.adjustCeLengthFrequencyKey, ["10"]
                ParamKeys.generationIntervalLastKey, ["2"]
                ParamKeys.paraRateKey, ["1.15"]
                ParamKeys.selfSymRateKey, ["1.75"]
                ParamKeys.mutationModKey, ["0"]
                ParamKeys.seedModificationRateKey, ["0.02"]
                ParamKeys.modificationRateKey, ["0.035"]
                ParamKeys.mutatorVariantKey, ["V2"]
            ]
            QueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
            EnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32
        }
        Execution = {
            FunctionName = ExecuteFunctionNames.standard
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
            EnhancerFunctionName = RunParamBuilderNames.Enhancer.evoMsuf32
        }
    } {
        MakerNames = EvolutionMakerNames.prefix
        GenSaveIntervalsName = "expInterval100_L50ss"
        GenSaveSubIntervalsName = "summaryInterval_C.1p5C"
    } |> run.SgdRun

let sossSource (modifiers: int list) =
    SgdRun.create {
        DatabaseName = UMX.tag "SossFixture"
        ProjectName = UMX.tag "Tests.Soss"
        RunName = UMX.tag "SossSource"
        Description = "Fixed Soss test fixture"
        AllowOverwrite = false
        Setup = {
            FunctionName = SetupCatalogNames.standard
            ParameterSpans = [
                ParamKeys.sorterCountPerPoolKey, ["4"]
                ParamKeys.sorterCountPerPoolSetKey, ["32"]
                ParamKeys.mutationModKey, List.map string modifiers
            ]
            QueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
            EnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32
        }
        Execution = {
            FunctionName = ExecuteFunctionNames.soss
            ParameterSpans = [
                ParamKeys.seedSossKey, ["1234"]
                ParamKeys.partitionCountKey, ["2"]
                ParamKeys.generationsPerSubsetKey, ["10"]
                ParamKeys.generationsPerMergedSetKey, ["20"]
                ParamKeys.codeModKey, ["NoMods"]
                ParamKeys.generationIntervalLastKey, ["1"]
                ParamKeys.paraRateKey, ["1.15"]
                ParamKeys.selfSymRateKey, ["1.75"]
                ParamKeys.seedModificationRateKey, ["0.02"]
                ParamKeys.modificationRateKey, ["0.035"]
                ParamKeys.mutatorVariantKey, ["V2"]
            ]
            QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
            EnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32
        }
    } {
        MakerNames = EvolutionMakerNames.sossPrefix
        GenSaveIntervalsName = "expInterval100_L50ss"
        GenSaveSubIntervalsName = "uniformInterval2"
    } |> run.SgdRun

let withSetupSpans overrides (source: run) =
    let dto = GeneSort.Project.Mp.V1.RunDto.fromDomain source
    let spans = dto.SetupParameterSpans |> Array.map (fun span ->
        match overrides |> List.tryFind (fst >> (=) span.Key) with
        | Some (_, values) -> { span with Values = List.toArray values }
        | None -> span)
    { dto with SetupParameterSpans = spans } |> GeneSort.Project.Mp.V1.RunDto.toDomain

let standardSweep v2 =
    standardSource () |> withSetupSpans [
        ParamKeys.sorterCountPerPoolSetKey, ["512"]
        ParamKeys.sorterCountPerPoolKey, [if v2 then "512" else "32"]
        ParamKeys.generationIntervalLastKey, ["3"]
        ParamKeys.paraRateKey, ["0.85"; "0.95"; "1.05"; "1.15"]
        ParamKeys.selfSymRateKey, ["0.75"; "1.25"; "1.5"; "1.75"]
        ParamKeys.seedModificationRateKey, (if v2 then ["0.015"; "0.02"; "0.025"] else ["0.02"; "0.03"; "0.04"; "0.05"])
        ParamKeys.modificationRateKey, (if v2 then ["0.02"; "0.025"; "0.03"; "0.035"; "0.04"] else ["0.015"; "0.02"; "0.025"; "0.03"; "0.04"; "0.05"])
        ParamKeys.mutatorVariantKey, [if v2 then "V2" else "V1"]
    ]

