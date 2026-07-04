namespace GeneSort.Dispatch.V1.SorterMutate


type sorterMutateExecutorType = 
    | GenStandard
    | GenMerge
    | MergeReport
    | StandardReport


module SorterMutateExecutorType =
    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | MergeReport -> "MergeReport"
        | StandardReport -> "StandardReport"



module CommonSorterMutate = ()



