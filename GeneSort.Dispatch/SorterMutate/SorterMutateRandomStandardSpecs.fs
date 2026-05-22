namespace GeneSort.Dispatch.V1.SorterMutate

open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1


module SorterMutateRandomStandardSpecs = 


    let rngType = 
            (runParameters.rngTypeKey, 
            [CommonSorterMutate.projectRngType;] |> List.map RngType.toString)
    
    // SortingWidths
    let smallSortingWidths = 
            (runParameters.sortingWidthKey, [4;5;6;7;8;9;10;11;12] |> List.map string)
    let mediumSortingWidths = 
            (runParameters.sortingWidthKey, [14;16;18;20;22] |> List.map string)

    // SorterCounts
    let testChildCount = (runParameters.sorterChildCountKey, ["10";] )
    let smallChildCount = (runParameters.sorterChildCountKey, ["1";] )
    let mediumChildCount = (runParameters.sorterChildCountKey, ["10";] )
    let largeChildCount = (runParameters.sorterChildCountKey, ["100";] )

    let testParentCount = (runParameters.sorterParentCountKey, ["10";] )
    let smallParentCount = (runParameters.sorterParentCountKey, ["10";] )
    let mediumParentCount = (runParameters.sorterParentCountKey, ["100";] )
    let largeParentCount = (runParameters.sorterParentCountKey, ["1000";] )



    // SimpleSorterModelTypes
    let allSimpleSorterModelTypes = 
            (runParameters.simpleSorterModelTypeKey, SimpleSorterModelType.all() |> List.map SimpleSorterModelType.toString)



    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)
        let mutationRate = rp.GetMutationRate()
        let insertionRate =
            maybe {
                let! mr = mutationRate
                return (%mr / 2.0) |> UMX.tag<insertionRate>
            }
        let deletionRate =
            maybe {
                let! mr = mutationRate
                return (%mr / 2.0) |> UMX.tag<deletionRate>
            }   
        rp.WithProjectName(Some host.Run.ProjectName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithInsertionRate(insertionRate)
          .WithDeletionRate(deletionRate)
          .WithId (Some qp.Value.Id)

    
    let private standardSorterModelTypeFilter (rp: runParameters) =
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let has2factor = (%sw % 2 = 0)
            let isPowerOf2 = (%sw &&& (%sw - 1) = 0)
            let isGt4 = (%sw > 4)
            let validMsce = (smt = simpleSorterModelType.Msce)
            let validMssi = (smt = simpleSorterModelType.Mssi) && has2factor
            let validMsrs = (smt = simpleSorterModelType.Msrs) && has2factor
            let validMsuf4 = (smt = simpleSorterModelType.Msuf4) && isPowerOf2 && isGt4
            return! if validMsce || validMssi || validMsrs || validMsuf4 then Some rp else None
        }


    module Specs =

        let SorterMutate_Standard_test (executorType: sorterMutateExecutorType)  : runHostSpec = {
            ProjectName = CommonSorterMutate.projectName
            DatabaseName = CommonSorterMutate.randomStandardDatabaseName
            RunName = sprintf @"SorterMutate_Standard_test_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Standard binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [
                rngType
                smallSortingWidths
                allSimpleSorterModelTypes
                testParentCount
                testChildCount
            ]
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 2
        }

        let SorterMutate_Standard_small (executorType: sorterMutateExecutorType) : runHostSpec = {
            ProjectName = CommonSorterMutate.projectName
            DatabaseName = CommonSorterMutate.randomStandardDatabaseName
            RunName = sprintf @"SorterMutate_Standard_small_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Standard binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [
                rngType
                smallSortingWidths
                allSimpleSorterModelTypes
                testParentCount
                largeChildCount
            ]
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 8
        }

        let SorterMutate_Standard_medium (executorType: sorterMutateExecutorType) : runHostSpec = {
            ProjectName = CommonSorterMutate.projectName
            DatabaseName = CommonSorterMutate.randomStandardDatabaseName
            RunName = sprintf @"SorterMutate_Standard_medium_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Standard binning for Msce/Mssi/Msrs/Msuf4"
            Spans = [
                rngType
                mediumSortingWidths
                allSimpleSorterModelTypes
                largeParentCount
                largeChildCount
            ]
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

    type configType =
        | SorterMutate_Standard_Test
        | SorterMutate_Standard_Small
        | SorterMutate_Standard_Medium

    let Configs = Map.ofList 
                    [ 
                        (configType.SorterMutate_Standard_Test, Specs.SorterMutate_Standard_test); 
                        (configType.SorterMutate_Standard_Small, Specs.SorterMutate_Standard_small);
                        (configType.SorterMutate_Standard_Medium, Specs.SorterMutate_Standard_medium);
                    ]

    let getConfig (config: configType) (executorType: sorterMutateExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


