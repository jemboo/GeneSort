namespace GeneSort.Dispatch.V1.SorterSgd


type sorterSgdExecutorType = 
    | GenStandard
    | GenMerge
    | FullReport
    | MutantReport


module SorterSgdExecutorType =
    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | FullReport -> "FullReport"
        | MutantReport -> "MutantReport"



module CommonSorterMutate = ()



