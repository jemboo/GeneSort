namespace GeneSort.Dispatch.V1.SorterSgd.cc24.p3b.Msuf6.Simple

open GeneSort.Project.V1.Parameters
open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Eval.V1

module SimpleParams =

    let simpleSetupParams =
        GeneSort.Dispatch.V1.SorterSgd.cc24.p3b.Msuf6.Msuf6Params.msuf6SetupParams
        @ [
            ParamKeys.sorterEvalMeasureInitialKey, ["SameAsEvaluation"]
        ]

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

    let standardExecutionDefaults =
        GeneSort.Dispatch.V1.SorterSgd.SorterSgdParams.evolutionExecutionDefaults
        @ [
            ParamKeys.generationCurrentKey, ["0"]
            ParamKeys.mutationModKey, ["0"]
            ParamKeys.selectedSorterCountPerPoolKey, ["All"]
            ParamKeys.codeModKey, ["NoMods"]
            ParamKeys.mutatorVariantKey, [GeneSort.Model.Sorting.V1.mutatorVariant.V1 |> GeneSort.Model.Sorting.V1.MutatorVariant.toString]
            ParamKeys.sorterPoolCountKey, ["4"]
            ParamKeys.sorterEvalMeasureKey, sorterEvalMeasures
            ParamKeys.modificationRateKey, [0.05; 0.075; 0.1; 0.125] |> List.map string
        ]

    // These legacy workflows apply the same defaults during setup and execution.
    let standardSetupDefaults = standardExecutionDefaults
