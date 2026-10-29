module GeneSort.Dispatch.V1.SorterSgd.p32.Msuf4a.Soss_condense

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



module Pool_X =

    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.sossMsuf32
    let executionFunctionName = ExecuteFunctionNames.soss
    let setupFunctionName = SetupCatalogNames.sgdSoss
    let sossExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSoss
    let sossAppendExecutionQueryCatalogName = QueryCatalogNamesRun.sorterSgdMsufSossAppend
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sossMsuf32
    let setupQueryCatalogName = QueryCatalogNamesSetup.sossMsuf32Seed

    let partitionCount = 4
    let sorterCount = 32
    let splitCount = 16
    let seletctionCount = 16
    let poolCount = splitCount * seletctionCount
    let generationsPerSubset = 20
    let generationsPerMergedSet = 20


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Base ()  : run =
        let names = RunNaming.create()
        let parameterSpansSetup =
            [
                (ParamKeys.sorterCountPerPoolKey, [sorterCount] |>  List.map string)
                (ParamKeys.sorterCountPerPoolSetKey, [sorterCount] |> List.map string)
                (ParamKeys.mutationModKey, [0 .. (poolCount - 1)] |> List.map string)
            ]
        let parameterSpansRun =
            [
                (ParamKeys.seedSossKey, [1234UL] |> List.map string)
                (ParamKeys.partitionCountKey, [partitionCount;] |> List.map string)
                (ParamKeys.generationsPerSubsetKey, [generationsPerSubset] |> List.map string)
                (ParamKeys.generationsPerMergedSetKey, [generationsPerMergedSet] |> List.map string)
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
                Description = "SossBase for Msuf32p4a"
                AllowOverwrite = false
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsDatabaseName = UMX.tag "Prefix"
                    SorterSetEvalsDatabaseName = UMX.tag "Prefix"
                    ParameterSpans = parameterSpansSetup
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpansRun
                    QueryCatalogName = sossExecutionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.sossPrefix
                GenSaveIntervalsName = GenIntervalNames.uniformInterval100
                GenSaveSubIntervalsName = GenIntervalNames.uniformInterval10
            }
        )


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Step1 () : run =
        let names = RunNaming.create()
        // Select seletctionCount winners within each source PartitionCount, then
        // split each winner: poolCount executions per PartitionCount.
        evoContinuationConfig.create {
            ProjectName = names.ProjectName
            DatabaseName = names.DatabaseName
            RunName = names.RunName
            RunQueryCatalogName = sossAppendExecutionQueryCatalogName
            Source = evoContinuationSource.create {
                SourceRun = Base ()
                GenerationInterval = 0
                SourceMutationMods = [0 .. (poolCount - 1)]
                SelectedSorterPoolCount = seletctionCount
            }
            ExecutionParameterSpans = [
                ParamKeys.generationIntervalLastKey, [1] |> List.map string
                (ParamKeys.mutationModKey, [0 .. (splitCount - 1)] |> List.map string)
            ]
        }
        |> EvoCondense.createSoss



    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Step2 () : run =
        let names = RunNaming.create()
        evoContinuationConfig.create {
            ProjectName = names.ProjectName
            DatabaseName = names.DatabaseName
            RunName = names.RunName
            RunQueryCatalogName = sossAppendExecutionQueryCatalogName
            Source = evoContinuationSource.create {
                SourceRun = Step1 ()
                GenerationInterval = 1
                SourceMutationMods = [0 .. (splitCount - 1)]
                SelectedSorterPoolCount = seletctionCount
            }
            ExecutionParameterSpans = [
                ParamKeys.generationIntervalLastKey, [2] |> List.map string
                (ParamKeys.mutationModKey, [0 .. (splitCount - 1)] |> List.map string)
            ]
        }
        |> EvoCondense.createSoss


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Step3 () : run =
        let names = RunNaming.create()
        evoContinuationConfig.create {
            ProjectName = names.ProjectName
            DatabaseName = names.DatabaseName
            RunName = names.RunName
            RunQueryCatalogName = sossAppendExecutionQueryCatalogName
            Source = evoContinuationSource.create {
                SourceRun = Step2 ()
                GenerationInterval = 2
                SourceMutationMods = [0 .. (splitCount - 1)]
                SelectedSorterPoolCount = seletctionCount
            }
            ExecutionParameterSpans = [
                ParamKeys.generationIntervalLastKey, [3] |> List.map string
                (ParamKeys.mutationModKey, [0 .. (splitCount - 1)] |> List.map string)
            ]
        }
        |> EvoCondense.createSoss



    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Step4 () : run =
        let names = RunNaming.create()
        evoContinuationConfig.create {
            ProjectName = names.ProjectName
            DatabaseName = names.DatabaseName
            RunName = names.RunName
            RunQueryCatalogName = sossAppendExecutionQueryCatalogName
            Source = evoContinuationSource.create {
                SourceRun = Step3 ()
                GenerationInterval = 3
                SourceMutationMods = [0 .. (splitCount - 1)]
                SelectedSorterPoolCount = seletctionCount
            }
            ExecutionParameterSpans = [
                ParamKeys.generationIntervalLastKey, [4] |> List.map string
                (ParamKeys.mutationModKey, [0 .. (splitCount - 1)] |> List.map string)
            ]
        }
        |> EvoCondense.createSoss


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Step5 () : run =
        let names = RunNaming.create()
        evoContinuationConfig.create {
            ProjectName = names.ProjectName
            DatabaseName = names.DatabaseName
            RunName = names.RunName
            RunQueryCatalogName = sossAppendExecutionQueryCatalogName
            Source = evoContinuationSource.create {
                SourceRun = Step4 ()
                GenerationInterval = 4
                SourceMutationMods = [0 .. (splitCount - 1)]
                SelectedSorterPoolCount = seletctionCount
            }
            ExecutionParameterSpans = [
                ParamKeys.generationIntervalLastKey, [5] |> List.map string
                (ParamKeys.mutationModKey, [0 .. (splitCount - 1)] |> List.map string)
            ]
        }
        |> EvoCondense.createSoss

    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.p32.Msuf4a.Soss_condense.Pool_X", [|
            runExample("Base", Base)
            runExample("Step1", Step1)
            runExample("Step2", Step2)
            runExample("Step3", Step3)
            runExample("Step4", Step4)
            runExample("Step5", Step5)
        |])
