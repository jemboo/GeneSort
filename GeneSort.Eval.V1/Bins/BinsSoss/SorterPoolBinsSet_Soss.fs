namespace GeneSort.Eval.V1.Sgd.Bins.Soss

open FSharp.UMX
open System
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Eval.V1.Sgd.Soss


type sorterPoolBinsSet_Soss =
    private {
        _sorterPoolEvalBinsSetId: Guid<sorterPoolBinsSetId>
        _sorterPoolSetId: Guid<sorterPoolSetId>
        _generationNumber: int<generationNumber>
        _sorterPoolEvalBinsMap: Map<Guid<sorterPoolBinsId>, sorterPoolBins_Soss>
    }
    with
    /// Creates an evaluated bin set collection directly from a sorterPoolSet
    static member create (id: Guid<sorterPoolBinsSetId>) (poolSet: sorterPoolSet_Soss) =
        let poolBinsMap =
            poolSet.SorterPools
            |> Map.values
            |> Seq.map (fun pool ->
                let binId = Guid.NewGuid() |> UMX.tag<sorterPoolBinsId>
                let poolBins = sorterPoolBins_Soss.create binId pool
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
                            (evalBinsMap: Map<Guid<sorterPoolBinsId>, sorterPoolBins_Soss>) =
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


module SorterPoolEvalBinsSet_Soss =

    /// Flattens all bins across all sorterPoolEvalBins into a sequence of dataTableRecord
    let makeDataTableRecords (source: sorterPoolBinsSet_Soss) : GeneSort.Core.dataTableRecord seq =
        let setRec =
            GeneSort.Core.dataTableRecord.createEmpty()
            |> GeneSort.Core.dataTableRecord.addKeyAndData "Generation" (source.GenerationNumber |> UMX.untag |> string)
        let childRecs =
            source.SorterPoolEvalBinsMap
            |> Map.values
            |> Seq.collect SorterPoolEvalBins_Soss.makeDataTableRecords

        setRec |> GeneSort.Core.dataTableRecord.combineWithMany childRecs