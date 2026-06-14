namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Project.V1
open GeneSort.Dispatch.V1
open CommonSortableTest


module SortableTestMergeSpecs =

    let private standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters host.Run.RunName)
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Value.Id)

    let private mergeDimensionDividesSortingWidth (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let md = rp.GetMergeDimension().Value
        if (%sw % %md = 0) then Some rp else None

    let private limitForMergeFillType (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let ft = rp.GetMergeSuffixType().Value
        if (ft.IsNoSuffix && %sw > 64) then None else Some rp

    let private limitForMergeDimension (rp: runParameters) =
        let sw = rp.GetSortingWidth().Value
        let md = rp.GetMergeDimension().Value
        if (%md > 6 && %sw > 144) then None else Some rp

    let standardParamMapFilter (rp: runParameters) = 
        Some rp
        |> Option.bind mergeDimensionDividesSortingWidth
        |> Option.bind limitForMergeFillType
        |> Option.bind limitForMergeDimension



    module Specs =

        let Merge_test  (executorType: executorType) : runHostSpec = {
            DatabaseName = CommonSortableTest.mergeDatabaseName
            RunName = sprintf @"Merge-test_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Int8 merge sorter test sets"
            Spans = [
                testSortingWidth
                dataFormatInt8v512
                mergeDimension2
                noSuffixSuffixType
            ]
            Filter = mergeDimensionDividesSortingWidth
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 2
        }


        let Merge_small (executorType: executorType) : runHostSpec = {
            DatabaseName = CommonSortableTest.mergeDatabaseName
            RunName = sprintf @"Merge-small_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge_small sorter test sets"
            Spans = [
                smallSortingWidths
                dataFormatInt8v512
                allMergeDimensions
                noSuffixSuffixType
            ]
            Filter = mergeDimensionDividesSortingWidth
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

        
        let Merge_MediumLd (executorType: executorType) : runHostSpec = {
            DatabaseName = CommonSortableTest.mergeDatabaseName
            RunName = sprintf @"Merge-MediumLd_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge_MediumLd sorter test sets"
            Spans = [
                mediumSortingWidths
                dataFormatInt8v512
                lowMergeDimensions
                noSuffixSuffixType
            ]
            Filter = mergeDimensionDividesSortingWidth
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

        
        let Merge_MediumHd (executorType: executorType) : runHostSpec = {
            DatabaseName = CommonSortableTest.mergeDatabaseName
            RunName = sprintf @"Merge-MediumHd_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge_MediumHd sorter test sets"
            Spans = [
                largeSortingWidths
                dataFormatInt8v512
                highMergeDimensions
                noSuffixSuffixType
            ]
            Filter = standardParamMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 2
        }

        let Merge_LargeLd (executorType: executorType) : runHostSpec = {
            DatabaseName = CommonSortableTest.mergeDatabaseName
            RunName = sprintf @"Merge-LargeLd_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge_LargeLd sorter test sets"
            Spans = [
                highMergeDimensions
                dataFormatInt8v512
                mergeDimension2
                noSuffixSuffixType
            ]
            Filter = standardParamMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 2
        }


    type configType =
        | Merge_Test
        | Merge_Small
        | Merge_MediumLd
        | Merge_MediumHd
        | Merge_LargeLd

    let Configs = Map.ofList 
                    [ 
                        (configType.Merge_Test, Specs.Merge_test); 
                        (configType.Merge_Small, Specs.Merge_small);
                        (configType.Merge_MediumLd, Specs.Merge_MediumLd);
                        (configType.Merge_MediumHd, Specs.Merge_MediumHd);
                        (configType.Merge_LargeLd, Specs.Merge_LargeLd);
                    ]

    let getConfig (config: configType) (executorType: executorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType