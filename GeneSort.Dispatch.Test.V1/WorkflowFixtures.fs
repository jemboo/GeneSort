module GeneSort.Dispatch.Test.V1.WorkflowFixtures

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingLib.Sorter
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
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
    } |> Run.ofSimple

let private prefixId (width: int) =
    prefixLibId.create (UMX.tag width) 4<stageLength> prefixLibVariant.PrefixA
    |> PrefixLibId.toString

let sortableTestsRun prefix =
    let spans = [
        ParamKeys.excludeSelfCeKey, ["true"]
        ParamKeys.collectSortableTestsKey, ["false"]
        ParamKeys.channelCountKey, [if prefix then "32" else "4"]
        ParamKeys.sortableDataFormatKey, ["BitVector512"]
        if prefix then ParamKeys.prefixLibIdKey, [prefixId 32]
        else ParamKeys.mergeLibIdKey, [mergeLibId.create 4<channelCount> 2<mergeDimension> sorterLibVariant.VariantA |> MergeLibId.toString]
    ]
    simpleFixture (if prefix then SortableTestsDbs.Prefix.projectName else SortableTestsDbs.Merge.projectName)
        (if prefix then SortableTestsDbs.Prefix.dbName else SortableTestsDbs.Merge.dbName)
        "SortableTestsFixture"
        {
            FunctionName = if prefix then SetupCatalogNames.prefix else SetupCatalogNames.merge
            ParameterSpans = spans
            QueryCatalogName = if prefix then QueryCatalogNamesSetup.sortableTestPrefix else QueryCatalogNamesSetup.sortableTestMerge
            ParametersEnhancerName = SetupParametersEnhancerNames.sortableTests
        }
        {
            FunctionName = ExecuteFunctionNames.sortableTests
            ParameterSpans = spans
            QueryCatalogName = if prefix then QueryCatalogNamesRun.sortableTestPrefix else QueryCatalogNamesRun.sortableTestMerge
            ParametersEnhancerName = RunParametersEnhancerNames.sortableTests
        }

let prefixEvaluation width =
    simpleFixture SorterEvalDbs.Prefix.projectName SorterEvalDbs.Prefix.dbName "PrefixEvaluationFixture"
        {
            FunctionName = SetupCatalogNames.prefix
            ParameterSpans = [
                ParamKeys.excludeSelfCeKey, ["true"]
                ParamKeys.collectSortableTestsKey, ["false"]
                ParamKeys.channelCountKey, [string width]
                ParamKeys.rngTypeKey, ["LCG"]
                ParamKeys.prefixLibIdKey, [prefixId width]
                ParamKeys.simpleSorterModelTypeKey, ["Msuf4"]
                ParamKeys.sortableDataFormatKey, ["BitVector512"]
                ParamKeys.sorterCountKey, ["8"; "16"]
            ]
            QueryCatalogName = QueryCatalogNamesSetup.sorterEvalPrefix
            ParametersEnhancerName = SetupParametersEnhancerNames.sorterEvalPrefix
        }
        {
            FunctionName = ExecuteFunctionNames.sorterEval
            ParameterSpans = [ParamKeys.sorterEvalTypeKey, ["V2"]]
            QueryCatalogName = QueryCatalogNamesRun.sorterEvalPrefix
            ParametersEnhancerName = RunParametersEnhancerNames.sorterEvalPrefix
        }

// Catalog-resolution fixtures only: no parameters are generated or executed.
let standardEvaluationHost () =
    simpleFixture (UMX.tag "Tests.Evaluation") (UMX.tag "Fixture") "StandardEvaluationHost"
        {
            FunctionName = SetupCatalogNames.standard
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesSetup.sorterEvalStandard
            ParametersEnhancerName = SetupParametersEnhancerNames.sorterEval
        }
        {
            FunctionName = ExecuteFunctionNames.sorterEval
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesRun.sorterEvalStandard
            ParametersEnhancerName = RunParametersEnhancerNames.sorterEvalStandard
        }

