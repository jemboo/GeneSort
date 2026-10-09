namespace GeneSort.Eval.V1.Sgd.Soss

open FSharp.UMX
open GeneSort.Core
open GeneSort.Eval.V1

type spsh_Soss = 
    private {
        sorterPoolSetId: Guid<sorterPoolSetId>
        saveGeneration: int<generationNumber>
        poolHistories: spHistory_Soss list
    }

    static member create
            (sorterPoolSetId: Guid<sorterPoolSetId>,
             saveGeneration: int<generationNumber>,
             poolHistories: spHistory_Soss list) : spsh_Soss =
        {
            sorterPoolSetId = sorterPoolSetId
            saveGeneration = saveGeneration
            poolHistories = poolHistories
        }

    member this.SorterPoolSetId with get() = this.sorterPoolSetId
    member this.SaveGeneration with get() = this.saveGeneration
    member this.PoolHistories with get() = this.poolHistories


module Spsh_Soss =

    let pruneAndCreateFromPoolSet 
            (currentGen: int<generationNumber>) 
            (poolSet: sorterPoolSet_Soss)
            (runningHistory: mhMap_Soss)
            : spsh_Soss * mhMap_Soss =

        let poolHistories, updatedMap =
            poolSet.SorterPools
            |> Map.toList
            |> List.fold (fun (accHist, accMap) (poolId, pool) ->
                let runningForPool = Map.tryFind poolId accMap |> Option.defaultValue Map.empty
                let poolHist, prunedForPool = SpHistory_Soss.pruneAndCreateForPool currentGen pool runningForPool
                (poolHist :: accHist, Map.add poolId prunedForPool accMap)
            ) ([], MhMap_Soss.toMap runningHistory)

        let setHistory = 
            spsh_Soss.create(
                sorterPoolSetId = poolSet.SorterPoolSetId,
                saveGeneration = currentGen,
                poolHistories = (poolHistories |> List.rev)
            )

        setHistory, MhMap_Soss.create updatedMap

    let toDataTableRecords (history: spsh_Soss) : dataTableRecord seq =
        history.PoolHistories 
        |> Seq.collect SpHistory_Soss.toDataTableRecords