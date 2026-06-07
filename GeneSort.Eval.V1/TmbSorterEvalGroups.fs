
namespace GeneSort.Eval.V1

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1

type groupSelectionType = 
    | Tmb
    | Profile
    | TopN


module GroupSelectionType =
    let toString = function
        | groupSelectionType.Tmb -> "Tmb"
        | Profile -> "Profile"
        | TopN -> "TopN"

    let fromString = function
        | "Tmb" -> groupSelectionType.Tmb
        | "Profile" -> Profile
        | "TopN" -> TopN
        | _ -> failwith "Invalid sorterEvalSelectionType string"


type evalLabel = 
    | Tmb of tmbGroup
    | Index of int

/// Captures the explicit strategy/provenance used to generate a selection subset
type selectionStrategy =
    | Tmb
    | TopN
    | EvenSampleByRankedValue
    | EvenSampleByRankedIndex

module SelectionStrategy =
    let toString = function
        | Tmb -> "Tmb"
        | TopN -> "TopN"
        | EvenSampleByRankedValue -> "EvenSampleByRankedValue"
        | EvenSampleByRankedIndex -> "EvenSampleByRankedIndex"


/// Single unified domain structure holding your labeled selections.
/// This cleanly encapsulates TopN, Profile, and TMB while maintaining strategy data.
type labeledSorterEvals = 
    private {
        strategy: selectionStrategy
        items: (evalLabel * sorterEval) array
    }

    static member Empty = { strategy = Tmb; items = [||] }

    static member create (strategy: selectionStrategy) (items: (evalLabel * sorterEval) array) = 
        { strategy = strategy; items = items }

    member this.Strategy = this.strategy
    member this.Items = this.items

    /// Standardized mapping function dealing strictly with Domain items and Labels
    member this.ToMap() : Map<Guid<sorterId>, (evalLabel * sorterEval)> =
        this.items 
        |> Array.map (fun (label, se) -> (se |> SorterEval.getSorterId, (label, se)))
        |> Map.ofArray

    /// Universal model set generator pulled up into the shared container to prevent duplication
    member this.MakeSorterModelSet 
                    (sorterModelSetId: Guid<sorterModelSetId>) 
                    (sorterModelGen: sorterModelGen) : sorterModelSet =
            let sorterModelIds = 
                this.items 
                |> Array.map(fun (_, se) -> 
                    se |> SorterEval.getSorterId |> UMX.untag |> UMX.tag<sorterModelId>)

            let sorterModels = SorterModelGen.makeSorterModelsFromIds sorterModelIds sorterModelGen
            sorterModelSet.create sorterModelSetId sorterModels



