namespace GeneSort.Dispatch.V1.SorterMutate.Msuf4


open FSharp.UMX
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.SortingOps
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Sorting
open GeneSort.Eval.V1
open GeneSort.Dispatch.V1.SorterMutate
open GeneSort.Dispatch.V1.SortableTest.CommonSortableTest
open GeneSort.Dispatch.V1.SorterMutate.CommonSorterMutate


module Msuf4MutateSpecsRm =

    let sorterEvalSelectionType = 
            (runParameters.sorterEvalSelectionType, 
            [ sorterEvalSelectionType.ValueSpan 5<sorterCount>;] |> List.map SorterEvalSelectionType.toString)


    let sorterEvalMeasure = 
            (runParameters.sorterEvalMeasureKey, 
            [ sorterEvalMeasure.CeSt (1.0, true);] |> List.map SorterEvalMeasure.toString)
   


    // MutationRates
    let orthoRates =
            (runParameters.orthoRateKey, [1.0;] |> List.map string)
    let paraRates =
            (runParameters.paraRateKey, [1.0;] |> List.map string)
    let selfSymRates =
            (runParameters.selfSymRateKey, [1.0;] |> List.map string)
    let seedModificationRates =
            (runParameters.modificationRateKey, [0.00; 0.0025;] |> List.map string)
    let modificationRates =
            (runParameters.modificationRateKey, [0.0025; 0.005; 0.01; 0.015; 0.02; 0.03; 0.04; 0.05 ] |> List.map string)


    // SorterCounts
    let testChildCount = (runParameters.sorterChildCountKey, ["10";] )
    let smallChildCount = (runParameters.sorterChildCountKey, ["10";] )
    let mediumChildCount = (runParameters.sorterChildCountKey, ["100";] )
    let largeChildCount = (runParameters.sorterChildCountKey, ["10000";] )


    // SorterModelTypes
    let msuf4ModelType = 
            (runParameters.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Msuf4] |> List.map SimpleSorterModelType.toString)


    let standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let qp = host.RunDb.MakeQueryParamsFromRunParams rp (outputDataType.Run host.Run.RunName)  
        rp.WithDatabaseName(Some host.Run.DatabaseName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithId (Some qp.Value.Id)



    let private paramMapFilter (rp: runParameters) : runParameters option = 
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let! md = rp.GetMergeDimension()
            let! mst = rp.GetMergeSuffixType()
        
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
                    if (isMuf4able && %sw < 256) then Some () else None
                | simpleSorterModelType.Msuf6 -> 
                    if isMuf6able then Some () else None
                | _ -> None

            // Merge dimension check: If it doesn't divide, return None to stop
            if (%sw % %md <> 0) then return! None
        
            // Suffix check: If it's NoSuffix and width > 128, return None to stop
            if (mst.IsNoSuffix && %sw > 128) then return! None
        
            return rp
        }


    module Specs =

        let Rand_Test (executorType: sorterMutateExecutorType)  : runHostSpec = {
            DatabaseName = Muf4MutateDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-test_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for merge Msuf4"
            Spans = [
                projectRngType
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalType
                orthoRates
                paraRates
                selfSymRates
                seedModificationRates
                modificationRates
                testSortingWidth
                msuf4ModelType
                mergeDimension2
                noSuffixSuffixType
                dataFormatInt8v512
                largeChildCount
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

        let Rand_Small (executorType: sorterMutateExecutorType) : runHostSpec = {
            DatabaseName = Muf4MutateDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-Small_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for merge Msuf4"
            Spans = [
                projectRngType
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalType
                orthoRates
                paraRates
                selfSymRates
                seedModificationRates
                modificationRates
                smallSortingWidths
                msuf4ModelType
                lowMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
                largeChildCount
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 8
        }

        let Rand_Medium (executorType: sorterMutateExecutorType) : runHostSpec = {
            DatabaseName = Muf4MutateDbs.RandomMerge.Uniform.dbName
            RunName = sprintf @"Rand-Medium_%s" (SorterMutateExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Mutation analysis for merge Msuf4"
            Spans = [
                projectRngType
                sorterEvalSelectionType
                sorterEvalMeasure
                sorterEvalType
                orthoRates
                paraRates
                selfSymRates
                seedModificationRates
                modificationRates
                mediumSortingWidths
                msuf4ModelType
                lowMergeDimensions
                noSuffixSuffixType
                dataFormatInt8v512
                largeChildCount
            ]
            Filter = paramMapFilter
            Enhancer = standardEnhancer
            AllowOverwrite = false |> UMX.tag
            MaxParallel = 4
        }

    type configType =
        | Rand_Test
        | Rand_Small
        | Rand_Medium

    let Configs = Map.ofList 
                    [ 
                        (configType.Rand_Test, Specs.Rand_Test); 
                        (configType.Rand_Small, Specs.Rand_Small);
                        (configType.Rand_Medium, Specs.Rand_Medium);
                    ]

    let getConfig (config: configType) (executorType: sorterMutateExecutorType) : runHostSpec =
        let specFunc = Configs.[config]
        specFunc executorType


