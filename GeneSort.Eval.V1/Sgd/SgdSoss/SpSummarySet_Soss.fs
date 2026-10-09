namespace GeneSort.Eval.V1.Sgd.Soss

open System
open FSharp.UMX
open GeneSort.Eval.V1
open GeneSort.Core

type spSummarySet_Soss =
    private {
        _sorterPoolSetSummarySetId: Guid<sorterPoolSetSummarySetId>
        _lastGeneration: int<generationNumber>
        _sorterPoolSetSummaries: sorterPoolSetSummary_Soss array
    }

    member this.SorterPoolSetSummarySetId with get() = this._sorterPoolSetSummarySetId
    member this.LastGeneration with get() = this._lastGeneration
    member this.SorterPoolSetSummaries with get() = this._sorterPoolSetSummaries

    static member create 
                    (setId: Guid<sorterPoolSetSummarySetId>) 
                    (lastGeneration: int<generationNumber>) 
                    (summaries: sorterPoolSetSummary_Soss array) =
        {
            _sorterPoolSetSummarySetId = setId
            _lastGeneration = lastGeneration
            _sorterPoolSetSummaries = summaries
        }


module SpSummarySet_Soss =

    /// Constructs a summary set from an explicit ID and an array of sorterPoolSetSummary instances
    let create (setId: Guid<sorterPoolSetSummarySetId>) (summaries: sorterPoolSetSummary_Soss array) : spSummarySet_Soss =
        if Array.isEmpty summaries then
            spSummarySet_Soss.create setId (0 |> UMX.tag) [||]
        else
            let maxGen = SorterPoolSetSummary_Soss.getMaxGeneration summaries
            spSummarySet_Soss.create setId maxGen summaries

    /// Constructs a summary set with a auto-generated Guid from an array of sorterPoolSetSummary instances
    let createNew (summaries: sorterPoolSetSummary_Soss array) : spSummarySet_Soss =
        create (Guid.NewGuid() |> UMX.tag<sorterPoolSetSummarySetId>) summaries

    /// Constructs a summary set directly from an array of heavy sorterPoolSet models using a provided ID
    let fromPoolSets (setId: Guid<sorterPoolSetSummarySetId>) (poolSets: sorterPoolSet_Soss array) : spSummarySet_Soss =
        poolSets
        |> Array.map SorterPoolSetSummary_Soss.fromPoolSet
        |> create setId

    /// Constructs a summary set directly from an array of heavy sorterPoolSet models using an auto-generated Guid
    let fromPoolSetsNew (poolSets: sorterPoolSet_Soss array) : spSummarySet_Soss =
        fromPoolSets (Guid.NewGuid() |> UMX.tag<sorterPoolSetSummarySetId>) poolSets

    /// Flattens the entire collection of pool set summaries into a single array of dataTableRecords,
    /// appending the root SorterPoolSetSummarySetId onto each record.
    let toDataTableRecords (prefix: string) (summarySet: spSummarySet_Soss) : dataTableRecord seq =
        let rootDtr =
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData (sprintf "%sSorterPoolSetSummarySetId" prefix) (string (%summarySet.SorterPoolSetSummarySetId))
        let childRecs =
            summarySet.SorterPoolSetSummaries
            |> Seq.collect (SorterPoolSetSummary_Soss.toDataTableRecords prefix)
        rootDtr |> GeneSort.Core.dataTableRecord.combineWithMany childRecs

