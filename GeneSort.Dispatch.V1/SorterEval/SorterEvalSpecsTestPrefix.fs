namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.CommonParams

module SorterEvalSpecsTestPrefix =

    let private mergeEnhancer 
                    (host: IRunHost) 
                    (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters host.Run.RunName)
                 |> Option.get

        let stf = rp.GetSortableTestFilter().Value

        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithSortingWidth(Some stf.sortingWidth)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Id)


    let private paramMapFilter (rp: runParameters) : runParameters option = 
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! stf = rp.GetSortableTestFilter()
            let sw = stf.sortingWidth
        
            let has2factor = (%sw % 2 = 0)
            let isMuf4able = (MathUtils.isAPowerOfTwo %sw)
            let isMuf6able = (%sw % 3 = 0) && (MathUtils.isAPowerOfTwo (%sw / 3))

            // We bind to unit just to enforce the filter
            let! _ = 
                match smt with
                | simpleSorterModelType.Msce -> Some ()
                | simpleSorterModelType.Mssi | simpleSorterModelType.Msrs -> 
                    if has2factor then Some () else None
                | simpleSorterModelType.Msuf4 -> 
                    if isMuf4able then Some () else None
                | simpleSorterModelType.Msuf6 -> 
                    if isMuf6able then Some () else None

            return rp
        }


    module Specs =

        let TestPrefixFilter_Test (executorType: sorterEvalExecutorType) : runHostSpec = {
            databaseName = SorterEvalDbs.Prefix.dbName
            runName = sprintf @"TestPrefixFilter-TestC_%s" (SorterEvalExecutorType.toString executorType) |> UMX.tag
            runDescription = "TestPrefixFilter eval for Msce/Mssi/Msrs/Msuf4"
            spans = [   
                rngTypeLcg
                sortableTestFilter_Prefix24_4
                allSimpleSorterModelTypes
                dataFomatBitv512
                sorterEvalTypeV2
                largeSorterCount
            ]
            filter = paramMapFilter
            enhancer = mergeEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }



    type configType =
        | TestPrefixFilter_Test


    let Configs = Map.ofList 
                    [ 
                        (configType.TestPrefixFilter_Test, Specs.TestPrefixFilter_Test);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterEvalExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType