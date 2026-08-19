namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Core
open GeneSort.Eval.V1

type sorterPoolSetHistory = {
    SorterPoolSetId: Guid<sorterPoolSetId>
    SaveGeneration: int<generationNumber>
    PoolHistories: sorterPoolHistory list
}

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

        let setHistory = {
            SorterPoolSetId = poolSet.SorterPoolSetId
            SaveGeneration = currentGen
            PoolHistories = poolHistories |> List.rev
        }

        setHistory, updatedRunningMap

    let toDataTableRecords (history: sorterPoolSetHistory) : dataTableRecord list =
        history.PoolHistories 
        |> List.collect SorterPoolHistory.toDataTableRecords