module GeneSort.Dispatch.V1.SorterSgd.Prefix.p24.Mssi3b.OrthoPara

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.SetupParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Evolution
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Db.V1
open GeneSort.FileDb.V1
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.SorterSgd.Prefix.p24.Mssi3b.Common
open GeneSort.Dispatch.V1.SorterSgd
let dbOrthoPara64Name = "OrthoPara64" |> UMX.tag<databaseName>
let dbOrthoPara128Name = "OrthoPara128" |> UMX.tag<databaseName>




module Specs64 =

    let NoMods ()  : run =
        let parameterSpans =
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
        run.SgdRun (
            SgdRun.create EvolutionMakerNames.prefix ExecuteFunctionNames.standard
                dbOrthoPara64Name
                projName
                (sprintf @"NoMods%s" "GenStandard" |> UMX.tag)
                "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                parameterSpans
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                QueryCatalogNames.sorterSgdMssiOrthoPara
                SetupCatalogNames.standard
                RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                false
                parameterSpans
                QueryCatalogNames.sorterSgdMssiOrthoPara
                SetupEnhancerFunctionNames.sorterSgd
                [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
        )


    let SymForceDiff1 ()  : run =
        let parameterSpans =
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
        run.SgdRun (
            SgdRun.create EvolutionMakerNames.prefix ExecuteFunctionNames.standard
                dbOrthoPara64Name
                projName
                (sprintf @"SymForce_Diff1%s" "GenStandard" |> UMX.tag)
                "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                parameterSpans
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                QueryCatalogNames.sorterSgdMssiOrthoPara
                SetupCatalogNames.standard
                RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                false
                parameterSpans
                QueryCatalogNames.sorterSgdMssiOrthoPara
                SetupEnhancerFunctionNames.sorterSgd
                [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
        )


    let SymForce ()  : run =
        let parameterSpans =
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
        run.SgdRun (
            SgdRun.create EvolutionMakerNames.prefix ExecuteFunctionNames.standard
                dbOrthoPara64Name
                projName
                (sprintf @"SymForce%s" "GenStandard" |> UMX.tag)
                "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                parameterSpans
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                QueryCatalogNames.sorterSgdMssiOrthoPara
                SetupCatalogNames.standard
                RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                false
                parameterSpans
                QueryCatalogNames.sorterSgdMssiOrthoPara
                SetupEnhancerFunctionNames.sorterSgd
                [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
        )


module Specs128 =

    let NoMods ()  : run =
        let parameterSpans =
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
        run.SgdRun (
            SgdRun.create EvolutionMakerNames.prefix ExecuteFunctionNames.standard
                dbOrthoPara128Name
                projName
                (sprintf @"NoMods%s" "GenStandard" |> UMX.tag)
                "OrthroPara rate comp for 24pfx3b Mssi, NoMods"
                parameterSpans
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                QueryCatalogNames.sorterSgdMssiOrthoPara
                SetupCatalogNames.standard
                RunParamBuilderNames.Enhancer.mssi24p3bOrthoPara
                false
                parameterSpans
                QueryCatalogNames.sorterSgdMssiOrthoPara
                SetupEnhancerFunctionNames.sorterSgd
                [geneSortDataType.SorterPoolSet ""; geneSortDataType.SortableTestsWithPrefix ""]
        )
