
namespace GeneSort.Project

open System

open FSharp.UMX

open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter
open GeneSort.Project.Params
open GeneSort.Model.Sorter.Ce
open GeneSort.Model.Sorter.Si
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter.Rs


module Exp1 =

    let projectDir = "c:\Projects"
    let experimentName = "Exp1"
    let experimentDesc = "Exp1 Description"
    let sorterModelKey = "SorterModelKey"
    let sortingWidth = "SortingWidth"

    let randomType = rngType.Lcg
    let excludeSelfCe = true


    let getSorterCountForSortingWidth (sortingWidth: int<sortingWidth>) : int<sorterCount> =
        match %sortingWidth with
        | 4 -> 1000<sorterCount>
        | 6 -> 1000<sorterCount>
        | 8 -> 1000<sorterCount>
        | 12 -> 1000<sorterCount>
        | 16 -> 50<sorterCount>
        | 24 -> 10<sorterCount>
        | 32 -> 10<sorterCount>
        | 48 -> 2<sorterCount>
        | 64 -> 1<sorterCount>
        | _ -> failwithf "Unsupported sorting width: %d" (%sortingWidth)


    let sortingWidthValues = 
        [4; 8; 16; 32; 64] |> List.map(fun d -> d.ToString())

    let sortingWidths() : string*string list =
        (sortingWidth, sortingWidthValues)


    let sorterModelKeyValues () : string list =
        [ SorterModelKey.Mcse; 
          SorterModelKey.Mssi;
          SorterModelKey.Msrs; 
          SorterModelKey.Msuf4; ]      |> List.map(SorterModelKey.toString)

    let sorterModelKeys () : string*string list =
        (sorterModelKey, sorterModelKeyValues() )

    let parameterSet = 
        [ sortingWidths(); sorterModelKeys() ]

    let workspace = Workspace.create experimentName experimentDesc projectDir parameterSet


    let executor (workspace: Workspace) (cycle: int<cycleNumber>) (run: Run) : Async<unit> =
        async {

            Console.WriteLine(sprintf "Executing Run %d   %A" run.Index run.Parameters)


            let sorterModelKey = (run.Parameters["SorterModel"]) |> SorterModelKey.fromString
            let sortingWidth = run |> Run.getSortingWidth
            let ceLength = SortingSuccess.getCeLengthForFull sortingSuccess.P999 sortingWidth

            let stageCount = SortingSuccess.getStageCountForFull sortingSuccess.P999 sortingWidth

            let modelMaker =
                match sorterModelKey with
                | SorterModelKey.Mcse -> (MsceRandGen.create randomType sortingWidth excludeSelfCe ceLength) |> SorterModelMaker.SmmMsceRandGen
                | SorterModelKey.Mssi -> (MssiRandGen.create randomType sortingWidth stageCount) |> SorterModelMaker.SmmMssiRandGen
                | SorterModelKey.Msrs -> 
                    let opsGenRatesArray = OpsGenRatesArray.createUniform %stageCount
                    (MsrsRandGen.create randomType sortingWidth opsGenRatesArray) |> SorterModelMaker.SmmMsrsRandGen
                | SorterModelKey.Msuf4 -> 
                    let uf4GenRatesArray = Uf4GenRatesArray.createUniform %stageCount %sortingWidth
                    (Msuf4RandGen.create randomType sortingWidth stageCount uf4GenRatesArray) |> SorterModelMaker.SmmMsuf4RandGen
                | SorterModelKey.Msuf6 -> 
                    let uf6GenRatesArray = Uf6GenRatesArray.createUniform %stageCount %sortingWidth
                
                    failwith "Msuf6 not supported in this experiment"

            let sorterCount = sortingWidth |> getSorterCountForSortingWidth
            let firstIndex = (%cycle * %sorterCount) |> UMX.tag<sorterCount>
            
            let sorterModelSetMaker = sorterModelSetMaker.create modelMaker firstIndex sorterCount
            let sorterModelSet = sorterModelSetMaker.MakeSorterModelSet (Rando.create)
            let sorterSet = SorterModelSet.makeSorterSet sorterModelSet

            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterSet |> outputData.SorterSet)
            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterModelSetMaker |> outputData.SorterModelSetMaker)


            Console.WriteLine(sprintf "Finished executing Run %d  Cycle  %d \n" run.Index %cycle)
        }


    let RunAll() =
        let cycle = 1<cycleNumber>
        WorkspaceOps.executeWorkspace workspace cycle 6 executor

