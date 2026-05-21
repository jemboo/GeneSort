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
    let testSorterCount = (runParameters.sorterCountKey, ["1000";] )
    let smallSorterCount = (runParameters.sorterCountKey, ["10000";] )
    let mediumSorterCount = (runParameters.sorterCountKey, ["100000";] )
    let largeSorterCount = (runParameters.sorterCountKey, ["1000000";] )
    
    // SimpleSorterModelTypes
    let allSimpleSorterModelTypes = 
            (runParameters.simpleSorterModelTypeKey, SimpleSorterModelType.all() |> List.map SimpleSorterModelType.toString)



    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.ProjectDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)
        rp.WithProjectName(Some host.Run.ProjectName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
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
                testSorterCount
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
                largeSorterCount
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
                largeSorterCount
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


