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


module MsceMutateSpecsRp = 

    let seedSorterPoolSelectionType = 
            (runParameters.seedSorterPoolSelectionTypeKey, 
            [ sorterSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterSelectionType.toString)

    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)  
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithExcludeSelfCe(Some (true |> UMX.tag<excludeSelfCe>))
          .WithCollectNewSortableTests(Some (false |> UMX.tag<collectNewSortableTests>))
          .WithId (Some qp.Value.Id)



    let private paramMapFilter (rp: runParameters) : runParameters option = 
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! mrgLibId = rp.GetPrefixLibId()
        
            let has2factor = (%mrgLibId.SortingWidth % 2 = 0)
            let isMuf4able = (MathUtils.isAPowerOfTwo %mrgLibId.SortingWidth)
            let isMuf6able = (%mrgLibId.SortingWidth % 3 = 0) && (MathUtils.isAPowerOfTwo (%mrgLibId.SortingWidth / 3))

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

        let Test_Msce (executorType: sorterMutateExecutorType)  : runHostSpec = {
            databaseName = SorterMutateDbs.RandomPrefix.Uniform.dbName
            runName = sprintf @"Test-Msce_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msce"
            spans = [
                msceModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                prefixLib_Prefix32_4
                (mutatorSpansMsce 32<sortingWidth> rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }


        let Test_Mssi (executorType: sorterMutateExecutorType)  : runHostSpec = {
            databaseName = SorterMutateDbs.RandomPrefix.Uniform.dbName
            runName = sprintf @"Test-Mssi_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Mssi"
            spans = [
                mssiModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                prefixLib_Prefix32_4
                (mutatorSpansMssi 32<sortingWidth> rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }


        let Test_Msrs (executorType: sorterMutateExecutorType)  : runHostSpec = {
            databaseName = SorterMutateDbs.RandomPrefix.Uniform.dbName
            runName = sprintf @"Test-Msrs_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msrs"
            spans = [
                msrsModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                prefixLib_Prefix32_4
                (mutatorSpansMsrs 32<sortingWidth> rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }

        let Test_Msuf4 (executorType: sorterMutateExecutorType)  : runHostSpec = {
            databaseName = SorterMutateDbs.RandomPrefix.Uniform.dbName
            runName = sprintf @"Test-Msuf4_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            runDescription = "Mutation analysis for merge Msuf4"
            spans = [
                msuf4ModelType
                rngTypeLcg
                dataFomatBitv512
                sorterEvalTypeV2
                seedSorterPoolSelectionType
                sorterEvalMeasure_CestM_noScw
                prefixLib_Prefix32_4
                (mutatorSpansMsuf4 32<sortingWidth> rngType.Lcg)
                modificationRate03
                testChildCount
                mutationMod1
            ]
            filter = paramMapFilter
            enhancer = standardEnhancer
            allowOverwrite = false |> UMX.tag
            maxParallel = 4
        }


    type configType =
        | Test_Msce
        | Test_Mssi
        | Test_Msrs
        | Test_Msuf4
        | Test_Msuf6

    let Configs = Map.ofList 
                    [ 
                        (configType.Test_Msce, Specs.Test_Msce); 
                        (configType.Test_Mssi, Specs.Test_Mssi); 
                        (configType.Test_Msrs, Specs.Test_Msrs); 
                        (configType.Test_Msuf4, Specs.Test_Msuf4);
                    ]

    let getRunHostSpec (config: configType) (executorType: sorterMutateExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


