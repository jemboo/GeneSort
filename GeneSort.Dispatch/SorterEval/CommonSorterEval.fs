namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Dispatch.V1.CommonParams


type sorterEvalExecutorType = 
    | GenStandard
    | GenMerge
    | FullReport
    | StageStatsReport
    | CeBinReport


module SorterEvalExecutorType =
    let toString = function
        | GenStandard -> "GenStandard"
        | GenMerge -> "GenMerge"
        | FullReport -> "FullReport"
        | StageStatsReport -> "StageStatsReport"
        | CeBinReport -> "CeBinReport"


module CommonSorterEval =

    let projectName = "SorterEval" |> UMX.tag<projectName>


    let getSimpleUniformSorterModelGen
            (rngType: rngType) 
            (sortingWidth: int<sortingWidth>)
            (simpleSorterModelType: simpleSorterModelType) 
                            : sorterModelGen =
            let stageLength = getStageLength simpleSorterModelType sortingWidth
            let rngFactory = rngType |> RngFactory.create
            SimpleSorterModelGen.makeUniform 
                                    rngFactory 
                                    sortingWidth 
                                    stageLength 
                                    simpleSorterModelType
                                    ExcludeSelfCe
                                    |> sorterModelGen.Simple






