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

    /// Adjusts at generation zero and every configured interval thereafter.
    let shouldAdjustCeLengths = GenerationSchedule.shouldAdjustCeLengths

    /// Selects each configured partition and then Merge for the configured number of generations apiece.
    let subsetIdForGeneration
            (generationsPerSubset: int<generationsPerSubset>)
            (generation: int<generationNumber>)
            (partitionIds: string<sortableTestsPartitionId> array) =
        if Array.isEmpty partitionIds then
            invalidArg "partitionIds" "The selection cycle requires at least one partition."
        if %generation < 0 then
            invalidArg "generation" "Generation must not be negative."
        if %generationsPerSubset <= 0 then
            invalidArg "generationsPerSubset" "GenerationsPerSubset must be greater than zero."
        let cycleIndex = (%generation / %generationsPerSubset) % (partitionIds.Length + 1)
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
