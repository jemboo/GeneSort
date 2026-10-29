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

        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let TestStandard ()  : run =
            let names = RunNaming.create()
            let parameterSpans =
                [
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
                        SortableTestsDatabaseName = UMX.tag "Prefix"
                        SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                        ParameterSpans = parameterSpans
                        QueryCatalogName = standardSetupQueryCatalogName
                        EnhancerFunctionName = standardSetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = standardExecutionFunctionName
                        ParameterSpans = []
                        QueryCatalogName = standardExecutionQueryCatalogName
                        EnhancerFunctionName = standardExecutionEnhancerFunctionName
                    }
                } {
                    MakerNames = EvolutionMakerNames.prefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1 
                }
            )


        [<MethodImpl(MethodImplOptions.NoInlining)>]
        let TestSoss ()  : run =
            let names = RunNaming.create()
            let parameterSpansSetup =
                [
                    (ParamKeys.sorterCountPerPoolKey, [32] |>  List.map string)
                    (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                    (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                ]
            let parameterSpansRun =
                [
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
                        SortableTestsDatabaseName = UMX.tag "Prefix"
                        SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                        ParameterSpans = parameterSpansSetup
                        QueryCatalogName = sossSetupQueryCatalogName
                        EnhancerFunctionName = sossSetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = sossExecutionFunctionName
                        ParameterSpans = parameterSpansRun
                        QueryCatalogName = sossExecutionQueryCatalogName
                        EnhancerFunctionName = sossExecutionEnhancerFunctionName
                    }
                } {
                    MakerNames = EvolutionMakerNames.sossPrefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                }
            )




        let description = "Small single-pool Standard and Soss evolutionary runs."

        let runType =
            GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool", description, [|
                runExample("TestStandard", TestStandard)
                runExample("TestSoss", TestSoss)
            |])

        module SimpleR =

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

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let Standard () : run =
                let names = RunNaming.create()
                let parameterSpansSetup =
                    [
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
                            SortableTestsDatabaseName = UMX.tag "Prefix"
                            SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                            ParameterSpans = parameterSpansSetup
                            QueryCatalogName = standardSetupQueryCatalogName
                            EnhancerFunctionName = standardSetupEnhancerFunctionName
                        }
                        Execution = {
                            FunctionName = standardExecutionFunctionName
                            ParameterSpans = parameterSpansRun
                            QueryCatalogName = standardExecutionQueryCatalogName
                            EnhancerFunctionName = standardExecutionEnhancerFunctionName
                        }
                    } {
                        MakerNames = EvolutionMakerNames.prefix
                        GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                        GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                    }
                )

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let Soss () : run =
                let names = RunNaming.create()
                let parameterSpansSetup =
                    [
                        (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                        (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                        (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                        (ParamKeys.aMaxKey, [aMax] |> List.map string)
                    ]
                let parameterSpansRun =
                    [
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
                            SortableTestsDatabaseName = UMX.tag "Prefix"
                            SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                            ParameterSpans = parameterSpansSetup
                            QueryCatalogName = sossSetupQueryCatalogName
                            EnhancerFunctionName = sossSetupEnhancerFunctionName
                        }
                        Execution = {
                            FunctionName = sossExecutionFunctionName
                            ParameterSpans = parameterSpansRun
                            QueryCatalogName = sossExecutionQueryCatalogName
                            EnhancerFunctionName = sossExecutionEnhancerFunctionName
                        }
                    } {
                        MakerNames = EvolutionMakerNames.sossPrefix
                        GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                        GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                    }
                )

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let StandardAppend () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    RunQueryCatalogName = standardAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = Standard ()
                        GenerationInterval = 0
                        SourceMutationMods = [0 .. 3]
                        SelectedSorterPoolCount = 4
                    }
                    ExecutionParameterSpans = [ParamKeys.generationIntervalLastKey, ["1"]]
                } |> EvoAppend.createStandard

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let SossCondense () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    RunQueryCatalogName = sossAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = Soss ()
                        GenerationInterval = 0
                        SourceMutationMods = [0 .. 3]
                        SelectedSorterPoolCount = 2
                    }
                    ExecutionParameterSpans = [
                        ParamKeys.generationIntervalLastKey, ["1"]
                        ParamKeys.mutationModKey, [0; 1] |> List.map string
                    ]
                } |> EvoCondense.createSoss


            let description = "Single-pool SimpleR runs and their append and condense continuations."

            let runType =
                GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.SimpleR", description, [|
                    runExample("Standard", Standard)
                    runExample("Soss", Soss)
                    runExample("StandardAppend", StandardAppend)
                    runExample("SossCondense", SossCondense)
                |])

        module SimpleS =

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

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let StandardS () : run =
                let names = RunNaming.create()
                let parameterSpansSetup =
                    [
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
                            SortableTestsDatabaseName = UMX.tag "Prefix"
                            SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                            ParameterSpans = parameterSpansSetup
                            QueryCatalogName = standardSetupQueryCatalogName
                            EnhancerFunctionName = standardSetupEnhancerFunctionName
                        }
                        Execution = {
                            FunctionName = standardExecutionFunctionName
                            ParameterSpans = parameterSpansRun
                            QueryCatalogName = standardExecutionQueryCatalogName
                            EnhancerFunctionName = standardExecutionEnhancerFunctionName
                        }
                    } {
                        MakerNames = EvolutionMakerNames.prefix
                        GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                        GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                    }
                )

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let SossS () : run =
                let names = RunNaming.create()
                let parameterSpansSetup =
                    [
                        (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                        (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                        (ParamKeys.mutationModKey, [0 .. 3] |> List.map string)
                        (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                    ]
                let parameterSpansRun =
                    [
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
                            SortableTestsDatabaseName = UMX.tag "Prefix"
                            SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                            ParameterSpans = parameterSpansSetup
                            QueryCatalogName = sossSetupQueryCatalogName
                            EnhancerFunctionName = sossSetupEnhancerFunctionName
                        }
                        Execution = {
                            FunctionName = sossExecutionFunctionName
                            ParameterSpans = parameterSpansRun
                            QueryCatalogName = sossExecutionQueryCatalogName
                            EnhancerFunctionName = sossExecutionEnhancerFunctionName
                        }
                    } {
                        MakerNames = EvolutionMakerNames.sossPrefix
                        GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                        GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                    }
                )

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let StandardAppendS () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    RunQueryCatalogName = standardAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = StandardS ()
                        GenerationInterval = 0
                        SourceMutationMods = [0 .. 3]
                        SelectedSorterPoolCount = 4
                    }
                    ExecutionParameterSpans = [ParamKeys.generationIntervalLastKey, ["1"]]
                } |> EvoAppend.createStandard

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let SossCondenseS () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    RunQueryCatalogName = sossAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = SossS ()
                        GenerationInterval = 0
                        SourceMutationMods = [0 .. 3]
                        SelectedSorterPoolCount = 2
                    }
                    ExecutionParameterSpans = [
                        ParamKeys.generationIntervalLastKey, ["1"]
                        ParamKeys.mutationModKey, [0; 1] |> List.map string
                    ]
                } |> EvoCondense.createSoss


            let description = "Single-pool SimpleS runs and their append and condense continuations."

            let runType =
                GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.SimpleS", description, [|
                    runExample("StandardS", StandardS)
                    runExample("SossS", SossS)
                    runExample("StandardAppendS", StandardAppendS)
                    runExample("SossCondenseS", SossCondenseS)
                |])

        module Controlled =

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

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let StandardSimple () : run =
                let names = RunNaming.create()
                let parameterSpansSetup =
                    [
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
                            SortableTestsDatabaseName = UMX.tag "Prefix"
                            SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                            ParameterSpans = parameterSpansSetup
                            QueryCatalogName = standardSetupQueryCatalogName
                            EnhancerFunctionName = standardSetupEnhancerFunctionName
                        }
                        Execution = {
                            FunctionName = standardExecutionFunctionName
                            ParameterSpans = parameterSpansRun
                            QueryCatalogName = standardExecutionQueryCatalogName
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
                            SortableTestsDatabaseName = UMX.tag "Prefix"
                            SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                            ParameterSpans = parameterSpansSetup
                            QueryCatalogName = standardSetupQueryCatalogName
                            EnhancerFunctionName = standardSetupEnhancerFunctionName
                        }
                        Execution = {
                            FunctionName = standardExecutionFunctionName
                            ParameterSpans = parameterSpansRun
                            QueryCatalogName = standardExecutionQueryCatalogName
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
                            SortableTestsDatabaseName = UMX.tag "Prefix"
                            SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                            ParameterSpans = parameterSpansSetup
                            QueryCatalogName = standardSetupQueryCatalogName
                            EnhancerFunctionName = standardSetupEnhancerFunctionName
                        }
                        Execution = {
                            FunctionName = standardExecutionFunctionName
                            ParameterSpans = parameterSpansRun
                            QueryCatalogName = standardExecutionQueryCatalogName
                            EnhancerFunctionName = standardExecutionEnhancerFunctionName
                        }
                    } {
                        MakerNames = EvolutionMakerNames.prefix
                        GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                        GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                    }
                )

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let SossSimple () : run =
                let names = RunNaming.create()
                let parameterSpansSetup =
                    [
                        (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                        (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                        (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                        (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                    ]
                let parameterSpansRun =
                    [
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
                            SortableTestsDatabaseName = UMX.tag "Prefix"
                            SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                            ParameterSpans = parameterSpansSetup
                            QueryCatalogName = sossSetupQueryCatalogName
                            EnhancerFunctionName = sossSetupEnhancerFunctionName
                        }
                        Execution = {
                            FunctionName = sossExecutionFunctionName
                            ParameterSpans = parameterSpansRun
                            QueryCatalogName = sossExecutionQueryCatalogName
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
                        (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                        (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                        (ParamKeys.aMaxKey, [aMax] |> List.map string)
                        (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                        (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                    ]
                let parameterSpansRun =
                    [
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
                            SortableTestsDatabaseName = UMX.tag "Prefix"
                            SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                            ParameterSpans = parameterSpansSetup
                            QueryCatalogName = sossSetupQueryCatalogName
                            EnhancerFunctionName = sossSetupEnhancerFunctionName
                        }
                        Execution = {
                            FunctionName = sossExecutionFunctionName
                            ParameterSpans = parameterSpansRun
                            QueryCatalogName = sossExecutionQueryCatalogName
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
                        (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                        (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                        (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                        (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                        (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                    ]
                let parameterSpansRun =
                    [
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
                            SortableTestsDatabaseName = UMX.tag "Prefix"
                            SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                            ParameterSpans = parameterSpansSetup
                            QueryCatalogName = sossSetupQueryCatalogName
                            EnhancerFunctionName = sossSetupEnhancerFunctionName
                        }
                        Execution = {
                            FunctionName = sossExecutionFunctionName
                            ParameterSpans = parameterSpansRun
                            QueryCatalogName = sossExecutionQueryCatalogName
                            EnhancerFunctionName = sossExecutionEnhancerFunctionName
                        }
                    } {
                        MakerNames = EvolutionMakerNames.sossPrefix
                        GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                        GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                    }
                )

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let MatchedStandardS () : run =
                let names = RunNaming.create()
                let parameterSpansSetup =
                    [
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
                            SortableTestsDatabaseName = UMX.tag "Prefix"
                            SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                            ParameterSpans = parameterSpansSetup
                            QueryCatalogName = standardSetupQueryCatalogName
                            EnhancerFunctionName = standardSetupEnhancerFunctionName
                        }
                        Execution = {
                            FunctionName = standardExecutionFunctionName
                            ParameterSpans = parameterSpansRun
                            QueryCatalogName = standardExecutionQueryCatalogName
                            EnhancerFunctionName = standardExecutionEnhancerFunctionName
                        }
                    } {
                        MakerNames = EvolutionMakerNames.prefix
                        GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                        GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                    }
                )

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let MatchedSossS () : run =
                let names = RunNaming.create()
                let parameterSpansSetup =
                    [
                        (ParamKeys.sorterCountPerPoolKey, [32] |> List.map string)
                        (ParamKeys.sorterCountPerPoolSetKey, [32] |> List.map string)
                        (ParamKeys.activationProbabilityKey, [activationProbability] |> List.map string)
                        (ParamKeys.controlledSeedAMaxKey, [aMax] |> List.map string)
                        (ParamKeys.mutationModKey, [0; 1] |> List.map string)
                    ]
                let parameterSpansRun =
                    [
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
                            SortableTestsDatabaseName = UMX.tag "Prefix"
                            SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                            ParameterSpans = parameterSpansSetup
                            QueryCatalogName = sossSetupQueryCatalogName
                            EnhancerFunctionName = sossSetupEnhancerFunctionName
                        }
                        Execution = {
                            FunctionName = sossExecutionFunctionName
                            ParameterSpans = parameterSpansRun
                            QueryCatalogName = sossExecutionQueryCatalogName
                            EnhancerFunctionName = sossExecutionEnhancerFunctionName
                        }
                    } {
                        MakerNames = EvolutionMakerNames.sossPrefix
                        GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                        GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                    }
                )


            let description = "Controlled single-pool comparisons of evolutionary models and settings."

            let runType =
                GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.Controlled", description, [|
                    runExample("StandardSimple", StandardSimple)
                    runExample("StandardR", StandardR)
                    runExample("CompareStandardS", CompareStandardS)
                    runExample("SossSimple", SossSimple)
                    runExample("SossR", SossR)
                    runExample("CompareSossS", CompareSossS)
                    runExample("MatchedStandardS", MatchedStandardS)
                    runExample("MatchedSossS", MatchedSossS)
                |])

        module Continuation =

            let standardAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
            let sossAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let TestStandardAppend () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    RunQueryCatalogName = standardAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = TestStandard ()
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
                    RunQueryCatalogName = standardAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = TestStandard ()
                        GenerationInterval = 0
                        SourceMutationMods = [0 .. 3]
                        SelectedSorterPoolCount = 2
                    }
                    ExecutionParameterSpans = [
                        ParamKeys.generationIntervalLastKey, ["1"]
                    ]
                }
                |> EvoCondense.createStandard

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let TestSossAppend () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    RunQueryCatalogName = sossAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = TestSoss ()
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
                    RunQueryCatalogName = sossAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = TestSoss ()
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




            let description = "Single-pool tests that append or condense a saved evolutionary state."

            let runType =
                GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.Continuation", description, [|
                    runExample("TestStandardAppend", TestStandardAppend)
                    runExample("TestStandardCondense", TestStandardCondense)
                    runExample("TestSossAppend", TestSossAppend)
                    runExample("TestSossCondense", TestSossCondense)
                |])

        module SameFamilyChain =

            let standardAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
            let sossAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let TestStandardAppendCondense () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    RunQueryCatalogName = standardAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = Continuation.TestStandardAppend ()
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
                    RunQueryCatalogName = standardAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = Continuation.TestStandardCondense ()
                        GenerationInterval = 1
                        SourceMutationMods = [0 .. 3]
                        SelectedSorterPoolCount = 2
                    }
                    ExecutionParameterSpans = [
                        ParamKeys.generationIntervalLastKey, ["2"]
                    ]
                }
                |> EvoAppend.createStandard

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let TestSossAppendCondense () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    RunQueryCatalogName = sossAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = Continuation.TestSossAppend ()
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
                    RunQueryCatalogName = sossAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = Continuation.TestSossCondense ()
                        GenerationInterval = 1
                        SourceMutationMods = [0 .. 3]
                        SelectedSorterPoolCount = 8
                    }
                    ExecutionParameterSpans = [
                        ParamKeys.generationIntervalLastKey, ["2"]
                    ]
                }
                |> EvoAppend.createSoss


            let description = "Chains of append and condense runs within the same workflow family."

            let runType =
                GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.SameFamilyChain", description, [|
                    runExample("TestStandardAppendCondense", TestStandardAppendCondense)
                    runExample("TestStandardCondenseAppend", TestStandardCondenseAppend)
                    runExample("TestSossAppendCondense", TestSossAppendCondense)
                    runExample("TestSossCondenseAppend", TestSossCondenseAppend)
                |])

        module MixedFamilyChain =

            let sossAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
            let standardAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let TestStandardAppendSossCondense () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    RunQueryCatalogName = sossAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = Continuation.TestStandardAppend ()
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
                    RunQueryCatalogName = sossAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = Continuation.TestStandardCondense ()
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

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let TestSossAppendStandardCondense () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    RunQueryCatalogName = standardAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = Continuation.TestSossAppend ()
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
                    RunQueryCatalogName = standardAppendExecutionQueryCatalogName
                    Source = evoContinuationSource.create {
                        SourceRun = Continuation.TestSossCondense ()
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

            let description = "Chains of append and condense runs switching between Standard and Soss."

            let runType =
                GeneSort.Dispatch.V1.runType("SorterSgd.p32.Test.OnePool.MixedFamilyChain", description, [|
                    runExample("TestStandardAppendSossCondense", TestStandardAppendSossCondense)
                    runExample("TestStandardCondenseSossAppend", TestStandardCondenseSossAppend)
                    runExample("TestSossAppendStandardCondense", TestSossAppendStandardCondense)
                    runExample("TestSossCondenseStandardAppend", TestSossCondenseStandardAppend)
                |])
