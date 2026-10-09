namespace GeneSort.Eval.V1.Sgd.Standard

open FSharp.UMX
open GeneSort.Core
open GeneSort.Eval.V1

type spsh_Standard = 
    private {
        sorterPoolSetId: Guid<sorterPoolSetId>
        saveGeneration: int<generationNumber>
        poolHistories: spHistory_Standard list
    }

    static member create
            (sorterPoolSetId: Guid<sorterPoolSetId>,
             saveGeneration: int<generationNumber>,
             poolHistories: spHistory_Standard list) : spsh_Standard =
        {
            sorterPoolSetId = sorterPoolSetId
            saveGeneration = saveGeneration
            poolHistories = poolHistories
        }

    member this.SorterPoolSetId with get() = this.sorterPoolSetId
    member this.SaveGeneration with get() = this.saveGeneration
    member this.PoolHistories with get() = this.poolHistories


module Spsh_Standard =

    let pruneAndCreateFromPoolSet 
            (currentGen: int<generationNumber>) 
            (poolSet: sorterPoolSet_Standard)
            (runningHistory: mhMap_Standard)
            : spsh_Standard * mhMap_Standard =

        let poolHistories, updatedMap =
            poolSet.SorterPools
            |> Map.toList
            |> List.fold (fun (accHist, accMap) (poolId, pool) ->
                let runningForPool = Map.tryFind poolId accMap |> Option.defaultValue Map.empty
                let poolHist, prunedForPool = SpHistory_Standard.pruneAndCreateForPool currentGen pool runningForPool
                (poolHist :: accHist, Map.add poolId prunedForPool accMap)
            ) ([], MhMap_Standard.toMap runningHistory)

        let setHistory = 
            spsh_Standard.create(
                sorterPoolSetId = poolSet.SorterPoolSetId,
                saveGeneration = currentGen,
                poolHistories = (poolHistories |> List.rev)
            )

        setHistory, MhMap_Standard.create updatedMap

    let toDataTableRecords (history: spsh_Standard) : dataTableRecord seq =
        history.PoolHistories 
        |> Seq.collect SpHistory_Standard.toDataTableRecords