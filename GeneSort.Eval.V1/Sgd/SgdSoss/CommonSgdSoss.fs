namespace GeneSort.Eval.V1.Sgd.Soss

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd


module CommonSgdSoss =

    let mergedSubsetId = "Merge" |> UMX.tag<sortableTestsSubsetId>

    /// Selects each of four partitions and then Merge for 100 generations apiece.
    let subsetIdForGeneration
            (generation: int<generationNumber>)
            (partitionIds: string<sortableTestsPartitionId> array) =
        if partitionIds.Length <> 4 then
            invalidArg "partitionIds" "The 500-generation selection cycle requires four partitions."
        if %generation < 0 then
            invalidArg "generation" "Generation must not be negative."
        let cycleIndex = (%generation % 500) / 100
        if cycleIndex < partitionIds.Length then
            UMX.cast<sortableTestsPartitionId, sortableTestsSubsetId> partitionIds.[cycleIndex]
        else
            mergedSubsetId

    /// Combines explicitly evaluated partitions without evaluating their merged tests again.
    let mergePartitionEvals
            (partitionEvals: Map<string<sortableTestsPartitionId>, Map<Guid<sorterPoolMemberId>, sorterEval>>) =
        let evaluations = partitionEvals |> Map.values |> Seq.toArray
        if Array.isEmpty evaluations then
            invalidArg "partitionEvals" "At least one partition evaluation is required."
        evaluations.[0]
        |> Map.map (fun memberId firstEval ->
            evaluations
            |> Array.skip 1
            |> Array.fold (fun merged partition -> SorterEval.merge merged partition.[memberId]) firstEval
            |> SorterEval.withSortableTestsSubsetId mergedSubsetId)
