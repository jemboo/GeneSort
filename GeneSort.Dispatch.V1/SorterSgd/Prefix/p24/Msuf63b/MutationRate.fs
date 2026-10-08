module GeneSort.Dispatch.V1.SorterSgd.Prefix.p24.Msuf63b.MutationRate

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Prefix.p24.Msuf63b.Common
open GeneSort.Dispatch.V1.SorterSgd

let dbVariableModR_32Name = "VariableModRates_Uf6_32" |> UMX.tag<databaseName>

let private sorterEvalMeasures =
    [ 4.5; 5.5; 7.5; 9.5 ]
    |> List.map (fun unsortedWeight ->
        ceStUcMeasure.create
            (1.1<stageWeight>)
            (unsortedWeight |> UMX.tag<unsortedWeight>)
            (false |> UMX.tag<filterReflectionSymmetric>)
        |> sorterEvalMeasure.CeStUc
        |> SorterEvalMeasure.toCompactString
    )



module VarModR_32 =


    let Test (executorType: sorterSgdExecutorType) : run =
        run.SgdRun (
            SgdRun.create
                dbVariableModR_32Name
                projName
                (sprintf @"Test%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag)
                "Mutation rate test for Msuf6 24p3b"
                [
            (runParameters.sorterCountPerPoolSetKey, [256] |> List.map string)
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [2] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [64] |> List.map string)
            (runParameters.sorterEvalMeasureKey, sorterEvalMeasures)
            (runParameters.paraRateKey, [1.001] |> List.map string)
            (runParameters.selfSymRateKey, [2.001] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.seedModificationRateKey, [0.05; 0.1] |> List.map string)
            (runParameters.modificationRateKey, [0.05; 0.075; 0.1; 0.125;] |> List.map string)
            (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map MutatorVariant.toString)
        ]
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                QueryCatalogNames.sorterSgdUf6MutationRate
                RunParamBuilderNames.Filter.identity
                RunParamBuilderNames.Enhancer.msuf624p3bMutationRate
                false
        )

    let WideTest (executorType: sorterSgdExecutorType) : run =
        run.SgdRun (
            SgdRun.create
                dbVariableModR_32Name
                projName
                (sprintf @"WideTest%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag)
                "Wide mutation rate test for Msuf6 24p3b"
                [
            (runParameters.sorterCountPerPoolSetKey, [512] |> List.map string)
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [10] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [128] |> List.map string)
            (runParameters.sorterEvalMeasureKey, sorterEvalMeasures)
            (runParameters.paraRateKey, [0.1; 0.5; 1.001; 1.5] |> List.map string)
            (runParameters.selfSymRateKey, [1.001; 1.5; 2.001; 3.001] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.seedModificationRateKey, [0.05] |> List.map string)
            (runParameters.modificationRateKey, [0.05; 0.075; 0.1; 0.125;] |> List.map string)
            (runParameters.mutatorVariantKey, [mutatorVariant.V1] |> List.map MutatorVariant.toString)
        ]
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                QueryCatalogNames.sorterSgdUf6MutationRate
                RunParamBuilderNames.Filter.identity
                RunParamBuilderNames.Enhancer.msuf624p3bMutationRate
                false
        )
