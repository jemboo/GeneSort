namespace GeneSort.Eval.V1.Sgd

open GeneSort.Core

/// Holds the combined results of the historical optimization run using light snapshot telemetry
type sorterRunResult = 
    private {
        _intermediateHistory: sorterPoolSetSummary array
        _finalPoolSet: sorterPoolSet
    }
    member this.FinalPoolSet with get() = this._finalPoolSet
    member this.IntermediateHistory with get() = this._intermediateHistory
    static member create 
                    (finalPoolSet:sorterPoolSet) 
                    (intermediateHistory:sorterPoolSetSummary []) =
            {
                _intermediateHistory = intermediateHistory
                _finalPoolSet = finalPoolSet
            }


module SorterRunResult =

    /// Extracts dataTableRecords out of the run result's intermediate summary history
    let toDataTableRecordsIntermediateHistory (prefix: string) (srRes: sorterRunResult) : dataTableRecord array =
        srRes.IntermediateHistory
        |> Array.collect (fun poolSetSummary ->
            poolSetSummary
            |> SorterPoolSetSummary.toDataTableRecords prefix
        )


    /// Extracts dataTableRecords out of the run result's FinalPoolSet
    let toDataTableRecordsSnapshot (prefix: string) (srRes: sorterRunResult) : dataTableRecord array =
        let yab = SorterPoolSetDescription.fromPoolSet srRes.FinalPoolSet
        SorterPoolSetDescription.toDataTableRecords "" yab