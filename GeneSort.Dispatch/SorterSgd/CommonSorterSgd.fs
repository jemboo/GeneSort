namespace GeneSort.Dispatch.V1.SorterSgd


type sorterSgdExecutorType = 
    | GenStandard
    | GenMerge
    | FullReport


module SorterSgdExecutorType =
    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | FullReport -> "FullReport"



module CommonSorterMutate = ()



