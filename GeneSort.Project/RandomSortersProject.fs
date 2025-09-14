
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


    let getCeLengthForSortingWidth (sortingWidth: int<sortingWidth>) : int<ceLength> =
        match %sortingWidth with
        | 4 -> 300 |> UMX.tag<ceLength>
        | 6 -> 600 |> UMX.tag<ceLength>
        | 8 -> 16 |> UMX.tag<ceLength>
        | 12 -> 24 |> UMX.tag<ceLength>
        | 16 -> 32 |> UMX.tag<ceLength>
        | 24 -> 48 |> UMX.tag<ceLength>
        | 32 -> 64 |> UMX.tag<ceLength>
        | 48 -> 96 |> UMX.tag<ceLength>
        | 64 -> 128 |> UMX.tag<ceLength>
        | 96 -> 192 |> UMX.tag<ceLength>
        | _ -> failwithf "Unsupported sorting width: %d" (%sortingWidth)


    let getStageLengthForSortingWidth (sortingWidth: int<sortingWidth>) : int<stageLength> =
        match %sortingWidth with
        | 4 -> 5 |> UMX.tag<stageLength>
        | 6 -> 10 |> UMX.tag<stageLength>
        | 8 -> 20 |> UMX.tag<stageLength>
        | 12 -> 30 |> UMX.tag<stageLength>
        | 16 -> 100 |> UMX.tag<stageLength>
        | 24 -> 150 |> UMX.tag<stageLength>
        | 32 -> 200 |> UMX.tag<stageLength>
        | 48 -> 300 |> UMX.tag<stageLength>
        | 64 -> 400 |> UMX.tag<stageLength>
        | 96 -> 600 |> UMX.tag<stageLength>
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
        [ sorterModelKey.Mcse; 
          sorterModelKey.Mssi;
          sorterModelKey.Msrs; 
          sorterModelKey.Msuf4; ]      |> List.map(SorterModelKey.toString)

    let sorterModelKeys4 () : string*string list =
        (Run.sorterModelTypeKey, sorterModelKeyValues4() )

    let sorterModelKeyValues6 () : string list =
        [ sorterModelKey.Mcse; 
          sorterModelKey.Mssi;
          sorterModelKey.Msrs; 
          sorterModelKey.Msuf6; ]      |> List.map(SorterModelKey.toString)

    let sorterModelKeys6 () : string*string list =
        (Run.sorterModelTypeKey, sorterModelKeyValues6() )


    let parameterSet4 = 
        [ sortingWidths4(); sorterModelKeys4() ]

    let parameterSet6 = 
        [ sortingWidths6(); sorterModelKeys6() ]

    let workspace4 = Workspace.create experimentName4 experimentDesc4 projectDir parameterSet4

    let workspace6 = Workspace.create experimentName6 experimentDesc6 projectDir parameterSet6


    let executor4 (workspace: workspace) (cycle: int<cycleNumber>) (run: Run) : Async<unit> =
        async {

            Console.WriteLine(sprintf "Executing Run %d   %A" run.Index run.Parameters)
            Run.setCycle run cycle

            let sorterModelKey = Run.getSorterModelKey run
            let sortingWidth = Run.getSortingWidth run

            let ceLength = getCeLengthForSortingWidth sortingWidth
            Run.setCeLength run ceLength

            let stageLength = getStageLengthForSortingWidth sortingWidth
            Run.setStageLength run stageLength

            let sorterModelMaker =
                match sorterModelKey with
                | sorterModelKey.Mcse -> (MsceRandGen.create randomType sortingWidth excludeSelfCe ceLength) |> sorterModelMaker.SmmMsceRandGen
                | sorterModelKey.Mssi -> (MssiRandGen.create randomType sortingWidth stageLength) |> sorterModelMaker.SmmMssiRandGen
                | sorterModelKey.Msrs -> 
                    let opsGenRatesArray = OpsGenRatesArray.createUniform %stageLength
                    (msrsRandGen.create randomType sortingWidth opsGenRatesArray) |> sorterModelMaker.SmmMsrsRandGen
                | sorterModelKey.Msuf4 -> 
                    let uf4GenRatesArray = Uf4GenRatesArray.createUniform %stageLength %sortingWidth
                    (msuf4RandGen.create randomType sortingWidth stageLength uf4GenRatesArray) |> sorterModelMaker.SmmMsuf4RandGen
                | sorterModelKey.Msuf6 -> 
                    failwith "Msuf6 not supported in this experiment"

            let cycleFactor = if (%cycle = 0) then 1 else 10
            let sorterCount = sortingWidth |> getSorterCountForSortingWidth cycleFactor
            Run.setSorterCount run sorterCount

            let firstIndex = (%cycle * %sorterCount) |> UMX.tag<sorterCount>
            
            let sorterModelSetMaker = sorterModelSetMaker.create sorterModelMaker firstIndex sorterCount
            let sorterModelSet = sorterModelSetMaker.MakeSorterModelSet (Rando.create)
            let sorterSet = SorterModelSet.makeSorterSet sorterModelSet

            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterSet |> outputData.SorterSet)
            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterModelSetMaker |> outputData.SorterModelSetMaker)

            Console.WriteLine(sprintf "Finished executing Run %d  Cycle  %d \n" run.Index %cycle)
        }


    let executor6 (workspace: workspace) (cycle: int<cycleNumber>) (run: Run) : Async<unit> =
        async {
            Console.WriteLine(sprintf "Executing Run %d   %A" run.Index run.Parameters)
            Run.setCycle run cycle

            let sorterModelKey = Run.getSorterModelKey run
            let sortingWidth = Run.getSortingWidth run

            let ceLength = getCeLengthForSortingWidth sortingWidth
            Run.setCeLength run ceLength

            let stageLength = getStageLengthForSortingWidth sortingWidth
            Run.setStageLength run stageLength

            let sorterModelMaker =
                match sorterModelKey with
                | sorterModelKey.Mcse -> (MsceRandGen.create randomType sortingWidth excludeSelfCe ceLength) |> sorterModelMaker.SmmMsceRandGen
                | sorterModelKey.Mssi -> (MssiRandGen.create randomType sortingWidth stageLength) |> sorterModelMaker.SmmMssiRandGen
                | sorterModelKey.Msrs -> 
                    let opsGenRatesArray = OpsGenRatesArray.createUniform %stageLength
                    (msrsRandGen.create randomType sortingWidth opsGenRatesArray) |> sorterModelMaker.SmmMsrsRandGen
                | sorterModelKey.Msuf4 -> 
                    failwith "Msuf4 not supported in this experiment"
                | sorterModelKey.Msuf6 -> 
                    let uf6GenRatesArray = Uf6GenRatesArray.createUniform %stageLength %sortingWidth
                    (msuf6RandGen.create randomType sortingWidth stageLength uf6GenRatesArray) |> sorterModelMaker.SmmMsuf6RandGen

            let cycleFactor = if (%cycle = 0) then 1 else 10
            let sorterCount = sortingWidth |> getSorterCountForSortingWidth cycleFactor
            Run.setSorterCount run sorterCount

            let firstIndex = (%cycle * %sorterCount) |> UMX.tag<sorterCount>
            
            let sorterModelSetMaker = sorterModelSetMaker.create sorterModelMaker firstIndex sorterCount
            let sorterModelSet = sorterModelSetMaker.MakeSorterModelSet (Rando.create)
            let sorterSet = SorterModelSet.makeSorterSet sorterModelSet

            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterSet |> outputData.SorterSet)
            do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterModelSetMaker |> outputData.SorterModelSetMaker)

            Console.WriteLine(sprintf "Finished executing Run %d  Cycle  %d \n" run.Index %cycle)
        }




    let RunAll4() =
        for i in 0 .. 0 do
            let cycle = i |> UMX.tag<cycleNumber>
            WorkspaceOps.executeWorkspace workspace4 cycle 6 executor4


    let RunAll6() =
        for i in 0 .. 0 do
            let cycle = i |> UMX.tag<cycleNumber>
            WorkspaceOps.executeWorkspace workspace6 cycle 6 executor6
