namespace GeneSort.Dispatch.V1.SorterMutate


type sorterMutateExecutorType = 
    | GenStandard
    | GenMerge
    | GenPrefix
    | StandardReport
    | MergeReport
    | PrefixReport


module SorterMutateExecutorType =
    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | GenPrefix -> "GenPrefix"
        | StandardReport -> "StandardReport"
        | MergeReport -> "MergeReport"
        | PrefixReport -> "PrefixReport"



module CommonSorterMutate = ()



