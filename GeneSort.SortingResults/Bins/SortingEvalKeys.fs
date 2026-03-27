namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting
open System.Collections.Generic

[<Struct; StructuralEquality; NoComparison>]
type sortingEvalKeys =
    private {
        sortingId:        Guid<sortingId>
        sorterEvalKeyMap: Dictionary<modelTag, sorterEvalKey>
    }

    static member create (sortingId: Guid<sortingId>) =
        {
            sortingId        = sortingId
            sorterEvalKeyMap = Dictionary<modelTag, sorterEvalKey>()
        }

    member this.SortingId        with get() = this.sortingId
    member this.SorterEvalKeyMap with get() = this.sorterEvalKeyMap :> IReadOnlyDictionary<modelTag, sorterEvalKey>

    member this.TryGetKey (tag: modelTag) : sorterEvalKey option =
        match this.sorterEvalKeyMap.TryGetValue(tag) with
        | true, key -> Some key
        | false, _  -> None

    member internal this.SetKey (tag: modelTag) (key: sorterEvalKey) =
        this.sorterEvalKeyMap.[tag] <- key


module SortingEvalKeys =

    let create (sortingId: Guid<sortingId>) : sortingEvalKeys =
        sortingEvalKeys.create sortingId

    //// Selects a representative sorterEvalKey per tag by applying orderFunc
    //// to each bin's layers, preferring isSorted=true bins, then taking the minimum.
    //let buildFromBinsMap
    //        (sortingId:        Guid<sortingId>)
    //        (orderFunc:        sorterEvalKey -> float)
    //        (binsMap:          Dictionary<modelTag, sorterEvalBins>)
    //        : sortingEvalKeys =
    //    let keys = sortingEvalKeys.create sortingId
    //    for kvp in binsMap do
    //        let layers = kvp.Value.Layers
    //        let best =
    //            layers
    //            |> Seq.filter  (fun lkvp -> lkvp.Key.IsSorted)
    //            |> Seq.sortBy  (fun lkvp -> orderFunc lkvp.Key)
    //            |> Seq.tryHead
    //            |> Option.orElseWith (fun () ->
    //                layers
    //                |> Seq.sortBy  (fun lkvp -> orderFunc lkvp.Key)
    //                |> Seq.tryHead)
    //        best |> Option.iter (fun lkvp -> keys.SetKey kvp.Key lkvp.Key)
    //    keys