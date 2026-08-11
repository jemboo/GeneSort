namespace GeneSort.Dispatch.V1.SorterSgd

type sorterSgdExecutorType = 
    | GenStandard
    | GenMerge
    | GenPrefix
    | SummaryReport
    | SnapshotReport


module SorterSgdExecutorType =
    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | GenPrefix -> "GenPrefix"
        | SummaryReport -> "SummaryReport"
        | SnapshotReport -> "SnapshotReport"
