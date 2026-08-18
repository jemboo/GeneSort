namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX

[<Measure>] type sorterPoolEvalBinsSetCollectionId

type sorterPoolEvalBinsSetCollection =
    private {
        _sorterPoolEvalBinsSetCollectionId: Guid<sorterPoolEvalBinsSetCollectionId>
        _sorterPoolEvalBinsSets: Map<Guid<sorterPoolEvalBinsSetId>, sorterPoolEvalBinsSet>
    }
    with
    /// Creates an empty collection or populates it from an initial sequence of bin sets
    static member create (id: Guid<sorterPoolEvalBinsSetCollectionId>) (sets: seq<sorterPoolEvalBinsSet>) =
        let setsMap =
            sets
            |> Seq.map (fun s -> s.SorterPoolEvalBinsSetId, s)
            |> Map.ofSeq
        {
            _sorterPoolEvalBinsSetCollectionId = id
            _sorterPoolEvalBinsSets = setsMap
        }

    /// Explicit reconstructor for deserialization or manual instantiation
    static member recreate (id: Guid<sorterPoolEvalBinsSetCollectionId>)
                           (sets: Map<Guid<sorterPoolEvalBinsSetId>, sorterPoolEvalBinsSet>) =
        {
            _sorterPoolEvalBinsSetCollectionId = id
            _sorterPoolEvalBinsSets = sets
        }

    static member empty (id: Guid<sorterPoolEvalBinsSetCollectionId>) =
        sorterPoolEvalBinsSetCollection.create id Seq.empty

    member this.SorterPoolEvalBinsSetCollectionId with get() = this._sorterPoolEvalBinsSetCollectionId
    member this.SorterPoolEvalBinsSets with get() = this._sorterPoolEvalBinsSets
    member this.Count with get() = this._sorterPoolEvalBinsSets.Count


module SorterPoolEvalBinsSetCollection =

    /// Adds a sorterPoolEvalBinsSet to the collection
    let add (binSet: sorterPoolEvalBinsSet) (collection: sorterPoolEvalBinsSetCollection) : sorterPoolEvalBinsSetCollection =
        let updatedMap = Map.add binSet.SorterPoolEvalBinsSetId binSet collection.SorterPoolEvalBinsSets
        sorterPoolEvalBinsSetCollection.recreate collection.SorterPoolEvalBinsSetCollectionId updatedMap

    /// Flattens all bins across all sets in the collection into a sequence of dataTableRecord
    let makeDataTableRecords (source: sorterPoolEvalBinsSetCollection) : GeneSort.Core.dataTableRecord seq =
        source.SorterPoolEvalBinsSets
        |> Map.values
        |> Seq.collect SorterPoolEvalBinsSet.makeDataTableRecords