namespace GeneSort.Eval.V1.Sgd.Bins.Standard

open FSharp.UMX
open GeneSort.Eval.V1.Bins
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd.Standard


type sorterPoolBins_Standard =
    private {
        _sorterPoolEvalBinsId: Guid<sorterPoolBinsId>
        _sorterPoolId: Guid<sorterPoolId>
        _sorterEvalBins: Map<sorterEvalKey, sorterEvalBin_Standard>
    }
    with
    /// Creates a bin set from a sorterPool by extracting evaluated members
    static member create (id: Guid<sorterPoolBinsId>) (pool: sorterPool_Standard) =
        let validEvals = 
            pool.SorterPoolMembers
            |> Seq.choose (fun memberObj -> memberObj.SorterEval)

        let bins = 
            validEvals
            |> Seq.groupBy SorterEvalKey.fromSorterEval
            |> Seq.map (fun (key, evals) -> 
                let bin = sorterEvalBin_Standard.createWithSorterEvals evals key
                (key, bin))
            |> Map.ofSeq

        {
            _sorterPoolEvalBinsId = id
            _sorterPoolId = pool.SorterPoolId
            _sorterEvalBins = bins
        }

    /// Explicit reconstructor for deserialization or manual instantiation
    static member recreate (id: Guid<sorterPoolBinsId>) 
                            (sorterPoolId: Guid<sorterPoolId>) 
                            (bins: Map<sorterEvalKey, sorterEvalBin_Standard>) =
        {
            _sorterPoolEvalBinsId = id
            _sorterPoolId = sorterPoolId
            _sorterEvalBins = bins
        }

    member this.SorterPoolEvalBinsId with get() = this._sorterPoolEvalBinsId
    member this.SorterPoolId with get() = this._sorterPoolId
    member this.Bins with get() = this._sorterEvalBins



module SorterPoolEvalBins_Standard = 

    ///// Returns one dataTableRecord for each bin member
    let makeDataTableRecords (source: sorterPoolBins_Standard) : GeneSort.Core.dataTableRecord seq =
        let setRec =
            GeneSort.Core.dataTableRecord.createEmpty()
            |> GeneSort.Core.dataTableRecord.addKeyAndData "SorterPoolId" (source.SorterPoolId |> UMX.untag |> string)
        let childRecs =
            source.Bins
            |> Seq.map (fun kvp -> SorterEvalBin_Standard.toDataTableRecord kvp.Value)

        setRec |> GeneSort.Core.dataTableRecord.combineWithMany childRecs