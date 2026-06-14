namespace GeneSort.Dispatch.V1.SorterMutate


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



module CommonSorterMutate = ()



