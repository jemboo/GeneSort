namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Core
open GeneSort.Eval.V1

type sorterPoolHistory = {
    SorterPoolId: Guid<sorterPoolId>
    SaveGeneration: int<generationNumber>
    MemberHistories: sorterPoolMemberHistory list
}

module SorterPoolHistory =

    /// Extracts a pool history snapshot from a live sorterPool
    let fromPool 
            (currentGen: int<generationNumber>) 
            (pool: sorterPool) : sorterPoolHistory =
        {
            SorterPoolId = pool.SorterPoolId
            SaveGeneration = currentGen
            MemberHistories = 
                pool.SorterPoolMembers 
                |> Seq.map (SorterPoolMemberHistory.fromPoolMember pool.SorterPoolId currentGen)
                |> Seq.toList
        }

    /// Flatten pool member histories into dataTableRecords for tabular saving
    let toDataTableRecords (history: sorterPoolHistory) : dataTableRecord list =
        history.MemberHistories 
        |> List.map SorterPoolMemberHistory.toDataTableRecord