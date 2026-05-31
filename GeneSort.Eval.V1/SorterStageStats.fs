namespace GeneSort.Eval.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting.Sorter


type sorterStageStats =
    private {
        sorterId: Guid<sorterId>
        stageLength: int<stageLength>
        stageIndex: int<stageLength>
        averageCeLength: float
    }

    static member create 
            (sorterId: Guid<sorterId>) 
            (stageLength: int<stageLength>) 
            (stageIndex: int<stageLength>) 
            (averageCeLength: float) : sorterStageStats =
        {
            sorterId = sorterId
            stageLength = stageLength
            stageIndex = stageIndex
            averageCeLength = averageCeLength
        }


    member this.toDataTableRecord() : dataTableRecord =
        dataTableRecord.createEmpty()
        |> dataTableRecord.addData "SorterId" (this.sorterId |> UMX.untag |> string)
        |> dataTableRecord.addData "StageLength" (this.stageLength |> UMX.untag |> string)
        |> dataTableRecord.addData "StageIndex" (this.stageIndex |> UMX.untag |> string)
        |> dataTableRecord.addData "AverageCeLength" (this.averageCeLength |> string)



module SorterStageStats = 

    let fromSorterEval (sorterEval: sorterEval) : sorterStageStats array =
        let ces = sorterEval |> SorterEval.getCeUseArray |> Array.map (fun cd -> cd.Ce)
        let sortingWidth = sorterEval |> SorterEval.getSortingWidth
        // 2. Process elements through your StageBuilderSequence layout pipeline
        let stageSeq = StageBuilderSequence.toStageSequence sortingWidth ces
        let stages = stageSeq.Stages
        let totalStagesCount = stages.Length |> UMX.tag<stageLength>

        // 3. Map individual stages in the structural sequence into discrete stats objects
        stages 
        |> Array.mapi (fun idx stg ->
            let currentStageIndex = idx |> UMX.tag<stageLength>
            sorterStageStats.create 
                (sorterEval |> SorterEval.getSorterId) 
                totalStagesCount 
                currentStageIndex 
                stg.AverageCeLength
        )
