module GeneSort.Dispatch.Test.V2.PoolPruningTests

open System
open FSharp.UMX
open Xunit
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple
open GeneSort.Model.Sorting.V1.Simple.Ce
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd
open GeneSort.Eval.V1.Sgd.Standard
open GeneSort.Eval.V1.Sgd.Soss

let private prune soss debug filterUnsorted (capacity: int) (samples: (int * int) option array) =
    let model = msce.create (UMX.tag (Guid.NewGuid())) 32<sortingWidth> [|1|] |> simpleSorterModel.Msce |> sorterModel.Simple
    let measure = ceLengthMeasure.create filterUnsorted false |> sorterEvalMeasure.CeLength
    let evaluations = samples |> Array.mapi (fun index sample ->
        sample |> Option.map (fun (unsorted, length) ->
            sorterEvalV1.create (UMX.tag (Guid.NewGuid())) SortableTestsSubsetId.Default 32<sortingWidth>
                (UMX.tag unsorted) (UMX.tag index) (UMX.tag length) 1<stageLength> (UMX.tag length)
                (UMX.tag false) (UMX.tag 0) (UMX.tag 0) |> sorterEval.V1))
    let chosen =
        if soss then
            let members = evaluations |> Array.map (fun evaluation ->
                let cache = evaluation |> Option.map (fun e -> Map.ofList [SortableTestsSubsetId.Default, e]) |> Option.defaultValue Map.empty
                spMember_Soss.create (UMX.tag (Guid.NewGuid())) model (UMX.tag 0) (UMX.tag 0) None cache 0<generationNumber>)
            let pool = sorterPool_Soss.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "test") (D1 0) members 100<ceLength> (UMX.tag 0)
            let prune = if debug then SorterPool_Soss.pruneSorterPoolDebug else SorterPool_Soss.pruneSorterPool
            (prune pool measure (UMX.tag false) (UMX.tag false) (UMX.tag capacity)).SorterPoolMembers
            |> Seq.map (fun m -> m.SorterEvalForSelection) |> Seq.toArray
        else
            let members = evaluations |> Array.map (fun evaluation ->
                spMember_Standard.create (UMX.tag (Guid.NewGuid())) model (UMX.tag 0) (UMX.tag 0) None evaluation 0<generationNumber>)
            let pool = sorterPool_Standard.create (UMX.tag (Guid.NewGuid())) None (UMX.tag "test") (D1 0) members 100<ceLength> (UMX.tag 0)
            let prune = if debug then SorterPool_Standard.pruneSorterPoolDebug else SorterPool_Standard.pruneSorterPool
            (prune pool measure (UMX.tag false) (UMX.tag false) (UMX.tag capacity)).SorterPoolMembers
            |> Seq.map (fun m -> m.SorterEval) |> Seq.toArray
    chosen |> Array.map (Option.get >> SorterEval.getSequenceHash >> int) |> Array.sort

[<Theory>]
[<InlineData(false, false)>]
[<InlineData(false, true)>]
[<InlineData(true, false)>]
[<InlineData(true, true)>]
let ``Strict pruning recovers by correctness before size when every candidate fails`` soss debug =
    let selected = prune soss debug true 2 [|Some (5, 1); Some (1, 50); Some (1, 40); None|]
    Assert.True((selected = [|1; 2|]))

[<Theory>]
[<InlineData(false, false)>]
[<InlineData(false, true)>]
[<InlineData(true, false)>]
[<InlineData(true, true)>]
let ``Recovery stops as soon as a candidate passes strict filtering`` soss debug =
    let selected = prune soss debug true 3 [|Some (0, 50); Some (1, 1); Some (0, 101); None|]
    Assert.True((selected = [|0|]))

[<Theory>]
[<InlineData(false, false)>]
[<InlineData(false, true)>]
[<InlineData(true, false)>]
[<InlineData(true, true)>]
let ``Recovery does not invent candidates or retain unevaluated members`` soss debug =
    Assert.Empty(prune soss debug true 2 [|None|])
    Assert.Empty(prune soss debug true 2 [||])
    Assert.Empty(prune soss debug true 0 [|Some (1, 1)|])

[<Theory>]
[<InlineData(false, false)>]
[<InlineData(false, true)>]
[<InlineData(true, false)>]
[<InlineData(true, true)>]
let ``Disabling strict filtering preserves the configured score ranking`` soss debug =
    Assert.True(prune soss debug false 1 [|Some (5, 1); Some (1, 50)|] = [|0|])
