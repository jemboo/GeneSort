namespace GeneSort.Dispatch.V1.SorterMutate

open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Eval.V1
open GeneSort.Sorting
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.SortingOps


module MutateSpecsRs = 

    let sorterEvalSelectionType = 
            (runParameters.seedSorterPoolSelectionTypeKey, 
            [ sorterSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterSelectionType.toString)
    

    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)  
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithExcludeSelfCe(Some (true |> UMX.tag<excludeSelfCe>))
          .WithSortableDataFormat(Some sortableDataFormat.BitVector512)
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

        let Test_Msrs (executorType: sorterMutateExecutorType)  : runHostSpec = {
            databaseName = SorterMutateDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Test_Msrs%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Msrs"
            spans = [
                msrsModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV1
                sorterEvalSelectionType
                sorterEvalMeasure_CestM_Scw
                (mutatorSpansMsrs 16<sortingWidth> rngType.Lcg)
                modificationRate90
                sortingWidth16
                testChildCount
                mutationMod1
            ]
            filter = standardSorterModelTypeFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }


        let Test_Msuf4 (executorType: sorterMutateExecutorType)  : runHostSpec = {
            databaseName = SorterMutateDbs.RandomStandard.Uniform.dbName
            runName = sprintf @"Rand-Test_Msuf4%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for Msuf4"
            spans = [
                msuf4ModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV1
                sorterEvalSelectionType
                sorterEvalMeasure_CestM_Scw
                (mutatorSpansMsrs 16<sortingWidth> rngType.Lcg)
                modificationRate90
                sortingWidth16
                testChildCount
                mutationMod1
            ]
            filter = standardSorterModelTypeFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 1
        }





    type configType =
        | Test_Msrs
        | Rand_Small
        | Rand_Medium

    let Configs = Map.ofList 
                    [ 
                        (configType.Test_Msrs, Specs.Test_Msrs); 
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterMutateExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


