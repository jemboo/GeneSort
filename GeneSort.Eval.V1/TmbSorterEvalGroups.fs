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


type evalLabel = 
    | Tmb of tmbGroup
    | Index of int


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



type tmbSorterEvalGroups = 
    private {
        top: sorterEval array
        middle: sorterEval array
        bottom: sorterEval array
    }
    
    static member create (top: sorterEval array) (middle: sorterEval array) (bottom: sorterEval array) =
        { top = top; middle = middle; bottom = bottom }

    member this.Top = this.top
    member this.Middle = this.middle
    member this.Bottom = this.bottom

    /// Converts the three explicit ranked groups into an itemized loop context for reports
    member this.ToGroupedArrays() : array<tmbGroup * sorterEval array> =
        [|
            (tmbGroup.Top, this.top)
            (tmbGroup.Middle, this.middle)
            (tmbGroup.Bottom, this.bottom)
        |]

    member this.ToRankedSorterEvals() : array<tmbGroup * sorterEval> =
        let topRecords = this.top |> Array.map (fun se -> (Top, se))
        let midRecords = this.middle |> Array.map (fun se -> (Middle, se))
        let botRecords = this.bottom |> Array.map (fun se -> (Bottom, se))
        Array.concat [topRecords; midRecords; botRecords]


    member this.ToMap() : Map<Guid<sorterId>, (sorterEval*dataTableRecord)> =
      this.ToRankedSorterEvals() |> Array.map(fun (group, se) -> 
                (se |> SorterEval.getSorterId, (se, group |> RankedGroup.toDataTableRecord))) 
                |> Map.ofArray


    member this.makeSorterModelSet 
                    (sorterModelSetId: Guid<sorterModelSetId>) 
                    (sorterModelGen: sorterModelGen) : sorterModelSet =
            let sorterModelIds = 
                this.ToRankedSorterEvals() 
                |> Array.map(fun (group, se) -> 
                    se |> SorterEval.getSorterId |> UMX.untag |> UMX.tag<sorterModelId>)

            let sorterModels = SorterModelGen.makeSorterModelsFromIds sorterModelIds sorterModelGen
            sorterModelSet.create sorterModelSetId sorterModels




module TmbSorterEvalGroups =

    /// Builds a tmbSorterEvalGroups instance from an array of evaluations, 
    /// ensuring only sorterEvals with distinct sequenceHashes are ranked and grouped.
    let fromEvaluations 
            (ranker: sorterEval -> float) 
            (groupSize: int) 
            (items: sorterEval array) : tmbSorterEvalGroups =
            
        // 1. Filter out duplicates based on their structural phenotype sequence hashes
        let distinctItems = items |> Array.distinctBy SorterEval.getSequenceHash
            
        // 2. Base group metrics dynamically off the unique subset population count
        let targetSize = Math.Min(groupSize, distinctItems.Length / 3)
        
        if targetSize <= 0 then 
            tmbSorterEvalGroups.create Array.empty Array.empty Array.empty
        else
            // 3. Sort the unique items descending by fitness/rank
            let sortedItems = distinctItems |> Array.sortByDescending ranker
            
            // 4. Extract safe math partitions
            let topGroup = sortedItems.[0 .. targetSize - 1]
            
            let midStart = distinctItems.Length / 2 - (targetSize / 2)
            let midGroup = sortedItems.[midStart .. midStart + targetSize - 1]
            
            let botGroup = sortedItems.[distinctItems.Length - targetSize .. distinctItems.Length - 1]
            
            tmbSorterEvalGroups.create topGroup midGroup botGroup


    let toDataTableRecords 
                (leadCols: dataTableRecord) 
                (recordMaker: sorterEval -> dataTableRecord [])
                (groups: tmbSorterEvalGroups) : dataTableRecord array =

            groups.ToGroupedArrays()
            |> Array.collect (fun (groupTag, evals) -> 
                // 1. Build the shared contextual header for this group tier
                let groupHeaderDtr = 
                    groupTag 
                    |> RankedGroup.toDataTableRecord 
                    |> dataTableRecord.combine leadCols
            
                // 2. Map items via the injected handler function and append the headers
                evals
                |> Array.collect recordMaker
                |> Array.map (fun customDtr -> 
                    customDtr |> dataTableRecord.combine groupHeaderDtr
                )
            )



