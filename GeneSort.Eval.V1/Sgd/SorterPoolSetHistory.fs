namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Core
open GeneSort.Eval.V1

type sorterPoolSetHistory = 
    private {
        sorterPoolSetId: Guid<sorterPoolSetId>
        saveGeneration: int<generationNumber>
        poolHistories: sorterPoolHistory list
    }

    static member create
            (sorterPoolSetId: Guid<sorterPoolSetId>,
             saveGeneration: int<generationNumber>,
             poolHistories: sorterPoolHistory list) : sorterPoolSetHistory =
        {
            sorterPoolSetId = sorterPoolSetId
            saveGeneration = saveGeneration
            poolHistories = poolHistories
        }

    member this.SorterPoolSetId with get() = this.sorterPoolSetId
    member this.SaveGeneration with get() = this.saveGeneration
    member this.PoolHistories with get() = this.poolHistories


module SorterPoolSetHistory =

    let pruneAndCreateFromPoolSet 
            (currentGen: int<generationNumber>) 
            (poolSet: sorterPoolSet)
            (runningHistory: Map<Guid<sorterPoolId>, Map<Guid<sorterPoolMemberId>, sorterPoolMemberHistory>>)
            : sorterPoolSetHistory * Map<Guid<sorterPoolId>, Map<Guid<sorterPoolMemberId>, sorterPoolMemberHistory>> =

        let poolHistories, updatedRunningMap =
            poolSet.SorterPools
            |> Map.toList
            |> List.fold (fun (accHist, accMap) (poolId, pool) ->
                let runningForPool = Map.tryFind poolId accMap |> Option.defaultValue Map.empty
                let poolHist, prunedForPool = SorterPoolHistory.pruneAndCreateForPool currentGen pool runningForPool
                (poolHist :: accHist, Map.add poolId prunedForPool accMap)
            ) ([], runningHistory)

        let setHistory = 
            sorterPoolSetHistory.create(
                sorterPoolSetId = poolSet.SorterPoolSetId,
                saveGeneration = currentGen,
                poolHistories = (poolHistories |> List.rev)
            )

        setHistory, updatedRunningMap

    let toDataTableRecords (history: sorterPoolSetHistory) : dataTableRecord seq =
        history.PoolHistories 
        |> Seq.collect SorterPoolHistory.toDataTableRecords