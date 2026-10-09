namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Eval.V1

[<Measure>] type sorterPoolBinsSetSeriesId

type sorterPoolBinsSetSeries =
    private {
        _sorterPoolEvalBinsSetCollectionId: Guid<sorterPoolBinsSetSeriesId>
        _sorterPoolEvalBinsSets: Map<Guid<sorterPoolBinsSetId>, sorterPoolBinsSet>
    }
    with
    /// Creates an empty collection or populates it from an initial sequence of bin sets
    static member create 
            (id: Guid<sorterPoolBinsSetSeriesId>)
            (sets: seq<sorterPoolBinsSet>) =
        let setsMap =
            sets
            |> Seq.map (fun s -> s.SorterPoolEvalBinsSetId, s)
            |> Map.ofSeq
        {
            _sorterPoolEvalBinsSetCollectionId = id
            _sorterPoolEvalBinsSets = setsMap
        }

    /// Explicit reconstructor for deserialization or manual instantiation
    static member recreate (id: Guid<sorterPoolBinsSetSeriesId>)
                           (sets: Map<Guid<sorterPoolBinsSetId>, sorterPoolBinsSet>) =
        {
            _sorterPoolEvalBinsSetCollectionId = id
            _sorterPoolEvalBinsSets = sets
        }

    static member empty (id: Guid<sorterPoolBinsSetSeriesId>) =
        sorterPoolBinsSetSeries.create id Seq.empty

    member this.SorterPoolEvalBinsSetCollectionId with get() = this._sorterPoolEvalBinsSetCollectionId
    member this.SorterPoolEvalBinsSets with get() = this._sorterPoolEvalBinsSets
    member this.Count with get() = this._sorterPoolEvalBinsSets.Count
    member this.MaxGeneration with get() = this.SorterPoolEvalBinsSets.Values 
                                            |> Seq.map(fun sp -> %sp.GenerationNumber)
                                            |> Seq.max |> UMX.tag<generationNumber>


module SorterPoolEvalBinsSetCollection =

    /// Adds a sorterPoolEvalBinsSet to the collection
    let add (binSet: sorterPoolBinsSet) (collection: sorterPoolBinsSetSeries) : sorterPoolBinsSetSeries =
        let updatedMap = Map.add binSet.SorterPoolEvalBinsSetId binSet collection.SorterPoolEvalBinsSets
        sorterPoolBinsSetSeries.recreate 
                    collection.SorterPoolEvalBinsSetCollectionId
                    updatedMap

    /// Flattens all bins across all sets in the collection into a sequence of dataTableRecord
    let makeDataTableRecords (source: sorterPoolBinsSetSeries) : GeneSort.Core.dataTableRecord seq =
        source.SorterPoolEvalBinsSets
        |> Map.values
        |> Seq.collect SorterPoolEvalBinsSet.makeDataTableRecords