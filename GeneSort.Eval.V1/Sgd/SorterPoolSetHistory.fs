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

    /// Extracts a pool set history snapshot from a live sorterPoolSet
    let fromPoolSet 
            (currentGen: int<generationNumber>) 
            (poolSet: sorterPoolSet) : sorterPoolSetHistory =
        {
            SorterPoolSetId = poolSet.SorterPoolSetId
            SaveGeneration = currentGen
            PoolHistories = 
                poolSet.SorterPools |> Map.toSeq |> Seq.map(snd)
                |> Seq.map (SorterPoolHistory.fromPool currentGen)
                |> Seq.toList
        }

    /// Flatten all contained member histories across all pools into a single record list
    let toDataTableRecords (history: sorterPoolSetHistory) : dataTableRecord list =
        history.PoolHistories 
        |> List.collect SorterPoolHistory.toDataTableRecords