namespace GeneSort.Dispatch.V1.SorterMutate


type sorterMutateExecutorType = 
    | GenStandard
    | GenMerge
    | FullReport
    | MutantReport


module SorterMutateExecutorType =
    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | FullReport -> "FullReport"
        | MutantReport -> "MutantReport"



module CommonSorterMutate = ()



