module GeneSort.Dispatch.V1.SorterSgd.Mssi24p3b.OrthoPara

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Mssi24p3b.Common
open GeneSort.Dispatch.V1.SorterSgd
let dbOrthoPara64Name = "OrthoPara64" |> UMX.tag<databaseName>
let dbOrthoPara128Name = "OrthoPara128" |> UMX.tag<databaseName>




module Specs64 =

    let NoMods (executorType: sorterSgdExecutorType)  : run =
        run.SgdRun (
            SgdRun.create
                dbOrthoPara64Name
                projName
                (sprintf @"NoMods%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag)
                "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                [
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [1] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [64] |>  List.map string)
            (runParameters.paraRateKey, [0.5; 1.01; 1.5; 2.01] |> List.map string)
            (runParameters.modificationRateKey, [ 0.09; 0.11; ] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [64;] |> List.map string)
        ]
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                "sorter-sgd.mssi-ortho-para"
                RunParamBuilderNames.Filter.identity
                RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                false
        )


    let SymForceDiff1 (executorType: sorterSgdExecutorType)  : run =
        run.SgdRun (
            SgdRun.create
                dbOrthoPara64Name
                projName
                (sprintf @"SymForce_Diff1%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag)
                "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                [
            (runParameters.codeModKey, ["SymForce_Diff1"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [5] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [64] |>  List.map string)
            (runParameters.paraRateKey, [0.5; 1.01;] |> List.map string)
            (runParameters.modificationRateKey, [ 0.20; 0.25; 0.30; ] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [64;] |> List.map string)
        ]
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                "sorter-sgd.mssi-ortho-para"
                RunParamBuilderNames.Filter.identity
                RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                false
        )


    let SymForce (executorType: sorterSgdExecutorType)  : run =
        run.SgdRun (
            SgdRun.create
                dbOrthoPara64Name
                projName
                (sprintf @"SymForce%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag)
                "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                [
            (runParameters.codeModKey, ["SymForce"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [5] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [64] |>  List.map string)
            (runParameters.paraRateKey, [0.5; 1.01;] |> List.map string)
            (runParameters.modificationRateKey, [ 0.20; 0.25; 0.30; ] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [64;] |> List.map string)
        ]
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                "sorter-sgd.mssi-ortho-para"
                RunParamBuilderNames.Filter.identity
                RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                false
        )


module Specs128 =

    let NoMods (executorType: sorterSgdExecutorType)  : run =
        run.SgdRun (
            SgdRun.create
                dbOrthoPara128Name
                projName
                (sprintf @"NoMods%s" (SorterSgdExecutorType.toString executorType) |> UMX.tag)
                "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                [
            (runParameters.codeModKey, ["NoMods"] |> List.map string)
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [6] |> List.map string)
            (runParameters.sorterCountPerPoolKey, [128] |>  List.map string)
            (runParameters.paraRateKey, [0.075; 0.1; 0.125; 0.15] |> List.map string)
            (runParameters.modificationRateKey, [0.09; 0.11;] |> List.map string)
            (runParameters.mutationModKey, [0] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [128;] |> List.map string)
        ]
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                "sorter-sgd.mssi-ortho-para"
                RunParamBuilderNames.Filter.identity
                RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                false
        )
