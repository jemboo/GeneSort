namespace GeneSort.Dispatch.V1.SorterMutate


open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Dispatch.V1.SortableTest
open GeneSort.SortingOps



type sorterMutateExecutorType = 
    | GenStandard
    | GenMerge
    | FullReport
    | MutantReport
    | MutantMergeReport


module SorterMutateExecutorType =
    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | FullReport -> "FullReport"
        | MutantReport -> "MutantReport"
        | MutantMergeReport -> "MutantMergeReport"



module CommonSorterMutate = 

    let CollectSortableTests = true

    let ExcludeSelfCe = true |> UMX.tag<excludeSelfCe>

    let standardSortableDataFormat = sortableDataFormat.BitVector512
    let mergeSortableDataFormat = CommonSortableTest.dataFormatInt8v512
    let projectRngType = 
            (runParameters.rngTypeKey, 
            [rngType.Lcg;] |> List.map RngType.toString)

    // MutationRates
    let mutationRates =
            (runParameters.mutationRateKey, [1.0] |> List.map string)
    let insertionRates =
            (runParameters.insertionRateKey, [0.1;] |> List.map string)
    let deletionRates =
            (runParameters.deletionRateKey, [0.1;] |> List.map string)
    let orthoRates =
            (runParameters.orthoRateKey, [1.0;] |> List.map string)
    let paraRates =
            (runParameters.paraRateKey, [1.0;] |> List.map string)
    let selfSymRates =
            (runParameters.selfSymRateKey, [1.0;] |> List.map string)

    let noSeedModificationRates =
            (runParameters.modificationRateKey, [0.00] |> List.map string)

    let modificationRates =
            (runParameters.modificationRateKey, [0.005; 0.01; 0.02; 0.04; 0.08 ] |> List.map string)


    // SorterCounts
    let testChildCount = (runParameters.sorterChildCountKey, ["10";] )
    let smallChildCount = (runParameters.sorterChildCountKey, ["10";] )
    let mediumChildCount = (runParameters.sorterChildCountKey, ["100";] )
    let largeChildCount = (runParameters.sorterChildCountKey, ["10000";] )

    let sorterEvalType = 
            (runParameters.sorterEvalTypeKey, 
            [ sorterEvalType.V1; ] |> List.map SorterEvalType.toString)


    // SorterModelTypes
    let msceModelType = 
            (runParameters.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Msce] |> List.map SimpleSorterModelType.toString)


    // SorterModelTypes
    let msrsModelType = 
            (runParameters.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Msrs] |> List.map SimpleSorterModelType.toString)


    // SorterModelTypes
    let mssiModelType = 
            (runParameters.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Mssi] |> List.map SimpleSorterModelType.toString)

    // SorterModelTypes
    let msuf4ModelType = 
            (runParameters.simpleSorterModelTypeKey, 
             [simpleSorterModelType.Msuf4] |> List.map SimpleSorterModelType.toString)


