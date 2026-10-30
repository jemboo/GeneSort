namespace GeneSort.Dispatch.V1.SorterSgd.p32.OnePool
open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.SorterSgd
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.p32.OnePool.Common

module SimpleS =

    let channelCount = 32<GeneSort.Sorting.channelCount>
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters =
        [
            ParamKeys.excludeSelfCeKey, ["true"]
            ParamKeys.collectSortableTestsKey, ["false"]
            ParamKeys.rngTypeKey, ["LCG"]
            ParamKeys.simpleSorterModelTypeKey, ["Msuf4"]
            ParamKeys.sortableDataFormatKey, ["BitVector512"]
            ParamKeys.prefixLibIdKey, [GeneSort.SortingLib.Sorter.prefixLibId.create channelCount 4<GeneSort.Sorting.stageLength> GeneSort.SortingLib.Sorter.prefixLibVariant.PrefixA |> GeneSort.SortingLib.Sorter.PrefixLibId.toString]
            ParamKeys.sorterEvalMeasureInitialKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
            ParamKeys.sorterEvalMeasureKey, [GeneSort.SortingOps.SorterEvalMeasure.stageBiasedFilterUnsorted |> GeneSort.SortingOps.SorterEvalMeasure.toCompactString]
        ]

    let standardExecutionEnhancerFunctionName = RunParamBuilderNames.Enhancer.evoMsuf32
    let sossExecutionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32
    let standardExecutionFunctionName = ExecuteFunctionNames.standard
    let standardSetupFunctionName = SetupCatalogNames.sgdStandard
    let sossExecutionFunctionName = ExecuteFunctionNames.soss
    let sossSetupFunctionName = SetupCatalogNames.sgdSoss
    let standardExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandard
    let sossExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
    let standardAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
    let sossAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
    let standardSetupEnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32
    let sossSetupEnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32
    let standardSetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
    let sossSetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed

    // ActivationProbability belongs to setup; the independent activator mutation rate belongs to execution.
    let activationProbability = 0.5
    let activatorMutationRate = 0.10

    let description = "Single-pool SimpleS runs and their append and condense continuations."

    module Standard =

        let executionQueryCatalogName = standardExecutionQueryCatalogName

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let StandardS () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                [
                    (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                    (ParamKeys.sorterPoolCountKey, [1] |> List.map string)
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                    (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                    (ParamKeys.paraRateKey, [1.15] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                    (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                    (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                    (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                ]
            let parameterSpansRun =
                [
                    (ParamKeys.orthoRateKey, ["4.001"])
                    (ParamKeys.sorterChildCountKey, ["1"])
                    (ParamKeys.distinctSorterHashesKey, ["true"])
                    (ParamKeys.prioritizeNewMutantsKey, ["true"])
                    (ParamKeys.sortedFractionKey, ["0.99"])
                    (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                    (ParamKeys.activatorMutationRateKey, [activatorMutationRate] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OnePool SimpleS generation and evolution"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = standardSetupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpansSetup
                        QueryCatalogName = standardSetupQueryCatalogName
                        EnhancerFunctionName = standardSetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = standardExecutionFunctionName
                        ParameterSpans = parameterSpansRun
                        QueryCatalogName = executionQueryCatalogName
                        EnhancerFunctionName = standardExecutionEnhancerFunctionName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                }
            )

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.p32.OnePool.SimpleS.Standard", description, channelCount, executionQueryCatalogName, [|
                runExample("StandardS", StandardS)
            |])

    module Soss =

        let executionQueryCatalogName = sossExecutionQueryCatalogName

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let SossS () : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                [
                    (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                    (ParamKeys.channelCountKey, [string (int channelCount)])
                    (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                    (ParamKeys.sorterPoolCountKey, [1] |> List.map string)
                    (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                    (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                ]
            let parameterSpansRun =
                [
                    (ParamKeys.orthoRateKey, ["4.001"])
                    (ParamKeys.sorterChildCountKey, ["1"])
                    (ParamKeys.distinctSorterHashesKey, ["true"])
                    (ParamKeys.prioritizeNewMutantsKey, ["true"])
                    (ParamKeys.sortedFractionKey, ["0.99"])
                    (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                    (ParamKeys.seedSossKey, [1234UL] |> List.map string)
                    (ParamKeys.partitionCountKey, [2] |> List.map string)
                    (ParamKeys.generationsPerSubsetKey, [10] |> List.map string)
                    (ParamKeys.generationsPerMergedSetKey, [20] |> List.map string)
                    (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                    (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                    (ParamKeys.paraRateKey, [1.15] |> List.map string)
                    (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                    (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                    (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                    (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                    (ParamKeys.activatorMutationRateKey, [activatorMutationRate] |> List.map string)
                ]
            run.EvolutionRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OnePool SimpleS generation and evolution"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = sossSetupFunctionName
                        SortableTestsModule = sortableTestsModule
                        SorterEvalModule = sorterEvalModule
                        ParameterSpans = moduleParameters @ parameterSpansSetup
                        QueryCatalogName = sossSetupQueryCatalogName
                        EnhancerFunctionName = sossSetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = sossExecutionFunctionName
                        ParameterSpans = parameterSpansRun
                        QueryCatalogName = executionQueryCatalogName
                        EnhancerFunctionName = sossExecutionEnhancerFunctionName
                    }
                } {
                    MakerNames = EvolutionMakerNames.sossPrefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                }
            )

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.p32.OnePool.SimpleS.Soss", description, channelCount, executionQueryCatalogName, [|
                runExample("SossS", SossS)
            |])

    module StandardAppend =

        let executionQueryCatalogName = standardAppendExecutionQueryCatalogName

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let StandardAppendS () : run =
            let names = RunNaming.create()
            evoContinuationConfig.create {
                ProjectName = names.ProjectName
                DatabaseName = names.DatabaseName
                RunName = names.RunName
                RunQueryCatalogName = executionQueryCatalogName
                Source = evoContinuationSource.create {
                    SourceRun = Standard.StandardS ()
                    GenerationInterval = 0
                    SourceMutationMods = [0 .. 3]
                    SelectedSorterPoolCount = 4
                }
                ExecutionParameterSpans = [ParamKeys.generationIntervalLastKey, ["1"]]
            } |> EvoAppend.createStandard

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.p32.OnePool.SimpleS.StandardAppend", description, channelCount, executionQueryCatalogName, [|
                runExample("StandardAppendS", StandardAppendS)
            |])

    module SossAppend =

        let executionQueryCatalogName = sossAppendExecutionQueryCatalogName

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let SossCondenseS () : run =
            let names = RunNaming.create()
            evoContinuationConfig.create {
                ProjectName = names.ProjectName
                DatabaseName = names.DatabaseName
                RunName = names.RunName
                RunQueryCatalogName = executionQueryCatalogName
                Source = evoContinuationSource.create {
                    SourceRun = Soss.SossS ()
                    GenerationInterval = 0
                    SourceMutationMods = [0 .. 3]
                    SelectedSorterPoolCount = 2
                }
                ExecutionParameterSpans = [
                    ParamKeys.generationIntervalLastKey, ["1"]
                    ParamKeys.mutationModKey, [0; 1] |> List.map string
                ]
            } |> EvoCondense.createSoss

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.p32.OnePool.SimpleS.SossAppend", description, channelCount, executionQueryCatalogName, [|
                runExample("SossCondenseS", SossCondenseS)
            |])
