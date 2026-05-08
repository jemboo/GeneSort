namespace GeneSort.Dispatch.V1.SorterEvalBins


type executorType = 
    | Standard
    | Merge
    | FullReport
    | BinsReport

module ExecutorType =
    let toString = function
        | Standard -> "Standard"
        | Merge -> "Merge"
        | FullReport -> "FullReport"
        | BinsReport -> "BinsReport"