module LabeledSorterEvals =

    /// Helper to filter input collections for sorted candidates and remove phenotype duplicates
    let private prepareDistinctSorted (items: sorterEval seq) =
        items
        |> Seq.filter SorterEval.getIsSorted
        |> Seq.distinctBy SorterEval.getSequenceHash
        |> Seq.toArray

    /// Generates Tmb groups mapped directly to evalLabel variants
    let makeTmbSelections (ranker: sorterEval -> float) (groupSize: int) (items: sorterEval array) : labeledSorterEvals =
        let distinctItems = items |> Array.distinctBy SorterEval.getSequenceHash
        let targetSize = Math.Min(groupSize, distinctItems.Length / 3)
        
        if targetSize <= 0 then 
            labeledSorterEvals.create selectionStrategy.Tmb Array.empty
        else
            let sortedItems = distinctItems |> Array.sortByDescending ranker
            
            let topGroup = sortedItems.[0 .. targetSize - 1] |> Array.map (fun se -> evalLabel.Tmb tmbGroup.Top, se)
            
            let midStart = distinctItems.Length / 2 - (targetSize / 2)
            let midGroup = sortedItems.[midStart .. midStart + targetSize - 1] |> Array.map (fun se -> evalLabel.Tmb tmbGroup.Middle, se)
            
            let botGroup = sortedItems.[distinctItems.Length - targetSize .. distinctItems.Length - 1] |> Array.map (fun se -> evalLabel.Tmb tmbGroup.Bottom, se)
            
            let combined = Array.concat [topGroup; midGroup; botGroup]
            labeledSorterEvals.create selectionStrategy.Tmb combined

    /// Generates TopN groups mapped directly to evalLabel Index variants
    let getTopN (ranker: sorterEval -> float) (n: int) (items: sorterEval array) : labeledSorterEvals =
        let distinctSorted = prepareDistinctSorted items
        let topN = distinctSorted |> Array.sortByDescending ranker |> Array.truncate n
        
        let labeledItems = topN |> Array.mapi (fun idx se -> evalLabel.Index idx, se)
        labeledSorterEvals.create selectionStrategy.TopN labeledItems

    /// Samples an array of unique evaluations spaced evenly across the ranked ARRAY INDEX layout.
    let evenSampleByRankedIndex 
                (ranker: sorterEval -> float)
                (count: int<sorterCount>) 
                (items: seq<sorterEval>) : labeledSorterEvals =
        
        let sampleCount = %count
        if sampleCount <= 0 then 
            failwithf "Requested sample count must be greater than zero, but was %d" sampleCount

        let cleanItems = prepareDistinctSorted items
        let availableCount = cleanItems.Length

        if availableCount < sampleCount then
            failwithf "Cannot sample %d items; only %d unique sorted options are available." sampleCount availableCount

        let sortedItems = cleanItems |> Array.sortByDescending ranker

        let result = 
            if sampleCount = 1 then
                [| sortedItems.[0] |]
            else
                let res = Array.zeroCreate sampleCount
                res.[0] <- sortedItems.[0]
                let step = float (availableCount - 1) / float (sampleCount - 1)
                for i in 1 .. (sampleCount - 1) do
                    let targetIndex = int (round (float i * step))
                    res.[i] <- sortedItems.[targetIndex]
                res

        let labeledItems = result |> Array.mapi (fun idx se -> evalLabel.Index idx, se)
        labeledSorterEvals.create selectionStrategy.EvenSampleByRankedIndex labeledItems

    /// Samples an array of unique evaluations spaced evenly along the actual RANKED VALUE score axis.
    let evenSampleByRankedValue 
                (ranker: sorterEval -> float)
                (count: int<sorterCount>) 
                (items: seq<sorterEval>) : labeledSorterEvals =
        
        let sampleCount = %count
        if sampleCount <= 0 then 
            failwithf "Requested sample count must be greater than zero, but was %d" sampleCount

        let cleanItems = prepareDistinctSorted items
        let availableCount = cleanItems.Length

        if availableCount < sampleCount then
            failwithf "Cannot sample %d items; only %d unique sorted options are available." sampleCount availableCount

        let sortedItems = cleanItems |> Array.sortByDescending ranker

        let result = 
            if sampleCount = 1 then
                [| sortedItems.[0] |]
            else
                let res = Array.zeroCreate sampleCount
                res.[0] <- sortedItems.[0]
                res.[sampleCount - 1] <- sortedItems.[availableCount - 1]

                let maxVal = ranker sortedItems.[0]
                let minVal = ranker sortedItems.[availableCount - 1]
                let valRange = maxVal - minVal

                if valRange = 0.0 then
                    let step = float (availableCount - 1) / float (sampleCount - 1)
                    for i in 1 .. (sampleCount - 1) do
                        let targetIndex = int (round (float i * step))
                        res.[i] <- sortedItems.[targetIndex]
                else
                    let targetStep = valRange / float (sampleCount - 1)
                    for i in 1 .. (sampleCount - 2) do
                        let targetValue = maxVal - (float i * targetStep)
                        let closestMatch = 
                            sortedItems 
                            |> Array.minBy (fun se -> Math.Abs(ranker se - targetValue))
                        res.[i] <- closestMatch
                res

        let labeledItems = result |> Array.mapi (fun idx se -> evalLabel.Index idx, se)
        labeledSorterEvals.create selectionStrategy.EvenSampleByRankedValue labeledItems



/// Separated reporting logic. Translates pure domain objects into output string layouts.
module EvalReporting =

    let evalLabelToDtr (label: evalLabel) : dataTableRecord =
        match label with
        | evalLabel.Tmb groupTag -> 
            groupTag |> RankedGroup.toDataTableRecord
        | evalLabel.Index idx -> 
            dataTableRecord.createEmpty() 
            |> dataTableRecord.addData "SorterEvalIndex" (string idx)

    // for reports having multiple dataTableRecords per eval
    let toManyDataTableRecords
                (leadCols: dataTableRecord) 
                (recordMaker: sorterEval -> dataTableRecord [])
                (selection: labeledSorterEvals) : dataTableRecord array =

        // Append the generation context to data table records
        let strategyDtr = 
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData "SelectionStrategy" (SelectionStrategy.toString selection.Strategy)
            |> dataTableRecord.combine leadCols

        selection.Items
        |> Array.collect (fun (label, se) -> 
            let labelDtr = evalLabelToDtr label |> dataTableRecord.combine strategyDtr
            se |> recordMaker
               |> Array.map (fun customDtr -> customDtr |> dataTableRecord.combine labelDtr)
        )


    let toDataTableRecords 
        (leadCols: dataTableRecord) 
        (labelPfx: string)
        (selection: labeledSorterEvals) : Map<Guid<sorterId>,dataTableRecord> =

        // Append the generation context to data table records
        let strategyDtr = 
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData "SelectionStrategy" (SelectionStrategy.toString selection.Strategy)
            |> dataTableRecord.combine leadCols

        selection.Items
        |> Array.map (
            fun (label, se) -> 
                let labelDtr = evalLabelToDtr label |> dataTableRecord.combine strategyDtr
                (
                    se |> SorterEval.getSorterId,
                    se |> SorterEval.toDataTableRecordWithPrefix labelPfx
                       |> dataTableRecord.combine labelDtr
                )
            ) |> Map.ofArray