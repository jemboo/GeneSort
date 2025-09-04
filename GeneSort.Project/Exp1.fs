
namespace GeneSort.Project

open System
open System.IO
open System.Threading
open System.Threading.Tasks

open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers

open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model
open GeneSort.Model.Sorter
open GeneSort.Project.Params
open GeneSort.Model.Sorter.Ce
open GeneSort.Model.Sorter.Si
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter.Rs


module Exp1 =

    let projectDir = "c:\Projects"
    let randomType = rngType.Lcg
    let excludeSelfCe = true
    let parameterSet = 
        [ SwFull.standardMapVals(); SorterModelKey.allButMusf6Kvps() ]

    let workspace = Workspace.create "Exp1" "Exp1" projectDir parameterSet


    let executor (workspace: Workspace) (cycle: int<cycleNumber>) (run: Run) : Async<unit> =
        async {

            Console.WriteLine(sprintf "Executing Run %d   %A" run.Index run.Parameters)


            let sorterModelKey = (run.Parameters["SorterModel"]) |> SorterModelKey.fromString
            let swFull = (run.Parameters["SortingWidth"]) |> SwFull.fromString
            let sortingWidth = swFull |> SwFull.toSortingWidth
            let ceCount = SortingSuccess.getCeCountForFull sortingSuccess.P999 sortingWidth

            let stageCount = SortingSuccess.getStageCountForFull sortingSuccess.P999 sortingWidth
            let opsGenRatesArray = OpsGenRatesArray.createUniform %stageCount
            let uf4GenRatesArray = Uf4GenRatesArray.createUniform %stageCount %sortingWidth

            let modelMaker =
                match sorterModelKey with
                | SorterModelKey.Mcse -> (MsceRandGen.create randomType sortingWidth excludeSelfCe ceCount) |> SorterModelMaker.SmmMsceRandGen
                | SorterModelKey.Mssi -> (MssiRandGen.create randomType sortingWidth stageCount) |> SorterModelMaker.SmmMssiRandGen
                | SorterModelKey.Msrs -> (MsrsRandGen.create randomType sortingWidth opsGenRatesArray) |> SorterModelMaker.SmmMsrsRandGen
                | SorterModelKey.Msuf4 -> (Msuf4RandGen.create randomType sortingWidth stageCount uf4GenRatesArray) |> SorterModelMaker.SmmMsuf4RandGen
                | SorterModelKey.Msuf6 -> failwith "Msuf6 not supported in this experiment"

            let sorterCount = swFull |> SorterCount.getSorterCountForSwFull
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

