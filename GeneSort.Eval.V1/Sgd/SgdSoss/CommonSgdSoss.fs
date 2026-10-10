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
