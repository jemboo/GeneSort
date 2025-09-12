
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
open GeneSort.Model.Sorter.Uf6


module RandomSortersProject =

    let projectDir = "c:\Projects"
    let experimentName4 = "RandomSorters4"
    let experimentDesc4 = "RandomSorters with SortingWidth divisibe by 4"

    let experimentName6 = "RandomSorters6"
    let experimentDesc6 = "RandomSorters with SortingWidth divisibe by 6"

    let randomType = rngType.Lcg
    let excludeSelfCe = true


    let getSorterCountForSortingWidth (factor:int) (sortingWidth: int<sortingWidth>) : int<sorterCount> =
        match %sortingWidth with
        | 4 -> (10 * factor) |> UMX.tag<sorterCount>
        | 6 -> (10 * factor) |> UMX.tag<sorterCount>
        | 8 -> (10 * factor) |> UMX.tag<sorterCount>
        | 12 -> (10 * factor) |> UMX.tag<sorterCount>
        | 16 -> (50 * factor) |> UMX.tag<sorterCount>
        | 24 -> (50 * factor) |> UMX.tag<sorterCount>
        | 32 -> (50 * factor) |> UMX.tag<sorterCount>
        | 48 -> (10 * factor) |> UMX.tag<sorterCount>
        | 64 -> (10 * factor) |> UMX.tag<sorterCount>
        | 96 -> (10 * factor) |> UMX.tag<sorterCount>
        | _ -> failwithf "Unsupported sorting width: %d" (%sortingWidth)


    let sortingWidthValues4 = 
        [4; 8; 16; 32; 64] |> List.map(fun d -> d.ToString())

    let sortingWidths4() : string*string list =
        (Run.sortingWidthKey, sortingWidthValues4)
        
    let sortingWidthValues6 = 
        [6; 12; 24; 48; 96] |> List.map(fun d -> d.ToString())

    let sortingWidths6() : string*string list =
        (Run.sortingWidthKey, sortingWidthValues6)


    let sorterModelKeyValues4 () : string list =
        [ SorterModelKey.Mcse; 
          SorterModelKey.Mssi;
          SorterModelKey.Msrs; 
          SorterModelKey.Msuf4; ]      |> List.map(SorterModelKey.toString)

    let sorterModelKeys4 () : string*string list =
        (Run.sorterModelNameKey, sorterModelKeyValues4() )

    let sorterModelKeyValues6 () : string list =
        [ SorterModelKey.Mcse; 
          SorterModelKey.Mssi;
          SorterModelKey.Msrs; 
          SorterModelKey.Msuf6; ]      |> List.map(SorterModelKey.toString)

    let sorterModelKeys6 () : string*string list =
        (Run.sorterModelNameKey, sorterModelKeyValues6() )


    let parameterSet4 = 
        [ sortingWidths4(); sorterModelKeys4() ]

    let parameterSet6 = 
        [ sortingWidths6(); sorterModelKeys6() ]

    let workspace4 = Workspace.create experimentName4 experimentDesc4 projectDir parameterSet4

    let workspace6 = Workspace.create experimentName6 experimentDesc6 projectDir parameterSet6


    let executor4 (workspace: Workspace) (cycle: int<cycleNumber>) (run: Run) : Async<unit> =
        async {

            Console.WriteLine(sprintf "Executing Run %d   %A" run.Index run.Parameters)
            Run.setCycle run cycle

            let sorterModelKey = Run.getSorterModelName run
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
                    failwith "Msuf6 not supported in this experiment"

            let cycleFactor = if (%cycle = 0) then 1 else 10

            let sorterCount = sortingWidth |> getSorterCountForSortingWidth cycleFactor


            let firstIndex = (%cycle * %sorterCount) |> UMX.tag<sorterCount>
            
            let sorterModelSetMaker = sorterModelSetMaker.create modelMaker firstIndex sorterCount
            let sorterModelSet = sorterModelSetMaker.MakeSorterModelSet (Rando.create)
            let sorterSet = SorterModelSet.makeSorterSet sorterModelSet

            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterSet |> outputData.SorterSet)
            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterModelSetMaker |> outputData.SorterModelSetMaker)


            Console.WriteLine(sprintf "Finished executing Run %d  Cycle  %d \n" run.Index %cycle)
        }


    let executor6 (workspace: Workspace) (cycle: int<cycleNumber>) (run: Run) : Async<unit> =
        async {

            Console.WriteLine(sprintf "Executing Run %d   %A" run.Index run.Parameters)
            Run.setCycle run cycle

            let sorterModelKey = Run.getSorterModelName run
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
                    failwith "Msuf4 not supported in this experiment"
                | SorterModelKey.Msuf6 -> 
                    let uf6GenRatesArray = Uf6GenRatesArray.createUniform %stageCount %sortingWidth
                    (Msuf6RandGen.create randomType sortingWidth stageCount uf6GenRatesArray) |> SorterModelMaker.SmmMsuf6RandGen

            let cycleFactor = if (%cycle = 0) then 1 else 10

            let sorterCount = sortingWidth |> getSorterCountForSortingWidth cycleFactor


            let firstIndex = (%cycle * %sorterCount) |> UMX.tag<sorterCount>
            
            let sorterModelSetMaker = sorterModelSetMaker.create modelMaker firstIndex sorterCount
            let sorterModelSet = sorterModelSetMaker.MakeSorterModelSet (Rando.create)
            let sorterSet = SorterModelSet.makeSorterSet sorterModelSet

            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterSet |> outputData.SorterSet)
            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterModelSetMaker |> outputData.SorterModelSetMaker)


            Console.WriteLine(sprintf "Finished executing Run %d  Cycle  %d \n" run.Index %cycle)
        }




    let RunAll4() =
        for i in 0 .. 1 do
            let cycle = i |> UMX.tag<cycleNumber>
            WorkspaceOps.executeWorkspace workspace4 cycle 6 executor4



    let RunAll6() =
        for i in 0 .. 1 do
            let cycle = i |> UMX.tag<cycleNumber>
            WorkspaceOps.executeWorkspace workspace6 cycle 6 executor6
