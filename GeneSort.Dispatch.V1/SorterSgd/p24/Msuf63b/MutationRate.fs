module GeneSort.Dispatch.V1.SorterSgd.p24.Msuf63b.MutationRate

open System.Runtime.CompilerServices
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
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
open GeneSort.Dispatch.V1.SorterSgd.p24.Msuf63b.Common


let private sorterEvalMeasures =
    [ 4.5; 5.5; 7.5; 9.5 ]
    |> List.map (fun unsortedWeight ->
        ceStUcMeasure.create {
            StageWeight = 1.1<stageWeight>
            UnsortedWeight = unsortedWeight |> UMX.tag<unsortedWeight>
            FilterReflectionSymmetric = false |> UMX.tag<filterReflectionSymmetric>
        }
        |> sorterEvalMeasure.CeStUc
        |> SorterEvalMeasure.toCompactString
    )



module VarModR_32 =

    let channelCount = 24<GeneSort.Sorting.channelCount>
    let sortableTestsModule = UMX.tag<runModuleName> (sprintf "SortableTests.Prefix.Channels%d" (int channelCount))
    let sorterEvalModule = UMX.tag<runModuleName> "SorterEval.Prefix"

    let moduleParameters =
        [
            ParamKeys.excludeSelfCeKey, ["true"]
            ParamKeys.collectSortableTestsKey, ["false"]
            ParamKeys.rngTypeKey, ["LCG"]
            ParamKeys.simpleSorterModelTypeKey, ["Msuf6"]
            ParamKeys.sortableDataFormatKey, ["BitVector512"]
            ParamKeys.prefixLibIdKey, [GeneSort.SortingLib.Sorter.prefixLibId.create channelCount 3<GeneSort.Sorting.stageLength> GeneSort.SortingLib.Sorter.prefixLibVariant.PrefixB |> GeneSort.SortingLib.Sorter.PrefixLibId.toString]
            ParamKeys.sorterEvalMeasureInitialKey, ["SameAsEvaluation"]
        ]


    let executionEnhancerFunctionName = RunParamBuilderNames.Enhancer.msuf624p3bMutationRate
    let executionFunctionName = ExecuteFunctionNames.standard
    let setupFunctionName = SetupCatalogNames.sgdStandard
    let executionQueryCatalogName = QueryCatalogNamesRun.sorterSgdUf6MutationRate
    let setupEnhancerFunctionName = SetupEnhancerFunctionNames.sorterSgd
    let setupQueryCatalogName = QueryCatalogNamesSetup.sorterSgdUf6MutationRate


    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let Test () : run =
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
                (ParamKeys.sorterPoolCountKey, [4] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [2] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [64] |> List.map string)
                (ParamKeys.sorterEvalMeasureKey, sorterEvalMeasures)
                (ParamKeys.paraRateKey, [1.001] |> List.map string)
                (ParamKeys.selfSymRateKey, [2.001] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.05; 0.1] |> List.map string)
                (ParamKeys.modificationRateKey, [0.05; 0.075; 0.1; 0.125;] |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V1] |> List.map MutatorVariant.toString)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Mutation rate test for Msuf6 24p3b"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans |> List.filter (fun (key, _) -> key <> ParamKeys.sorterEvalMeasureKey)
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
                    ParameterSpans = moduleParameters @ parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    let WideTest () : run =
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
                (ParamKeys.sorterPoolCountKey, [4] |> List.map string)
                (ParamKeys.codeModKey, ["NoMods"] |> List.map string)
                (ParamKeys.generationCurrentKey, [0] |> List.map string)
                (ParamKeys.generationIntervalCountKey, [10] |> List.map string)
                (ParamKeys.sorterCountPerPoolKey, [128] |> List.map string)
                (ParamKeys.sorterEvalMeasureKey, sorterEvalMeasures)
                (ParamKeys.paraRateKey, [0.1; 0.5; 1.001; 1.5] |> List.map string)
                (ParamKeys.selfSymRateKey, [1.001; 1.5; 2.001; 3.001] |> List.map string)
                (ParamKeys.mutationModKey, [0] |> List.map string)
                (ParamKeys.seedModificationRateKey, [0.05] |> List.map string)
                (ParamKeys.modificationRateKey, [0.05; 0.075; 0.1; 0.125;] |> List.map string)
                (ParamKeys.mutatorVariantKey, [mutatorVariant.V1] |> List.map MutatorVariant.toString)
            ]
        run.EvolutionRun (
            SgdRun.create {
                DatabaseName = names.DatabaseName
                ProjectName = names.ProjectName
                RunName = names.RunName
                Description = "Wide mutation rate test for Msuf6 24p3b"
                AllowOverwrite = false
                Execution = {
                    FunctionName = executionFunctionName
                    ParameterSpans = parameterSpans |> List.filter (fun (key, _) -> key <> ParamKeys.sorterEvalMeasureKey)
                    QueryCatalogName = executionQueryCatalogName
                    EnhancerFunctionName = executionEnhancerFunctionName
                }
                Setup = {
                    FunctionName = setupFunctionName
                    SortableTestsModule = sortableTestsModule
                    SorterEvalModule = sorterEvalModule
                    ParameterSpans = moduleParameters @ parameterSpans
                    QueryCatalogName = setupQueryCatalogName
                    EnhancerFunctionName = setupEnhancerFunctionName
                }
            } {
                MakerNames = EvolutionMakerNames.prefix
                GenSaveIntervalsName = GenIntervalNames.expInterval100_L50ss
                GenSaveSubIntervalsName = GenIntervalNames.summaryInterval_C_1p5C
            }
        )

    let description = "Compare mutation-rate settings for 24-input Msuf63b evolution (VarModR_32)."

    let runType =
        GeneSort.Dispatch.V1.runType("SorterSgd.p24.Msuf63b.MutationRate.VarModR_32", description, channelCount, executionQueryCatalogName, [|
            runExample("Test", Test)
            runExample("WideTest", WideTest)
        |])
