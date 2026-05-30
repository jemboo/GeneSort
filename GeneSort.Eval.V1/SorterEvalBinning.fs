namespace GeneSort.Eval.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Sortable
open System


/// Contains percentiles and averages for both the CE Data Sequence Position Index and its Use Count.
type ceBinSummaryStats = {
    BinIndex: int
    ItemCount: int
    
    // Position Index within the Sorter Evaluation Sequence metrics
    AvgSequencePosIndex: float
    P10SequencePosIndex: float
    P90SequencePosIndex: float
    
    // Comparator Network Element Use Count metrics
    AvgUseCount: float
    P10UseCount: float
    P90UseCount: float
}

module BinSummaryStats =
    let toDataTableRecord (stats: ceBinSummaryStats) : dataTableRecord =
        dataTableRecord.createEmpty()
        |> dataTableRecord.addData "BinIndex" (stats.BinIndex |> string)
        |> dataTableRecord.addData "ItemCount" (stats.ItemCount |> string)
        |> dataTableRecord.addData "AvgSequencePosIndex" (stats.AvgSequencePosIndex |> string)
        |> dataTableRecord.addData "P10SequencePosIndex" (stats.P10SequencePosIndex |> string)
        |> dataTableRecord.addData "P90SequencePosIndex" (stats.P90SequencePosIndex |> string)
        |> dataTableRecord.addData "AvgUseCount" (stats.AvgUseCount |> string)
        |> dataTableRecord.addData "P10UseCount" (stats.P10UseCount |> string)
        |> dataTableRecord.addData "P90UseCount" (stats.P90UseCount |> string)


/// Simple payload to hold extracted raw values during reduction
type private CeMetricSample = {
    SequencePosIndex: float
    UseCount: float
}


module SorterEvalBinning =

    /// Helper to compute percentile value from a sorted array of floats using linear interpolation
    let private percentile (p: float) (sortedItems: float array) =
        if sortedItems.Length = 0 then 0.0
        elif sortedItems.Length = 1 then sortedItems.[0]
        else
            let idx = p * float (sortedItems.Length - 1)
            let lowerIdx = int (Math.Floor(idx))
            let upperIdx = int (Math.Ceiling(idx))
            if lowerIdx = upperIdx then sortedItems.[lowerIdx]
            else
                let remainder = idx - float lowerIdx
                sortedItems.[lowerIdx] + remainder * (sortedItems.[upperIdx] - sortedItems.[lowerIdx])

    /// Divides the ceData.CeIndex range into bins of size (sortingWidth/2)
    /// and calculates metrics to support graph generation.
    let makeBins (sorterEvals: sorterEval seq) : ceBinSummaryStats array =
        
        // 1. Flatten all ceData objects across V2 and V3 records along with their array position index
        let rawSamples = 
            sorterEvals
            // Filter out V1 because it doesn't contain a ceDataSequence
            |> Seq.filter (fun eval -> 
                match eval with | V1 _ -> false | _ -> true)
            |> Seq.collect (fun eval ->
                let sortingWidth = % (SorterEval.getSortingWidth eval)
                let binSize = Math.Max(1, sortingWidth / 2) // Guard against division by 0
                let ceSequence = SorterEval.getCeDataSequence eval
                
                ceSequence 
                |> Array.mapi (fun posIndex ceData ->
                    let ceIdxVal = % ceData.CeIndex
                    let targetBin = ceIdxVal / binSize
                    
                    let sample = { 
                        SequencePosIndex = float posIndex
                        UseCount = float ceData.UseCount 
                    }
                    (targetBin, sample)
                )
            )

        // 2. Group the extracted items by calculated bin identifier and run statistical aggregates
        rawSamples
        |> Seq.groupBy fst
        |> Seq.map (fun (binIndex, group) ->
            let samples = group |> Seq.map snd |> Seq.toArray
            
            // Extract and sort values for percentile computations
            let posIndicesSorted = samples |> Array.map (fun s -> s.SequencePosIndex) |> Array.sort
            let useCountsSorted = samples |> Array.map (fun s -> s.UseCount) |> Array.sort
            
            {
                BinIndex = binIndex
                ItemCount = samples.Length
                
                AvgSequencePosIndex = posIndicesSorted |> Array.average
                P10SequencePosIndex = posIndicesSorted |> percentile 0.10
                P90SequencePosIndex = posIndicesSorted |> percentile 0.90
                
                AvgUseCount = useCountsSorted |> Array.average
                P10UseCount = useCountsSorted |> percentile 0.10
                P90UseCount = useCountsSorted |> percentile 0.90
            }
        )
        |> Seq.sortBy (fun stats -> stats.BinIndex)
        |> Seq.toArray