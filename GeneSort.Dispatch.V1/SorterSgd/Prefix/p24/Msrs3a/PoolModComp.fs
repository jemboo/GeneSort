module GeneSort.Dispatch.V1.SorterSgd.Prefix.p24.Msrs3a.PoolModComp

open GeneSort.Project.V1.Catalogs.RunParameters
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.Queries
open GeneSort.Project.V1.Catalogs.Evolution
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
let dbName_Sz_2048_Of_4096 = "Sz_2048_Of_4096" |> UMX.tag<databaseName>
let dbNamePools4096_2_vs_256 = "PoolsSelSzTest_2_vs_256" |> UMX.tag<databaseName>
let dbNamePools4096_4096 = "Pools4096_4096" |> UMX.tag<databaseName>
let dbNamePoolSz128 = "PoolSelSz128" |> UMX.tag<databaseName>




module Specs =

    let TestSpec ()  : run =
        run.SgdRun (
            SgdRun.create EvolutionMakerNames.prefix ExecuteFunctionNames.standard
                dbNamePoolSz128
                projName
                (sprintf @"PoolSz32_Mod_Testc%s" "GenStandard" |> UMX.tag)
                "Para rate comp for 24pfx3a Msrs"
                [
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [8] |> List.map string)
            (runParameters.sorterCountPerPoolKey, ["32"] |> List.map string)
            (runParameters.modificationRateKey, [0.99;] |> List.map string)
            (runParameters.paraRateKey, [0.075; 0.1; 0.125;] |> List.map string)
            (runParameters.selfSymRateKey, [1.25; 1.75; 2.25; 2.75] |> List.map string)
            (runParameters.mutationModKey, [0 .. 1] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, ["32";] |> List.map string)
        ]
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                QueryCatalogNames.sorterSgdMsrsPoolModComp
                SetupCatalogNames.standard
                RunParamBuilderNames.Filter.identity
                RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
                false
        )


    let Sz_2048_Of_4096 ()  : run =
        run.SgdRun (
            SgdRun.create EvolutionMakerNames.prefix ExecuteFunctionNames.standard
                dbName_Sz_2048_Of_4096
                projName
                (sprintf @"Sz_2048_Of_4096_%s" "GenStandard" |> UMX.tag)
                "Selection size comp for 24pfx3a Msrs"
                [
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [12] |> List.map string)
            (runParameters.sorterCountPerPoolKey, ["4096";])
            (runParameters.mutationModKey, [0 .. 7;] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, [2048;] |> List.map string)
        ]
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                QueryCatalogNames.sorterSgdMsrsPoolModComp
                SetupCatalogNames.standard
                RunParamBuilderNames.Filter.identity
                RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
                false
        )


    let PoolSz_2n256 ()  : run =
        run.SgdRun (
            SgdRun.create EvolutionMakerNames.prefix ExecuteFunctionNames.standard
                dbNamePools4096_2_vs_256
                projName
                (sprintf @"PoolSz_2_vs_256_%s" "GenStandard" |> UMX.tag)
                "Pool size comp (2 vs 256) for 24pfx3a Msrs"
                [
            (runParameters.generationCurrentKey, [0] |> List.map string)
            (runParameters.generationIntervalCountKey, [5] |> List.map string)
            (runParameters.sorterCountPerPoolKey, ["2"; "256";])
            (runParameters.mutationModKey, [0 .. 63;] |> List.map string)
            (runParameters.selectedSorterCountPerPoolKey, ["64"; "128"; "256"] |> List.map string)
        ]
                "expInterval100_L50ss"
                "summaryInterval_C.1p5C"
                QueryCatalogNames.sorterSgdMsrsPoolModComp
                SetupCatalogNames.standard
                RunParamBuilderNames.Filter.identity
                RunParamBuilderNames.Enhancer.msrs24p3aPoolModComp
                false
        )
