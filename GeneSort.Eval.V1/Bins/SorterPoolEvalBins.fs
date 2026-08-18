namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Eval.V1.Bins
open GeneSort.Eval.V1

[<Measure>] type sorterPoolEvalBinsId

type sorterPoolEvalBins =
    private {
        _sorterPoolEvalBinsId: Guid<sorterPoolEvalBinsId>
        _sorterPoolId: Guid<sorterPoolId>
        _sorterEvalBins: Map<sorterEvalKey, sorterEvalBin>
    }
    with
    /// Creates a bin set from a sorterPool by extracting evaluated members
    static member create (id: Guid<sorterPoolEvalBinsId>) (pool: sorterPool) =
        let validEvals = 
            pool.SorterPoolMembers
            |> Seq.choose (fun memberObj -> memberObj.SorterEval)

        let bins = 
            validEvals
            |> Seq.groupBy SorterEvalKey.fromSorterEval
            |> Seq.map (fun (key, evals) -> 
                let bin = sorterEvalBin.createWithSorterEvals evals key
                (key, bin))
            |> Map.ofSeq

        {
            _sorterPoolEvalBinsId = id
            _sorterPoolId = pool.SorterPoolId
            _sorterEvalBins = bins
        }

    /// Explicit reconstructor for deserialization or manual instantiation
    static member recreate (id: Guid<sorterPoolEvalBinsId>) 
                            (sorterPoolId: Guid<sorterPoolId>) 
                            (bins: Map<sorterEvalKey, sorterEvalBin>) =
        {
            _sorterPoolEvalBinsId = id
            _sorterPoolId = sorterPoolId
            _sorterEvalBins = bins
        }

    member this.SorterPoolEvalBinsId with get() = this._sorterPoolEvalBinsId
    member this.SorterPoolId with get() = this._sorterPoolId
    member this.Bins with get() = this._sorterEvalBins


module SorterPoolEvalBins = 

    /// Returns one dataTableRecord for each bin member
    let makeDataTableRecords (source: sorterPoolEvalBins) : GeneSort.Core.dataTableRecord seq =
        source.Bins
        |> Seq.map (fun kvp -> SorterEvalBin.toDataTableRecord kvp.Value)