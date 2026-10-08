module GeneSort.Dispatch.V1.SorterSgd.Prefix.p24.Msrs3a.OrthoPara

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Prefix.p24.Msrs3a.Common
open GeneSort.Dispatch.V1.SorterSgd
let dbOrthoPara32Name = "OrthoPara32" |> UMX.tag<databaseName>







module Specs =

    let PickMode2_2 (executorType: sorterSgdExecutorType)  : run =
        run.SgdRun (
            SgdRun.create
                dbOrthoPara32Name
                projName
                (sprintf @"PickMode2_2_%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag)
                "OrthroPara rate comp for 24pfx3a Msrs, PickMode2_2"
                [
            (runParameters.codeModKey, ["PickMode2_2"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [5] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
            (runParameters.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
            (runParameters.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [32;] |> List.map string)
        ]
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                QueryCatalogNames.sorterSgdMsrsOrthoPara
                RunParamBuilderNames.Filter.identity
                RunParamBuilderNames.Enhancer.msrs24p3aOrthoPara
                false
        )

    let NoMods (executorType: sorterSgdExecutorType)  : run =
        run.SgdRun (
            SgdRun.create
                dbOrthoPara32Name
                projName
                (sprintf @"NoMods%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag)
                "OrthroPara rate comp for 24pfx3a Msrs, NoMods"
                [
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [7] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [32] |>  List.map string)
            (runParameters.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
            (runParameters.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [32;] |> List.map string)
        ]
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                QueryCatalogNames.sorterSgdMsrsOrthoPara
                RunParamBuilderNames.Filter.identity
                RunParamBuilderNames.Enhancer.msrs24p3aOrthoPara
                false
        )
