namespace GeneSort.SortingResults

open FSharp.UMX
open GeneSort.Sorter
open GeneSort.SortingOps
open System.Collections.Generic

[<Struct; StructuralEquality; NoComparison>]
type sorterEvalKey = {
    ceCount: int<ceCount>
    stageCount: int<stageCount>
    unsortedCount: int
}

type sorterEvalBin = {
    mutable binCount: int
    mutable sorterEvals: ResizeArray<sorterEval>
}


module SorterEvalBin =
    //returns [(uuct1, ct1), (uuct2. ct2) ... ], a histogram report of
    // unsortedCount properties of sorterEvalBin.sorterEvals, where ct1 is the
    // number of sorterEvals that have unsortedCount = ct1
    let getUnsortedHistogram (seb: sorterEvalBin) :string =
            seb.sorterEvals
            |> Seq.groupBy (fun se -> se.UnsortedCount)
            |> Seq.sortBy fst
            |> Seq.map (fun (key, group) -> sprintf "(%d, %d)" key (Seq.length group))
            |> String.concat "; "
            |> sprintf "[%s]"



type sorterSetEvalBins = {
    sorterSetEvalId: Guid<sorterSetEvalId>
    mutable totalSampleCount: int
    maxBinCount: int
    evalBins: Dictionary<sorterEvalKey, sorterEvalBin>
}

module SorterSetEvalBins =

    let addSorterEval (sorterEval: sorterEval) 
                      (sorterEvalSamples: sorterSetEvalBins) : unit =
        let key = {
            ceCount = sorterEval.getUsedCeCount()
            stageCount = sorterEval.getStageCount()
            unsortedCount = 0 // sorterEval. |> SorterEval.
        }
        let sorterEvalBin =
            if sorterEvalSamples.evalBins.ContainsKey(key) then
                sorterEvalSamples.evalBins.[key]
            else
                let newBin = {
                    binCount = 0
                    sorterEvals = ResizeArray<sorterEval>()
                }
                sorterEvalSamples.evalBins.[key] <- newBin
                newBin
        
        sorterEvalBin.binCount <- sorterEvalBin.binCount + 1
        if sorterEvalBin.sorterEvals.Count < sorterEvalSamples.maxBinCount then
            sorterEvalBin.sorterEvals.Add(sorterEval)
        
        sorterEvalSamples.totalSampleCount <- sorterEvalSamples.totalSampleCount + 1

    let create (maxBinCount: int) (sorterSetEval:sorterSetEval) : sorterSetEvalBins =
        let sorterEvalSamples = {
            sorterSetEvalId = sorterSetEval.SorterSetEvalId
            totalSampleCount = 0
            maxBinCount = maxBinCount
            evalBins = Dictionary<sorterEvalKey, sorterEvalBin>()
        }
        sorterSetEval.SorterEvals
        |> Array.iter (fun se -> addSorterEval se sorterEvalSamples)
        sorterEvalSamples


    /// Returns an array of int arrays, each inner array containing [| ceCount; stageCount; binCount |]
    let getBinCountReport (sorterEvalSamples: sorterSetEvalBins) : string array array =
        let lines = 
            sorterEvalSamples.evalBins
            |> Seq.map (fun kvp -> 
                        [| 
                            (%kvp.Key.ceCount).ToString(); 
                            (%kvp.Key.stageCount).ToString(); 
                            (kvp.Value.binCount).ToString();
                            (kvp.Value |> SorterEvalBin.getUnsortedHistogram)
                        |])
            |> Seq.toArray
        lines