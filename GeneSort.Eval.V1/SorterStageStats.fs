namespace GeneSort.Eval.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting.Sorter

//The paper "Large [g,d] Sorting Networks" by David C. Van Voorhis from August 1971 describes special constructions for [2^r, 2^r]
//f-networks that reduce the number of comparators required by large N-sorter networks. Stanford
//Key Results for Powers of 2
//Van Voorhis provides a table (Table 3) showing G(2^m) values for m ≤ 16, where G(N) represents the minimum number of comparators 
//required by an N-sorter network using the [g,d] strategy. Stanford Here are the specific values:

//N=16 (m=4): 61 comparators (though Green's network uses 60)
//N=32 (m=5): 187 comparators
//N=64 (m=6): 525 comparators
//N=128 (m=7): 1,427 comparators
//N=256 (m=8): 3,705 comparators
//N=512 (m=9): 9,457 comparators
//N=1,024 (m=10): 23,357 comparators
//N=2,048 (m=11): 56,787 comparators
//N=4,096 (m=12): 135,417 comparators
//N=8,192 (m=13): 319,827 comparators
//N=16,384 (m=14): 743,421 comparators
//N=32,768 (m=15): 1,714,003 comparators
//N=65,536 (m=16): 3,899,397 comparators






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
