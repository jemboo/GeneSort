namespace GeneSort.Project.V1.Catalogs.Evolution

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Project.V1
open GeneSort.Model.Sorting.Simple.V1

module SeedSorterModelGeneration =

    let getStageLength 
                (smt: simpleSorterModelType) 
                (sw: int<sortingWidth>) : int<stageLength> =
        match %sw with
        | 4 -> 15
        | 5 -> 25
        | 6 -> 40 
        | 7 -> 50 
        | 8 -> 60
        | 9 -> 70
        | 10 -> 80
        | 11 -> 90
        | 12 -> 100
        | 14 -> 120
        | 16 -> match smt with | simpleSorterModelType.Msuf4 -> 300 | _ -> 150
        | 18 -> 180
        | 20 -> 200
        | 22 -> 250
        | 24 -> 300
        | 28 -> 300
        | 32 -> match smt with | simpleSorterModelType.Msuf4 -> 600 | _ -> 300
        | 36 -> 350
        | 48 -> 400
        | 64 -> match smt with | simpleSorterModelType.Msuf4 -> 2000 | _ -> 600
        | 96 -> 800
        | 128 -> match smt with | simpleSorterModelType.Msuf4 -> 4000 | _ -> 1200
        | 192 -> 2000
        | 256 -> match smt with | simpleSorterModelType.Msuf4 -> 6000 | _ -> 3000
        | 512 -> match smt with | simpleSorterModelType.Msuf4 -> 8000 | _ -> 8000
        | _ -> failwithf "Unsupported sorting width: %d" %sw
        |> UMX.tag


    let getSimpleUniformSorterModelGen
            (rngType: rngType) 
            (sortingWidth: int<sortingWidth>)
            (simpleSorterModelType: simpleSorterModelType) 
            (excludeSelfCe: bool<excludeSelfCe>)
                            : sorterModelGen =
            let stageLength = getStageLength simpleSorterModelType sortingWidth
            let rngFactory = rngType |> RngFactory.create
            SimpleSorterModelGen.makeUniform 
                                    rngFactory 
                                    sortingWidth 
                                    stageLength 
                                    simpleSorterModelType
                                    excludeSelfCe
                                    |> sorterModelGen.Simple






