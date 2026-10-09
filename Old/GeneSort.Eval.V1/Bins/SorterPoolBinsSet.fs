namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open System
open GeneSort.Eval.V1

[<Measure>] type sorterPoolBinsSetId

type sorterPoolBinsSet =
    private {
        _sorterPoolEvalBinsSetId: Guid<sorterPoolBinsSetId>
        _sorterPoolSetId: Guid<sorterPoolSetId>
        _generationNumber: int<generationNumber>
        _sorterPoolEvalBinsMap: Map<Guid<sorterPoolBinsId>, sorterPoolBins>
    }
    with
    /// Creates an evaluated bin set collection directly from a sorterPoolSet
    static member create (id: Guid<sorterPoolBinsSetId>) (poolSet: sorterPoolSet) =
        let poolBinsMap =
            poolSet.SorterPools
            |> Map.values
            |> Seq.map (fun pool ->
                let binId = Guid.NewGuid() |> UMX.tag<sorterPoolBinsId>
                let poolBins = sorterPoolBins.create binId pool
                (poolBins.SorterPoolEvalBinsId, poolBins))
            |> Map.ofSeq

        {
            _sorterPoolEvalBinsSetId = id
            _sorterPoolSetId = poolSet.SorterPoolSetId
            _generationNumber = poolSet.GenerationNumber
            _sorterPoolEvalBinsMap = poolBinsMap
        }

    /// Explicit reconstructor for deserialization or manual instantiation
    static member recreate (id: Guid<sorterPoolBinsSetId>)
                            (sorterPoolSetId: Guid<sorterPoolSetId>)
                            (generationNumber: int<generationNumber>)
                            (evalBinsMap: Map<Guid<sorterPoolBinsId>, sorterPoolBins>) =
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
    let makeDataTableRecords (source: sorterPoolBinsSet) : GeneSort.Core.dataTableRecord seq =
        let setRec =
            GeneSort.Core.dataTableRecord.createEmpty()
            |> GeneSort.Core.dataTableRecord.addKeyAndData "Generation" (source.GenerationNumber |> UMX.untag |> string)
        let childRecs =
            source.SorterPoolEvalBinsMap
            |> Map.values
            |> Seq.collect SorterPoolEvalBins.makeDataTableRecords

        setRec |> GeneSort.Core.dataTableRecord.combineWithMany childRecs