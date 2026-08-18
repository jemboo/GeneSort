namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open System
open GeneSort.Eval.V1

[<Measure>] type sorterPoolEvalBinsSetId

type sorterPoolEvalBinsSet =
    private {
        _sorterPoolEvalBinsSetId: Guid<sorterPoolEvalBinsSetId>
        _sorterPoolSetId: Guid<sorterPoolSetId>
        _generationNumber: int<generationNumber>
        _sorterPoolEvalBinsMap: Map<Guid<sorterPoolEvalBinsId>, sorterPoolEvalBins>
    }
    with
    /// Creates an evaluated bin set collection directly from a sorterPoolSet
    static member create (id: Guid<sorterPoolEvalBinsSetId>) (poolSet: sorterPoolSet) =
        let poolBinsMap =
            poolSet.SorterPools
            |> Map.values
            |> Seq.map (fun pool ->
                let binId = Guid.NewGuid() |> UMX.tag<sorterPoolEvalBinsId>
                let poolBins = sorterPoolEvalBins.create binId pool
                (poolBins.SorterPoolEvalBinsId, poolBins))
            |> Map.ofSeq

        {
            _sorterPoolEvalBinsSetId = id
            _sorterPoolSetId = poolSet.SorterPoolSetId
            _generationNumber = poolSet.GenerationNumber
            _sorterPoolEvalBinsMap = poolBinsMap
        }

    /// Explicit reconstructor for deserialization or manual instantiation
    static member recreate (id: Guid<sorterPoolEvalBinsSetId>)
                            (sorterPoolSetId: Guid<sorterPoolSetId>)
                            (generationNumber: int<generationNumber>)
                            (evalBinsMap: Map<Guid<sorterPoolEvalBinsId>, sorterPoolEvalBins>) =
        {
            _sorterPoolEvalBinsSetId = id
            _sorterPoolSetId = sorterPoolSetId
            _generationNumber = generationNumber
            _sorterPoolEvalBinsMap = evalBinsMap
        }

    member this.SorterPoolEvalBinsSetId with get() = this._sorterPoolEvalBinsSetId
    member this.SorterPoolSetId with get() = this._sorterPoolSetId
    member this.GenerationNumber with get() = this._generationNumber
    member this.SorterPoolEvalBinsMap with get() = this._sorterPoolEvalBinsMap


module SorterPoolEvalBinsSet =

    /// Flattens all bins across all sorterPoolEvalBins into a sequence of dataTableRecord
    let makeDataTableRecords (source: sorterPoolEvalBinsSet) : GeneSort.Core.dataTableRecord seq =
        source.SorterPoolEvalBinsMap
        |> Map.values
        |> Seq.collect SorterPoolEvalBins.makeDataTableRecords