type indexedSorterEvals =
    private {
        sorterEvals: sorterEval array
    }
    static member create (sorterEvals: sorterEval array) =
        { sorterEvals = sorterEvals }

    member this.SorterEvals = this.sorterEvals

    /// Maps the internal array to a keyed dictionary using the Sorter ID
    member this.ToMap() : Map<Guid<sorterId>, (sorterEval * dataTableRecord)> =
        this.sorterEvals 
        |> Array.mapi (fun idx se ->
            let labelDtr = 
                dataTableRecord.createEmpty() 
                |> dataTableRecord.addData "SorterEvalIndex" (string idx)
            (se |> SorterEval.getSorterId, (se, labelDtr))
        )
        |> Map.ofArray


    member this.makeSorterModelSet 
                    (sorterModelSetId: Guid<sorterModelSetId>) 
                    (sorterModelGen: sorterModelGen) : sorterModelSet =
            let sorterModelIds = 
                this.SorterEvals
                |> Array.map(fun se -> 
                    se |> SorterEval.getSorterId |> UMX.untag |> UMX.tag<sorterModelId>)

            let sorterModels = SorterModelGen.makeSorterModelsFromIds sorterModelIds sorterModelGen
            sorterModelSet.create sorterModelSetId sorterModels





module IndexedSorterEvals =

    /// Helper to filter input collections for sorted candidates and remove phenotype duplicates
    let private prepareDistinctSorted (items: sorterEval seq) =
        items
        |> Seq.filter SorterEval.getIsSorted
        |> Seq.distinctBy SorterEval.getSequenceHash
        |> Seq.toArray


    let getTopN (ranker: sorterEval -> float) (n: int) (items: sorterEval array) : indexedSorterEvals =
        let distinctSorted = prepareDistinctSorted items
        let topN = distinctSorted |> Array.sortByDescending ranker |> Array.truncate n
        indexedSorterEvals.create topN


    /// Samples an array of unique evaluations spaced evenly across the ranked ARRAY INDEX layout.
    let evenSampleByRankedIndex 
                (ranker: sorterEval -> float)
                (count: int<sorterCount>) 
                (items: sorterEval seq) : indexedSorterEvals =
        
        let sampleCount = %count
        if sampleCount <= 0 then 
            failwithf "Requested sample count must be greater than zero, but was %d" sampleCount

        let cleanItems = prepareDistinctSorted items
        let availableCount = cleanItems.Length

        if availableCount < sampleCount then
            failwithf "Cannot sample %d items; only %d unique sorted options are available." sampleCount availableCount

        // Sort descending based on rank values
        let sortedItems = cleanItems |> Array.sortByDescending ranker

        if sampleCount = 1 then
            indexedSorterEvals.create [| sortedItems.[0] |]
        else
            let result = Array.zeroCreate sampleCount
            result.[0] <- sortedItems.[0]

            let step = float (availableCount - 1) / float (sampleCount - 1)
            for i in 1 .. (sampleCount - 1) do
                let targetIndex = int (round (float i * step))
                result.[i] <- sortedItems.[targetIndex]

            indexedSorterEvals.create result


    /// Samples an array of unique evaluations spaced evenly along the actual RANKED VALUE score axis.
    let evenSampleByRankedValue 
                (ranker: sorterEval -> float)
                (count: int<sorterCount>) 
                (items: sorterEval seq) : indexedSorterEvals =
        
        let sampleCount = %count
        if sampleCount <= 0 then 
            failwithf "Requested sample count must be greater than zero, but was %d" sampleCount

        let cleanItems = prepareDistinctSorted items
        let availableCount = cleanItems.Length

        if availableCount < sampleCount then
            failwithf "Cannot sample %d items; only %d unique sorted options are available." sampleCount availableCount

        let sortedItems = cleanItems |> Array.sortByDescending ranker

        if sampleCount = 1 then
            indexedSorterEvals.create [| sortedItems.[0] |]
        else
            let result = Array.zeroCreate sampleCount
            // Anchor both ends of the spectrum
            result.[0] <- sortedItems.[0]
            result.[sampleCount - 1] <- sortedItems.[availableCount - 1]

            let maxVal = ranker sortedItems.[0]
            let minVal = ranker sortedItems.[availableCount - 1]
            let valRange = maxVal - minVal

            if valRange = 0.0 then
                // If scores are uniform, fallback directly onto normal index step slicing
                let step = float (availableCount - 1) / float (sampleCount - 1)
                for i in 1 .. (sampleCount - 1) do
                    let targetIndex = int (round (float i * step))
                    result.[i] <- sortedItems.[targetIndex]
            else
                let targetStep = valRange / float (sampleCount - 1)
                
                // For each inner slot, find the evaluation closest to the mathematical value target
                for i in 1 .. (sampleCount - 2) do
                    let targetValue = maxVal - (float i * targetStep)
                    
                    let closestMatch = 
                        sortedItems 
                        |> Array.minBy (fun se -> Math.Abs(ranker se - targetValue))
                    
                    result.[i] <- closestMatch

            indexedSorterEvals.create result




type evalGroupSelection =
    | Tmb of tmbSorterEvalGroups
    | Indexed of indexedSorterEvals
    | Unknown


module EvalGroupSelection = 

    let toMap (egs: evalGroupSelection) : Map<Guid<sorterId>, (sorterEval*dataTableRecord)> =
        match egs with
        | Tmb groups -> groups.ToMap()
        | Indexed indexed -> indexed.ToMap()
        | Unknown -> Map.empty