let mutationHost () =
    simpleFixture (UMX.tag "Tests.Mutation") (UMX.tag "Fixture") "MutationHost"
        {
            FunctionName = SetupCatalogNames.mutantPrefix
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesSetup.sorterMutatePrefix
            ParametersEnhancerName = SetupParametersEnhancerNames.sorterMutate
        }
        {
            FunctionName = ExecuteFunctionNames.sorterMutate
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesRun.sorterMutatePrefix
            ParametersEnhancerName = RunParametersEnhancerNames.sorterMutateStandard
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
            FunctionName = GeneSort.Project.V1.Catalogs.Setup.SetupCatalogNames.sgdStandard
            SortableTestsModule = UMX.tag "SortableTests.Prefix.Channels32"
            SorterEvalModule = UMX.tag "SorterEval.Prefix"
            ParameterSpans = [
                ParamKeys.orthoRateKey, ["4.001"]
                ParamKeys.sorterChildCountKey, ["1"]
                ParamKeys.selectedSorterCountPerPoolKey, ["All"]
                ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"]
                ParamKeys.sortedFractionKey, ["0.99"]
                ParamKeys.distinctSorterHashesKey, ["true"]
                ParamKeys.prioritizeNewMutantsKey, ["true"]
                ParamKeys.rngTypeKey, ["LCG"]
                ParamKeys.simpleSorterModelTypeKey, ["Msuf4"]
                ParamKeys.sortableDataFormatKey, ["BitVector512"]
                ParamKeys.prefixLibIdKey, [prefixId 32]
                ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
                ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
                ParamKeys.excludeSelfCeKey, ["true"]
                ParamKeys.collectSortableTestsKey, ["false"]
                ParamKeys.channelCountKey, ["32"]
                ParamKeys.sorterCountPerPoolKey, ["32"]
                ParamKeys.sorterPoolCountKey, ["4"]
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
            ParametersEnhancerName = SetupParametersEnhancerNames.standardMsuf32
        }
        Execution = {
            FunctionName = ExecuteFunctionNames.standard
            ParameterSpans = []
            QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
            ParametersEnhancerName = RunParametersEnhancerNames.evoMsuf32
        }
    } {
        MakerNames = EvolutionMakerNames.prefix
        GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
        GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
    } |> Run.ofSgd

let sossSource (modifiers: int list) =
    SgdRun.create {
        DatabaseName = UMX.tag "SossFixture"
        ProjectName = UMX.tag "Tests.Soss"
        RunName = UMX.tag "SossSource"
        Description = "Fixed Soss test fixture"
        AllowOverwrite = false
        Setup = {
            FunctionName = GeneSort.Project.V1.Catalogs.Setup.SetupCatalogNames.sgdSoss
            SortableTestsModule = UMX.tag "SortableTests.Prefix.Channels32"
            SorterEvalModule = UMX.tag "SorterEval.Prefix"
            ParameterSpans = [
                ParamKeys.orthoRateKey, ["4.001"]
                ParamKeys.sorterChildCountKey, ["1"]
                ParamKeys.selectedSorterCountPerPoolKey, ["All"]
                ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"]
                ParamKeys.sortedFractionKey, ["0.99"]
                ParamKeys.distinctSorterHashesKey, ["true"]
                ParamKeys.prioritizeNewMutantsKey, ["true"]
                ParamKeys.rngTypeKey, ["LCG"]
                ParamKeys.simpleSorterModelTypeKey, ["Msuf4"]
                ParamKeys.sortableDataFormatKey, ["BitVector512"]
                ParamKeys.prefixLibIdKey, [prefixId 32]
                ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
                ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
                ParamKeys.excludeSelfCeKey, ["true"]
                ParamKeys.collectSortableTestsKey, ["false"]
                ParamKeys.channelCountKey, ["32"]
                ParamKeys.sorterCountPerPoolKey, ["4"]
                ParamKeys.sorterPoolCountKey, ["8"]
                ParamKeys.mutationModKey, List.map string modifiers
            ]
            QueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
            ParametersEnhancerName = SetupParametersEnhancerNames.sossMsuf32
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
            ParametersEnhancerName = RunParametersEnhancerNames.sossMsuf32
        }
    } {
        MakerNames = EvolutionMakerNames.sossPrefix
        GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
        GenSaveSubIntervalsName = GenIntervalNames.uniformInterval2
    } |> Run.ofSgd

let withSetupSpans overrides (source: run) =
    let dto = GeneSort.Project.Mp.V1.RunDto.fromDomain source
    let spans = dto.SetupParameterSpans |> Array.map (fun span ->
        match overrides |> List.tryFind (fst >> (=) span.Key) with
        | Some (_, values) -> { span with Values = List.toArray values }
        | None -> span)
    { dto with SetupParameterSpans = spans } |> GeneSort.Project.Mp.V1.RunDto.toDomain

let standardSweep v2 =
    standardSource () |> withSetupSpans [
        ParamKeys.sorterPoolCountKey, [if v2 then "1" else "16"]
        ParamKeys.sorterCountPerPoolKey, [if v2 then "512" else "32"]
        ParamKeys.generationIntervalLastKey, ["3"]
        ParamKeys.paraRateKey, ["0.85"; "0.95"; "1.05"; "1.15"]
        ParamKeys.selfSymRateKey, ["0.75"; "1.25"; "1.5"; "1.75"]
        ParamKeys.seedModificationRateKey, (if v2 then ["0.015"; "0.02"; "0.025"] else ["0.02"; "0.03"; "0.04"; "0.05"])
        ParamKeys.modificationRateKey, (if v2 then ["0.02"; "0.025"; "0.03"; "0.035"; "0.04"] else ["0.015"; "0.02"; "0.025"; "0.03"; "0.04"; "0.05"])
        ParamKeys.mutatorVariantKey, [if v2 then "V2" else "V1"]
    ]

