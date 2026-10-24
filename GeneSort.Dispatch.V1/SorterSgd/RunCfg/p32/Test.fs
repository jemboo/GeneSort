namespace GeneSort.Dispatch.V1.SorterSgd.RunCfg.p32
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
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd

module Test =

    module OnePool =

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
            run.SgdRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Rate comp for 32p4a Msuf4"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = SetupCatalogNames.standard
                        ParameterSpans = parameterSpans
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
            run.SgdRun (
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "Soss rate comparison for Msuf32p4a"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = SetupCatalogNames.standard
                        ParameterSpans = parameterSpansSetup
                        QueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
                        EnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32
                    }
                    Execution = {
                        FunctionName = ExecuteFunctionNames.soss
                        ParameterSpans = parameterSpansRun
                        QueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
                        EnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32
                    }
                } {
                    MakerNames = EvolutionMakerNames.sossPrefix
                    GenSaveIntervalsName = GenIntervalNames.uniformInterval10
                    GenSaveSubIntervalsName = GenIntervalNames.uniformInterval1
                }
            )



        module SimpleR =

            // AMax belongs to setup; the independent activator mutation rate belongs to execution.
            let aMax = 3
            let activatorMutationRate = 0.10

            let private createBase (names: runNames) (source: run) =
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OnePool SimpleR generation and evolution"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = source.SetupFunctionName
                        ParameterSpans = source.SetupParameterSpans @ [ParamKeys.aMaxKey, [string aMax]]
                        QueryCatalogName = source.SetupQueryCatalogName
                        EnhancerFunctionName = source.SetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = source.ExecuteFunctionName
                        ParameterSpans = source.RunParameterSpans @ [ParamKeys.activatorMutationRateKey, [string activatorMutationRate]]
                        QueryCatalogName = source.RunQueryCatalogName
                        EnhancerFunctionName = source.RunEnhancerFunctionName
                    }
                } {
                    MakerNames = source.MakerNames.Value
                    GenSaveIntervalsName = source.GenSaveIntervalsName.Value
                    GenSaveSubIntervalsName = source.GenSaveSubIntervalsName.Value
                } |> run.SgdRun

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let Standard () : run = createBase (RunNaming.create()) (TestStandard ())

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let Soss () : run = createBase (RunNaming.create()) (TestSoss ())

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let StandardAppend () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
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
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
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

        module SimpleS =

            // ActivationProbability belongs to setup; the independent activator mutation rate belongs to execution.
            let activationProbability = 0.5
            let activatorMutationRate = 0.10

            let private createBase (names: runNames) (source: run) =
                SgdRun.create {
                    DatabaseName = names.DatabaseName
                    ProjectName = names.ProjectName
                    RunName = names.RunName
                    Description = "OnePool SimpleS generation and evolution"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = source.SetupFunctionName
                        ParameterSpans = source.SetupParameterSpans @ [ParamKeys.activationProbabilityKey, [string activationProbability]]
                        QueryCatalogName = source.SetupQueryCatalogName
                        EnhancerFunctionName = source.SetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = source.ExecuteFunctionName
                        ParameterSpans = source.RunParameterSpans @ [ParamKeys.activatorMutationRateKey, [string activatorMutationRate]]
                        QueryCatalogName = source.RunQueryCatalogName
                        EnhancerFunctionName = source.RunEnhancerFunctionName
                    }
                } {
                    MakerNames = source.MakerNames.Value
                    GenSaveIntervalsName = source.GenSaveIntervalsName.Value
                    GenSaveSubIntervalsName = source.GenSaveSubIntervalsName.Value
                } |> run.SgdRun

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let StandardS () : run = createBase (RunNaming.create()) (TestStandard ())

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let SossS () : run = createBase (RunNaming.create()) (TestSoss ())

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let StandardAppendS () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
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
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
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

        module Controlled =

            let replicaCount = 5
            let generationCount = 100
            let aMax = 3
            let activationProbability = float aMax / (2.0 * float aMax + 1.0)
            let activatorMutationRate = 0.1

            // Only 0 -> 1 and 1 -> 0 change activity; each has probability rate / 2.
            // This matches the uniform seed distribution, not an arbitrary evolved distribution.
            let matchedFlipRate = activatorMutationRate / float (2 * aMax + 1)

            let private create (names: runNames) soss family mutationRate =
                let source = if soss then TestSoss() else TestStandard()
                let excluded = [ParamKeys.mutationModKey; ParamKeys.generationIntervalLastKey]
                let clean spans = spans |> List.filter (fun (key, _) -> not (List.contains key excluded))
                let activation =
                    match family with
                    | "R" -> [ParamKeys.aMaxKey, [string aMax]]
                    | "S" -> [ParamKeys.activationProbabilityKey, [string activationProbability]]
                    | _ -> []
                SgdRun.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    Description = "Paired OnePool comparison: common underlying seeds and matched R/S activation masks"
                    AllowOverwrite = false
                    Setup = {
                        FunctionName = source.SetupFunctionName
                        ParameterSpans = clean source.SetupParameterSpans @ activation @ [
                            ParamKeys.controlledSeedAMaxKey, [string aMax]
                            ParamKeys.mutationModKey, ["0"; "1"]
                        ]
                        QueryCatalogName = source.SetupQueryCatalogName
                        EnhancerFunctionName = source.SetupEnhancerFunctionName
                    }
                    Execution = {
                        FunctionName = source.ExecuteFunctionName
                        ParameterSpans = clean source.RunParameterSpans @ [
                            ParamKeys.generationIntervalLastKey, [string (generationCount / 10 - 1)]
                        ] @ (if family = "" then [] else [ParamKeys.activatorMutationRateKey, [string mutationRate]])
                        QueryCatalogName = source.RunQueryCatalogName
                        EnhancerFunctionName = source.RunEnhancerFunctionName
                    }
                } {
                    MakerNames = source.MakerNames.Value
                    GenSaveIntervalsName = source.GenSaveIntervalsName.Value
                    GenSaveSubIntervalsName = source.GenSaveSubIntervalsName.Value
                } |> run.SgdRun

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let StandardSimple () = create (RunNaming.create()) false "" activatorMutationRate
            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let StandardR () = create (RunNaming.create()) false "R" activatorMutationRate
            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let CompareStandardS () = create (RunNaming.create()) false "S" activatorMutationRate
            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let SossSimple () = create (RunNaming.create()) true "" activatorMutationRate
            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let SossR () = create (RunNaming.create()) true "R" activatorMutationRate
            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let CompareSossS () = create (RunNaming.create()) true "S" activatorMutationRate

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let MatchedStandardS () = create (RunNaming.create()) false "S" matchedFlipRate
            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let MatchedSossS () = create (RunNaming.create()) true "S" matchedFlipRate

        module Continuation =

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let TestStandardAppend () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
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
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
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
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
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
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
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



        module SameFamilyChain =

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let TestStandardAppendCondense () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
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
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
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
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
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
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
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

        module MixedFamilyChain =

            [<MethodImpl(MethodImplOptions.NoInlining)>]
            let TestStandardAppendSossCondense () : run =
                let names = RunNaming.create()
                evoContinuationConfig.create {
                    ProjectName = names.ProjectName
                    DatabaseName = names.DatabaseName
                    RunName = names.RunName
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
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
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
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
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
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
                    SetupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdMsufStandard
                    RunQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufStandardAppend
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
