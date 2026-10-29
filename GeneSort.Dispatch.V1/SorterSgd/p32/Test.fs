namespace GeneSort.Dispatch.V1.SorterSgd.p32
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

module Test =

    module OnePool =

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
        let standardSetupEnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32
        let sossSetupEnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32
        let standardSetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
        let sossSetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed

        let description = "Small single-pool Standard and Soss evolutionary runs."

        module Standard =

            let executionQueryCatalogName = standardExecutionQueryCatalogName

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let TestStandard ()  : run =
                let names = RunNaming.create()
                let parameterSpans =
                    [
                        (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                        (ParamKeys.orthoRateKey, ["4.001"])
                        (ParamKeys.sorterChildCountKey, ["1"])
                        (ParamKeys.distinctSorterHashesKey, ["true"])
                        (ParamKeys.prioritizeNewMutantsKey, ["true"])
                        (ParamKeys.sortedFractionKey, ["0.99"])
                        (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                        (ParamKeys.channelCountKey, [string (int channelCount)])
                        (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                        (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                        (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                        (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                        (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                        (ParamKeys.paraRateKey,    [1.15;] |> List.map string)
                        (ParamKeys.selfSymRateKey, [1.75;]  |> List.map string)
                        (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                        (ParamKeys.seedModificationRateKey, [0.020;] |> List.map string)
                        (ParamKeys.modificationRateKey, [0.035;]  |> List.map string)
                        (ParamKeys.mutatorVariantKey, [mutatorVariant.V2;] |> List.map (MutatorVariant.toString))
                    ]
                run.EvolutionRun (
                    SgdRun.create {
                        DatabaseName = names.DatabaseName
                        ProjectName = names.ProjectName
                        RunName = names.RunName
                        Description = "Rate comp for 32p4a Msuf4"
                        AllowOverwrite = false
                        Setup = {
                            FunctionName = standardSetupFunctionName
                            SortableTestsModule = sortableTestsModule
                            SorterEvalModule = sorterEvalModule
                            ParameterSpans = moduleParameters @ parameterSpans
                            QueryCatalogName = standardSetupQueryCatalogName
                            EnhancerFunctionName = standardSetupEnhancerFunctionName
                        }
                        Execution = {
                            FunctionName = standardExecutionFunctionName
                            ParameterSpans = []
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
                GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.Standard", description, channelCount, executionQueryCatalogName, [|
                    runExample("TestStandard", TestStandard)
                |])

        module Soss =

            let executionQueryCatalogName = sossExecutionQueryCatalogName

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let TestSoss ()  : run =
                let names = RunNaming.create()
                let parameterSpansSetup =
                    [
                        (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                        (ParamKeys.channelCountKey, [string (int channelCount)])
                        (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                        (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                        (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
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
                        (ParamKeys.partitionCountKey, [2;] |> List.map string)
                        (ParamKeys.generationsPerSubsetKey, [10] |> List.map string)
                        (ParamKeys.generationsPerMergedSetKey, [20;] |> List.map string)
                        (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                        (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                        (ParamKeys.paraRateKey,    [1.15;] |> List.map string)
                        (ParamKeys.selfSymRateKey, [1.75;]  |> List.map string)
                        (ParamKeys.seedModificationRateKey, [0.020;] |> List.map string)
                        (ParamKeys.modificationRateKey, [0.035;]  |> List.map string)
                        (ParamKeys.mutatorVariantKey, [mutatorVariant.V2;] |> List.map (MutatorVariant.toString))
                    ]
                run.EvolutionRun (
                    SgdRun.create {
                        DatabaseName = names.DatabaseName
                        ProjectName = names.ProjectName
                        RunName = names.RunName
                        Description = "Soss rate comparison for Msuf32p4a"
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
                GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.Soss", description, channelCount, executionQueryCatalogName, [|
                    runExample("TestSoss", TestSoss)
                |])


        module SimpleR =

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

            // AMax belongs to setup; the independent activator mutation rate belongs to execution.
            let aMax = 3
            let activatorMutationRate = 0.10

            let description = "Single-pool SimpleR runs and their append and condense continuations."

            module Standard =

                let executionQueryCatalogName = standardExecutionQueryCatalogName

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let Standard () : run =
                    let names = RunNaming.create()
                    let parameterSpansSetup =
                        [
                            (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                            (ParamKeys.channelCountKey, [string (int channelCount)])
                            (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                            (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                            (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                            (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                            (ParamKeys.generationIntervalLastKey, [0] |> List.map string)
                            (ParamKeys.paraRateKey, [1.15] |> List.map string)
                            (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                            (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                            (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                            (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                            (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                            (ParamKeys.aMaxKey, [aMax] |> List.map string)
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
                            Description = "OnePool SimpleR generation and evolution"
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
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.SimpleR.Standard", description, channelCount, executionQueryCatalogName, [|
                        runExample("Standard", Standard)
                    |])

            module Soss =

                let executionQueryCatalogName = sossExecutionQueryCatalogName

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let Soss () : run =
                    let names = RunNaming.create()
                    let parameterSpansSetup =
                        [
                            (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                            (ParamKeys.channelCountKey, [string (int channelCount)])
                            (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                            (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                            (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                            (ParamKeys.aMaxKey, [aMax] |> List.map string)
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
                            Description = "OnePool SimpleR generation and evolution"
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
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.SimpleR.Soss", description, channelCount, executionQueryCatalogName, [|
                        runExample("Soss", Soss)
                    |])

            module StandardAppend =

                let executionQueryCatalogName = standardAppendExecutionQueryCatalogName

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let StandardAppend () : run =
                    let names = RunNaming.create()
                    evoContinuationConfig.create {
                        ProjectName = names.ProjectName
                        DatabaseName = names.DatabaseName
                        RunName = names.RunName
                        RunQueryCatalogName = executionQueryCatalogName
                        Source = evoContinuationSource.create {
                            SourceRun = Standard.Standard ()
                            GenerationInterval = 0
                            SourceMutationMods = [0 .. 3]
                            SelectedSorterPoolCount = 4
                        }
                        ExecutionParameterSpans = [ParamKeys.generationIntervalLastKey, ["1"]]
                    } |> EvoAppend.createStandard

                let runType =
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.SimpleR.StandardAppend", description, channelCount, executionQueryCatalogName, [|
                        runExample("StandardAppend", StandardAppend)
                    |])

            module SossAppend =

                let executionQueryCatalogName = sossAppendExecutionQueryCatalogName

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let SossCondense () : run =
                    let names = RunNaming.create()
                    evoContinuationConfig.create {
                        ProjectName = names.ProjectName
                        DatabaseName = names.DatabaseName
                        RunName = names.RunName
                        RunQueryCatalogName = executionQueryCatalogName
                        Source = evoContinuationSource.create {
                            SourceRun = Soss.Soss ()
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
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.SimpleR.SossAppend", description, channelCount, executionQueryCatalogName, [|
                        runExample("SossCondense", SossCondense)
                    |])


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
                            (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
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
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.SimpleS.Standard", description, channelCount, executionQueryCatalogName, [|
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
                            (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
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
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.SimpleS.Soss", description, channelCount, executionQueryCatalogName, [|
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
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.SimpleS.StandardAppend", description, channelCount, executionQueryCatalogName, [|
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
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.SimpleS.SossAppend", description, channelCount, executionQueryCatalogName, [|
                        runExample("SossCondenseS", SossCondenseS)
                    |])


        module Controlled =

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
            let standardSetupEnhancerFunctionName = SetupEnhancerFunctionNames.standardMsuf32
            let sossSetupEnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32
            let standardSetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
            let sossSetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed

            let replicaCount = 5
            let generationCount = 100
            let aMax = 3
            let activationProbability = float aMax / (2.0 * float aMax + 1.0)
            let activatorMutationRate = 0.1

            // Only 0 -> 1 and 1 -> 0 change activity; each has probability rate / 2.
            // This matches the uniform seed distribution, not an arbitrary evolved distribution.
            let matchedFlipRate = activatorMutationRate / float (2 * aMax + 1)

            let description = "Controlled single-pool comparisons of evolutionary models and settings."

            module Standard =

                let executionQueryCatalogName = standardExecutionQueryCatalogName

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let StandardSimple () : run =
                    let names = RunNaming.create()
                    let parameterSpansSetup =
                        [
                            (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                            (ParamKeys.channelCountKey, [string (int channelCount)])
                            (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                            (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                            (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                            (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                            (ParamKeys.paraRateKey, [1.15] |> List.map string)
                            (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                            (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                            (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                            (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                            (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                            (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                        ]
                    let parameterSpansRun =
                        [
                            (ParamKeys.orthoRateKey, ["4.001"])
                            (ParamKeys.sorterChildCountKey, ["1"])
                            (ParamKeys.distinctSorterHashesKey, ["true"])
                            (ParamKeys.prioritizeNewMutantsKey, ["true"])
                            (ParamKeys.sortedFractionKey, ["0.99"])
                            (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                            (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                        ]
                    run.EvolutionRun (
                        SgdRun.create {
                            DatabaseName = names.DatabaseName
                            ProjectName = names.ProjectName
                            RunName = names.RunName
                            Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
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

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let StandardR () : run =
                    let names = RunNaming.create()
                    let parameterSpansSetup =
                        [
                            (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                            (ParamKeys.channelCountKey, [string (int channelCount)])
                            (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                            (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                            (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                            (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                            (ParamKeys.paraRateKey, [1.15] |> List.map string)
                            (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                            (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                            (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                            (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                            (ParamKeys.aMaxKey, [aMax] |> List.map string)
                            (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                            (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                        ]
                    let parameterSpansRun =
                        [
                            (ParamKeys.orthoRateKey, ["4.001"])
                            (ParamKeys.sorterChildCountKey, ["1"])
                            (ParamKeys.distinctSorterHashesKey, ["true"])
                            (ParamKeys.prioritizeNewMutantsKey, ["true"])
                            (ParamKeys.sortedFractionKey, ["0.99"])
                            (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                            (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                            (ParamKeys.activatorMutationRateKey, [activatorMutationRate] |> List.map string)
                        ]
                    run.EvolutionRun (
                        SgdRun.create {
                            DatabaseName = names.DatabaseName
                            ProjectName = names.ProjectName
                            RunName = names.RunName
                            Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
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

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let CompareStandardS () : run =
                    let names = RunNaming.create()
                    let parameterSpansSetup =
                        [
                            (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                            (ParamKeys.channelCountKey, [string (int channelCount)])
                            (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                            (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                            (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                            (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                            (ParamKeys.paraRateKey, [1.15] |> List.map string)
                            (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                            (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                            (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                            (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                            (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                            (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                            (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                        ]
                    let parameterSpansRun =
                        [
                            (ParamKeys.orthoRateKey, ["4.001"])
                            (ParamKeys.sorterChildCountKey, ["1"])
                            (ParamKeys.distinctSorterHashesKey, ["true"])
                            (ParamKeys.prioritizeNewMutantsKey, ["true"])
                            (ParamKeys.sortedFractionKey, ["0.99"])
                            (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                            (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                            (ParamKeys.activatorMutationRateKey, [activatorMutationRate] |> List.map string)
                        ]
                    run.EvolutionRun (
                        SgdRun.create {
                            DatabaseName = names.DatabaseName
                            ProjectName = names.ProjectName
                            RunName = names.RunName
                            Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
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

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let MatchedStandardS () : run =
                    let names = RunNaming.create()
                    let parameterSpansSetup =
                        [
                            (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                            (ParamKeys.channelCountKey, [string (int channelCount)])
                            (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                            (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                            (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                            (ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string)
                            (ParamKeys.paraRateKey, [1.15] |> List.map string)
                            (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                            (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                            (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                            (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                            (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                            (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                            (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                        ]
                    let parameterSpansRun =
                        [
                            (ParamKeys.orthoRateKey, ["4.001"])
                            (ParamKeys.sorterChildCountKey, ["1"])
                            (ParamKeys.distinctSorterHashesKey, ["true"])
                            (ParamKeys.prioritizeNewMutantsKey, ["true"])
                            (ParamKeys.sortedFractionKey, ["0.99"])
                            (ParamKeys.selectedSorterCountPerPoolKey, ["All"])
                            (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                            (ParamKeys.activatorMutationRateKey, [matchedFlipRate] |> List.map string)
                        ]
                    run.EvolutionRun (
                        SgdRun.create {
                            DatabaseName = names.DatabaseName
                            ProjectName = names.ProjectName
                            RunName = names.RunName
                            Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
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
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.Controlled.Standard", description, channelCount, executionQueryCatalogName, [|
                        runExample("StandardSimple", StandardSimple)
                        runExample("StandardR", StandardR)
                        runExample("CompareStandardS", CompareStandardS)
                        runExample("MatchedStandardS", MatchedStandardS)
                    |])

            module Soss =

                let executionQueryCatalogName = sossExecutionQueryCatalogName

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let SossSimple () : run =
                    let names = RunNaming.create()
                    let parameterSpansSetup =
                        [
                            (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                            (ParamKeys.channelCountKey, [string (int channelCount)])
                            (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                            (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                            (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                            (ParamKeys.mutationModKey, [0; 1] |> List.map string)
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
                            (ParamKeys.paraRateKey, [1.15] |> List.map string)
                            (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                            (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                            (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                            (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                            (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                        ]
                    run.EvolutionRun (
                        SgdRun.create {
                            DatabaseName = names.DatabaseName
                            ProjectName = names.ProjectName
                            RunName = names.RunName
                            Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
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

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let SossR () : run =
                    let names = RunNaming.create()
                    let parameterSpansSetup =
                        [
                            (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                            (ParamKeys.channelCountKey, [string (int channelCount)])
                            (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                            (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                            (ParamKeys.aMaxKey, [aMax] |> List.map string)
                            (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                            (ParamKeys.mutationModKey, [0; 1] |> List.map string)
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
                            (ParamKeys.paraRateKey, [1.15] |> List.map string)
                            (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                            (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                            (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                            (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                            (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                            (ParamKeys.activatorMutationRateKey, [activatorMutationRate] |> List.map string)
                        ]
                    run.EvolutionRun (
                        SgdRun.create {
                            DatabaseName = names.DatabaseName
                            ProjectName = names.ProjectName
                            RunName = names.RunName
                            Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
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

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let CompareSossS () : run =
                    let names = RunNaming.create()
                    let parameterSpansSetup =
                        [
                            (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                            (ParamKeys.channelCountKey, [string (int channelCount)])
                            (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                            (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                            (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                            (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                            (ParamKeys.mutationModKey, [0; 1] |> List.map string)
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
                            (ParamKeys.paraRateKey, [1.15] |> List.map string)
                            (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                            (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                            (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                            (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                            (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                            (ParamKeys.activatorMutationRateKey, [activatorMutationRate] |> List.map string)
                        ]
                    run.EvolutionRun (
                        SgdRun.create {
                            DatabaseName = names.DatabaseName
                            ProjectName = names.ProjectName
                            RunName = names.RunName
                            Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
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

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let MatchedSossS () : run =
                    let names = RunNaming.create()
                    let parameterSpansSetup =
                        [
                            (ParamKeys.seedSorterPoolSelectionTypeKey, ["GuidOrder:512"])
                            (ParamKeys.channelCountKey, [string (int channelCount)])
                            (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                            (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                            (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                            (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                            (ParamKeys.mutationModKey, [0; 1] |> List.map string)
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
                            (ParamKeys.paraRateKey, [1.15] |> List.map string)
                            (ParamKeys.selfSymRateKey, [1.75] |> List.map string)
                            (ParamKeys.seedModificationRateKey, [0.020] |> List.map string)
                            (ParamKeys.modificationRateKey, [0.035] |> List.map string)
                            (ParamKeys.mutatorVariantKey, [mutatorVariant.V2] |> List.map MutatorVariant.toString)
                            (ParamKeys.generationIntervalLastKey, [generationCount / 10 - 1] |> List.map string)
                            (ParamKeys.activatorMutationRateKey, [matchedFlipRate] |> List.map string)
                        ]
                    run.EvolutionRun (
                        SgdRun.create {
                            DatabaseName = names.DatabaseName
                            ProjectName = names.ProjectName
                            RunName = names.RunName
                            Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
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
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.Controlled.Soss", description, channelCount, executionQueryCatalogName, [|
                        runExample("SossSimple", SossSimple)
                        runExample("SossR", SossR)
                        runExample("CompareSossS", CompareSossS)
                        runExample("MatchedSossS", MatchedSossS)
                    |])


        module Continuation =

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

            let standardAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
            let sossAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend

            let description = "Single-pool tests that append or condense a saved evolutionary state."

            module StandardAppend =

                let executionQueryCatalogName = standardAppendExecutionQueryCatalogName

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let TestStandardAppend () : run =
                    let names = RunNaming.create()
                    evoContinuationConfig.create {
                        ProjectName = names.ProjectName
                        DatabaseName = names.DatabaseName
                        RunName = names.RunName
                        RunQueryCatalogName = executionQueryCatalogName
                        Source = evoContinuationSource.create {
                            SourceRun = Standard.TestStandard ()
                            GenerationInterval = 0
                            SourceMutationMods = [0]
                            SelectedSorterPoolCount = 1
                        }
                        ExecutionParameterSpans = [
                            ParamKeys.mutationModKey, [0 .. 2] |> List.map string
                            ParamKeys.generationIntervalLastKey, ["1"]
                        ]
                    }
                    |> EvoAppend.createStandard

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let TestStandardCondense () : run =
                    let names = RunNaming.create()
                    evoContinuationConfig.create {
                        ProjectName = names.ProjectName
                        DatabaseName = names.DatabaseName
                        RunName = names.RunName
                        RunQueryCatalogName = executionQueryCatalogName
                        Source = evoContinuationSource.create {
                            SourceRun = Standard.TestStandard ()
                            GenerationInterval = 0
                            SourceMutationMods = [0 .. 3]
                            SelectedSorterPoolCount = 2
                        }
                        ExecutionParameterSpans = [
                            ParamKeys.generationIntervalLastKey, ["1"]
                        ]
                    }
                    |> EvoCondense.createStandard

                let runType =
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.Continuation.StandardAppend", description, channelCount, executionQueryCatalogName, [|
                        runExample("TestStandardAppend", TestStandardAppend)
                        runExample("TestStandardCondense", TestStandardCondense)
                    |])

            module SossAppend =

                let executionQueryCatalogName = sossAppendExecutionQueryCatalogName

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let TestSossAppend () : run =
                    let names = RunNaming.create()
                    evoContinuationConfig.create {
                        ProjectName = names.ProjectName
                        DatabaseName = names.DatabaseName
                        RunName = names.RunName
                        RunQueryCatalogName = executionQueryCatalogName
                        Source = evoContinuationSource.create {
                            SourceRun = Soss.TestSoss ()
                            GenerationInterval = 0
                            SourceMutationMods = [0]
                            SelectedSorterPoolCount = 1
                        }
                        ExecutionParameterSpans = [
                            ParamKeys.mutationModKey, [0 .. 2] |> List.map string
                            ParamKeys.generationIntervalLastKey, ["1"]
                        ]
                    }
                    |> EvoAppend.createSoss

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let TestSossCondense () : run =
                    let names = RunNaming.create()
                    evoContinuationConfig.create {
                        ProjectName = names.ProjectName
                        DatabaseName = names.DatabaseName
                        RunName = names.RunName
                        RunQueryCatalogName = executionQueryCatalogName
                        Source = evoContinuationSource.create {
                            SourceRun = Soss.TestSoss ()
                            GenerationInterval = 0
                            SourceMutationMods = [0 .. 3]
                            SelectedSorterPoolCount = 2
                        }
                        ExecutionParameterSpans = [
                            ParamKeys.mutationModKey, [0 .. 3] |> List.map string
                            ParamKeys.generationIntervalLastKey, ["1"]
                        ]
                    }
                    |> EvoCondense.createSoss

                let runType =
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.Continuation.SossAppend", description, channelCount, executionQueryCatalogName, [|
                        runExample("TestSossAppend", TestSossAppend)
                        runExample("TestSossCondense", TestSossCondense)
                    |])


        module SameFamilyChain =

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

            let standardAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
            let sossAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend

            let description = "Chains of append and condense runs within the same workflow family."

            module StandardAppend =

                let executionQueryCatalogName = standardAppendExecutionQueryCatalogName

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let TestStandardAppendCondense () : run =
                    let names = RunNaming.create()
                    evoContinuationConfig.create {
                        ProjectName = names.ProjectName
                        DatabaseName = names.DatabaseName
                        RunName = names.RunName
                        RunQueryCatalogName = executionQueryCatalogName
                        Source = evoContinuationSource.create {
                            SourceRun = Continuation.StandardAppend.TestStandardAppend ()
                            GenerationInterval = 1
                            SourceMutationMods = [0 .. 2]
                            SelectedSorterPoolCount = 2
                        }
                        ExecutionParameterSpans = [
                            ParamKeys.generationIntervalLastKey, ["2"]
                        ]
                    }
                    |> EvoCondense.createStandard

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let TestStandardCondenseAppend () : run =
                    let names = RunNaming.create()
                    evoContinuationConfig.create {
                        ProjectName = names.ProjectName
                        DatabaseName = names.DatabaseName
                        RunName = names.RunName
                        RunQueryCatalogName = executionQueryCatalogName
                        Source = evoContinuationSource.create {
                            SourceRun = Continuation.StandardAppend.TestStandardCondense ()
                            GenerationInterval = 1
                            SourceMutationMods = [0 .. 3]
                            SelectedSorterPoolCount = 2
                        }
                        ExecutionParameterSpans = [
                            ParamKeys.generationIntervalLastKey, ["2"]
                        ]
                    }
                    |> EvoAppend.createStandard

                let runType =
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.SameFamilyChain.StandardAppend", description, channelCount, executionQueryCatalogName, [|
                        runExample("TestStandardAppendCondense", TestStandardAppendCondense)
                        runExample("TestStandardCondenseAppend", TestStandardCondenseAppend)
                    |])

            module SossAppend =

                let executionQueryCatalogName = sossAppendExecutionQueryCatalogName

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let TestSossAppendCondense () : run =
                    let names = RunNaming.create()
                    evoContinuationConfig.create {
                        ProjectName = names.ProjectName
                        DatabaseName = names.DatabaseName
                        RunName = names.RunName
                        RunQueryCatalogName = executionQueryCatalogName
                        Source = evoContinuationSource.create {
                            SourceRun = Continuation.SossAppend.TestSossAppend ()
                            GenerationInterval = 1
                            SourceMutationMods = [0 .. 2]
                            SelectedSorterPoolCount = 2
                        }
                        ExecutionParameterSpans = [
                            ParamKeys.generationIntervalLastKey, ["2"]
                        ]
                    }
                    |> EvoCondense.createSoss

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let TestSossCondenseAppend () : run =
                    let names = RunNaming.create()
                    evoContinuationConfig.create {
                        ProjectName = names.ProjectName
                        DatabaseName = names.DatabaseName
                        RunName = names.RunName
                        RunQueryCatalogName = executionQueryCatalogName
                        Source = evoContinuationSource.create {
                            SourceRun = Continuation.SossAppend.TestSossCondense ()
                            GenerationInterval = 1
                            SourceMutationMods = [0 .. 3]
                            SelectedSorterPoolCount = 8
                        }
                        ExecutionParameterSpans = [
                            ParamKeys.generationIntervalLastKey, ["2"]
                        ]
                    }
                    |> EvoAppend.createSoss

                let runType =
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.SameFamilyChain.SossAppend", description, channelCount, executionQueryCatalogName, [|
                        runExample("TestSossAppendCondense", TestSossAppendCondense)
                        runExample("TestSossCondenseAppend", TestSossCondenseAppend)
                    |])


        module MixedFamilyChain =

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

            let sossAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
            let standardAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend

            let description = "Chains of append and condense runs switching between Standard and Soss."

            module StandardAppend =

                let executionQueryCatalogName = standardAppendExecutionQueryCatalogName

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let TestSossAppendStandardCondense () : run =
                    let names = RunNaming.create()
                    evoContinuationConfig.create {
                        ProjectName = names.ProjectName
                        DatabaseName = names.DatabaseName
                        RunName = names.RunName
                        RunQueryCatalogName = executionQueryCatalogName
                        Source = evoContinuationSource.create {
                            SourceRun = Continuation.SossAppend.TestSossAppend ()
                            GenerationInterval = 1
                            SourceMutationMods = [0 .. 2]
                            SelectedSorterPoolCount = 2
                        }
                        ExecutionParameterSpans = [
                            ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string
                            ParamKeys.generationIntervalLastKey, ["2"]
                        ]
                    }
                    |> EvoCondense.createStandard

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let TestSossCondenseStandardAppend () : run =
                    let names = RunNaming.create()
                    evoContinuationConfig.create {
                        ProjectName = names.ProjectName
                        DatabaseName = names.DatabaseName
                        RunName = names.RunName
                        RunQueryCatalogName = executionQueryCatalogName
                        Source = evoContinuationSource.create {
                            SourceRun = Continuation.SossAppend.TestSossCondense ()
                            GenerationInterval = 1
                            SourceMutationMods = [0 .. 3]
                            SelectedSorterPoolCount = 8
                        }
                        ExecutionParameterSpans = [
                            ParamKeys.adjustCeLengthFrequencyKey, [10] |> List.map string
                            ParamKeys.generationIntervalLastKey, ["2"]
                        ]
                    }
                    |> EvoAppend.createStandard

                let runType =
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.MixedFamilyChain.StandardAppend", description, channelCount, executionQueryCatalogName, [|
                        runExample("TestSossAppendStandardCondense", TestSossAppendStandardCondense)
                        runExample("TestSossCondenseStandardAppend", TestSossCondenseStandardAppend)
                    |])

            module SossAppend =

                let executionQueryCatalogName = sossAppendExecutionQueryCatalogName

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let TestStandardAppendSossCondense () : run =
                    let names = RunNaming.create()
                    evoContinuationConfig.create {
                        ProjectName = names.ProjectName
                        DatabaseName = names.DatabaseName
                        RunName = names.RunName
                        RunQueryCatalogName = executionQueryCatalogName
                        Source = evoContinuationSource.create {
                            SourceRun = Continuation.StandardAppend.TestStandardAppend ()
                            GenerationInterval = 1
                            SourceMutationMods = [0 .. 2]
                            SelectedSorterPoolCount = 2
                        }
                        ExecutionParameterSpans = [
                            ParamKeys.seedSossKey, [1234UL] |> List.map string
                            ParamKeys.partitionCountKey, [2] |> List.map string
                            ParamKeys.generationsPerSubsetKey, [10] |> List.map string
                            ParamKeys.generationsPerMergedSetKey, [20] |> List.map string
                            ParamKeys.generationIntervalLastKey, ["2"]
                        ]
                    }
                    |> EvoCondense.createSoss

                [<MethodImpl(MethodImplOptions.NoInlining)>]
                let TestStandardCondenseSossAppend () : run =
                    let names = RunNaming.create()
                    evoContinuationConfig.create {
                        ProjectName = names.ProjectName
                        DatabaseName = names.DatabaseName
                        RunName = names.RunName
                        RunQueryCatalogName = executionQueryCatalogName
                        Source = evoContinuationSource.create {
                            SourceRun = Continuation.StandardAppend.TestStandardCondense ()
                            GenerationInterval = 1
                            SourceMutationMods = [0 .. 3]
                            SelectedSorterPoolCount = 2
                        }
                        ExecutionParameterSpans = [
                            ParamKeys.seedSossKey, [1234UL] |> List.map string
                            ParamKeys.partitionCountKey, [2] |> List.map string
                            ParamKeys.generationsPerSubsetKey, [10] |> List.map string
                            ParamKeys.generationsPerMergedSetKey, [20] |> List.map string
                            ParamKeys.generationIntervalLastKey, ["2"]
                        ]
                    }
                    |> EvoAppend.createSoss

                let runType =
                    GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.MixedFamilyChain.SossAppend", description, channelCount, executionQueryCatalogName, [|
                        runExample("TestStandardAppendSossCondense", TestStandardAppendSossCondense)
                        runExample("TestStandardCondenseSossAppend", TestStandardCondenseSossAppend)
                    |])
